using System;
using System.Collections.Generic;

namespace Devn.TrayUsbDeviceControl.Core.Models;

public sealed record PairedDevice(
    string Id,
    string Name,
    bool IsConnected,
    string Kind,
    ulong Address = 0,
    bool IsConnectedByOS = false
);

public sealed record DeviceService(
    Guid Id,
    string Name
);

public sealed class BluetoothClassicDevice
{
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public bool IsConnected { get; set; }
    public bool IsRemembered { get; set; }
    public bool IsAuthenticated { get; set; }
    public List<DeviceService> Services { get; internal set; }
    public ulong AddressId { get; internal set; }
}