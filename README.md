# XMZTech Develop Tools
## 工作开发中总结的小工具，制作成UnityPackage，使用时导入Untiy即可使用。开箱即用。
### 1.Http Tool  
Disc：使用UnityWebReqest编写，并封装了异步Awaiter。脱离MonoBehaviour使用。
功能|描述
-|- 
a.GetAsyc|   
b.PostAsyc|  可以添加Header、表单
c.GetImg| 下载网络及本地图片
### 2.Socket Tool 
Disc：包含TCP/UDP协议的网络连接逻辑。
功能|描述
-|- 
a.TCP|TCP 连接方式
b.UDP|UDP 连接方式(待添加)
c.TimeTrigger|解决非主线程无法调用Unity主线API的问题
### 3.WebSocket Tool (待添加)
Disc：使用Hlsconmication工具制作。 
功能|描述
-|-  
a.TimeTrigger|解决非主线程无法调用Unity主线API的问题
### 4.Messenger
Disc： 事件回调管理器，应用于框架中的消息机制，解耦框架，将逻辑层和UI层合理的分离开来。
功能|描述
-|-  
方法定义|private string MyEventHandler(float f1) { return "Test " + f1; }
订阅监听|Messenger<float>.AddListener("myEvent", MyEventHandler);
广播监听|Messenge<float>.Broadcast("myEvent", 1.0f);
针对播放监听|Messenger<float>.Broadcast<string>("myEvent", 1.0f, MyEventHandler);
### 4.Ultraleap Gesture Tool
Disc：针对 Ultraleap 设备的手势检测工具，封装了可扩展的手势枚举以及手势管理器 GestureManager，注意：本工具需要搭配Ultraleap.UnityPlugin_6.12.1以上版本SDK使用。
功能|描述
-|-  
1.HandGestureManager| 手势管理器
2.Demo|包中已包含示例脚本
易于扩展
