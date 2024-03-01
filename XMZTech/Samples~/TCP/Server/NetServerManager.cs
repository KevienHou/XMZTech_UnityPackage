using Newtonsoft.Json;

using System.Collections;
using System.Collections.Generic;
  
using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace XMZTech.Net.TCP
{


    public class NetServerManager : MonoBehaviour
    {

        public static NetServerManager instance;

        private TCP_Server TCP_Server;
        private Dictionary<string, TimeActionUpdate> allActions = new Dictionary<string, TimeActionUpdate>();
        public string ipShow;
        private string tempLocalIP;
        private string receiveStr;

        private const string Build = "Build";
        private const string Receive = "Receive";
        private const string Connect = "Connect";



        private void Awake()
        {
            instance = this;
            allActions[Build] = new TimeActionUpdate(endAction: () =>
            {
                ipShow = tempLocalIP;
            });
            allActions[Receive] = new TimeActionUpdate(endAction: () =>
            {
                //receiveStr 
            });
            allActions[Connect] = new TimeActionUpdate(endAction: () =>
            {
                ipShow = tempLocalIP + " , 连接成功，建议点击左侧隐藏IP地址";
                //TCP_Server.Send();
            });
            TCP_Server = new TCP_Server();
            TCP_Server.OnBuild += ServerBuild;
            TCP_Server.OnReceive += ServerReceive;
            TCP_Server.OnConnect += ServerConnect;
        }

        private void Update()
        {
            foreach (var item in allActions)
            {
                item.Value.Update(Time.deltaTime);
            }
        }


        private void ServerConnect(string str)
        {
            allActions[Connect].Trigger();
        }

        private void ServerBuild(string ip)
        {
            tempLocalIP = ip;
            allActions[Build].Trigger();
        }



        public void Init()
        {
            string ip = GetLocalipAddress.GetLocalIP();
            if (string.IsNullOrEmpty(ip))
            {
                ipShow = "服务器创建失败！";
                Debug.Log(ipShow);
                return;
            }
            TCP_Server.InitServer(ip, "8086");
        }




        private void ServerReceive(string obj)
        {
            receiveStr = obj;
            allActions[Receive].Trigger();
        } 
    }
}