using Devn.TrayUsbDeviceControl.Core.Models;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Devn.TrayUsbDeviceControl.Core.Services;

public sealed class CachedDeviceStore
{
    private readonly string _filePath;

    public CachedDeviceStore()
    {
        var appData = Path.GetDirectoryName(Application.ExecutablePath);
        Directory.CreateDirectory(appData!);
        _filePath = Path.Combine(appData!, "devices.json");
    }

    public async Task<List<PairedDevice>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<PairedDevice>();

        try
        {
            await using var stream = File.OpenRead(_filePath);
            var devices = await JsonSerializer.DeserializeAsync<List<PairedDevice>>(stream);
            return devices ?? new List<PairedDevice>();
        }
        catch { return new List<PairedDevice>(); }
    }

    public async Task SaveAsync(List<PairedDevice> devices)
    {
        try
        {
            await using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, devices, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch { }
    }
}
