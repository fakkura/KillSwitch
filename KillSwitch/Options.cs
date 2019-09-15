using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Management;

namespace KillSwitch
{
    public partial class Options : Form
    {
        [DllImport("user32.dll")]
        static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr iconName);
        [DllImport("kernel32.dll")]
        static extern IntPtr GetModuleHandle(string moduleName);
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        enum KeyModifier
        {
            None = 0,
            Alt = 1,
            Control = 2,
            Shift = 4,
            WinKey = 8
        }

        public Options()
        {
            InitializeComponent();
            RegisterHotKey(Handle, 0, (int)KeyModifier.None, Keys.NumPad0.GetHashCode());
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg == 0x0312)
            {
                if (!string.IsNullOrEmpty(InetSelection.Text) && InetSelection.SelectedIndex >= 0)
                {
                    Interface inet = (InetSelection.SelectedItem as Interface);
                    SelectQuery wmiQuery = new SelectQuery("SELECT * FROM Win32_NetworkAdapter");
                    ManagementObjectSearcher searchProcedure = new ManagementObjectSearcher(wmiQuery);
                    foreach (ManagementObject item in searchProcedure.Get())
                    {
                        if (((string)item["DeviceID"]) == inet.Id)
                        {
                            if ((bool)item["NetEnabled"])
                            {
                                item.InvokeMethod("Disable", null);
                            }
                            else
                            {
                                item.InvokeMethod("Enable", null);
                            }
                        }
                    }
                }
            }
        }

        private void Options_Load(object sender, EventArgs e)
        {
            IntPtr hInstance = GetModuleHandle(null);
            IntPtr hIcon = LoadIcon(hInstance, new IntPtr(32512));
            if (hIcon != IntPtr.Zero)
            {
                Icon = Icon.FromHandle(hIcon);
                Tray.Icon = Icon.FromHandle(hIcon);
            }

            SelectQuery wmiQuery = new SelectQuery("SELECT * FROM Win32_NetworkAdapter");
            ManagementObjectSearcher searchProcedure = new ManagementObjectSearcher(wmiQuery);
            foreach (ManagementObject item in searchProcedure.Get())
            {
                Interface inet = new Interface();
                inet.Id = (string)item["DeviceID"];
                inet.Name = (string)item["Name"];
                InetSelection.Items.Add(inet);
            }

            if(InetSelection.Items.Count > 0)
              InetSelection.SelectedIndex = 0;
        }

        private void Options_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                Tray.Visible = true;
            }
        }

        private void Tray_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            Tray.Visible = false;
        }

        private void Options_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(Handle, 0);
        }
    }
}
