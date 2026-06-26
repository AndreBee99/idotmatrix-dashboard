using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace idotmatrix_gui
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        [DllImport("shell32.dll", SetLastError = true)]
        private static extern void SetCurrentProcessExplicitAppUserModelID([MarshalAs(UnmanagedType.LPWStr)] string AppID);

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Explicitly set the AppUserModelID so Windows permits toast notifications and listener authorization
                SetCurrentProcessExplicitAppUserModelID("iDotMatrix.Dashboard.App");

                // Automatically create a Start Menu shortcut with the same AUMID (required by Windows to authorize listeners)
                ShellLinkHelper.CreateStartMenuShortcut("iDotMatrix.Dashboard.App");
            }
            catch
            {
                // Fallback for older OS versions
            }

            base.OnStartup(e);
        }
    }
}

