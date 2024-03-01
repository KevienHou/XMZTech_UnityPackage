using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

using UnityEngine;


namespace XMZTech.Net.WebSocket
{ 
    public class WebSocketClient
    {
        #region 实现单例的代码
        //变量
        private volatile static WebSocketClient m_instance;          //单例本身。使用volatile关键字修饰，禁止优化，确保多线程访问时访问到的数据都是最新的
        private static object m_locker = new object();          //线程锁。当多线程访问时，同一时刻仅允许一个线程访问

        //属性
        public static WebSocketClient Instance
        {
            get
            {
                //线程锁。防止同时判断为null时同时创建对象
                lock (m_locker)
                {
                    //如果不存在对象则创建对象
                    if (m_instance == null)
                    {
                        m_instance = new WebSocketClient();
                    }
                }
                return m_instance;
            }
        }
        #endregion

        //私有化构造
        private WebSocketClient() { }

        //客户端webSocket
        private ClientWebSocket m_clientWebSocket;

        //处理接收数据的线程
        private Thread m_dataReceiveThread;
        //线程持续执行的标识符
        private bool m_isDoThread;


        /// <summary>
        /// ClientWebSocket，与服务器建立连接。
        /// </summary>
        /// <param name="uriStr"></param>
        public void Connect(string uriStr)
        {
            try
            {
                //创建ClientWebSocket
                m_clientWebSocket = new ClientWebSocket();

                //初始化标识符
                m_isDoThread = true;

                //创建线程
                m_dataReceiveThread = new Thread(ReceiveData);  //创建数据接收线程  
                m_dataReceiveThread.IsBackground = true;        //设置为后台可以运行，主线程关闭时，此线程也会关闭（实际在Unity中并没什么用，还是要手动关闭）

                //设置请求头部
                //m_clientWebSocket.Options.SetRequestHeader("headerName", "hearValue");

                //开始连接
                var task = m_clientWebSocket.ConnectAsync(new Uri(uriStr), CancellationToken.None);
                task.Wait();    //等待

                //启动数据接收线程
                m_dataReceiveThread.Start(m_clientWebSocket);

                //输出提示
                if (m_clientWebSocket.State == WebSocketState.Open)
                {
                    Debug.Log("连接服务器完毕。");
                }
            }
            catch (WebSocketException ex)
            {
                Debug.LogError("连接出错：" + ex.Message);
                Debug.LogError("WebSokcet状态：" + m_clientWebSocket.State);
                //关闭连接
                //函数内可能需要考虑WebSokcet状态不是WebSocketState.Open时如何关闭连接的情况。目前没有处理这种情况。
                //比如正在连接时出现了异常，当前状态还是Connecting状态，那么该如何停止呢？
                //虽然我有了解到ClientWebSocket包含的Abort()、Dispose()方法，但并未出现过这种异常情况所以也没继续深入下去，放在这里当个参考吧。
                CloseClientWebSocket();
            }

        }

        /// <summary>
        /// 持续接收服务器的信息。
        /// </summary>
        /// <param name="socket"></param>
        private void ReceiveData(object socket)
        {
            //类型转换
            ClientWebSocket socketClient = (ClientWebSocket)socket;
            //持续接收信息
            while (m_isDoThread)
            {
                //接收数据
                string data = Receive(socketClient);
                //数据处理（可以和服务器一样使用事件（委托）来处理）
                if (data != null)
                {
                    Debug.Log("接收的服务器消息：" + data);
                }
            }
            Debug.Log("接收信息线程结束。");
        }

        /// <summary>
        /// 接收服务器信息。
        /// </summary>
        /// <param name="socket"></param>
        /// <returns></returns>
        private string Receive(ClientWebSocket socket)
        {
            try
            {
                //接收消息时，对WebSocketState是有要求的，所以加上if判断（如果不是这两种状态，会报出异常）
                if (socket != null && (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseSent))
                {
                    byte[] arrry = new byte[1024];  //注意长度，如果服务器发送消息过长，这也需要跟着调整
                    ArraySegment<byte> buffer = new ArraySegment<byte>(arrry);  //实例化一个ArraySegment结构体
                                                                                //接收数据
                    var task = socket.ReceiveAsync(buffer, CancellationToken.None);
                    task.Wait();//等待

                    //仅作状态展示。在客户端发送关闭消息后，服务器会回复确认信息，在收到确认信息后状态便是CloseReceived，这里打印输出。
                    Debug.Log("socekt当前状态：" + socket.State);

                    //如果是结束消息确认，则返回null，不再解析信息
                    if (socket.State == WebSocketState.CloseReceived || task.Result.MessageType == WebSocketMessageType.Close)
                    {
                        return null;
                    }
                    //将接收数据转为string类型，并返回。注意只解析我们接收到的字节数目（task.Result.Count）
                    return Encoding.UTF8.GetString(buffer.Array, 0, task.Result.Count);
                }
                else
                {
                    return null;
                }
            }
            catch (WebSocketException ex)
            {
                Debug.LogError("接收服务器信息错误：" + ex.Message);
                CloseClientWebSocket();
                return null;
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="content"></param>
        public void Send(string content)
        {
            try
            {
                //发送消息时，对WebSocketState是有要求的，加上if判断（如果不是这两种状态，会报出异常）
                if (m_clientWebSocket != null && (m_clientWebSocket.State == WebSocketState.Open || m_clientWebSocket.State == WebSocketState.CloseReceived))
                {
                    ArraySegment<byte> array = new ArraySegment<byte>(Encoding.UTF8.GetBytes(content)); //创建内容的字节编码数组并实例化一个ArraySegment结构体
                    var task = m_clientWebSocket.SendAsync(array, WebSocketMessageType.Binary, true, CancellationToken.None);  //发送
                    task.Wait();  //等待

                    Debug.Log("发送了一个消息到服务器。");
                }
            }
            catch (WebSocketException ex)
            {
                Debug.LogError("向服务器发送信息错误：" + ex.Message);
                CloseClientWebSocket();
            }
        }

        /// <summary>
        /// 关闭ClientWebSocket。
        /// </summary>
        public void CloseClientWebSocket()
        {
            //关闭Socket
            if (m_clientWebSocket != null && m_clientWebSocket.State == WebSocketState.Open)
            {
                var task = m_clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                Debug.Log("如果打印过快，↓下面↓这个socket状态可能为Open，出现Open就多试几次，我们想看的是CloseSent状态。");
                Debug.Log("socekt当前状态：" + m_clientWebSocket.State);
                task.Wait();
                Debug.Log("socekt当前状态：" + m_clientWebSocket.State);
                Debug.Log("连接已断开。");
            }
            //关闭线程
            if (m_dataReceiveThread != null && m_dataReceiveThread.IsAlive)
            {
                m_isDoThread = false;   //别想Abort()了，unity中的线程关闭建议使用bool来控制线程结束。
                m_dataReceiveThread = null;
            }
        }
    }
}