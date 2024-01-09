using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using DotnetBleServer.Core;
using DotnetBleServer.Device;
using Tmds.DBus;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Avalonia.Threading;
using Avalonia;

namespace bletest;

public class MainWindowViewModel : BindableBase
{

    private ObservableCollection<DeviceModel> _PairedList = new ObservableCollection<DeviceModel>();
    public ObservableCollection<DeviceModel> PairedList
    {
        get { return _PairedList; }
        set
        {
            SetProperty(ref _PairedList, value);
        }
    }  
    public ICommand RemoveDeviceCommand { get; set; }
    public ICommand StartCommand { get; set; }
    public ICommand StopCommand { get; set; }
    private ServerContext _CurrentServerContext { get; set; } = new ServerContext();

    public MainWindowViewModel()
    {
        RemoveDeviceCommand = new RelayCommand(OnRemoveClick);
        StartCommand = new RelayCommand(OnStartClick);
        StopCommand = new RelayCommand(OnStopClick);
        _CurrentServerContext.Connect();
        _CurrentServerContext.Connection.StateChanged += StateChanged;
        GetPairedList();
    }
    private void OnStartClick(object device) 
    { 
        StartBleServer();
    }
    private void OnStopClick(object device)
    {
        _CurrentServerContext.Dispose();
    }
    private async void OnRemoveClick(object device)
    {
        DeviceModel slectedDevice = (DeviceModel)device;
        var iDevice = await DeviceManager.GetDeviceAsync(_CurrentServerContext, slectedDevice.Address);
        await DeviceManager.RemoveDeviceAsync(_CurrentServerContext, iDevice);
        PairedList.Remove(PairedList.Where(i => i.Address == slectedDevice.Address).Single());
    } 
    private async void GetPairedList()
    {
        var devices = await DeviceManager.GetDeviceListAsync(_CurrentServerContext);
        int serialNumber = 0;
        PairedList.Clear();
        foreach (var device in devices)
        {
            serialNumber++;
            var deviceName = await device.GetNameAsync();
            var address = await device.GetAddressAsync();
            var alias = await device.GetAliasAsync();
            var paired = await device.GetPairedAsync();
            var trusted = await device.GetTrustedAsync();
            var uuids = await device.GetUUIDsAsync();
            string modalias = string.Empty;
            try
            {
                modalias = await device.GetModaliasAsync();
            }
            catch
            {

            }
            PairedList.Add(new DeviceModel()
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
        //GenerateDummyDeviceList(10);
    }
    private void GenerateDummyDeviceList(int count)
    {

        for (int i = 1; i <= count; i++)
        {
            PairedList.Add(new DeviceModel
            {
                Sn = i,
                Name = $"Device{i}",
                Address = $"00:11:22:33:44:5{i}",
                Alias = $"Alias{i}",
                Paired = i % 2 == 0,
                Trusted = i % 3 == 0,
                UUIDs = new string[] { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() },
                Modalias = $"Modalias{i}"
            });
        }
    }
    private void StateChanged(object? sender, ConnectionStateChangedEventArgs e)
    {
        var state = e.State;
        Console.WriteLine("Connection status: " + e.State);
    }
    private void StartBleServer()
    {
        Task.Run(async () =>
        {
            await BleAdvertisement.RegisterAdvertisement(_CurrentServerContext);
            await BleGattApplication.RegisterGattApplication(_CurrentServerContext);
            DeviceManager.SetOnDeviceConnectionChangeListener(_CurrentServerContext, OnDeviceConnected, OnpairingRequestd);
        }).Wait();
    }

    private async void OnpairingRequestd(ObjectPath path, string interfaceName, IDevice1 device, KeyValuePair<string, IDictionary<string, object>> items)
    {
        var paired = await device.GetPairedAsync();   
        if(!paired)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await ConfirmPairing(device);
            });     

        }
    }
    async Task ConfirmPairing(IDevice1 device)
    {
        var name = await device.GetAliasAsync();
            var box = MessageBoxManager
                    .GetMessageBoxStandard($"Pair {name}?", $"Are you sure you want to pair {name}?",
                        ButtonEnum.YesNo);

        ButtonResult result = await box.ShowAsync();
        if (result == ButtonResult.Yes)
        {
            await CheckPairing(device);
        }
        else
        {
            await DeviceManager.RemoveDeviceAsync(_CurrentServerContext, device);
        }
         GetPairedList();
    }
    private async void OnDeviceConnected(IDevice1 device, PropertyChanges changes)
    {
        foreach (var change in changes.Changed)
        {
            Console.WriteLine($"{change.Key}:{change.Value}");            
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

    private async Task CheckPairing(IDevice1 device)
    {
        var paired = await device.GetPairedAsync();
        if (!paired)
        {
            var response = await DeviceManager.PairDeviceAsync(_CurrentServerContext,device);
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