using System.Runtime.InteropServices;

namespace Devn.TrayUsbDeviceControl;

public sealed class BluetoothServiceInspector
{
    public List<DeviceService> GetInstalledServices(ulong bluetoothAddress)
    {
        var deviceInfo = new BLUETOOTH_DEVICE_INFO
        {
            dwSize = Marshal.SizeOf<BLUETOOTH_DEVICE_INFO>(),
            Address = bluetoothAddress
        };

        int serviceCount = 0;

        int hr = BluetoothEnumerateInstalledServices(
            IntPtr.Zero,
            ref deviceInfo,
            ref serviceCount,
            null);

        if (serviceCount <= 0)
            return new List<DeviceService>();

        var services = new Guid[serviceCount];

        hr = BluetoothEnumerateInstalledServices(
            IntPtr.Zero,
            ref deviceInfo,
            ref serviceCount,
            services);

        if (hr != 0)
            return new List<DeviceService>();

        return services
            .Select(srv => new DeviceService (srv, BluetoothServiceNames.GetName(srv) ))
            .ToList();
    }

    [DllImport("bthprops.cpl", SetLastError = true)]
    private static extern int BluetoothEnumerateInstalledServices(
        IntPtr hRadio,
        ref BLUETOOTH_DEVICE_INFO pbtdi,
        ref int pcServices,
        [Out] Guid[]? pGuidServices);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct BLUETOOTH_DEVICE_INFO
    {
        public int dwSize;
        public ulong Address;
        public uint ulClassofDevice;
        [MarshalAs(UnmanagedType.Bool)] public bool fConnected;
        [MarshalAs(UnmanagedType.Bool)] public bool fRemembered;
        [MarshalAs(UnmanagedType.Bool)] public bool fAuthenticated;
        public SYSTEMTIME stLastSeen;
        public SYSTEMTIME stLastUsed;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 248)]
        public string szName;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SYSTEMTIME
    {
        public ushort wYear;
        public ushort wMonth;
        public ushort wDayOfWeek;
        public ushort wDay;
        public ushort wHour;
        public ushort wMinute;
        public ushort wSecond;
        public ushort wMilliseconds;
    }
}

public static class BluetoothServiceGuids
{
    public static readonly Guid AudioSink = new("0000110B-0000-1000-8000-00805F9B34FB");
    public static readonly Guid AudioSource = new("0000110A-0000-1000-8000-00805F9B34FB");
    public static readonly Guid Handsfree = new("0000111E-0000-1000-8000-00805F9B34FB");
    public static readonly Guid Headset = new("00001108-0000-1000-8000-00805F9B34FB");
    public static readonly Guid A2dp = new("0000110D-0000-1000-8000-00805F9B34FB");
    public static readonly Guid Avrcp = new("0000110E-0000-1000-8000-00805F9B34FB");
    public static readonly Guid AvrcpTarget = new("0000110C-0000-1000-8000-00805F9B34FB");
}

public static class BluetoothServiceNames
{
    private static readonly Dictionary<Guid, string> _serviceNames = new()
    {
        { BluetoothServiceGuids.AudioSink, "Audio Sink" },
        { BluetoothServiceGuids.AudioSource, "Audio Source" },
        { BluetoothServiceGuids.Handsfree, "Handsfree" },
        { BluetoothServiceGuids.Headset, "Headset" },
        { BluetoothServiceGuids.A2dp, "A2DP" }
    };
    public static string GetName(Guid serviceGuid)
    {
        if (_serviceNames.TryGetValue(serviceGuid, out var name))
            return name;
        return serviceGuid.ToString();
    }
}
