namespace Devn.TrayUsbDeviceControl;

using System.Management;
using System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

public static class BluetoothInspectorServiceTest
{

    public static void RunTests()
    {
        TestFindDevice();
        return;
        var svc = new BluetoothClassicService();
        var inspector = new BluetoothServiceInspector();
        var devices = svc.GetPairedDevices();

        foreach (var device in devices)
        {
            device.AddressId = Convert.ToUInt64(device.Address, 16);

            System.Diagnostics.Debug.Print($"{device.Name} - {device.Address} - Connected: {device.IsConnected}");

            //Identifica os serviços habilitados para o dispositivo
            device.Services = inspector.GetInstalledServices(device.AddressId);

            foreach (var service in device.Services)
            {
                System.Diagnostics.Debug.Print($"{service.Id} - {service.Name}");
            }


            if (device.Name== "JBL GO 2")
            {

                var audioSink = device.Services.Find(s => s.Name == "Audio Sink");

                var audioSinkId = audioSink.Id;// "0000110b-0000-1000-8000-00805f9b34fb"; // Audio Sink UUID
                var jblDeviceAddress = Convert.ToUInt64("70991C18E858", 16); // Replace with your JBL device's Bluetooth address

                TestConnectDevice(device.AddressId, audioSink.Id);
            }

            //TestConnectDevice(jblDeviceAddress, audioSinkId);


            //Test(address);
        }



    }

    public static void TestFindDevice()
    {

        var finder = new PnpDeviceFinder();
        var matches = finder.FindByName("JBL GO 2");

        foreach (var item in matches)
        {
            System.Diagnostics.Debug.Print($"Name: {item.Name}");
            System.Diagnostics.Debug.Print($"DeviceID: {item.DeviceID}");
            System.Diagnostics.Debug.Print($"PNPDeviceID: {item.PNPDeviceID}");
            System.Diagnostics.Debug.Print($"Status: {item.Status}");
            System.Diagnostics.Debug.Print(new string('-', 50));
        }

        
    }

    public static void TestConnectDevice(ulong address, Guid audioSink)
    {

        var manager = new BluetoothServiceStateManager();

        //ulong address = Convert.ToUInt64("70991C18E858", 16);
        int hrDisable = manager.DisableService(address, audioSink);
        System.Diagnostics.Debug.Print($"Disable Audio Sink => {hrDisable}");

        int hrEnable = manager.EnableService(address, audioSink);
        System.Diagnostics.Debug.Print($"Enable Audio Sink => {hrEnable}");

    }

    public static void Test()
    {
        var classic = new BluetoothClassicService();
        var manager = new BluetoothServiceStateManager();

        ulong address = Convert.ToUInt64("70991C18E858", 16);
        Guid audioSink = new Guid("0000110b-0000-1000-8000-00805f9b34fb");

        void PrintState(string title)
        {
            var current = classic.GetPairedDevices()
                .FirstOrDefault(d => string.Equals(d.Address, "70991C18E858", StringComparison.OrdinalIgnoreCase));

            if (current is null)
            {
                System.Diagnostics.Debug.Print($"{title}: dispositivo não encontrado");
                return;
            }

            System.Diagnostics.Debug.Print($"{title}: Connected={current.IsConnected}, Remembered={current.IsRemembered}, Authenticated={current.IsAuthenticated}");
        }

        PrintState("Antes");

        int hrDisable = manager.DisableService(address, audioSink);
        System.Diagnostics.Debug.Print($"Disable Audio Sink => {hrDisable}");

        Thread.Sleep(3000);
        PrintState("Depois do Disable");

        int hrEnable = manager.EnableService(address, audioSink);
        System.Diagnostics.Debug.Print($"Enable Audio Sink => {hrEnable}");

        Thread.Sleep(3000);
        PrintState("Depois do Enable");



    }
}






public sealed class PnpDeviceFinder
{
    public List<PnpDeviceInfo> FindByName(string namePart)
    {
        var result = new List<PnpDeviceInfo>();

        using var searcher = new ManagementObjectSearcher(
            "SELECT Name, DeviceID, PNPDeviceID, Status FROM Win32_PnPEntity");

        using var collection = searcher.Get();

        foreach (ManagementObject obj in collection)
        {
            string name = obj["Name"]?.ToString() ?? "";
            if (!name.Contains(namePart, StringComparison.OrdinalIgnoreCase))
                continue;

            result.Add(new PnpDeviceInfo
            {
                Name = name,
                DeviceID = obj["DeviceID"]?.ToString() ?? "",
                PNPDeviceID = obj["PNPDeviceID"]?.ToString() ?? "",
                Status = obj["Status"]?.ToString() ?? ""
            });
        }

        return result;
    }
}

public sealed class PnpDeviceInfo
{
    public string Name { get; set; } = "";
    public string DeviceID { get; set; } = "";
    public string PNPDeviceID { get; set; } = "";
    public string Status { get; set; } = "";
}