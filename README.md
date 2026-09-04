# omochat

Bitchat のタイムラインを覗いたり投稿したりできるシンプルな Nostr クライアントです。  
タイムラインの投稿を「伺か」(SSP) にリアルタイム送信する SSTP 連携機能も備えています。

---

## 動作環境
- **OS**: Windows 10 / Windows 11 (x64)
- **ランタイム**: [.NET 8.0 ランタイム (Desktop Runtime)](https://dotnet.microsoft.com/download/dotnet/8.0)

---

## 主な機能

- **Bitchat タイムライン受信**: Bitchat 形式（kind: 20000）のメッセージをリアルタイム受信
- **インデクサリレー連携**: プロフィール未取得のアカウントもインデクサリレー（`directory.yabu.me`）から非同期に自動取得
- **投稿機能**: ポストバーから Bitchat へのメッセージ投稿（設定画面で Nostr 秘密鍵 `nsec1...` の登録が必要）
- **Geohash 表示・フィルタ**: 各投稿の Geohash 表示、選択行の Geohash へのジャンプ（Xキー）、World view（所属外をグレー表示）の切り替え
- **Web ビュー・Google 翻訳連携**: 投稿を内蔵 Web ビューで開く機能および Google 翻訳によるワンキー翻訳
- **ミュート機能**: アカウントミュート（pubkey）に加え、名前（NameMute）およびチャット本文（ChatMute）の部分一致ワードミュートに対応
- **表示順序・自動スクロール**: 昇順表示／降順表示の切り替え、新着投稿への自動スクロール
- **「伺か」(SSP) 連携**: DirectSSTP による投稿通知（Nostr/0.5仕様、アバター画像やBitchat情報 g, n, t タグ送信対応）

---

## 操作方法・ショートカット

| キー / 操作 | 動作 |
|---|---|
| `ESC` | 設定画面を開く |
| `F1` / `F12` | ポストバー（投稿欄）の表示 / 非表示を切り替え |
| `F2` | 時間列（time）の表示 / 非表示を切り替え |
| `F3` | 名前列（name）の表示 / 非表示を切り替え |
| `F4` | ユーザーハッシュ列（hash）の表示 / 非表示を切り替え（デフォルト非表示） |
| `F5` | Geohash 列の表示 / 非表示を切り替え |
| `F9` / `Z` / 余白ダブルクリック | 本文の折り返し表示の切り替え |
| `F10` / 余白右クリック | マニアック設定画面（ミュートアカウント一覧、ワードミュート等）を開く |
| `F11` | メイン画面を最小化 |
| `Shift` + `W` | 最上行のイベントへ移動 |
| `W` / `↑` | 選択行を上へ移動 |
| `S` / `↓` | 選択行を下へ移動 |
| `Shift` + `S` | 最下行のイベントへ移動 |
| `A` / `←` / イベント右クリック | 内蔵 Web ビューを開く |
| `D` | 選択投稿の Google 翻訳ページを Web ビューで開く |
| `C` | 内蔵 Web ビューを閉じる |
| `X` | 選択行の Geohash に現在位置を移動 |
| `F` | メンション（ポストバーに選択ユーザーの `@名前` を挿入） |
| `H` | hug を送信 |
| `T` | slap を送信 |
| `R` | 返信（Nostr の返信形式でポストバーを開く） |
| `Ctrl` + `Shift` + `Z` | メインフォームをアクティブ化（グローバルホットキー） |

---

## 外部連携

### 「伺か」(SSP) 連携（DirectSSTP）
タイムラインの投稿を「伺か」(SSP) にリアルタイムで流すことができます。
- [SSP](https://ssp.shillest.net/) / [keshiki](https://keshiki.nobody.jp/)
- [GhostSpeaker](https://github.com/apxxxxxxe/GhostSpeaker) と [棒読みちゃん](https://chi.usamimi.info/Program/Application/BouyomiChan/) を組み合わせることで、タイムラインの音声読み上げも可能です。
- 「伺か」(SSP) 用ゴースト「[nostalk](https://github.com/nikolat/nostalk)」の Nostr イベント通知仕様 (Nostr/0.5) に対応し、アバター画像や Bitchat 情報（`g`, `n`, `t` タグ）をゴースト側に送信できます。

---

## 各種設定ファイル
アプリの実行フォルダ内に以下の設定ファイルが保存されます：
- `omochat.config`: ウィンドウ位置、カラム表示設定、表示順序、Geohash等の基本設定（XML形式）
- `relays.json`: 接続先リレー情報
- `users.json`: プロフィール情報のキャッシュ（表示名、ニックネーム、ミュート設定等）
- `clients.json`: クライアントタグに応じた時間の背景色マッピング
- `namemute.json`: 名前ミュートワードリスト
- `chatmute.json`: 本文ミュートワードリスト

---

## 利用ライブラリ / NuGet パッケージ
- [CredentialManagement](https://www.nuget.org/packages/CredentialManagement)
- [Microsoft.Web.WebView2](https://www.nuget.org/packages/Microsoft.Web.WebView2)
- [NNostr](https://github.com/Kukks/NNostr) (NNostr.Client を一部変更して同梱)
- [DirectSSTPTester](https://github.com/nikolat/DirectSSTPTester) (SSTPLib を利用)