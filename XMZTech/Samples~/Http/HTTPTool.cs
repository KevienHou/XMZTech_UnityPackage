using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Newtonsoft.Json;

using UnityEngine;
using UnityEngine.Networking;

using Task = System.Threading.Tasks.Task;

namespace XMZTech.Net.HTTP
{
    [Serializable]
    public class LostHeartActionDic
    {
        public LostType lostType;
        public string url;
        public WWWForm form;
        public Dictionary<string, string> header;
        public Action<string> callback;


        public enum LostType
        {
            None,
            Post,
            Get
        }
    }


    public class HTTPTool
    {
        private static HTTPTool inst;

        public static HTTPTool Instance
        {
            get
            {
                if (inst == null)
                {
                    inst = new HTTPTool();
                } 
                return inst;
            }
        }


        public async void PostAsync(string uri, WWWForm form, Dictionary<string, string> header = null, Action<string> toDo = null)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Post(uri, form))
            {
                if (header != null)
                {
                    foreach (var item in header)
                    {
                        webRequest.SetRequestHeader(item.Key, item.Value);
                    }
                }

                await webRequest.SendWebRequest();
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        Debug.Log(webRequest.error + "  url : " + uri);

                        //AddLostNet(new LostHeartActionDic()
                        //{
                        //    url = uri,
                        //    form = form,
                        //    header = header,
                        //    callback = toDo,
                        //    lostType = LostHeartActionDic.LostType.Post
                        //});

                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.Log("数据上传_不_成功 : DataProcessingError " + webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        toDo?.Invoke(webRequest.downloadHandler.text);
                        break;
                    case UnityWebRequest.Result.Success:
                        toDo?.Invoke(webRequest.downloadHandler.text);
                        break;
                    case UnityWebRequest.Result.InProgress:
                        Debug.Log("数据上传_不_成功 : InProgress" + webRequest.error);
                        break;
                    default:
                        Debug.LogError("数据上传_不_成功 : default");
                        break;
                }
            }
        }

        internal async void GetAsync(string uri, Action<string> toDo = null)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                await webRequest.SendWebRequest();
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:

                        Debug.Log(webRequest.error + "  url : " + uri);

                        //AddLostNet(new LostHeartActionDic()
                        //{
                        //    url = uri,
                        //    callback = toDo,
                        //    lostType = LostHeartActionDic.LostType.Get
                        //});

                        break;
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.Log(webRequest.error);
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.Log(webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        toDo?.Invoke(webRequest.downloadHandler.text);
                        break;
                }
            }
        }


        public async void GetImg(string uri, Action<Texture2D> toDo = null)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
            {
                await webRequest.SendWebRequest();
                switch (@webRequest.result)
                {
                    case UnityWebRequest.Result.InProgress:
                        break;
                    case UnityWebRequest.Result.Success:
                        // 先获取到图片的数据
                        byte[] results = webRequest.downloadHandler.data;
                        Texture2D texture = new Texture2D(10, 10);
                        texture.LoadImage(results);
                        await Task.Delay(10);
                        toDo?.Invoke(texture);
                        break;
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.ProtocolError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.Log(webRequest.error + "  url : " + uri);
                        break;
                }
            }
        }


        public List<LostHeartActionDic> lostQueue = new List<LostHeartActionDic>();


        //int AddLostNet(LostHeartActionDic lost)
        //{
        //    if (lostQueue.Find((x) =>
        //    {
        //        if (x.url == lost.url) return true;
        //        return false;
        //    }) == null)
        //    {
        //        lostQueue.Add(lost);
        //    }
        //    HandleLostNet();
        //    return LostQueueCount;
        //}

        //int RemoveLostNet(string url)
        //{
        //    if (LostQueueCount <= 0)
        //    {
        //        return 0;
        //    }

        //    var tmpLost = lostQueue.Find((x) =>
        //    {
        //        if (x.url == url) return true;
        //        return false;
        //    });
        //    if (tmpLost != null)
        //    {
        //        lostQueue.Remove(tmpLost);
        //    }

        //    HandleLostNet();
        //    return LostQueueCount;
        //}

        //public int LostQueueCount => lostQueue.Count;

        //void HandleLostNet()
        //{
        //    if (LostQueueCount <= 0) return;
        //    var temp = lostQueue[0];
        //    switch (temp.lostType)
        //    {
        //        case LostHeartActionDic.LostType.Post:
        //            Debug.Log("重连一次,Post,url:" + temp.url);
        //            StartCoroutine(PostLost(temp));
        //            break;
        //        case LostHeartActionDic.LostType.Get:
        //            Debug.Log("重连一次,Get,url:" + temp.url);
        //            StartCoroutine(GetLost(temp));
        //            break;
        //    }
        //}


        //public IEnumerator PostLost(LostHeartActionDic lost)
        //{
        //    using (UnityWebRequest webRequest = UnityWebRequest.Post(lost.url, lost.form))
        //    {
        //        if (lost.header != null)
        //        {
        //            foreach (var item in lost.header)
        //            {
        //                webRequest.SetRequestHeader(item.Key, item.Value);
        //            }
        //        }
        //        yield return webRequest.SendWebRequest();
        //        switch (webRequest.result)
        //        {
        //            case UnityWebRequest.Result.ConnectionError:
        //                Debug.Log(webRequest.error + "  url : " + lost.url);
        //                HandleLostNet();
        //                break;
        //            case UnityWebRequest.Result.DataProcessingError:
        //                Debug.Log(webRequest.error);
        //                break;
        //            case UnityWebRequest.Result.ProtocolError:
        //                Debug.Log(webRequest.error);
        //                break;
        //            case UnityWebRequest.Result.Success:
        //                lost.callback?.Invoke(webRequest.downloadHandler.text);
        //                RemoveLostNet(lost.url);
        //                break;
        //        }
        //    }
        //}

        //internal IEnumerator GetLost(LostHeartActionDic lost)
        //{
        //    using (UnityWebRequest webRequest = UnityWebRequest.Get(lost.url))
        //    {
        //        yield return webRequest.SendWebRequest();
        //        switch (webRequest.result)
        //        {
        //            case UnityWebRequest.Result.ConnectionError:
        //                Debug.Log(webRequest.error + "  url : " + lost.url);
        //                HandleLostNet();
        //                break;
        //            case UnityWebRequest.Result.DataProcessingError:
        //                Debug.Log(webRequest.error);
        //                break;
        //            case UnityWebRequest.Result.ProtocolError:
        //                Debug.Log(webRequest.error);
        //                break;
        //            case UnityWebRequest.Result.Success:
        //                lost.callback?.Invoke(webRequest.downloadHandler.text);
        //                RemoveLostNet(lost.url);
        //                break;
        //        }
        //    }
        //}


    }


    public static class NetPlus
    {
        /// <summary>
        /// 异步重载
        /// </summary>
        /// <param name="asyncOp"></param>
        /// <returns></returns>
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); };
            return ((Task)tcs.Task).GetAwaiter();
        }
    }


}