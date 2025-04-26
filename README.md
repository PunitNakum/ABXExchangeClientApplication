# ABXExchangeClientApplication


This is a C# console application that connects to the **ABX Mock Exchange Server**, receives stock ticker packets over TCP, handles any missing sequences, and outputs the final result as a JSON file.

---

## 🛠 Requirements

- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/)
- [.NET 6.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
- [Node.js](https://nodejs.org/) (to run the ABX exchange server)

---

## 🚀 How to Run

### 1. Clone or Download the Project

Place the code into a folder or use Visual Studio's **"Create new project"** option and paste the content into `Program.cs`.

---

### 2. Start the ABX Exchange Server

Make sure you have the `main.js` server file from the ABX mock server provider.

In the terminal:

```bash
cd path/to/abx-server
node main.js
