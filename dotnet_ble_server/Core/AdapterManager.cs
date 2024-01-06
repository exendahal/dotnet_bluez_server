
using DotnetBleServer.Constants;
using System.Threading.Tasks;
using Tmds.DBus;

namespace DotnetBleServer.Core
{
    public static class AdapterManager
    {
        public static IAdapter1 CurrentAdapter { get; private set; } = null;
        public static IAdapter1 GetAdapter(ServerContext context)
        {
            CurrentAdapter = context.Connection.CreateProxy<IAdapter1>(
               BlueZConstants.BASE_PATH,
                BlueZConstants.ADAPTER_PATH
            );
            return CurrentAdapter;
        }
        public static async Task<bool> SetAdapterPowerOn(ServerContext context)
        {
            if (CurrentAdapter == null)
                CurrentAdapter = GetAdapter(context);
            await CurrentAdapter.SetPoweredAsync(true);
            return await CurrentAdapter.GetPoweredAsync();
        }
        public static async Task SetAdapterPowerOff(ServerContext context)
        {
            if (CurrentAdapter == null)
                CurrentAdapter = GetAdapter(context);
            await CurrentAdapter.SetPoweredAsync(false);
        }

        public static async Task RemoveDeviceAsync(ServerContext context, ObjectPath path)
        {
            if (CurrentAdapter == null)
                CurrentAdapter = GetAdapter(context);
            await CurrentAdapter.RemoveDeviceAsync(path);
        }
    }

}
