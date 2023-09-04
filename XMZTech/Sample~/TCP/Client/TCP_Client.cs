using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO;

using System.Threading;
using UnityEngine;
using UnityEngine.UI;
namespace ThursayTool.TCP
{
    public class TCP_Client : MonoBehaviour
    {
        private string staInfo = "NULL";             //״̬��Ϣ
        private string inputIp = "127.0.0.1";   //����ip��ַ
        private string inputPort = "8080";           //����˿ں�
        public string inputMes = "NULL";             //���͵���Ϣ
        private int recTimes = 0;                    //���յ���Ϣ�Ĵ���
        private string recMes = "NULL";              //���յ�����Ϣ
        private Socket socketSend;                   //�ͻ����׽��֣���������Զ�˷�����
        private bool clickSend = false;              //�Ƿ������Ͱ�ť

        public Action<string> OnBuild;
        public Action<string> OnFeild;
        public Action<string> OnReceived;


        public void InitClient(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress) || ipAddress.Contains(":") == false)
            {
                Debug.Log("��������ĵ�ַ");
                return;
            }
            var splits = ipAddress.Split(':');
            inputIp = splits[0];
            inputPort = splits[1];
            ClickConnect();
        }


        public void Send(string obj)
        {
            inputMes = obj;
            clickSend = true;
        }
        private string ConnectSocket(IPEndPoint ipep, Socket sock)
        {
            string exmessage = "";
            try
            {
                sock.Connect(ipep);
            }
            catch (System.Exception ex)
            {
                exmessage = ex.Message;
            }
            finally
            {
            }

            return exmessage;
        }

        private delegate string ConnectSocketDelegate(IPEndPoint ipep, Socket sock);
        //��������
        private void ClickConnect()
        {
            try
            {
                int _port = Convert.ToInt32(inputPort);             //��ȡ�˿ں�
                string _ip = inputIp;                               //��ȡip��ַ

                //�����ͻ���Socket�����Զ��ip�Ͷ˿ں�
                socketSend = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ip = IPAddress.Parse(_ip);
                IPEndPoint point = new IPEndPoint(ip, _port);
                //allSockets.Connect(point); 
                //staInfo = ip + ":" + _port + "  ���ӳɹ�"; 

                ConnectSocketDelegate connect = ConnectSocket;
                IAsyncResult asyncResult = connect.BeginInvoke(point, socketSend, null, null);

                bool connectSuccess = asyncResult.AsyncWaitHandle.WaitOne(2000, false); //2������
                if (!connectSuccess)
                {
                    staInfo = "����ʧ�ܣ�������Ϣ�����ӳ�ʱ";
                    Debug.Log(staInfo);//2��󵯳�
                    OnFeild?.Invoke(staInfo);
                }
                else
                {
                    staInfo = "���ӳɹ� , " + " ip = " + ip + " port = " + _port;
                    Debug.Log(staInfo);
                    OnBuild?.Invoke(ip + ":" + _port);
                }

                Thread r_thread = new Thread(Received);             //�����µ��̣߳���ͣ�Ľ��շ�������������Ϣ
                r_thread.IsBackground = true;
                r_thread.Start();

                Thread s_thread = new Thread(SendMessage);          //�����µ��̣߳���ͣ�ĸ�������������Ϣ
                s_thread.IsBackground = true;
                s_thread.Start();
            }
            catch (Exception)
            {
                Debug.Log("IP���߶˿ںŴ���......");
                staInfo = "IP���߶˿ںŴ���......";
                OnFeild?.Invoke(staInfo);
            }
        }

        /// <summary>
        /// ���շ���˷��ص���Ϣ
        /// </summary>
        void Received()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024 * 10];
                    //ʵ�ʽ��յ�����Ч�ֽ���
                    int len = socketSend.Receive(buffer);
                    if (len == 0)
                    {
                        break;
                    }

                    recMes = Encoding.UTF8.GetString(buffer, 0, len);

                    OnReceived?.Invoke(recMes);
                    Debug.Log("�ͻ��˽��յ������� �� " + recMes);

                    recTimes++;
                    Debug.Log("���մ���Ϊ��" + recTimes);
                }
                catch { }
            }
        }

        /// <summary>
        /// �������������Ϣ
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void SendMessage()
        {
            try
            {
                while (true)
                {
                    if (clickSend)                              //�������˷��Ͱ�ť
                    {
                        clickSend = false;
                        string msg = inputMes;
                        byte[] buffer = new byte[1024 * 6];
                        buffer = Encoding.UTF8.GetBytes(msg);
                        socketSend.Send(buffer);
                        Debug.Log("���͵�����Ϊ��" + msg);
                    }
                }
            }
            catch { }
        }


        private void OnDisable()
        {
            Debug.Log("begin OnDisable()");

            if (socketSend == null) return;

            if (socketSend.Connected)
            {
                try
                {
                    socketSend.Shutdown(SocketShutdown.Both);    //����Socket�ķ��ͺͽ��չ���
                    socketSend.Close();                          //�ر�Socket���Ӳ��ͷ����������Դ
                }
                catch (Exception e)
                {
                    print(e.Message);
                }
            }

            Debug.Log("end OnDisable()");
        }

        ////�û�����
        //void OnGUI()
        //{
        //    GUI.color = Color.black;

        //    GUI.Label(new Rect(65, 10, 60, 20), "״̬��Ϣ");

        //    GUI.Label(new Rect(135, 10, 80, 60), staInfo);

        //    GUI.Label(new Rect(65, 70, 50, 20), "������ip��ַ");

        //    inputIp = GUI.TextField(new Rect(125, 70, 100, 20), inputIp, 20);

        //    GUI.Label(new Rect(65, 110, 50, 20), "�������˿�");

        //    inputPort = GUI.TextField(new Rect(125, 110, 100, 20), inputPort, 20);

        //    GUI.Label(new Rect(65, 150, 80, 20), "���յ���Ϣ��");

        //    GUI.Label(new Rect(155, 150, 80, 20), recMes);

        //    GUI.Label(new Rect(65, 190, 80, 20), "���͵���Ϣ��");

        //    inputMes = GUI.TextField(new Rect(155, 190, 100, 20), inputMes, 20);

        //    if (GUI.Button(new Rect(65, 230, 60, 20), "��ʼ����"))
        //    {
        //        ClickConnect();
        //    }

        //    if (GUI.Button(new Rect(65, 270, 60, 20), "������Ϣ"))
        //    {
        //        clickSend = true;
        //    }
        //}
    }
}