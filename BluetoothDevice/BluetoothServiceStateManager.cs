namespace Devn.TrayUsbDeviceControl;

using System;
using System.Runtime.InteropServices;

public sealed class BluetoothServiceStateManager
{
    private const uint BLUETOOTH_SERVICE_DISABLE = 0x00;
    private const uint BLUETOOTH_SERVICE_ENABLE = 0x01;

    public int EnableService(ulong bluetoothAddress, Guid serviceGuid)
    {
        var deviceInfo = new BLUETOOTH_DEVICE_INFO
        {
            dwSize = Marshal.SizeOf<BLUETOOTH_DEVICE_INFO>(),
            Address = bluetoothAddress
        };

        return BluetoothSetServiceState(
            IntPtr.Zero,
            ref deviceInfo,
            ref serviceGuid,
            BLUETOOTH_SERVICE_ENABLE);
    }

    public int DisableService(ulong bluetoothAddress, Guid serviceGuid)
    {
        var deviceInfo = new BLUETOOTH_DEVICE_INFO
        {
            dwSize = Marshal.SizeOf<BLUETOOTH_DEVICE_INFO>(),
            Address = bluetoothAddress
        };

        return BluetoothSetServiceState(
            IntPtr.Zero,
            ref deviceInfo,
            ref serviceGuid,
            BLUETOOTH_SERVICE_DISABLE);
    }

    [DllImport("bthprops.cpl", SetLastError = true)]
    private static extern int BluetoothSetServiceState(
        IntPtr hRadio,
        ref BLUETOOTH_DEVICE_INFO pbtdi,
        ref Guid pGuidService,
        uint dwServiceFlags);

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