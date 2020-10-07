# KokoroUpTime
 
フレームワークを実行ファイル群に組み込むことにより、
Net.Core関連のRuntimeを別途インストールする必要はなくなりました。
念のためにToolsフォルダーにRuntimeを確保しておきます。
(windowsdesktop-runtime-3.1.8-win-x64.exe)

## 配布ファイルの作り方

- Visual Studio でプロジェクトを右クリックし発行を選ぶ（すでに設定済みなので発行ボタンを押すだけ）。
- bin/Release/netcoreapp3.1/publish/KokoroUpTime 自体が配布するフォルダになります。
- 配布するKokoroUpTimeフォルダ内のProgramsフォルダと同じ階層にLancherプロジェクトのPublishフォルダにある「こころあっぷタイム.exe」をコピーしてください。
- KokoroUpTimeフォルダを圧縮してください。最終的に配布するのは、そのzipファイルとなります。

## GitHubで公開する場合

1ファイル100MB以上のアップロードはできないので、zipファイルにせず、新しいレポジトリを作るなどして、そこへ非圧縮のままKokoroUpTimeフォルダごとアップロードし、ユーザにクローン（ダウンロード）してもらいましょう。

## Dropboxで公開する場合

Dropboxの中に適当なフォルダをつくり、その中に配布ファイルのzipをそのまま収納します。あとはそのフォルダを共有フォルダとして設定し、ユーザにリンクを教えるだけでOKです。
