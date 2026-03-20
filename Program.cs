using Devn.TrayUsbDeviceControl.UI;
using System;
using System.Windows.Forms;

namespace Devn.TrayUsbDeviceControl;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new TrayApplicationContext());
    }
}
