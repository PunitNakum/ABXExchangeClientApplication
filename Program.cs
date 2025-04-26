using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
public class Program
{
    private static async Task Main(string[] args)
    {
        string serverAddress = "127.0.0.1"; // <-- Change if needed
        int serverPort = 3000;

        List<Packet> packets = await StreamAllPackets(serverAddress, serverPort);

        List<int> missingSequences = FindMissingSequences(packets);

        foreach (int missingSeq in missingSequences)
        {
            Packet missingPacket = await ResendPacket(serverAddress, serverPort, missingSeq);
            packets.Add(missingPacket);
        }

        packets = packets.OrderBy(p => p.Sequence).ToList();

        string json = JsonSerializer.Serialize(packets, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync("packets.json", json);

        Console.WriteLine("Finished! Output saved to packets.json");
    }
    private static async Task<List<Packet>> StreamAllPackets(string address, int port)
    {
        List<Packet> packets = new List<Packet>();

        using (TcpClient client = new TcpClient())
        {
            await client.ConnectAsync(address, port);
            NetworkStream stream = client.GetStream();

            byte[] request = new byte[2] { 1, 0 };
            await stream.WriteAsync(request, 0, request.Length);

            byte[] buffer = new byte[17];

            try
            {
                int bytesRead;
                while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    if (bytesRead == 17)
                    {
                        Packet packet = ParsePacket(buffer);
                        packets.Add(packet);
                    }
                }
            }
            catch (IOException)
            {
                Console.WriteLine("Server closed!! please check that server is running...");
            }
        }

        return packets;
    }
    private static Packet ParsePacket(byte[] buffer)
    {
        string symbol = Encoding.ASCII.GetString(buffer, 0, 4);
        char buySell = (char)buffer[4];

        int quantity = BitConverter.ToInt32(buffer.Skip(5).Take(4).Reverse().ToArray(), 0);
        int price = BitConverter.ToInt32(buffer.Skip(9).Take(4).Reverse().ToArray(), 0);
        int sequence = BitConverter.ToInt32(buffer.Skip(13).Take(4).Reverse().ToArray(), 0);

        return new Packet
        {
            Symbol = symbol,
            BuySellIndicator = buySell,
            Quantity = quantity,
            Price = price,
            Sequence = sequence
        };
    }
    private static async Task<Packet> ResendPacket(string address, int port, int seq)
    {
        using (TcpClient client = new TcpClient())
        {
            await client.ConnectAsync(address, port);
            NetworkStream stream = client.GetStream();

            byte[] request = new byte[2] { 2, (byte)seq };
            await stream.WriteAsync(request, 0, request.Length);

            byte[] buffer = new byte[17];
            await stream.ReadAsync(buffer, 0, buffer.Length);

            return ParsePacket(buffer);
        }
    }
    private static List<int> FindMissingSequences(List<Packet> packets)
    {
        List<int> sequences = packets.Select(p => p.Sequence).ToList();
        sequences.Sort();

        List<int> missing = new List<int>();

        for (int i = sequences.First(); i < sequences.Last(); i++)
        {
            if (!sequences.Contains(i))
            {
                missing.Add(i);
            }
        }

        return missing;
    }
}
public class Packet
{
    public string Symbol { get; set; }
    public char BuySellIndicator { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
    public int Sequence { get; set; }
}