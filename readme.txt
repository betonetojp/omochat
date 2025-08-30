◆ 動作環境

Windows11 22H2 (x64)
.NET 8.0
※ランタイムが必要です。インストールしていない場合は初回起動時の案内に従ってください。


◆ omochat.exe

簡易的にBitchatを覗いたり投稿したりするNostrクライアントです。

投稿機能を使うには、設定画面でNostr秘密鍵（nsec1...）の入力が必要です。
設定画面で『Create new key』ボタンを押すと新しいNostr秘密鍵を生成します。

  ESC：設定画面
  F1 ：ポストバーの表示と非表示（F12キーでも動作）
  F2 ：時間の表示と非表示
  F3 ：名前の表示と非表示
  F4 ：ユーザーハッシュ表示と非表示（デフォルト非表示）
  F5 ：Geohashの表示と非表示（デフォルト非表示）
  F9 ：コンテンツの折り返し表示の切り替え（Zキーまたは余白ダブルクリックでも動作）
  F10：ユーザーミュート設定画面（余白右クリックでも動作）※現状kind:0の存在しないBitchatユーザーはミュートできません
  F11：メイン画面の表示と非表示

  Shift + W：イベント最上行へ移動
  W / [↑] ：イベント選択上移動
  S / [↓] ：イベント選択下移動
  Shift + S：イベント最下行へ移動
  A / [←] ：Webビューを開く（イベントを右クリックでも動作）
  C：Webビューを閉じる

  F：mention（ポストバーに選択ユーザーの名前を入力）
  H：hugを送信
  T：slapを送信

  R：返信 ※Bitchatの機能ではなくNostrの返信です

  Ctrl + Shift + Z：メインフォームをアクティブにする

タイムラインを「伺か」(SSP)に流すことができます。
https://ssp.shillest.net/
https://keshiki.nobody.jp/

GhostSpeakerと棒読みちゃんを組み合わせて読み上げさせるのがおすすめです。
https://github.com/apxxxxxxe/GhostSpeaker
https://chi.usamimi.info/Program/Application/BouyomiChan/

「伺か」(SSP)用ゴースト「nostalk」のNostrイベント通知(Nostr/0.4)に対応しアバター画像を送信できます。
https://github.com/nikolat/nostalk


◆ 更新履歴

2025/08/30 v0.0.2
タイトルバーにteleportタグのオンオフ表示
ユーザーハッシュ表示列追加
mention、hug、slap機能追加
クライアントカラーにいくつか追加
※clients.jsonが存在すると更新されないので追加したい場合は削除してください

2025/08/29 v0.0.1
初公開


◆ 利用NuGetパッケージ

CredentialManagement
https://www.nuget.org/packages/CredentialManagement


◆ Nostrクライアントライブラリ

NNostr
https://github.com/Kukks/NNostr
内のNNostr.Client Ver0.0.49を一部変更して利用しています。


◆ DirectSSTP送信ライブラリ

DirectSSTPTester
https://github.com/nikolat/DirectSSTPTester
内のSSTPLib Ver4.0.0を利用しています。
