using System.Diagnostics;
using System.Globalization;

namespace FresenLaser
{
    public partial class Form1 : Form
    {
        private string filePath = "";

        public Form1()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-GB", false);
            InitializeComponent();
            llb_filePath.Visible = false;
            bt_start.Enabled = false;
        }

        //Actions
        private void bt_selectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new();
            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                llb_filePath.Text = filePath;
                llb_filePath.LinkClicked += Llb_filePath_LinkClicked;
                llb_filePath.Visible = true;
                bt_start.Enabled = true;
            }
        }

        private void Llb_filePath_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer.exe", filePath);
        }

        private void bt_start_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Selecteer eerst een bestand.");
                return;
            }
            ConversionSettings.TravelSpeed = (int)nud_moveSpeed.Value;
            ConversionSettings.CutSpeed = (int)nud_cutSpeed.Value;
            Converter.Convert(filePath);
        }
    }
}