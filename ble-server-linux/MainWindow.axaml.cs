using System;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using DotnetBleServer.Core;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using DotnetBleServer.Device;
using Tmds.DBus;
using Avalonia.Threading;
using System.Threading;


namespace bletest;
public partial class MainWindow : Window
{
    private ServerContext _CurrentServerContext { get; set; } = new ServerContext();
    ObservableCollection<DeviceModel> pairedList = new ObservableCollection<DeviceModel>();
    public MainWindow()
    {
        InitializeComponent();
        _CurrentServerContext.Connect();
        _CurrentServerContext.Connection.StateChanged += StateChanged;
        GetPairedList();
    }

    private async void GetPairedList()
    {
        var devices = await DeviceManager.GetDeviceListAsync(_CurrentServerContext);
        int serialNumber = 0;
        foreach (var device in devices)
        {
            serialNumber++;
            var deviceName = await device.GetNameAsync();
            var address = await device.GetAddressAsync();
            var alias = await device.GetAliasAsync();
            var paired = await device.GetPairedAsync();
            var trusted = await device.GetTrustedAsync();
            var uuids = await device.GetUUIDsAsync();
            var modalias = await device.GetModaliasAsync();         
            pairedList.Add(
            new DeviceModel()
            {
                Sn = serialNumber,
                Name = deviceName,
                Address = address,
                Alias = alias,
                Paired = paired,
                Trusted = trusted,
                UUIDs = uuids,
                Modalias = modalias
            });
        }
        var listBox = this.FindControl<ListBox>("list");
        dataGrid.Items = pairedList;
    }
    private void StateChanged(object? sender, Tmds.DBus.ConnectionStateChangedEventArgs e)
    {
        var state = e.State;
        Console.WriteLine("Connection status: " + e.State);
    }
    private void OnStartBle(object sender, RoutedEventArgs e)
    {        
        StartBleServer();    
    }

    private void OnStopBle(object sender, RoutedEventArgs e)
    {
        _CurrentServerContext.Dispose();
    }
    private void StartBleServer()
    {
        Task.Run(async () =>
        {
            await BleAdvertisement.RegisterAdvertisement(_CurrentServerContext);
            await BleGattApplication.RegisterGattApplication(_CurrentServerContext);
            DeviceManager.SetOnDeviceConnectionChangeListener(_CurrentServerContext, OnDeviceConnected);
        }).Wait();
    }
    private async void OnDeviceConnected(IDevice1 device, PropertyChanges changes)
    {
        foreach (var change in changes.Changed)
        {
            Console.WriteLine($"{change.Key}:{change.Value}");
            if (change.Key == "UUIDs")
            {
                await CheckPairing(device);
            }
            if (change.Key == "Connected")
            {
                if (Convert.ToBoolean(change.Value))
                {
                    await CheckPairing(device);
                }
                else
                {
                    
                }
            }
        }
    }

     async Task CheckPairing(IDevice1 device)
    {       
        var paired = await device.GetPairedAsync();
        if (!paired)
        {
            var response = await DeviceManager.PairDeviceAsync(device);
            if (response)
            {
                var isPaired = await device.GetPairedAsync();
                var address = await device.GetAddressAsync();
                var name = await device.GetAliasAsync();                
            }
            else
            {
               
            }
        }
        else
        {
            var isPaired = await device.GetPairedAsync();
            var address = await device.GetAddressAsync();
            var name = await device.GetAliasAsync();            
        }
    }
}
