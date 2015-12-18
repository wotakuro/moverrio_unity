# moverrio_unity
Moverio周りを扱う関係のモノをアップします

#初めに
下記のMoverio SDKサイトより、SDKを落としてください
https://tech.moverio.epson.com/ja/life/bt-200/sdk_download.html

zip解凍後にあります「BT200Ctrl.jar」を「Assets/Moverio/Plugins/Android」にコピーしてください。

#使用方法
通常カメラの代わりに、CameraMoverio.prefab を置いてください。

CameraDistanceで左右の立体視の具合の調節が出来ます。
0にすると立体視じゃない形で描画するようになります。

CameraMoveでは、Cameraをジャイロを使って動かすのか、固定するか、地磁気センサーを用いて方位で行うか等が選択可能になっています。

また、LockRotationで、特定の軸のみ回転させることも可能になっております。


