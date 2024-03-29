﻿using System.Threading.Tasks;
using DotnetBleServer.Core;
using DotnetBleServer.Gatt.BlueZModel;

namespace DotnetBleServer.Gatt
{
    public abstract class ICharacteristicSource
    {
        public PropertiesBase<GattCharacteristic1Properties> Properties;
        public abstract Task WriteValueAsync(byte[] value, bool response,string objPath);
        public abstract Task<byte[]> ReadValueAsync(string objPath);
        public abstract Task StartNotifyAsync();
        public abstract Task StopNotifyAsync();
        public abstract Task ConfirmAsync();
    }
}
