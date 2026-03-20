namespace Devn.TrayUsbDeviceControl;

public sealed class DeviceConnectionService
{
    private readonly BluetoothServiceStateManager _stateManager = new();

    public async Task ConnectAsync(PairedDevice device)
    {
        if (device.Address == 0)
        {
            MessageBox.Show("Endereço do dispositivo não disponível.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            _stateManager.EnableService(device.Address, BluetoothServiceGuids.AudioSink);
            _stateManager.EnableService(device.Address, BluetoothServiceGuids.Handsfree);
            _stateManager.EnableService(device.Address, BluetoothServiceGuids.A2dp);
            _stateManager.EnableService(device.Address, BluetoothServiceGuids.Headset);
            _stateManager.EnableService(device.Address, BluetoothServiceGuids.Avrcp);
            _stateManager.EnableService(device.Address, BluetoothServiceGuids.AvrcpTarget);

            await Task.Delay(500); // Wait bit more for OS stack
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao conectar: {ex.Message}");
        }
    }

    public async Task DisconnectAsync(PairedDevice device)
    {
        if (device.Address == 0)
        {
            MessageBox.Show("Endereço do dispositivo não disponível.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        try
        {
            // First try standard disconnect of known services
            _stateManager.DisableService(device.Address, BluetoothServiceGuids.AudioSink);
            _stateManager.DisableService(device.Address, BluetoothServiceGuids.Handsfree);
            _stateManager.DisableService(device.Address, BluetoothServiceGuids.A2dp);
            _stateManager.DisableService(device.Address, BluetoothServiceGuids.Headset);
            _stateManager.DisableService(device.Address, BluetoothServiceGuids.Avrcp);
            _stateManager.DisableService(device.Address, BluetoothServiceGuids.AvrcpTarget);

            // Force cleanup of ANY other installed service (The "Nuclear" option)
            var inspector = new BluetoothServiceInspector();
            var services = inspector.GetInstalledServices(device.Address);
            foreach (var service in services)
            {
                _stateManager.DisableService(device.Address, service.Id);
            }

            await Task.Delay(500);
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao desconectar: {ex.Message}");
        }
    }
}
