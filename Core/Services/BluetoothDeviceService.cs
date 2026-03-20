using Devn.TrayUsbDeviceControl.Core.Models;
using Devn.TrayUsbDeviceControl.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace Devn.TrayUsbDeviceControl.Core.Services;

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
}
