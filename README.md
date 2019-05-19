# NoitomHi5InertiaToFingerMuscle
Noitom Hi5を用いてHumanoidモデルの指を動かすことが可能になるスクリプト。  
![demo](Documents/demo.gif)

## 概要
公式から出されているNoitom Hi5の[SDK内にあるサンプルスクリプト](https://hi5vrglove.com/downloads/unity)は、特定のボーン構造をしているモデルのみ正常に動作するように作られています。  
またTransformを直接変更しているため、パフォーマンス的にあまりよろしくありません。  
さらに手首及び全ての指ボーンを個別に登録しなければいけないため、セットアップが非常に大変です。

そこで、TransformではなくHumanPose.musclesを変更するようにカスタマイズすることで、Humanoidモデルであればどんなモデルでも正常に指を動かすことができるようにし、セットアップも非常に簡略化したスクリプトを作成しました。  
もちろん[VRM](https://github.com/vrm-c/UniVRM)でも正常に動作することを確認しています。

## 使い方
1. [Releases](https://github.com/Bizcast/NoitomHi5InertiaToFingerMuscle)からUnityPackageをダウンロードしてインポートする。
1. [Noitom Hi5 Unity SDK](https://hi5vrglove.com/downloads/unity)と（必要であれば）[SteamVR Plugin](https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647)をダウンロードしてインポートする。
1. 適当なGameObjectに `NoitomHi5InertiaToFingerMuscle` コンポーネントをアタッチする。
1. 指を動かしたい対象のHumanoid Animatorを `Animator` に登録する。
1. 左右どちらか片方の指を動かしたい場合は `HandType` から設定する。`Both` だと左右両方の指が動きます。
1. **（非推奨）** 手首を動かしたい場合は `HandRotationWeight` に適当な値を設定する。
1. Noitom Hi5を接続してエディタを再生する。
1. エンジョイ！

## 動作環境
- Unity2018.2.21f1
- Noitom Hi5 Unity SDK v1.0.0.655.16
- SteamVR Plugin v2.2.0  
  Noitom Hi5 Unity SDKのサンプルスクリプトを実行するために必要ですが、もしサンプルスクリプトを必要としないのであれば不要です。

## FAQ
### 手首が変な方向に曲がるんだけど
手首の回転のみHumanPose.musclesに落とし込むことができませんでした。  
その為、手首を回転させる処理のみSDK内にあるサンプルスクリプトと同じ処理をしています。  
`HandRotationWeight` の値を `0` にして、Perception NeuronやVive Trackerなどの外部デバイスを用いて手首を回転させてあげるのが良いかと思います。

## ライセンス
本リポジトリは[MITライセンス](https://github.com/Bizcast/NoitomHi5InertiaToFingerMuscle/blob/master/LICENSE)の下で公開しています。

本リポジトリのREADME及びサンプルには、[ユニティちゃんライセンス条項](http://unity-chan.com/contents/license_jp/)の元に提供されているコンテンツを含んでいます。  
これらのコンテンツを利用される場合は、同梱しているライセンスファイルに従ってくさだい。  
![UCLLogo](http://unity-chan.com/images/imageLicenseLogo.png)  
© Unity Technologies Japan/UCL
