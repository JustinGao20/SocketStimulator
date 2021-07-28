using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SocketProject
{
    public partial class ServerSide : Form
    {
        public ServerSide()
        {
            InitializeComponent();
        }

        //创建sockets对象
        Socket socketCom;                   //创建用于通信的socket对象
        Socket socketListener;              //创建用于监听的socket对象

        String ip;                          //远程主机的ip地址和端口号

        //将远程连接的客户端的ip地址和socket存入到集合中
        Dictionary<String, Socket> arrSocket = new Dictionary<string, Socket>();

        //定义回调
        private delegate void setTextValueCallBack(string callback); //定义显示消息提醒的回调
        private delegate void receiveMessageCallBack(string receive); //定义接受客户端发送消息的回调

        //申明回调
        private setTextValueCallBack setTextValue;  
        private receiveMessageCallBack receiveMessageValue;
         
        //创建线程
        Thread acceptSocket;               //创建监听连接的线程
        Thread receiveSocket;              //创建接受客户端发送消息的线程

        //按下开始开始监听的按钮
        private void button1_Click(object sender, EventArgs e)
        {
            //当用户点击开始监听的的时候，在客户端实例化一个socket对象来监听ip地址和端口号,基于tcp协议
            socketListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //获取用户端的ip地址
            IPAddress IP = IPAddress.Parse(this.textBox1.Text.Trim());

            //实例化连接端口,准备和ip地址绑定
            IPEndPoint endpoint = new IPEndPoint(IP, Convert.ToInt32(this.textBox2.Text.Trim()));

            //绑定ip地址和端口号
            socketListener.Bind(endpoint);

            //发送消息
            this.richTextBox1.AppendText("系统监听成功!"+ "\n");

            //开始监听，设定最大的连接请求
            socketListener.Listen(20);

            //将回调实例化
            setTextValue = new setTextValueCallBack(setText);
            receiveMessageValue = new receiveMessageCallBack(receiveText);

            //创建开始监听的线程
            acceptSocket = new Thread(new ParameterizedThreadStart(StartListen));
        
            //后台线程，随着主线程停止而停止
            acceptSocket.IsBackground = true;

            //改变系统的线程状态
            acceptSocket.Start(socketListener);
        }

        //等待客户端的连接，实例化和其通讯的socket
        private void StartListen(object obj)
        {
            //把object转换成Socket类，并且赋值到新的socket实例上
            socketListener = obj as Socket;
            //死循环，一直等待客户端的连接
            while (true)
            {
                //等待客户端的连接，创建用于通信的socket
                socketCom = socketListener.Accept();

                //获取远程主机的ip地址和端口号
                ip = socketCom.RemoteEndPoint.ToString();

                //把ip地址和通信socket加入到dictionary里面
                arrSocket.Add(ip, socketCom);

                String message = "远程主机" + socketCom.RemoteEndPoint + "已经连接成功！";

                //使用回调
                richTextBox1.Invoke(setTextValue, message);

                //定义接受客户端消息的线程
                receiveSocket = new Thread(new ParameterizedThreadStart(Receive));

                //后台线程，随着主线程停止而停止
                receiveSocket.IsBackground = true;

                //改变系统的线程状态
                receiveSocket.Start(socketCom);
            }
        }

        //当按下发送消息按钮的时候，服务端给客户端发送消息
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                //把用户输入的消息convert成string
                String message = this.richTextBox2.Text.Trim();
                byte[] buffer = Encoding.Default.GetBytes(message);

                List<byte> list = new List<byte>();
                list.Add(0);
                list.AddRange(buffer);

                byte[] newBuffer = list.ToArray();
                arrSocket[ip].Send(newBuffer);

            }
            catch (Exception err)
            {
                MessageBox.Show("连接客户端的时候发生错误" + err.Message);
            }
        }

        //服务端不停接受客户端发送的消息
        private void Receive(object obj)
        {
            //把object转换成Socket类，并且赋值到新的socket实例上
            socketCom = obj as Socket;
            //死循环，一直等待客户端的连接
            while (true)
            {
                //客户端连接后，定义服务器能接受客户发送消息的最大字节数量
                byte[] max = new byte[4096];

                //实际接收到的有效字节数量
                int status = socketCom.Receive(max);

                //客户端关闭，要退出循环
                if(status == 0)
                {
                    break;
                }
                else
                {
                    string message = Encoding.Default.GetString(max, 0, status);
                    //使用回调，发送消息
                    string display = "接收" + socketCom.RemoteEndPoint + "发送的消息：" + message;
                    richTextBox1.Invoke(receiveMessageValue,display);
                }
            }
        }

        //按下停止监听按钮，关闭通讯和线程
        private void button2_Click(object sender, EventArgs e)
        {
            socketListener.Close();
            socketCom.Close();
            acceptSocket.Abort();
            receiveSocket.Abort();
        }

        //回调委托需要执行的方法
        private void setText(string textValue)
        {
            this.richTextBox1.AppendText(textValue + "\n");
        }

        private void receiveText(string textValue)
        {
            this.richTextBox1.AppendText(textValue + "\n");
        }

        private void App_Load(object sender, EventArgs e)
        {

        }
    }
}
