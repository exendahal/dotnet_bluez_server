using System;
using System.Threading.Tasks;
using DotnetBleServer.Advertisements;
using DotnetBleServer.Core;

namespace bletest;


public class BleAdvertisement
{
    public static async Task RegisterAdvertisement(ServerContext serverContext)
    {
        var advertisementProperties = new AdvertisementProperties
        {
            Type = "peripheral",
            ServiceUUIDs = new[] { BleParam.SERVICE_UUID },
            LocalName = BleParam.DEVICE_NAME
        };
        await new AdvertisingManager(serverContext).CreateAdvertisement(advertisementProperties, OnAdvertisementReceived);
    }

    private static void OnAdvertisementReceived(AdvertisementReceivedEventArgs args)
    {
        Console.WriteLine(args.AdvertisementData);
        Console.WriteLine(args.DeviceAddress);
    }

}
