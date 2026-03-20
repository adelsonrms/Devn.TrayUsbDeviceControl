using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using Devn.TrayUsbDeviceControl.Core.Models;

namespace Devn.TrayUsbDeviceControl.Infrastructure;

public sealed class BluetoothClassicService
{
    public List<BluetoothClassicDevice> GetPairedDevices()
    {
        var result = new List<BluetoothClassicDevice>();

        BLUETOOTH_DEVICE_SEARCH_PARAMS searchParams = new()
        {
            dwSize = Marshal.SizeOf<BLUETOOTH_DEVICE_SEARCH_PARAMS>(),
            fReturnAuthenticated = true,
            fReturnRemembered = true,
            fReturnUnknown = false,
            fReturnConnected = true,
            fIssueInquiry = false,
            cTimeoutMultiplier = 0,
            hRadio = IntPtr.Zero
        };

        BLUETOOTH_DEVICE_INFO deviceInfo = new()
        {
            dwSize = Marshal.SizeOf<BLUETOOTH_DEVICE_INFO>()
        };

        IntPtr findHandle = BluetoothFindFirstDevice(ref searchParams, ref deviceInfo);

        if (findHandle == IntPtr.Zero)
            return result;

        try
        {
            do
            {
                result.Add(new BluetoothClassicDevice
                {
                    Name = deviceInfo.szName,
                    Address = deviceInfo.Address.ToString("X"),
                    IsConnected = deviceInfo.fConnected,
                    IsRemembered = deviceInfo.fRemembered,
                    IsAuthenticated = deviceInfo.fAuthenticated
                });

                deviceInfo = new BLUETOOTH_DEVICE_INFO
                {
                    dwSize = Marshal.SizeOf<BLUETOOTH_DEVICE_INFO>()
                };
            }
            while (BluetoothFindNextDevice(findHandle, ref deviceInfo));
        }
        finally
        {
            BluetoothFindDeviceClose(findHandle);
        }

        return result;
    }

    [DllImport("bthprops.cpl", SetLastError = true)]
    private static extern IntPtr BluetoothFindFirstDevice(
        ref BLUETOOTH_DEVICE_SEARCH_PARAMS searchParams,
        ref BLUETOOTH_DEVICE_INFO deviceInfo);

    [DllImport("bthprops.cpl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool BluetoothFindNextDevice(
        IntPtr hFind,
        ref BLUETOOTH_DEVICE_INFO deviceInfo);

    [DllImport("bthprops.cpl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool BluetoothFindDeviceClose(IntPtr hFind);

    [StructLayout(LayoutKind.Sequential)]
    private struct BLUETOOTH_DEVICE_SEARCH_PARAMS
    {
        public int dwSize;
        [MarshalAs(UnmanagedType.Bool)] public bool fReturnAuthenticated;
        [MarshalAs(UnmanagedType.Bool)] public bool fReturnRemembered;
        [MarshalAs(UnmanagedType.Bool)] public bool fReturnUnknown;
        [MarshalAs(UnmanagedType.Bool)] public bool fReturnConnected;
        [MarshalAs(UnmanagedType.Bool)] public bool fIssueInquiry;
        public byte cTimeoutMultiplier;
        public IntPtr hRadio;
    }

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
