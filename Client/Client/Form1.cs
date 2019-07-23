using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.IO;
using System.Security.Cryptography;

namespace Client
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            isActive = false;
            webBrowserChat.Document.Write("<html><head><style>body,table{font-size: 10pt; font-family: Verdana; margin: 3px 3px 3px 3px; font - color: black;}</style></head><body width =\"" +(webBrowserChat.ClientSize.Width-20).ToString() + "\">");
            //LoginPanel form2 = new LoginPanel();
            isValid = false;
        }


        //POLA
        private TcpClient client;
        private string addressIPServer;
        private BinaryWriter write;
        private bool isActive;

        public bool isValid;
        private string login;
        private string password;
        public bool validUser(string userLogin, string userPassword, string serverIp, string operation)
        {
            try
            {
              
                addressIPServer = serverIp;
                string hashPassword = getHash(userPassword);
                client = new TcpClient(addressIPServer, 2500);
                NetworkStream ns = client.GetStream();
                write = new BinaryWriter(ns);
                write.Write(userLogin + ":HI:" + operation + ":" + hashPassword);
                BinaryReader read = new BinaryReader(ns);
                if (read.ReadString() == "HI")
                {
                    backgroundWorkerMainThread.RunWorkerAsync();
                    isActive = true;
                    login = userLogin;
                    password = hashPassword;
                return true;
                }
                LoginPanel loginPanel = new LoginPanel();
                loginPanel.validMessage("Błędne hasło lub login. Spróbuj ponownie");
                return false;  
            }
            catch (Exception ex)
            {
                MessageBox.Show("Nie można nawiązać połączenia " + ex.Message);
                return false;
            }
            

        }
        //DELEGATY- Bezpieczne odwoływanie się do własności kontrolek formy
        //z poziomu innego wątku
        delegate void SetTextCallBack(ListBox lista, string tekst);
        private void SetText(ListBox lista, string tekst)
        {
            if (lista.InvokeRequired)
            {
                SetTextCallBack f = new SetTextCallBack(SetText);
                this.Invoke(f, new object[] { lista, tekst });
            }
            else
            {
                lista.Items.Add(tekst);
            }
        }
        delegate void SetTextHTMLCallBack(string tekst);
        private void SetTextHTML(string tekst)
        {
            if (webBrowserChat.InvokeRequired)
            {
                SetTextHTMLCallBack f = new SetTextHTMLCallBack(SetTextHTML);
                this.Invoke(f, new object[] { tekst });
            }
            else
            {
                this.webBrowserChat.Document.Write(tekst);
            }
        }
        delegate void SetScrollCallBack();
        private void SetScroll()
        {
            if (webBrowserChat.InvokeRequired)
            {
                SetScrollCallBack f = new SetScrollCallBack(SetScroll);
                this.Invoke(f);
            }
            else
            {
                this.webBrowserChat.Document.Window.ScrollTo(1, int.MaxValue);
            }
        }
        //KONIEC DELEGAT
        //Metoda WypiszTekst
        private void AddText(string who, string message)
        {
            SetTextHTML("<table><tr><td style='font-size:11px;' width=\"30%\"><span style='color:#a1a7ba'>" + String.Format("{0:d/M/yyyy HH:mm:ss}", DateTime.Now) + "</span></td><td width=\"10%\"><b style='text-align:left'>[" + who + "]:</b></ td > ");

            SetTextHTML("<td colspan=2>" + message + "</td></tr></table>");
            SetScroll();
        }

        //funkcja hashująca
        private string getHash(string password)
        {
            var sha1 = new SHA1CryptoServiceProvider();
            var sha1data = sha1.ComputeHash(Encoding.ASCII.GetBytes(password));
            return new ASCIIEncoding().GetString(sha1data);
        }

        private void backgroundWorkerMainThread_DoWork(object sender, DoWorkEventArgs e)
        {
            UdpClient client = new UdpClient(2500);
            IPEndPoint addressIP = new IPEndPoint(IPAddress.Parse(addressIPServer), 0);
            string message = "";
            
            while (!backgroundWorkerMainThread.CancellationPending){
                Byte[] bufor = client.Receive(ref addressIP);
                string data = Encoding.UTF8.GetString(bufor);
                data = Cipher.DecryptStringAES(data, "message");
                string[] cmd = data.Split(new char[] { ':' });
                
                if (cmd[1] == "BYE")
                {
                    AddText("system", "klient odłączony");
                    client.Close();
                    return;
                }
                if (cmd.Length > 2)
                {
                    
                    message = cmd[2];
                    for (int i = 3; i < cmd.Length; i++)
                        message += ":" + cmd[i];
                }
                AddText(cmd[0], message);
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            try {
                if ( isActive && textBoxMessage.Text != String.Empty)
                {
                    string encryptedMessage = Cipher.EncryptStringAES(login + ":SAY:" + textBoxMessage.Text, "message");
                    write.Write(encryptedMessage);
                    textBoxMessage.Text = String.Empty;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Nie można nawiązać połączenia " + ex.Message);
            }
            
        }

        private void textBoxMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.buttonSend_Click(sender, null);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            try
            {
                string encryptedMessage = Cipher.EncryptStringAES(login + ":BYE:" + "pusty", "message");
                write.Write(encryptedMessage);
                write.Close();
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd");
            }
        }
            

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void textBoxMessage_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Close();
            Application.Exit();
        }
    }
    
}
