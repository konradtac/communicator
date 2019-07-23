﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.IO;

namespace Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            clientsList = new ArrayList();
            namesClients = new ArrayList();
            passwordClients = new ArrayList();
            namesLoggedClients = new ArrayList();
            isServerActive = false;
            webBrowserChat.Document.Write("<html><head><style>body,table { font-size: 10pt; font-family: Verdana; margin: 3px 3px 3px 3px; font-color: black;}</style></head><body width=\"" + (webBrowserChat.ClientSize.Width - 20).ToString() + "\">");

        }
        //POLA APLIKACJI
        private TcpListener server;
        private TcpClient client;
        private ArrayList clientsList;
        private ArrayList namesLoggedClients;
        private ArrayList namesClients;
        private ArrayList passwordClients;
        private bool isServerActive;


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
        delegate void RemoveTextCallBack(int i);
        private void RemoveText(int i)
        {
            if (listBoxUsers.InvokeRequired)
            {
                RemoveTextCallBack f = new RemoveTextCallBack(RemoveText);
                this.Invoke(f, new object[] { i });
            }
            else
            {
                listBoxUsers.Items.RemoveAt(i);
            }
        }
        //KONIEC DELEGAT
        //Metoda WypiszTekst
        private void AddText(string who, string message)
        {
            SetTextHTML("<table><tr><td style='font-size:11px;' width=\"30%\"><span style='color:#a1a7ba'>" +
                String.Format("{0:d/M/yyyy HH:mm:ss}", DateTime.Now) + 
                "</span></td><td width=\"10%\"><b style='text-align:left'>"+
                "[" + who + "]:</b></ td > ");

            SetTextHTML("<td colspan=2>" + message + "</td></tr></table>");
            SetScroll();
        }
        private String getCommand(string word)
        {
            return word.IndexOf(" ") > -1
                  ? word.Substring(0, word.IndexOf(" "))
                  : word;
        }
        private String getNameFromMessage(string message)
        {
            return message.Split(' ')[1];
        }
        private String getTextFromMessage(string message)
        {
            string[] messages = message.Split(new char[] { ' ' });
            string text = "";
            for(int i=2; i<messages.Length; ++i)
            {
                text = text +" "+ messages[i];
            }
            return text;

        }
        private string getUserByName(string userName)
        {
            for(int i=0; i<listBoxUsers.Items.Count; ++i)
            {
                if (namesLoggedClients[i].ToString() == userName)
                {
                    return listBoxUsers.Items[i].ToString();
                }
            }
            return "";
            
        }

        private void sendTo(string name, string message, string who)
        {
            for (int i = 0; i < listBoxUsers.Items.Count; ++i)
            {
                if (namesClients[i].ToString() == name)
                {
                    AddText(name, message);
                    SendUdpMessage(who + ":SAY:" + message, listBoxUsers.Items[i].ToString());
                    break;
                }
            }
        }

        private String parseListToString(ArrayList listToParse)
        {
            return String.Join(", ", listToParse.ToArray());

        }


        //funkcja rozsyłającą tekst do wszystkich podłączonych
        //klientów
        private void SendUdpMessage(string message)
        {
            string encryptedMessage = Cipher.EncryptStringAES(message, "message");
            foreach (string user in listBoxUsers.Items)
                using (UdpClient klientUDP = new UdpClient(user, 2500)) 
                {
                    byte[] bufor = Encoding.UTF8.GetBytes(encryptedMessage);
                    klientUDP.Send(bufor, bufor.Length);
                }
        }
        private void SendUdpMessage(string message, string user)
        {
            string encryptedMessage = Cipher.EncryptStringAES(message, "message");
            using (UdpClient klientUDP = new UdpClient(user, 2500))
            {
                byte[] bufor = Encoding.UTF8.GetBytes(encryptedMessage);
                klientUDP.Send(bufor, bufor.Length);
            }
        }


        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!isServerActive)
                try
                {
                    server = new TcpListener(IPAddress.Parse(comboBoxIpAddress.Text), (int)numericUpDownPort.Value);
                    backgroundWorkerMainLoop.RunWorkerAsync();
                    isServerActive = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Błąd inicjacji serwera (" + ex.Message + ")");
                }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (isServerActive)
            {
                SendUdpMessage("administrator:SAY:Serwer zostanie wyłączony");
                if (client != null) client.Close();
                server.Stop();
                listBoxServer.Items.Add("Serwer wyłączony");
                listBoxUsers.Items.Clear();
                namesClients.Clear();
                clientsList.Clear();
                namesLoggedClients.Clear();
            }

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            closeConnection(listBoxUsers.SelectedIndex);
            
        }
        private void closeConnection(int index)
        {
            using (UdpClient clientUdp = new UdpClient(listBoxUsers.Items[index].ToString(), 2500))
            {
                byte[] buff = Encoding.UTF8.GetBytes("administrator:SAY:Zostałeś odłączony");
                clientUdp.Send(buff, buff.Length);
                byte[] bufor2 = Encoding.UTF8.GetBytes("administrator:BYE:pusty");
                clientUdp.Send(bufor2, bufor2.Length);
            }

            listBoxServer.Items.Add("Klient [" + namesClients[index].ToString() + "] rozłączony");
            ((BackgroundWorker)clientsList[index]).CancelAsync();
            SendUdpMessage("administrator:SAY:Użytkownik " + namesClients[index].ToString() + " został  odłączony");
            listBoxUsers.Items.RemoveAt(index);
            clientsList.RemoveAt(index);
            namesLoggedClients.RemoveAt(index);
        }

        private void Send_Click(object sender, EventArgs e)
        {

            if (textBoxMessage.Text != String.Empty && textBoxMessage.Text.Trim() != String.Empty)
            {

                AddText("administrator", textBoxMessage.Text);
                if (isServerActive) SendUdpMessage("administrator:SAY:" + textBoxMessage.Text);

            }
        }

        private void backgroundWorkerMainLoop_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                server.Start();
                SetText(listBoxServer, "Serwer oczekuje na połączenia ...");
                while (true)
                {
                    client = server.AcceptTcpClient();
                    NetworkStream ns = client.GetStream();
                    BinaryReader read = new BinaryReader(ns);
                    string data = read.ReadString();
                    string[] cmd = data.Split(new char[] { ':' });
                    BinaryWriter write = new BinaryWriter(ns);

                    if (cmd[2] == "login" && cmd[1] == "HI")
                        {
                            if(namesClients.Count <= 0)
                            {
                                write.Write("ERROR:Użytkownik o podanej nazwie już istnieje");
                                return;
                            }
                            for (int i = 0; i < namesClients.Count; ++i)
                            {
                                if (cmd[0] == namesClients[i].ToString() && cmd[3] == passwordClients[i].ToString())
                                {
                                    write.Write("HI");
                                    BackgroundWorker clientThread = new BackgroundWorker();
                                    clientThread.WorkerSupportsCancellation = true;
                                    clientThread.DoWork += new DoWorkEventHandler(clientThread_DoWork);
                                    clientsList.Add(clientThread);
                                    namesLoggedClients.Add(cmd[0]);
                                    clientThread.RunWorkerAsync();
                                    SendUdpMessage("administrator:SAY:Użytkownik " + cmd[0] + " zalogował się");
                                    SetText(listBoxServer, "Klient podłączony");
                                }
                            }
                            write.Write("ERROR:Użytkownik o podanej nazwie już istnieje");
                        }
                        if (cmd[2] == "register" && cmd[1] == "HI")
                        {
                            if (namesClients.BinarySearch(cmd[0]) > -1)
                            {
                                write.Write("ERROR:Użytkownik o podanej nazwie już istnieje");
                            }
                            else
                            {
                                write.Write("HI");
                                BackgroundWorker clientThread = new BackgroundWorker();
                                clientThread.WorkerSupportsCancellation = true;
                                clientThread.DoWork += new DoWorkEventHandler(clientThread_DoWork);
                                namesClients.Add(cmd[0]);
                                clientsList.Add(clientThread);
                                namesLoggedClients.Add(cmd[0]);
                                passwordClients.Add(cmd[3]);
                                clientThread.RunWorkerAsync();
                                SendUdpMessage("administrator:SAY:Użytkownik " + cmd[0] + " zarejestrował się");
                                SetText(listBoxServer, "Klient podłączony");
                            }
                        }  
                        
                     
                            //MessageBox.Show("Klient nie dokonał  autoryzacji");
                            //client.Close();
                        
                }
            }
            catch
            {
                isServerActive = false;
                server.Stop();
                SetText(listBoxServer, "Połączenie przerwane");
            }
        }

        private void buttonSend_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void textBoxMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) Send_Click(this, null);
        }


        void clientThread_DoWork(object sender, DoWorkEventArgs e)
        {
            IPEndPoint IP = (IPEndPoint)client.Client.RemoteEndPoint;
            SetText(listBoxUsers, IP.Address.ToString());
            SetText(listBoxServer, "Klient [" + IP.Address.ToString() + "] uwieżytelniony");

            NetworkStream ns = client.GetStream();
            BinaryReader read = new BinaryReader(ns);
            string[] cmd = null;
            BackgroundWorker bw = (BackgroundWorker)sender;
            
            try
            {
                while ((cmd = Cipher.DecryptStringAES(read.ReadString(), "message").Split(new char[] { ':' }))[1] != "BYE" && bw.CancellationPending == false)
                {
                    string message = null;
                    if (cmd.Length > 2)
                    {
                        message = cmd[2];
                        for (int i = 3; i < cmd.Length; i++)
                            message += ":" + cmd[i];
                    }
                    switch (cmd[1])
                    {
                        case "SAY":
                            if (getCommand(message)=="#showall")
                            {
                                AddText(cmd[0], "Lista użytkowników: "+ parseListToString(namesLoggedClients));
                                SendUdpMessage(cmd[0] + ":" + cmd[1] + ":" + "Lista użytkowników: " + 
                                    parseListToString(namesLoggedClients), getUserByName(cmd[0]));
                                break;
                            }
                            if (getCommand(message) == "#sendto")
                            {
                                sendTo(getNameFromMessage(message), "{Priv}" + getTextFromMessage(message), cmd[0]);
                                sendTo(cmd[0], "{Priv}" + getTextFromMessage(message), cmd[0]);
                                AddText(cmd[0], "{Priv}" + (message));
                                break;
                            }
                            AddText(cmd[0], message);
                            SendUdpMessage(cmd[0] + ":" + cmd[1] + ":" + message);
                            break;
                    }
                    
                }

                SetText(listBoxServer, "Użytkownik [" + cmd[0] + "] opuścił serwer");
                for (int i = 0; i < listBoxUsers.Items.Count; i++)
                    if (IP.Address.ToString() == listBoxUsers.Items[i].ToString())
                    {
                        RemoveText(i);
                        clientsList.RemoveAt(i);
                        namesLoggedClients.RemoveAt(i);
                    }
                SendUdpMessage("administrator:SAY:Użytkownik " + cmd[0] + " opuścił rozmowę");

                read.Close();
                ns.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        

    }

}
