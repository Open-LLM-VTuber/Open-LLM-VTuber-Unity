# Open-LLM-Vtuber-Unity

## 介绍
这是原项目的移动端适配版本，基本上开箱即用。

## 准备工作
1. 使用dev版本的后端作为服务器。克隆对应的仓库到本地：
```bash
git clone https://github.com/Open-LLM-VTuber/Open-LLM-VTuber.git -b dev --recursive
```
2. 按照README的指引安装好依赖。如果已经有了main版本的后端，可以将`.venv`,`live2d-models`文件夹移动到dev版本的文件夹里。
3. 运行dev版后端。

## Windows电脑作为服务器时，如何使用？

1. 到Release页面下载Vifu.apk后安装到手机上。
2. 长按安装好的app，点击应用信息，设置权限都为允许。
3. 在你的电脑上，点击右下角的Wifi图标, 再点击右下角的齿轮打开“网络和 Internet设置”。
4. 在网络设置页面点击“共享热点”，然后让手机打开Wifi连接这个热点。
5. 用Windows+R键呼出”运行”窗口，输入cmd，进入命令行界面。再输入ipconfig，弹出当前网络连接相关信息。看到“无线网络适配器 本地连接”有ipv4地址的部分。示例如下：
```plaintext
无线局域网适配器 本地连接* 1:

   媒体状态  . . . . . . . . . . . . : 媒体已断开连接
   连接特定的 DNS 后缀 . . . . . . . :

无线局域网适配器 本地连接* 2:

   连接特定的 DNS 后缀 . . . . . . . :
   本地链接 IPv6 地址. . . . . . . . : fe80::4889:be44:2637:8f23%17
   IPv4 地址 . . . . . . . . . . . . : 192.168.137.1
   子网掩码  . . . . . . . . . . . . : 255.255.255.0
   默认网关. . . . . . . . . . . . . :

```
所以我们的目标IP地址应该是192.168.137.1。

6. 在手机上点击灰色齿轮按钮，看到右边弹出菜单中的Websocket地址和BaseUrl配置，把127.0.0.1换成192.168.137.1即可。点击保存配置，退出。

7. 重新进入，等待Live2D人物刷新。如果没有出现人物，可以尝试清理应用缓存再重进入。