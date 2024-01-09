using System;
using System.ComponentModel;

namespace bletest;

public class DeviceModel
{    
    public int Sn { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public bool Paired { get; set; }
    public bool Trusted { get; set; }
    public string[] UUIDs { get; set; }
    public string Modalias { get; set; } = string.Empty;
    public string ConcatenatedUUIDs => string.Join(", ", UUIDs ?? Array.Empty<string>());  
}