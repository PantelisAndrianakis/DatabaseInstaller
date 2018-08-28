using System;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

/**
 * @author Pantelis Andrianakis
 */
namespace DatabaseInstaller
{
    public partial class frmMain : Form
    {
        private MySqlConnection con;
        private MySqlCommand cmd;
        private string[] filePaths;
        private string SQL_PATH = "sql";
        private string MSG_WARNING = "Warning";
        private string MSG_SUCCESS = "Success";

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            // Disable install button.
            btnInstall.Enabled = false;

            // Check if sql directory exists.
            if (!Directory.Exists(SQL_PATH))
            {
                MessageBox.Show("Could not access sql directory.", MSG_WARNING);
                btnInstall.Enabled = true;
                return;
            }

            // Create database if it does not exist.
            con = new MySqlConnection("server=" + txtAddress.Text + ";port=" + txtPort.Text + ";user=" + txtUser.Text + ";password=" + txtPassword.Text + ";SslMode=none;");
            try
            {
                con.Open();
                cmd = new MySqlCommand("CREATE DATABASE IF NOT EXISTS `" + txtDbName.Text + "`;", con);
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MSG_WARNING);
                btnInstall.Enabled = true;
                return;
            }

            // Connect to database.
            con = new MySqlConnection("server=" + txtAddress.Text + ";port=" + txtPort.Text + ";database=" + txtDbName.Text + ";user=" + txtUser.Text + ";password=" + txtPassword.Text + ";SslMode=none;");
            try
            {
                con.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MSG_WARNING);
                btnInstall.Enabled = true;
                return;
            }

            // Process sql files.
            filePaths = Directory.GetFiles(SQL_PATH, "*.sql", SearchOption.AllDirectories);
            progressBar.Maximum = filePaths.Length;
            progressBar.Value = 0;
            progressBar.Step = 1;
            foreach (string fileName in filePaths)
            {
                try
                {
                    cmd = new MySqlCommand(File.ReadAllText(fileName), con);
                    cmd.ExecuteNonQuery();
                    progressBar.PerformStep();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, MSG_WARNING);
                    btnInstall.Enabled = true;
                    if (con != null)
                    {
                        con.Close();
                    }
                    return;
                }
            }

            // Close the connection.
            try
            {
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, MSG_WARNING);
            }

            // Re-enable install button.
            btnInstall.Enabled = true;

            // Complete message.
            MessageBox.Show("Installation is complete!", MSG_SUCCESS);
        }
    }
}
