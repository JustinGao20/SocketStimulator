using System;
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
using System.Threading;
using System.Threading.Tasks;


namespace SocketProjectClient
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        //创建sockets对象
        Socket socketCom;                   //创建用于通信的socket对象
        Socket socketListener;              //创建用于监听的socket对象
        Thread receiveSocket;               //创建接受服务端发送消息的线程

        //定义回调
        private delegate void setTextValueCallBack(string callback); //定义显示消息提醒的回调
        private delegate void receiveMessageCallBack(string receive); //定义接受客户端发送消息的回调

        //申明回调
        private setTextValueCallBack setTextValue;
        private receiveMessageCallBack receiveMessageValue;

        //当用户点击连接的按钮时
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //当用户点击开始连接的的时候，在客户端实例化一个socket对象来发送ip地址和端口号,基于tcp协议
                socketCom = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //输入ip地址
                IPAddress IP = IPAddress.Parse(this.textBox1.Text.Trim());

                //绑定端口号准备把信息发送出去
                socketCom.Connect(IP, Convert.ToInt32(this.textBox2.Text.Trim()));

                //将回调实例化
                setTextValue = new setTextValueCallBack(setValue);
                receiveMessageValue = new receiveMessageCallBack(setValue);
                this.richTextBox1.Invoke(setTextValue, "系统连接成功!");

                //创建一个新的线程不停的接受服务端发送消息的线程
                receiveSocket = new Thread(new ThreadStart(Receive));

                //后台线程，随着主线程停止而停止
                receiveSocket.IsBackground = true;

                //改变系统的线程状态
                receiveSocket.Start();
            }
            catch (Exception err)
            {
                MessageBox.Show("连接服务端的时候发生错误" + err.Message);
            }
        }

        //接受服务器发送的消息
        private void Receive()
        {
            try
            {
                while (true)
                {
                    //服务端连接后，定义服务器能接受客户发送消息的最大字节数量
                    byte[] max = new byte[4096];

                    //实际接收到的有效字节数量
                    int status = socketCom.Receive(max);

                    if (status == 0)
                    {
                        break;
                    }
                    else
                    {
                        if(max[0] == 0)
                        {
                            string message = Encoding.Default.GetString(max, 1, status - 1);
                            //使用回调，发送消息
                            string display = "接收" + socketCom.RemoteEndPoint + "发送的消息：" + message;
                            richTextBox1.Invoke(receiveMessageValue, display);
                        }
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show("接收服务端消息的时候发生错误" + err.Message);
            }
        }

        //当按下发送消息按钮的时候，客户端给服务端发送消息
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                //把用户输入的消息convert成string
                String message = this.richTextBox2.Text.Trim();

                //定义服务器能接受客户发送消息的最大字节数量
                byte[] max = new byte[4096];

                max = Encoding.Default.GetBytes(message);

                int receive = socketCom.Send(max);
            }
            catch (Exception err)
            {
                MessageBox.Show("发生消息出错：" + err.Message);
            }
        }

        //


        //当按下终止连接的按钮的时候，关闭通讯函数和线程
        private void button3_Click(object sender, EventArgs e)
        {
            socketCom.Close();
            receiveSocket.Abort();
        }


        //准备在消息栏把消息打印出来
        private void setValue(string textValue)
        {
            this.richTextBox1.AppendText(textValue + "\n");
        }
    }
}
