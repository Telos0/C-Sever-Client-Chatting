using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ServerEx1
{
    public partial class Form1 : Form
    {
        TcpListener sever = null;
        TcpClient clientSocket = null;
        static int counter = 0;
        string date;
        NetworkStream stream = null;

        public Dictionary<TcpClient, string> clientList = new Dictionary<TcpClient, string>();


        public Form1()
        {
            InitializeComponent();
            Thread t = new Thread(InitSocket);
            t.IsBackground = true;
            t.Start();
        }

        private void InitSocket()
        {
            sever = new TcpListener(IPAddress.Any, 9999);
            clientSocket = default(TcpClient);
            sever.Start();
            DisplayText(">> Server Started");

            while (true)
            {
                try
                {
                    counter++;
                    clientSocket = sever.AcceptTcpClient();
                    DisplayText(">> Accept connection from client");

                    NetworkStream stream = clientSocket.GetStream();
                    byte[] buffer = new byte[1024];
                    int bytes = stream.Read(buffer, 0, buffer.Length); //buffer에 읽은 내용을 넣고 넣은 크기를 bytes로 리턴
                    string user_name = Encoding.Unicode.GetString(buffer, 0, bytes);
                    user_name = user_name.Substring(0, user_name.IndexOf("$"));

                    clientList.Add(clientSocket, user_name);

                    SendMessageAll(user_name + "님이 입장하셨습니다.", "", false); // 클라이언트에게 전달

                    Thread listen = new Thread(OnReceived);
                    listen.Start();



                }
                catch (SocketException se)
                {
                    break;
                }
                catch (Exception ex)
                {
                    break;
                }
            }
            clientSocket.Close();
            sever.Stop();
        }
        /// <summary>
        /// /////////
        /// </summary>
        /// <param name="message"></param>
        /// <param name="user_name"></param>
        private void OnReceived() // cleint로 부터 받은 데이터
        {
            while (true)
            {
                stream = clientSocket.GetStream();
                int BUFFERSIZE = clientSocket.ReceiveBufferSize;
                byte[] buffer = new byte[BUFFERSIZE];
                int bytes = stream.Read(buffer, 0, buffer.Length);

                string message = Encoding.Unicode.GetString(buffer, 0, bytes);

                string displayMessage = "From client : " + " : " + message;
                DisplayText(displayMessage); // Server단에 출력
                SendMessageAll(message, "됩니까" , true); // 모든 Client에게 전송

            }

            
        }

        //

        public void SendMessageAll(string message, string user_name, bool flag)
        {
            foreach (var pair in clientList)
            {
                date = DateTime.Now.ToString("yyyy.MM.dd.HH:mm:ss");

                TcpClient client = pair.Key as TcpClient;
                NetworkStream stream = client.GetStream();
                byte[] buffer = null;

                if (flag)
                {
                    if (message.Equals("leaveChat"))
                    {
                        buffer = Encoding.Unicode.GetBytes(user_name + "님이 나갔습니다.");
                    }
                    else
                    {
                        buffer = Encoding.Unicode.GetBytes("[" + date + "]" + user_name + ":" + message);
                    }
                }
                else
                {
                    buffer = Encoding.Unicode.GetBytes(message);
                }

                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
            }

        }

        private void DisplayText(string text)
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
    }
}
