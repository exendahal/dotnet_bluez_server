using System;

namespace DotnetBleServer.Gatt.Description
{
    [Flags]
    public enum CharacteristicFlags
    {
        Read = 1,
        Write = 2,
        WritableAuxiliaries = 4,
        Notify = 8,
        Indicate = 16
    }

    public enum GattDescriptorFLags
    {
        read, write
    }
}
