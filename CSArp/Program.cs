using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSArp.View;

namespace CSArp.Service;

public static class Program
{
    private const string retryMarker = "--AlreadyTried";

    public static bool IsElevated => new WindowsPrincipal(
        WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    public static void Main(string[] args)
    {
        if (!args.Contains("--Elevate") || IsElevated)
        {
            RunApplication();
            return;
        }

        if (args.Contains(retryMarker))
        {
            Application.EnableVisualStyles();
            _ = MessageBox.Show("Requires elevation to run");
            return;
        }
        ElevateApplication(args);
    }

    private static void ElevateApplication(string[] args)
    {
        // runs with the same arguments plus flag mentioning the main action performing
        var info = new ProcessStartInfo(
            Assembly.GetEntryAssembly().Location.Replace(".dll", ".exe"),
            string.Join(" ", args.Concat([retryMarker]))) {
            Verb = "runas", // indicates to elevate privileges
            UseShellExecute = true, // actually uses the werb in shell
        };

        var process = new Process {
            StartInfo = info,
        };

        process.Start();
    }

    private static void RunApplication()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new ScannerForm());
    }
}
