# Zego Express Example Topics Unity (c#)

[English](README.md) | [中文](README_zh.md)

Zego Express Example Topics Unity (c#) 示例专题 Demo

## 下载 SDK

此 Repository 中可能缺少运行 Demo 工程所需的SDK ，需要下载SDK并将Plugins和Scripts文件夹放入 Demo 工程的 `Assets` 文件夹下。

**[https://storage.zego.im/express/video/unity3d/zego-express-video-unity3d.zip](https://storage.zego.im/express/video/unity3d/zego-express-video-unity3d.zip)**

最终, 目录下的结构应如下

```tree
.
├── README.md
├── README_zh.md
└── Assets
       ├──  Plugins
       │
       ├──  Scenes
       │
       ├──  Scripts
       │
       ├──  Test
```

## 填写 SDK 所需的 appID 与 appSign

到 [ZEGO 管理控制台](https://console-express.zego.im/acount/register) 申请 appID 与 appSign , 然后将其填入 `GetAppIdConfig.cs`文件中，否则在首次编译时会在这个文件报错。
