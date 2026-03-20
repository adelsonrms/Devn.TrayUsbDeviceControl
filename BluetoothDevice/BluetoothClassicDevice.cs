namespace Devn.TrayUsbDeviceControl;

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


