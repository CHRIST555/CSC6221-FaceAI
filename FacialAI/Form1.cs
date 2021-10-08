using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Data.OleDb;
using FacialAI.Azure;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;

namespace FacialAI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            FaceModels model = new FaceModels();
            _ = model.FindSimilar();

            imageControl.SizeMode = PictureBoxSizeMode.StretchImage;

        }

        FilterInfoCollection filterInfoCollection;
        VideoCaptureDevice captureDevice;

        OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=DatabaseFaceAI.mdb");
        OleDbCommand cmd = new OleDbCommand();
        OleDbDataAdapter da = new OleDbDataAdapter();

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkbxShowPass.Checked)
            {
                txtPassword.PasswordChar = '\0';
                txtComPassword.PasswordChar = '\0';
            }
            else
            {
                txtPassword.PasswordChar = '•';
                txtComPassword.PasswordChar = '•';
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (txtUsername.Text == "" && txtPassword.Text == "" && txtComPassword.Text == "")
            {
                MessageBox.Show("Username and Password fields are empty", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (txtPassword.Text == txtComPassword.Text)
            {
                con.Open();
                string register = "INSERT INTO tbl_users VALUES ('" + txtUsername.Text + "','" + txtPassword.Text + "')";
                cmd = new OleDbCommand(register, con);
                cmd.ExecuteNonQuery();
                con.Close();
                txtUsername.Text = "";
                txtPassword.Text = "";
                txtComPassword.Text = "";

                MessageBox.Show("Your Account has been Successfully Created", "Registration Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
            else
            {
                MessageBox.Show("Passwords does not match, Please Re-enter", "Registration Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.Text = "";
                txtComPassword.Text = "";
                txtPassword.Focus();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtUsername.Text = "";
            txtPassword.Text = "";
            txtComPassword.Text = "";
            txtUsername.Focus();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new FaceAILogin().Show();
            this.Hide();
        }

        private void btnTakePicture_Click(object sender, EventArgs e)
        {

            captureDevice.Stop();
            captureDevice = new VideoCaptureDevice(filterInfoCollection[cboCameras.SelectedIndex].MonikerString);
            captureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            captureDevice.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            filterInfoCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach(FilterInfo filter in filterInfoCollection)
            {
                cboCameras.Items.Add(filter.Name);
            }

            cboCameras.SelectedIndex = 0;
            captureDevice = new VideoCaptureDevice();
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            imageControl.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (captureDevice.IsRunning==true)
            {
                captureDevice.Stop();
            }
        }

        private void cboCameras_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (captureDevice != null) {
                if (captureDevice.IsRunning == true)
                    captureDevice.Stop();
                captureDevice = new VideoCaptureDevice(filterInfoCollection[cboCameras.SelectedIndex].MonikerString);
                captureDevice.NewFrame += VideoCaptureDevice_NewFrame;
                captureDevice.Start();
            }
        }
    }
}
