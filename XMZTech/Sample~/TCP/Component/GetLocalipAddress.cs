using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;

using UnityEngine;

public class GetLocalipAddress : MonoBehaviour
{
    public static string GetLocalIP()
    {
        IPAddress loacl_IP = null;
        try
        {
            IPAddress[] ips;
            ips = Dns.GetHostAddresses(Dns.GetHostName());
            loacl_IP = ips.First(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            return loacl_IP.ToString();
        }
        catch (System.Exception)
        {
            Debug.Log("本机IP获取失败！");
            return string.Empty;
        }
    }
}
