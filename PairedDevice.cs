namespace Devn.TrayUsbDeviceControl;

public sealed record PairedDevice(
    string Id,
    string Name,
    bool IsConnected, // Real-time status (FromIdAsync)
    string Kind,
    ulong Address = 0,
    bool IsConnectedByOS = false // Cached background status from DeviceInformation
);


public sealed record DeviceService(
    Guid Id,
    string Name);



