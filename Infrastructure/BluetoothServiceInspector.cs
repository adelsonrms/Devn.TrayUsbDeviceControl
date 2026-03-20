using Devn.TrayUsbDeviceControl.Core.Models;
using System.Runtime.InteropServices;

namespace Devn.TrayUsbDeviceControl.Infrastructure;

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
