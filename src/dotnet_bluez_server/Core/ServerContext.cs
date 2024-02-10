using DotnetBleServer.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Tmds.DBus;

namespace DotnetBleServer.Core
{
    public class ServerContext : IDisposable
    {
        public ServerContext()
        {
            if (OsUtility.IsOperatingSystem(OSPlatform.Linux))
                Connection = new Connection(Address.System);
        }

        public async Task Connect()
        {
            if (!OsUtility.IsOperatingSystem(OSPlatform.Windows))
                await Connection.ConnectAsync();
        }

        public Connection Connection { get; }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}