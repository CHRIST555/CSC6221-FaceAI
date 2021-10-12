using System;
using System.Data.OleDb;
using System.Windows.Forms;

namespace FacialAI
{
    public partial class FaceAILogin : Form
    {
        public FaceAILogin()
        {
            InitializeComponent();
        }

        readonly OleDbConnection con = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=DatabaseFaceAI.mdb");
        OleDbCommand cmd = new OleDbCommand();
        readonly OleDbDataAdapter da = new OleDbDataAdapter();

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            con.Open();
            string login = "SELECT * FROM tbl_users WHERE username= '" + txtusername.Text + "' and password= '" + txtpassword.Text + "'";
            cmd = new OleDbCommand(login, con);
            OleDbDataReader dr = cmd.ExecuteReader();

            if (dr.Read() == true)
            {
                MessageBox.Show("Username and Password", "Confirm!", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("Invalid Username and Password1", "Please Try Again", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtpassword.Text = "";
                txtpassword.Focus();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            txtusername.Text = "";
            txtpassword.Text = "";
            txtusername.Focus();
        }

        private void checkbxShowPass_CheckedChanged(object sender, EventArgs e)
        {
            if (checkbxShowPass.Checked)
            {
                txtpassword.PasswordChar = '\0';

            }
            else
            {
                txtpassword.PasswordChar = '•';

            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            new frm_home().Show();
            Hide();
        }
    }
}
