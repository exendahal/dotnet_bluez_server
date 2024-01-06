using System.Threading.Tasks;
using DotnetBleServer.Core;
using Tmds.DBus;
using System.Collections.Generic;
using System;
using System.Reflection;
using DotnetBleServer.Constants;

namespace DotnetBleServer.Device
{
    public static class DeviceManager
    {
        public static void SetOnDeviceAddedListener(ServerContext context, Action<(ObjectPath @object, IDictionary<string, IDictionary<string, object>> interfaces)> action)
        {
            var bluezObject = context.Connection.CreateProxy<IObjectManager>(BlueZConstants.BASE_PATH, "/");
            bluezObject.WatchInterfacesAddedAsync(action);
        }

        public static async void SetOnDeviceConnectionChangeListener(ServerContext context, Action<IDevice1, PropertyChanges> handler)
        {
            var adapter = AdapterManager.CurrentAdapter;
            if (adapter == null)
                adapter = AdapterManager.GetAdapter(context);
            await adapter.WatchPropertiesAsync(AdapterEvent);
            var devices = await GetDeviceListAsync(context);
            foreach (var device in devices)
            {
                await device.WatchPropertiesAsync(changes => handler(device, changes));
            }

            async void OnDeviceAdded((ObjectPath objectPath, IDictionary<string, IDictionary<string, object>> interfaces) args)
            {
                Console.WriteLine($"Object path: {args.objectPath}");

                foreach (var item in args.interfaces)
                {
                    Console.WriteLine($"Key: {item.Key}");
                    foreach (var value in item.Value)
                    {
                        Console.WriteLine($"value:{value.Value}");
                    }
                }
                if (BlueZManager.IsMatch(BlueZConstants.IDEVICE_PATH, args.objectPath, args.interfaces, adapter))
                {
                    IDevice1 device1 = context.Connection.CreateProxy<IDevice1>(BlueZConstants.BASE_PATH, args.objectPath);
                    void overriddenHandler(IDevice1 device, PropertyChanges propertyChanges)
                    {
                        foreach (var change in propertyChanges.Changed)
                        {
                            Console.WriteLine($"{change.Key}:{change.Value}");
                        }
                        handler(device, propertyChanges);
                    }
                    await device1.WatchPropertiesAsync(changes => overriddenHandler(device1, changes));
                }
            }
            SetOnDeviceAddedListener(context, OnDeviceAdded);
        }
        private static void AdapterEvent(PropertyChanges changes)
        {
            foreach (var change in changes.Changed)
            {
                Console.WriteLine(change.Key);
            }
        }

        public static async Task<Dictionary<ObjectPath, IDevice1>> GetDeviceDictionaryListAsync(ServerContext context)
        {
            var devices = new Dictionary<ObjectPath, IDevice1>();
            var bluezObject = context.Connection.CreateProxy<IObjectManager>(BlueZConstants.BASE_PATH, "/");
            var ifaceList = await bluezObject.GetManagedObjectsAsync();

            foreach (var entry in ifaceList)
            {
                var path = entry.Key;
                var interfaces = entry.Value;
                if (interfaces.ContainsKey(BlueZConstants.IDEVICE_PATH))
                {
                    var device = context.Connection.CreateProxy<IDevice1>(BlueZConstants.BASE_PATH, path);
                    devices[path] = device;
                }
            }
            return devices;
        }
        public static async Task<List<IDevice1>> GetDeviceListAsync(ServerContext context)
        {
            List<IDevice1> list = new List<IDevice1>();
            var devices = await GetDeviceDictionaryListAsync(context);
            foreach (var device in devices)
            {
                list.Add(device.Value);
            }
            return list;
        }

        public static async Task<List<IDevice1>> GetDevicePairedListAsync(ServerContext context)
        {
            List<IDevice1> list = new List<IDevice1>();
            var devices = await GetDeviceDictionaryListAsync(context);
            foreach (var device in devices)
            {
                var paired = await device.Value.GetPairedAsync();
                if (paired)
                {
                    list.Add(device.Value);
                }
            }
            return list;
        }
        public static async Task<bool> PairDeviceAsync(IDevice1 device)
        {
            var info = await device.GetAllAsync();
            if (info != null)
            {
                if (!info.Paired)
                {
                    await device.PairAsync();
                }
                return await device.GetPairedAsync();
            }
            return false;
        }

        public static async Task<bool> RemoveDeviceAsync(ServerContext context, IDevice1 device)
        {
            if (device != null)
            {
                var path = await FindDevicePathAsync(context, device);
                Console.WriteLine($"Removing device: {path}");
                if (!string.IsNullOrWhiteSpace(path))
                {
                    await AdapterManager.RemoveDeviceAsync(context, new ObjectPath(path));
                    return true;
                }
            }
            return false;
        }

        public static async Task CancelPairingAsync(IDevice1 device)
        {
            await device.CancelPairingAsync();
        }
        static async Task<string> FindDevicePathAsync(ServerContext context, IDevice1 device)
        {
            try
            {
                Dictionary<ObjectPath, IDevice1> devices = await GetDeviceDictionaryListAsync(context);
                var deviceInfo = await device.GetAllAsync();
                foreach (var item in devices)
                {
                    var info = await item.Value.GetAllAsync();
                    if (info.Address == deviceInfo.Address)
                    {
                        return item.Key.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return string.Empty;
        }

        public static async Task<IDevice1> GetDeviceAsync(ServerContext context, string address)
        {
            Dictionary<ObjectPath, IDevice1> devices = await GetDeviceDictionaryListAsync(context);
            foreach (var item in devices)
            {
                var info = await item.Value.GetAllAsync();
                if (info.Address == address)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public static void PrintProperties(Device1Properties obj)
        {
            Type type = obj.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                object value = property.GetValue(obj);
                Console.WriteLine($"{property.Name}: {value}");
            }
        }

        static async Task<bool> CheckDeviceByAddress(ServerContext context, string address)
        {
            var devices = await GetDeviceListAsync(context);
            foreach (var device in devices)
            {
                var currentDeviceAddress = await device.GetAddressAsync();
                var paired = await device.GetPairedAsync();
                if (currentDeviceAddress == address && paired)
                {
                    return true;
                }
            }
            return false;
        }
    }
}