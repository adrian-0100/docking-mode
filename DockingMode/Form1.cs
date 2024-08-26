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
            //logic to activate docking mode
        }


        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void DisableLidCloseSleep()
        {
            try
            {
                // Cambiar la acción al cerrar la tapa cuando está en corriente alterna
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = "/SETACVALUEINDEX SCHEME_CURRENT SUB_BUTTONS LIDACTION 0",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                // Cambiar la acción al cerrar la tapa cuando está en batería
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powercfg.exe",
                    Arguments = "/SETDCVALUEINDEX SCHEME_CURRENT SUB_BUTTONS LIDACTION 0",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                // Aplicar la configuración
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

    }
}
