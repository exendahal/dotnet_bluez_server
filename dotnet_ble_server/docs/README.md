# Overview

.NET BlueZ Server is a powerful library designed to facilitate the creation of Bluetooth Low Energy (BLE) peripherals and efficiently manage connected centrals on Linux using .NET Core. Leveraging the capabilities of BlueZ under the hood, this library offers a seamless API that preserves the object structure of BlueZ while streamlining D-Bus communication for a more developer-friendly experience.

## Key features
- BLE Peripheral Creation
- Adapter Management
- Central Management
- Pair device
- Remove Device
- Get all paired devices
- Get central object path on Read and Write request

## Example


Create BLE peripheral

```
ServerContext _CurrentServerContext = new ServerContext();
await BleAdvertisement.RegisterAdvertisement(_CurrentServerContext);
await BleGattApplication.RegisterGattApplication(_CurrentServerContext);
DeviceManager.SetDevicePropertyListenerAsync(_CurrentServerContext, OnDeviceConnectedAsync);
```

Listen Central property change

```
private async void OnDeviceConnectedAsync(IDevice1 device, PropertyChanges changes)
{
    foreach (var change in changes.Changed)
    {
        Console.WriteLine($"{change.Key}:{change.Value}");                  
    }
}
```

Get BLE Adapter 

```
AdapterManager.GetAdapter(_CurrentServerContext);
```

Turn ON/OFF BLE Adapter

```
var isAdapterOn = AdapterManager.SetAdapterPowerOn(_CurrentServerContext);
var isAdapterOff = AdapterManager.SetAdapterPowerOff(_CurrentServerContext);
```

Get paired devices

```
var devices = await DeviceManager.GetDeviceListAsync(_CurrentServerContext);
```

Remove paired device

```
await DeviceManager.RemoveDeviceAsync(_CurrentServerContext, iDevice);
```

Get Device from Address

```
 var device = await DeviceManager.GetDeviceAsync(_CurrentServerContext, address);
```

Get Device from object path

```
var device = await DeviceManager.GetDeviceByObject(_CurrentServerContext, address);
```

Pair device

```
var paired = await device.GetPairedAsync();
if (!paired)
{
    var response = await DeviceManager.PairDeviceAsync(_CurrentServerContext,device);
}
```

## Useful references
| Resource                                | Link                                                                                           |
|-----------------------------------------|------------------------------------------------------------------------------------------------|
| BlueZ GATT API documentation            | [https://git.kernel.org/pub/scm/bluetooth/bluez.git/tree/doc/gatt-api.txt](https://git.kernel.org/pub/scm/bluetooth/bluez.git/tree/doc/gatt-api.txt) |
| Presentation BLE on Linux               | [https://elinux.org/images/3/32/Doing_Bluetooth_Low_Energy_on_Linux.pdf](https://elinux.org/images/3/32/Doing_Bluetooth_Low_Energy_on_Linux.pdf)       |
