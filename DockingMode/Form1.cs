using System.Diagnostics;
using System.Runtime.InteropServices;


namespace DockingMode
{
    public partial class Form1 : Form
    {
        // Import functions from PowrProf.dll
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

        // Necessary GUIDs
        private static readonly Guid GUID_SYSTEM_BUTTON_SUBGROUP = new Guid("4f971e89-eebd-4455-a8de-9e59040e7347");
        private static readonly Guid GUID_LIDACTION = new Guid("5ca83367-6e45-459f-a27b-476b1d01c936");

        public Form1()
        {
            InitializeComponent();
            CheckDockingModeStatus();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Hide();
        }

        private async Task CheckDockingModeStatus()
        {
            try
            {
                var (lidActionAC, lidActionDC) = await GetLidActionValues();
                bool isDockingModeActive = (lidActionAC == 0 && lidActionDC == 0);

                activateDockingModeToolStripMenuItem.Checked = isDockingModeActive;
                disableDockingModeToolStripMenuItem.Checked = !isDockingModeActive;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to check Docking Mode status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            MessageBox.Show("Docking Mode Activated", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void disableDockingModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await SetLidCloseAction(1);
            await CheckDockingModeStatus();
            MessageBox.Show("Docking Mode Deactivated", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private async Task SetLidCloseAction(int action)
        {
            try
            {
                await Task.Run(() =>
                {
                    ExecutePowercfgCommand($"/SETACVALUEINDEX SCHEME_CURRENT SUB_BUTTONS LIDACTION {action}");
                    ExecutePowercfgCommand($"/SETDCVALUEINDEX SCHEME_CURRENT SUB_BUTTONS LIDACTION {action}");
                    ExecutePowercfgCommand("/SETACTIVE SCHEME_CURRENT");
                });
            }
            catch (Exception ex)
            {
                string mode = action == 0 ? "activate" : "deactivate";
                MessageBox.Show($"Failed to {mode} Docking Mode: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExecutePowercfgCommand(string arguments)
        {
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                process.Start();
                process.WaitForExit();
            }
        }
    }
}