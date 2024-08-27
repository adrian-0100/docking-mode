using System.Diagnostics;

namespace DockingMode
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void activateDockingModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisableLidCloseSleep();
            MessageBox.Show("Docking Mode Activated");
        }
        private void disableDockingModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EnableLidCloseSleep();
            MessageBox.Show("Docking Mode Deactivated");
        }


        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void DisableLidCloseSleep()
        {
            try
            {
                // Change the action when closing the lid: when plugged into AC
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = "/SETACVALUEINDEX SCHEME_CURRENT SUB_BUTTONS LIDACTION 0",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                // Change the action when closing the lid: when on battery
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = "/SETDCVALUEINDEX SCHEME_CURRENT SUB_BUTTONS LIDACTION 0",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                // Apply the changes
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = "/SETACTIVE SCHEME_CURRENT",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to activate Docking Mode: " + ex.Message);
            }
        }

        private void EnableLidCloseSleep()
        {
            try
            {
                // Change the action when closing the lid: when plugged into AC
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = "/SETACVALUEINDEX SCHEME_CURRENT SUB_BUTTONS LIDACTION 1",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                // Change the action when closing the lid: when on battery
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = "/SETDCVALUEINDEX SCHEME_CURRENT SUB_BUTTONS LIDACTION 1",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                // Apply the changes
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = "/SETACTIVE SCHEME_CURRENT",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to deactivate Docking Mode: " + ex.Message);
            }
        }
    }
}
