using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
  
using UnityEngine;
using UnityEngine.UI;

using Debug = UnityEngine.Debug;

namespace XMZTech.Net.TCP
{  
    public class Config
    {
        public string ipStr;
    }



    public class NetClientManager : MonoBehaviour
    {
        public static NetClientManager instance;

        public InputField ipAddress;
        Dictionary<string, TimeActionUpdate> allActions = new Dictionary<string, TimeActionUpdate>();
        private const string ConnectSuc = "ConnectSuc";
        private const string ConnectFaild = "ConnectFaild";
        private const string OnReceive = "OnReceive";
        private string receiveStr;
        private TCP_Client TCP_Client;
        public Config config;

        private void Awake()
        {
            instance = this;
            TCP_Client = new TCP_Client();
            TCP_Client.OnReceived += OnReceived;
            TCP_Client.OnBuild += OnBuild;
            TCP_Client.OnFeild += OnFeild;
        }


        public void Start()
        {
            allActions[OnReceive] = new TimeActionUpdate(endAction: () =>
            {
                //receiveStr
            });

            allActions[ConnectSuc] = new TimeActionUpdate(endAction: () =>
            {

            });

            allActions[ConnectFaild] = new TimeActionUpdate(endAction: () =>
            {

                //Process.Start(@"C:\WINDOWS\system32\osk.exe");//启动屏幕键盘
            });

            if (File.Exists(Application.streamingAssetsPath + "/Config.txt"))
            {
                var textt = File.ReadAllText(Application.streamingAssetsPath + "/Config.txt");
                config = JsonUtility.FromJson<Config>(textt);
                Connect(config.ipStr);
            }
            else
            {
                File.Create(Application.streamingAssetsPath + "/Config.txt");
            }
        }



        private void Update()
        {
            foreach (var item in allActions)
            {
                item.Value.Update(Time.deltaTime);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"> xxx.xxx.xxx.xxx:xxxx</param>
        public void Connect(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Contains(":") == false)
            {
                return;
            }
            TCP_Client.InitClient(str);
        }


        private void OnBuild(string obj)
        {
            allActions[ConnectSuc].Trigger();
            config.ipStr = obj;
            //写数据
            var str = JsonUtility.ToJson(config);
            File.WriteAllText(Application.streamingAssetsPath + "/Config.txt", str);
        }

        private void OnFeild(string obj)
        {
#if UNITY_EDITOR
            Debug.Log("连接失败，调起窗口");
#else
        allActions[ConnectFaild].Trigger();
#endif
        }


        internal void Send<T>(T obj)
        {
            string comStr = JsonConvert.SerializeObject(obj);
            TCP_Client.Send(comStr);
        }

        private void OnReceived(string str)
        {
            receiveStr = str;
            allActions[OnReceive].Trigger();
        }

    }
}