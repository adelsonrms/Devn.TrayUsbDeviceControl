using Devn.TrayUsbDeviceControl.Core.Models;
using Devn.TrayUsbDeviceControl.Core.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Devn.TrayUsbDeviceControl.UI;

public sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _menu;
    private readonly BluetoothDeviceService _deviceService;
    private readonly DeviceConnectionService _connectionService;
    private readonly CachedDeviceStore _cacheStore;

    private PairedDevice? _primaryDevice;
    private bool _isBusy;
    private readonly Timer _spinnerTimer;
    private int _spinnerAngle = 0;
    private readonly CancellationTokenSource _monitorCts = new();

    public TrayApplicationContext()
    {
        _deviceService = new BluetoothDeviceService();
        _connectionService = new DeviceConnectionService();
        _cacheStore = new CachedDeviceStore();

        _menu = new ContextMenuStrip();
        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            Text = "JBL Tray Connector",
            ContextMenuStrip = _menu
        };

        _spinnerTimer = new Timer { Interval = 100 };
        _spinnerTimer.Tick += (_, _) => { _spinnerAngle = (_spinnerAngle + 45) % 360; UpdateTrayIcon(); };

        _notifyIcon.MouseUp += async (_, e) =>
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_isBusy) return;
                if (_primaryDevice != null)
                    await ToggleDeviceDirectAsync(_primaryDevice);
                else
                    await ShowMenuManualAsync();
            }
            else if (e.Button == MouseButtons.Right)
            {
                await RebuildMenuAsync();
            }
        };

        UpdateTrayIcon();
        _ = InitializeAsync();
        _ = BackgroundStatusMonitorAsync(_monitorCts.Token);
    }

    private async Task ShowMenuManualAsync()
    {
        await RebuildMenuAsync();
        typeof(NotifyIcon)
            .GetMethod("ShowContextMenu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?.Invoke(_notifyIcon, null);
    }

    private async Task InitializeAsync()
    {
        _menu.Items.Clear();
        _menu.Items.Add("Carregando...");

        var cachedDevices = await _cacheStore.LoadAsync();
        if (cachedDevices.Count > 0)
        {
            _primaryDevice = cachedDevices.FirstOrDefault(d => d.Name.Contains("JBL", StringComparison.OrdinalIgnoreCase)) 
                            ?? cachedDevices.FirstOrDefault();
            await RebuildMenuFromListAsync(cachedDevices);
        }

        try
        {
            var freshDevices = await _deviceService.GetPairedAudioDevicesAsync();

            freshDevices = freshDevices
                .GroupBy(d => d.Address)
                .Select(g => g.OrderByDescending(d => d.IsConnected || d.IsConnectedByOS).First())
                .OrderBy(d => d.Name)
                .ToList();

            _primaryDevice = freshDevices.FirstOrDefault(d => d.Name.Contains("JBL", StringComparison.OrdinalIgnoreCase)) 
                            ?? freshDevices.FirstOrDefault();

            await _cacheStore.SaveAsync(freshDevices);
            await RebuildMenuFromListAsync(freshDevices);
        }
        catch { }
        finally { UpdateTrayIcon(); }
    }

    private Task RebuildMenuFromListAsync(List<PairedDevice> devices)
    {
        _menu.Items.Clear();

        if (devices.Count == 0)
        {
            _menu.Items.Add("Nenhum dispositivo encontrado");
        }
        else
        {
            foreach (var device in devices.OrderBy(d => d.Name))
            {
                bool isGhost = device.IsConnectedByOS && !device.IsConnected;
                string label = device.Name + (isGhost ? " (⚠️ Status preso)" : "");

                var item = new ToolStripMenuItem(label)
                {
                    Tag = device,
                    ForeColor = isGhost ? Color.DarkOrange : SystemColors.ControlText,
                    Image = CreateStatusIcon(device.IsConnected, isGhost)
                };

                item.Click += async (_, _) => await ToggleDeviceAsync(item);
                _menu.Items.Add(item);

                if (isGhost)
                {
                    var cleanupItem = new ToolStripMenuItem("  └─ Forçar Desconexão (Limpar status)")
                    {
                        Tag = device,
                        ForeColor = Color.Red,
                        Image = SystemIcons.Shield.ToBitmap()
                    };
                    cleanupItem.Click += async (_, _) => await ForceCleanupAsync(cleanupItem, device);
                    _menu.Items.Add(cleanupItem);
                }
            }
        }

        _menu.Items.Add(new ToolStripSeparator());
        _menu.Items.Add(new ToolStripMenuItem("Atualizar", null, async (_, _) => await InitializeAsync()));
        _menu.Items.Add(new ToolStripMenuItem("Sair", null, (_, _) => ExitThread()));

        return Task.CompletedTask;
    }

    private async Task BackgroundStatusMonitorAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(5000, ct);
                if (_isBusy) continue;

                var freshDevices = await _deviceService.GetPairedAudioDevicesAsync();
                var newPrimary = freshDevices
                    .GroupBy(d => d.Address)
                    .Select(g => g.OrderByDescending(d => d.IsConnected || d.IsConnectedByOS).First())
                    .FirstOrDefault(d => d.Name.Contains("JBL", StringComparison.OrdinalIgnoreCase)) 
                    ?? freshDevices.FirstOrDefault();

                if (newPrimary != null && (newPrimary.IsConnected != _primaryDevice?.IsConnected))
                {
                    _primaryDevice = newPrimary;
                    UpdateTrayIcon();
                }
            }
            catch (TaskCanceledException) { break; }
            catch { }
        }
    }

    private void UpdateTrayIcon()
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            bool isConnected = _primaryDevice?.IsConnected ?? false;
            Color statusColor = isConnected ? Color.LimeGreen : Color.LightSlateGray;
            
            using var brush = new SolidBrush(statusColor);
            g.FillEllipse(brush, 4, 4, 24, 24);
            using var pen = new Pen(Color.FromArgb(100, 0, 0, 0), 2);
            g.DrawEllipse(pen, 4, 4, 24, 24);

            if (_isBusy)
            {
                using var spinnerPen = new Pen(Color.White, 4);
                g.DrawArc(spinnerPen, 4, 4, 24, 24, _spinnerAngle, 90);
            }
        }

        var oldIcon = _notifyIcon.Icon;
        _notifyIcon.Icon = Icon.FromHandle(bmp.GetHicon());
        oldIcon?.Dispose();
    }

    private async Task ToggleDeviceDirectAsync(PairedDevice device)
    {
        if (_isBusy || device == null) return;
        _isBusy = true;
        _spinnerTimer.Start();
        
        bool wasConnected = device.IsConnected;
        _primaryDevice = device with { IsConnected = !wasConnected };
        UpdateTrayIcon();

        try
        {
            if (wasConnected) await _connectionService.DisconnectAsync(device);
            else await _connectionService.ConnectAsync(device);
        }
        catch (Exception ex)
        {
            _primaryDevice = device with { IsConnected = wasConnected };
            _notifyIcon.ShowBalloonTip(3000, "Erro Bluetooth", ex.Message, ToolTipIcon.Warning);
        }
        finally
        {
            _isBusy = false;
            _spinnerTimer.Stop();
            UpdateTrayIcon();
            _ = Task.Delay(2000).ContinueWith(_ => InitializeAsync());
        }
    }

    private async Task ToggleDeviceAsync(ToolStripMenuItem item)
    {
        if (item.Tag is not PairedDevice device) return;
        await ToggleDeviceDirectAsync(device);
    }

    private Bitmap CreateStatusIcon(bool connected, bool ghost)
    {
        var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        Color color = ghost ? Color.Orange : (connected ? Color.LimeGreen : Color.Gray);
        using var brush = new SolidBrush(color);
        g.FillEllipse(brush, 4, 4, 8, 8);
        using var pen = new Pen(Color.FromArgb(50, 0, 0, 0));
        g.DrawEllipse(pen, 4, 4, 8, 8);
        return bmp;
    }

    private async Task RebuildMenuAsync() => await InitializeAsync();

    private async Task ForceCleanupAsync(ToolStripMenuItem item, PairedDevice device)
    {
        item.Enabled = false;
        item.Text = "  └─ Limpando status...";
        try { await _connectionService.DisconnectAsync(device); }
        catch { }
        finally { await RebuildMenuAsync(); }
    }

    protected override void ExitThreadCore()
    {
        _monitorCts.Cancel();
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
        _menu.Dispose();
        base.ExitThreadCore();
    }
}
