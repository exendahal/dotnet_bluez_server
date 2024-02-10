using System;
using System.Text;
using System.Threading.Tasks;
using DotnetBleServer.Gatt;

namespace bletest;


public class CharacteristicSource : ICharacteristicSource
{
    public override Task ConfirmAsync()
    {
        return Task.CompletedTask;
    }

    public override Task<byte[]> ReadValueAsync(string objectPath)
    {
        TaskCompletionSource<byte[]> tcs = new();
        tcs.TrySetResult(Encoding.ASCII.GetBytes($"Read Operation"));
        return tcs.Task;
    }

    public override Task StartNotifyAsync()
    {
        return Task.CompletedTask;
    }

    public override Task StopNotifyAsync()
    {
        return Task.CompletedTask;
    }

    public override Task WriteValueAsync(byte[] value, bool response,string objectPath)
    {
        string dataString = Encoding.UTF8.GetString(value);
        Console.WriteLine($"Write from central: {dataString}");
        return Task.CompletedTask;
    }
}