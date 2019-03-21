using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClientEx1
{
    public partial class Form1 : Form
    {

        TcpClient clientSocket = new TcpClient();
        NetworkStream stream = default(NetworkStream);
        string message = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                clientSocket.Connect("192.168.219.102", 9999);
                stream = clientSocket.GetStream();
            }
            catch (Exception ex)
            {
                MessageBox.Show("서버가 실행중이 아닙니다.");
                Application.Exit();
            }

            message = "채팅서버에 연결 되었습니다.";
            DisplayTextOnClient(message);

            byte[] buffer = Encoding.Unicode.GetBytes("접속자$");
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();

            Thread t_handler = new Thread(GetMessage);
            t_handler.IsBackground = true;
            t_handler.Start();
        }

        private void GetMessage()
        {
            while (true)
            {
                stream = clientSocket.GetStream();
                int BUFFSIZE = clientSocket.ReceiveBufferSize;
                byte[] buffer = new byte[BUFFSIZE];
                int bytes = stream.Read(buffer, 0, buffer.Length); //버퍼에 읽어옴

                string message = Encoding.Unicode.GetString(buffer, 0, bytes);
                DisplayTextOnClient(message);
            }
        }

        private void DisplayTextOnClient(string text)
        {
            if (textBox1.InvokeRequired)
            {
                textBox1.BeginInvoke(new MethodInvoker(delegate { textBox1.AppendText(text + Environment.NewLine); }));
            }
            else
            {
                textBox1.AppendText(text + Environment.NewLine);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox2.Focus();
            byte[] buffer = Encoding.Unicode.GetBytes(textBox2.Text + "$");
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
            textBox2.Text = "";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            byte[] buffer = Encoding.Unicode.GetBytes("leaveChat$");
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
            Application.ExitThread();
            Environment.Exit(0);
        }
    }
}
