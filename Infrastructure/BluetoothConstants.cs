using System;
using System.Collections.Generic;

namespace Devn.TrayUsbDeviceControl.Infrastructure;

public static class BluetoothServiceGuids
{
    public static readonly Guid AudioSink = new Guid("0000110b-0000-1000-8000-00805f9b34fb");
    public static readonly Guid AudioSource = new Guid("0000110a-0000-1000-8000-00805f9b34fb");
    public static readonly Guid Handsfree = new Guid("0000111e-0000-1000-8000-00805f9b34fb");
    public static readonly Guid A2dp = new Guid("0000110d-0000-1000-8000-00805f9b34fb");
    public static readonly Guid Headset = new Guid("00001108-0000-1000-8000-00805f9b34fb");
    public static readonly Guid Avrcp = new Guid("0000110e-0000-1000-8000-00805f9b34fb");
    public static readonly Guid AvrcpTarget = new Guid("0000110c-0000-1000-8000-00805f9b34fb");
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
