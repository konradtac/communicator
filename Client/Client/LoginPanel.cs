using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class LoginPanel : Form
    {
        public LoginPanel()
        {
            InitializeComponent();
            

        }
        Form1 form1 = new Form1();

        public void validMessage(string message)
        {
            MessageBox.Show(message);
        }

        private void LoginPanel_Load(object sender, EventArgs e)
        {

        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        
        private void buttonRegister_Click(object sender, EventArgs e)
        {
            
        }

        private void buttonRegister_Click_1(object sender, EventArgs e)
        {
            if (textBoxRegisterPassword.Text != textBoxRegisterPasswordRepeat.Text)
            {
                MessageBox.Show("hasło nie zgadza się z powtórzonym hasłem");
                return;
            }
            if (form1.validUser(textBoxRegisterLogin.Text, textBoxRegisterPassword.Text, textBoxIp.Text, "register"))
            {
                this.Hide();
                form1.Show();
            }
        }

        private void buttonLogin_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (textBoxLogin.Text != String.Empty && textBoxIp.Text != String.Empty && textBoxPassword.Text != String.Empty)
                {
                    if (form1.validUser(textBoxLogin.Text, textBoxPassword.Text, textBoxIp.Text, "login"))
                    {
                        this.Hide();
                        form1.Show();
                    }
                }
                else
                {
                    MessageBox.Show("Uzupełnij pola login oraz hasło");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("nie można się zalogować " + ex.Message);
            }
        }
    }
}
