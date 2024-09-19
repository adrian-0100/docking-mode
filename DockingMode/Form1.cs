using System.Diagnostics;
using System.Runtime.InteropServices;


namespace DockingMode
{
    public partial class Form1 : Form
    {
        private static class NativeMethods
        {
            [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
            public static extern uint PowerGetActiveScheme(IntPtr UserPowerKey, out IntPtr ActivePolicyGuid);

            [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
            public static extern uint PowerReadDCValueIndex(IntPtr RootPowerKey,
                [MarshalAs(UnmanagedType.LPStruct)] Guid SchemeGuid,
                [MarshalAs(UnmanagedType.LPStruct)] Guid SubGroupOfPowerSettingsGuid,
                [MarshalAs(UnmanagedType.LPStruct)] Guid PowerSettingGuid,
                out uint DcValueIndex);

            [DllImport("PowrProf.dll", CharSet = CharSet.Unicode)]
            public static extern uint PowerReadACValueIndex(IntPtr RootPowerKey,
                [MarshalAs(UnmanagedType.LPStruct)] Guid SchemeGuid,
                [MarshalAs(UnmanagedType.LPStruct)] Guid SubGroupOfPowerSettingsGuid,
                [MarshalAs(UnmanagedType.LPStruct)] Guid PowerSettingGuid,
                out uint AcValueIndex);
        }

        private static readonly Guid GUID_SYSTEM_BUTTON_SUBGROUP = new Guid("4f971e89-eebd-4455-a8de-9e59040e7347");
        private static readonly Guid GUID_LIDACTION = new Guid("5ca83367-6e45-459f-a27b-476b1d01c936");

        public Form1()
        {
            InitializeComponent();
            notifyIcon1.MouseClick += NotifyIcon1_MouseClick;
            Load += Form1_Load;
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            Hide();
            await CheckDockingModeStatus();
        }

        private async void NotifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                await ToggleDockingMode();
            }
        }

        private async Task ToggleDockingMode()
        {
            var (lidActionAC, lidActionDC) = await GetLidActionValues();
            bool isDockingModeActive = lidActionAC == 0 && lidActionDC == 0;

            await SetLidCloseAction(isDockingModeActive ? 1 : 0);
            UpdateNotifyIcon(!isDockingModeActive);
            await CheckDockingModeStatus();

        }

        private async Task CheckDockingModeStatus()
        {
            try
            {
                var (lidActionAC, lidActionDC) = await GetLidActionValues();
                bool isDockingModeActive = lidActionAC == 0 && lidActionDC == 0;

                activateDockingModeToolStripMenuItem.Checked = isDockingModeActive;
                disableDockingModeToolStripMenuItem.Checked = !isDockingModeActive;

                UpdateNotifyIcon(isDockingModeActive);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to check Docking Mode status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateNotifyIcon(bool isDockingModeActive)
        {
            notifyIcon1.Text = isDockingModeActive ? "Docking Mode: Enabled" : "Docking Mode: Disabled";

            // Use the correct resource path based on the project namespace and file names
            string iconResourceName = isDockingModeActive ? "DockingMode.enabledIcon.ico" : "DockingMode.disabledIcon.ico"; // Replace 'DockingMode' with your actual namespace if different


            // Load the icon from the embedded resources
            using (var stream = GetType().Assembly.GetManifestResourceStream(iconResourceName))
            {
                if (stream != null)
                {
                    notifyIcon1.Icon = new Icon(stream);
                }
                else
                {
                    MessageBox.Show($"Unable to find the icon resource: {iconResourceName}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        private async Task<(uint AC, uint DC)> GetLidActionValues()
        {
            return await Task.Run(() =>
            {
                NativeMethods.PowerGetActiveScheme(IntPtr.Zero, out IntPtr pActiveSchemeGuid);
                Guid activeSchemeGuid = Marshal.PtrToStructure<Guid>(pActiveSchemeGuid);
                Marshal.FreeHGlobal(pActiveSchemeGuid);

                NativeMethods.PowerReadACValueIndex(IntPtr.Zero, activeSchemeGuid, GUID_SYSTEM_BUTTON_SUBGROUP, GUID_LIDACTION, out uint lidActionAC);
                NativeMethods.PowerReadDCValueIndex(IntPtr.Zero, activeSchemeGuid, GUID_SYSTEM_BUTTON_SUBGROUP, GUID_LIDACTION, out uint lidActionDC);

                return (lidActionAC, lidActionDC);
            });
        }

        private async void activateDockingModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await SetLidCloseAction(0);
            await CheckDockingModeStatus();
        }

        private async void disableDockingModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await SetLidCloseAction(1);
            await CheckDockingModeStatus();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async Task SetLidCloseAction(int action)
        {
            try
            {
                await Task.Run(() => ApplyPowerSettings(action));
            }
            catch (Exception ex)
            {
                string mode = action == 0 ? "activate" : "deactivate";
                MessageBox.Show($"Failed to {mode} Docking Mode: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyPowerSettings(int action)
        {
            string[] commands = {
        $"/SETACVALUEINDEX SCHEME_CURRENT SUB_BUTTONS LIDACTION {action}",
        $"/SETDCVALUEINDEX SCHEME_CURRENT SUB_BUTTONS LIDACTION {action}",
        "/SETACTIVE SCHEME_CURRENT"
        };

            foreach (var command in commands)
            {
                ExecutePowercfgCommand(command);
            }
        }

        private void ExecutePowercfgCommand(string arguments)
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();
        }
    }
}