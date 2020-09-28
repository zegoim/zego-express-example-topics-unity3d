# Zego Express Example Topics Unity (c#)

[English](README.md) | [Chinese](README_zh.md)

Zego Express Example Topics Unity (c#)

## Download SDK

The SDK required to run the Demo project may be missing from this Repository. You need to download the SDK and put the Plugins and Scripts folders into the Demo project's Assets folder.
**[https://storage.zego.im/express/video/unity3d/zego-express-video-unity3d.zip](https://storage.zego.im/express/video/unity3d/zego-express-video-unity3d.zip)**

Finally, the structure under the directory should be as follows

```tree
.
├── README.md
├── README_zh.md
└── Assets
       ├── Plugins
       │
       ├── Scenes
       │
       ├── Scripts
       │
       ├── Test
```

## Fill in the appID and appSign required by the SDK

Go to [ZEGO Management Console](https://console-express.zego.im/acount/register) and apply for appID and appSign, then fill it in the ZegoExpressEngine.CreateEngine method of `QuickStartDemo.cs`, otherwise compile for the first time Will report an error in this file.