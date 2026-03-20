namespace Devn.TrayUsbDeviceControl;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Devices.Enumeration;

public sealed class BluetoothDeviceService
{
    public async Task<List<PairedDevice>> GetPairedAudioDevicesAsync()
    {
        return await Task.Run(() =>
        {
            var result = new List<PairedDevice>();
            var classic = new BluetoothClassicService();

            var devices = classic.GetPairedDevices();

            foreach (var device in devices)
            {
                ulong address = 0;
                try { address = Convert.ToUInt64(device.Address, 16); } catch { }

                bool isConnectedReal = device.IsConnected;
                
                // GetPnpStatus chama o WMI, que pode ser lento. Task.Run garante que não trave a UI.
                string pnpStatus = GetPnpStatus(address);

                result.Add(new PairedDevice(
                    device.Address,
                    device.Name,
                    isConnectedReal,
                    "Bluetooth",
                    address,
                    pnpStatus == "OK"
                ));
            }

            return result.OrderBy(x => x.Name).ToList();
        });
    }

    private string GetPnpStatus(ulong address)
    {
        try
        {
            // O DeviceID no WMI para Bluetooth costuma conter o endereço hex (Pode ser BTHENUM\{...}_DEV_ADDRESS)
            string hexAddress = address.ToString("X12");
            using var searcher = new ManagementObjectSearcher(
                $"SELECT Status, ConfigManagerErrorCode FROM Win32_PnPEntity WHERE DeviceID LIKE '%{hexAddress}%'");
            
            using var collection = searcher.Get();
            foreach (ManagementObject obj in collection)
            {
                return obj["Status"]?.ToString() ?? "Unknown";
            }
        }
        catch { }
        return "Unknown";
    }

    private static ulong ExtractAddressFromId(string id)
    {
        try
        {
            // O endereço do dispositivo remoto costuma ser a última parte após o '-'
            string lastPart = id.Split('-').Last();
            string hexAddress = lastPart.Replace(":", "");
            return ulong.Parse(hexAddress, System.Globalization.NumberStyles.HexNumber);
        }
        catch { return 0; }
    }

    private static bool GetBool(DeviceInformation device, string propertyName)
    {
        if (device.Properties.TryGetValue(propertyName, out var value) && value is bool b)
            return b;
        return false;
    }

    // Method removed as it used WinRT DeviceInformation which is no longer needed
}

public sealed class CachedDeviceStore
{
    private readonly string _filePath;

    public CachedDeviceStore()
    {
        var appData = Path.GetDirectoryName(Application.ExecutablePath);

        Directory.CreateDirectory(appData);

        _filePath = Path.Combine(appData, "devices.json");
    }

    public async Task<List<PairedDevice>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<PairedDevice>();

        await using var stream = File.OpenRead(_filePath);
        var devices = await JsonSerializer.DeserializeAsync<List<PairedDevice>>(stream);

        return devices ?? new List<PairedDevice>();
    }

    public async Task SaveAsync(List<PairedDevice> devices)
    {
        await using var stream = File.Create(_filePath);

        await JsonSerializer.SerializeAsync(stream, devices, new JsonSerializerOptions
        {
            WriteIndented = true
        });
    }
}