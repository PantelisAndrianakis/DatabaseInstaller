﻿using System;
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
            if (!Directory.Exists("sql"))
            {
                MessageBox.Show("Could not access sql directory.", MSG_WARNING);
                btnInstall.Enabled = true;
                return;
            }

            // Create database if it does not exist.
            string createCommand = "CREATE DATABASE IF NOT EXISTS `" + txtDbName.Text + "`;";
            MySqlConnection con = new MySqlConnection("server=" + txtAddress.Text + ";port=" + txtPort.Text + ";user=" + txtUser.Text + ";password=" + txtPassword.Text + ";SslMode=none;");
            MySqlCommand cmd;
            try
            {
                con.Open();
                cmd = new MySqlCommand(createCommand, con);
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
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, MSG_WARNING);
                btnInstall.Enabled = true;
                return;
            }

            // Process sql files.
            string[] filePaths = Directory.GetFiles("sql", "*.sql", SearchOption.AllDirectories);
            progressBar.Maximum = filePaths.Length;
            progressBar.Value = 0;
            progressBar.Step = 1;
            foreach (String fileName in filePaths)
            {
                try
                {
                    cmd = new MySqlCommand(File.ReadAllText(fileName), con);
                    cmd.ExecuteNonQuery();
                    progressBar.PerformStep();
                }
                catch (MySqlException ex)
                {
                    con.Close();
                    MessageBox.Show(ex.Message, MSG_WARNING);
                    btnInstall.Enabled = true;
                    return;
                }
            }

            // Close the connection.
            try
            {
                con.Close();
            }
            catch (MySqlException ex)
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