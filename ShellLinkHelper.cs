using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;

namespace idotmatrix_gui
{
    public static class ShellLinkHelper
    {
        // COM interfaces for ShellLink
        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        internal class ShellLink { }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214F9-0000-0000-C000-000000000046")]
        internal interface IShellLinkW
        {
            void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out IntPtr pfd, int fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out short pwHotkey);
            void SetHotkey(short wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
            void Resolve(IntPtr hwnd, int fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        // IPropertyStore COM interface
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99")]
        internal interface IPropertyStore
        {
            void GetCount(out uint cProps);
            void GetAt(uint iProp, out PropertyKey pkey);
            void GetValue(ref PropertyKey key, [Out] PropVariant pv);
            void SetValue(ref PropertyKey key, [In] PropVariant pv);
            void Commit();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal struct PropertyKey
        {
            public Guid fmtid;
            public uint pid;

            public PropertyKey(Guid guid, uint id)
            {
                fmtid = guid;
                pid = id;
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        internal class PropVariant : IDisposable
        {
            [FieldOffset(0)] public ushort vt;
            [FieldOffset(8)] public IntPtr ptr;

            public PropVariant(string value)
            {
                vt = 31; // VT_LPWSTR
                ptr = Marshal.StringToCoTaskMemUni(value);
            }

            ~PropVariant()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr);
                    ptr = IntPtr.Zero;
                }
                GC.SuppressFinalize(this);
            }
        }

        // System.AppUserModel.ID property key in Windows shell metadata
        private static readonly PropertyKey AppUserModelIdKey = new PropertyKey(new Guid("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3"), 5);

        public static void CreateStartMenuShortcut(string appUserModelId)
        {
            try
            {
                string startMenuPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    @"Microsoft\Windows\Start Menu\Programs"
                );
                
                string shortcutPath = Path.Combine(startMenuPath, "BeeMatrix.lnk");
                
                // Get the current running executable path
                string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName ?? "";
                if (string.IsNullOrEmpty(exePath)) return;

                // Create and configure the shortcut object
                var link = (IShellLinkW)new ShellLink();
                link.SetPath(exePath);
                link.SetWorkingDirectory(Path.GetDirectoryName(exePath) ?? "");
                link.SetDescription("BeeMatrix Controller & Scene Builder");

                // Set AppUserModelID property on the shortcut
                var store = (IPropertyStore)link;
                using (var val = new PropVariant(appUserModelId))
                {
                    var key = AppUserModelIdKey;
                    store.SetValue(ref key, val);
                    store.Commit();
                }

                // Save it using IPersistFile COM interface
                var file = (IPersistFile)link;
                file.Save(shortcutPath, true);
            }
            catch (Exception)
            {
                // Ignore failure (e.g. running in locked-down sandbox)
            }
        }
    }
}
