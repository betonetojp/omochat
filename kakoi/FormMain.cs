using NNostr.Client;
using NNostr.Client.Protocols;
using omochat.Properties;
using SSTPLib;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

namespace omochat
{
    public partial class FormMain : Form
    {
        #region フィールド
        private const int HOTKEY_ID = 1;
        private const int MOD_CONTROL = 0x0002;
        private const int MOD_SHIFT = 0x0004;
        private const int WM_HOTKEY = 0x0312;


        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly string _configPath = Path.Combine(Application.StartupPath, "omochat.config");

        private readonly FormSetting _formSetting = new();
        private readonly FormPostBar _formPostBar = new();
        private FormManiacs _formManiacs = new();
        private FormRelayList _formRelayList = new();
        private FormWeb _formWeb = new();
        internal Point _formWebLocation = new();
        internal Size _formWebSize = new();

        private string _nsec = string.Empty;
        private string _npubHex = string.Empty;

        /// <summary>
        /// ユーザー辞書
        /// </summary>
        internal Dictionary<string, User?> Users = [];

        private bool _minimizeToTray;
        private bool _worldView = false;
        private string _geohash = string.Empty;
        private bool _addTeleport;
        private string _nickname = string.Empty;
        private bool _autoScroll = true;
        private bool _descendingOrder = false;
        private bool _addClient = true;
        private bool _sendDSSTP = false;

        private double _tempOpacity = 1.00;

        private static readonly DSSTPSender _ds = new("SakuraUnicode");
        private readonly string _SSTPMethod = "NOTIFY SSTP/1.1";
        private readonly Dictionary<string, string> _baseSSTPHeader = new(){
            {"Charset","UTF-8"},
            {"SecurityLevel","local"},
            {"Sender","omochat"},
            {"Option","nobreak,notranslate"},
            {"Event","OnNostr"},
            {"Reference0","Nostr/0.5"}
        };

        private string _ghostName = string.Empty;
        // 重複イベントIDを保存するリスト
        private readonly LinkedList<string> _displayedEventIds = new();

        private List<Client> _clients = [];

        internal List<string> NameMute = [];
        internal List<string> ChatMute = [];

        private readonly ImeStatus _imeStatus = new();
        private bool _reallyClose = false;
        private static Mutex? _mutex;

        // 前回の最新created_at
        internal DateTimeOffset LastCreatedAt = DateTimeOffset.MinValue;
        // 最新のcreated_at
        internal DateTimeOffset LatestCreatedAt = DateTimeOffset.MinValue;
        #endregion

        #region コンストラクタ
        // コンストラクタ
        public FormMain()
        {
            InitializeComponent();

            // アプリケーションの実行パスを取得
            string exePath = Application.ExecutablePath;
            string mutexName = $"omochatMutex_{exePath.Replace("\\", "_")}";

            // 二重起動を防ぐためのミューテックスを作成
            _mutex = new Mutex(true, mutexName, out bool createdNew);

            if (!createdNew)
            {
                // 既に起動している場合はメッセージを表示して終了
                MessageBox.Show("Already running.", "omochat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }

            // ボタンの画像をDPIに合わせて表示
            float scale = CreateGraphics().DpiX / 96f;
            int size = (int)(16 * scale);
            if (scale < 2.0f)
            {
                buttonRelayList.Image = new Bitmap(Resources.icons8_list_16, size, size);
                buttonStart.Image = new Bitmap(Resources.icons8_start_16, size, size);
                buttonStop.Image = new Bitmap(Resources.icons8_stop_16, size, size);
                buttonSetting.Image = new Bitmap(Resources.icons8_setting_16, size, size);
            }
            else
            {
                buttonRelayList.Image = new Bitmap(Resources.icons8_list_32, size, size);
                buttonStart.Image = new Bitmap(Resources.icons8_start_32, size, size);
                buttonStop.Image = new Bitmap(Resources.icons8_stop_32, size, size);
                buttonSetting.Image = new Bitmap(Resources.icons8_setting_32, size, size);
            }

            Setting.Load(_configPath);
            Users = Tools.LoadUsers();
            _clients = Tools.LoadClients();
            NameMute = Tools.LoadNameMute();
            ChatMute = Tools.LoadChatMute();

            Location = Setting.Location;
            if (new Point(0, 0) == Location || Location.X < 0 || Location.Y < 0)
            {
                StartPosition = FormStartPosition.CenterScreen;
            }
            Size = Setting.Size;
            TopMost = Setting.TopMost;
            Opacity = Setting.Opacity;
            if (0 == Opacity)
            {
                Opacity = 1;
            }
            _tempOpacity = Opacity;
            _formPostBar.Opacity = Opacity;
            _minimizeToTray = Setting.MinimizeToTray;
            _worldView = Setting.WorldView;
            _geohash = Setting.Geohash;
            _addTeleport = Setting.AddTeleport;
            _nickname = Setting.Nickname;
            notifyIcon.Visible = _minimizeToTray;
            _autoScroll = Setting.AutoScroll;
            _descendingOrder = Setting.DescendingOrder;
            _sendDSSTP = Setting.SendDSSTP;
            _addClient = Setting.AddClient;

            _formPostBar.Location = Setting.PostBarLocation;
            if (new Point(0, 0) == _formPostBar.Location || _formPostBar.Location.X < 0 || _formPostBar.Location.Y < 0)
            {
                _formPostBar.StartPosition = FormStartPosition.CenterScreen;
            }
            _formPostBar.Size = Setting.PostBarSize;

            _formWebLocation = Setting.WebViewLocation;
            if (new Point(0, 0) == _formWebLocation || _formWebLocation.X < 0 || _formWebLocation.Y < 0)
            {
                _formWeb.StartPosition = FormStartPosition.CenterScreen;
            }
            _formWebSize = Setting.WebViewSize;

            dataGridViewNotes.Columns["name"].Width = Setting.NameColumnWidth;
            dataGridViewNotes.GridColor = Tools.HexToColor(Setting.GridColor);
            dataGridViewNotes.DefaultCellStyle.SelectionBackColor = Tools.HexToColor(Setting.GridColor);

            _formSetting.PostBarForm = _formPostBar;
            _formPostBar.MainForm = this;
            _formManiacs.MainForm = this;
        }
        #endregion

        #region Startボタン
        // Startボタン
        private async void ButtonStart_Click(object sender, EventArgs e)
        {
            try
            {
                int connectCount;
                if (NostrAccess.Clients != null)
                {
                    connectCount = await NostrAccess.ConnectAsync();
                }
                else
                {
                    connectCount = await NostrAccess.ConnectAsync();

                    if (NostrAccess.Clients != null)
                    {
                        NostrAccess.Clients.EventsReceived += OnClientOnUsersInfoEventsReceived;
                        NostrAccess.Clients.EventsReceived += OnClientOnTimeLineEventsReceived;
                    }
                }

                toolTipRelays.SetToolTip(labelRelays, string.Join("\n", NostrAccess.RelayStatusList));

                switch (connectCount)
                {
                    case 0:
                        labelRelays.Text = "No relay enabled.";
                        buttonStart.Enabled = false;
                        return;
                    case 1:
                        labelRelays.Text = $"{connectCount} relay";
                        break;
                    default:
                        labelRelays.Text = $"{connectCount} relays";
                        break;
                }

                await NostrAccess.SubscribeAsync();

                buttonStart.Enabled = false;
                buttonStop.Enabled = true;
                dataGridViewNotes.Focus();
                _formPostBar.textBoxPost.Enabled = true;
                _formPostBar.buttonPost.Enabled = true;

                // ログイン済みの時
                if (!string.IsNullOrEmpty(_npubHex))
                {
                    //// フォロイーを購読をする
                    //await NostrAccess.SubscribeFollowsAsync(_npubHex);

                    if (!string.IsNullOrEmpty(_nickname))
                    {
                        Text = $"@{_nickname}  #{_geohash}{(_addTeleport ? "📍" : "")} {(_worldView ? "🌐" : "")}";
                        notifyIcon.Text = $"omochat - @{_nickname}  #{_geohash}{(_addTeleport ? "📍" : "")} {(_worldView ? "🌐" : "")}";
                    }
                }

                dataGridViewNotes.Rows.Clear();
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Could not start.";
            }
        }
        #endregion

        #region ユーザー情報イベント受信時処理
        /// <summary>
        /// ユーザー情報イベント受信時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnClientOnUsersInfoEventsReceived(object? sender, (string subscriptionId, NostrEvent[] events) args)
        {
            if (args.subscriptionId == NostrAccess.GetProfilesSubscriptionId)
            {
                #region プロフィール購読
                foreach (var nostrEvent in args.events)
                {
                    if (RemoveCompletedEventIds(nostrEvent.Id))
                    {
                        continue;
                    }

                    // プロフィール
                    if (0 == nostrEvent.Kind && nostrEvent.Content != null && nostrEvent.PublicKey != null)
                    {
                        var newUserData = Tools.JsonToUser(nostrEvent.Content, nostrEvent.CreatedAt);
                        if (newUserData != null)
                        {
                            DateTimeOffset? cratedAt = DateTimeOffset.MinValue;
                            if (Users.TryGetValue(nostrEvent.PublicKey, out User? existingUserData))
                            {
                                cratedAt = existingUserData?.CreatedAt;
                            }
                            if (false == existingUserData?.Mute)
                            {
                                // 既にミュートオフのアカウントのミュートを解除
                                newUserData.Mute = false;
                            }
                            if (cratedAt == null || (cratedAt < newUserData.CreatedAt))
                            {
                                newUserData.LastActivity = DateTime.Now;
                                newUserData.Nickname = existingUserData?.Nickname;
                                Tools.SaveUsers(Users);
                                // 辞書に追加（上書き）
                                Users[nostrEvent.PublicKey] = newUserData;
                                Debug.WriteLine($"cratedAt updated {cratedAt} -> {newUserData.CreatedAt}");
                                Debug.WriteLine($"プロフィール更新: {newUserData.DisplayName} @{newUserData.Name}");
                            }
                        }
                    }
                }
                #endregion
            }
        }
        #endregion

        #region タイムラインイベント受信時処理
        /// <summary>
        /// タイムラインイベント受信時処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void OnClientOnTimeLineEventsReceived(object? sender, (string subscriptionId, NostrEvent[] events) args)
        {
            if (args.subscriptionId == NostrAccess.SubscriptionId)
            {
                #region タイムライン購読
                foreach (var nostrEvent in args.events)
                {
                    if (RemoveCompletedEventIds(nostrEvent.Id))
                    {
                        continue;
                    }

                    var content = nostrEvent.Content;
                    if (content != null)
                    {
                        // ChatMuteに含まれている文字列がcontentに含まれている時は表示しない
                        if (ChatMute.Any(mute => content.Contains(mute, StringComparison.OrdinalIgnoreCase)))
                        {
                            continue;
                        }

                        string nickname = string.Empty;
                        User? user = null;

                        string speaker = "\\1"; //"\\u\\p[1]\\s[10]";
                        //// 本体側がしゃべる
                        //string speaker = "\\0"; //"\\h\\p[0]\\s[0]";

                        #region テキストノート
                        if (20000 == nostrEvent.Kind)
                        {
                            var g = nostrEvent.GetTaggedData("g");
                            if (g == null || g.Length == 0)
                            {
                                continue;
                            }
                            if (!_worldView && g[0] != _geohash)
                            {
                                continue;
                            }

                            string editedContent = content;

                            // ミュートしている時は表示しない
                            if (IsMuted(nostrEvent.PublicKey))
                            {
                                continue;
                            }
                            // pタグにミュートされている公開鍵が含まれている時は表示しない
                            if (nostrEvent.GetTaggedPublicKeys().Any(pk => IsMuted(pk)))
                            {
                                continue;
                            }

                            // ユーザー表示名取得
                            var n = nostrEvent.GetTaggedData("n");
                            if (n != null && 0 < n.Length)
                            {
                                nickname = n[0];

                                // NameMuteに含まれている文字列がnicknameに含まれている時は表示しない
                                if (NameMute.Any(mute => nickname.Contains(mute, StringComparison.OrdinalIgnoreCase)))
                                {
                                    continue;
                                }

                                // ユーザー取得
                                if (user != null)
                                {
                                    user.Nickname = nickname;
                                    user.LastActivity = DateTime.Now;
                                    Tools.SaveUsers(Users);
                                    // 辞書に追加（上書き）
                                    Users[nostrEvent.PublicKey] = user;
                                }
                                else
                                {
                                    var newUserData = new User
                                    {
                                        Nickname = n[0],
                                        LastActivity = DateTime.Now
                                    };
                                    Tools.SaveUsers(Users);
                                    // 辞書に追加（上書き）
                                    Users[nostrEvent.PublicKey] = newUserData;
                                }
                            }
                            else
                            {
                                nickname = GetUserName(nostrEvent.PublicKey);
                            }

                            // tタグ
                            var t = nostrEvent.GetTaggedData("t");

                            // プロフィール購読
                            await NostrAccess.SubscribeProfilesAsync([nostrEvent.PublicKey]);

                            // ユーザー取得
                            user = await GetUserAsync(nostrEvent.PublicKey);

                            bool isReply = false;
                            var e = nostrEvent.GetTaggedData("e");
                            var p = nostrEvent.GetTaggedData("p");
                            var q = nostrEvent.GetTaggedData("q");
                            if (e != null && 0 < e.Length ||
                                p != null && 0 < p.Length ||
                                q != null && 0 < q.Length)
                            {
                                isReply = true;
                                //headMark = "<";

                                if (p != null && 0 < p.Length)
                                {
                                    string mentionedUserNames = string.Empty;
                                    foreach (var u in p)
                                    {
                                        mentionedUserNames = $"{mentionedUserNames} {GetUserName(u)}";
                                    }
                                    editedContent = $"［💬{mentionedUserNames}］\r\n{editedContent}";
                                }
                            }

                            // グリッドに表示
                            //_noteEvents.AddFirst(nostrEvent);
                            DateTimeOffset dto = nostrEvent.CreatedAt ?? DateTimeOffset.Now;
                            if (_descendingOrder)
                            {
                                dataGridViewNotes.Rows.Insert(
                                    0,
                                    dto.ToLocalTime(),
                                    //$"{headMark} {userName}",
                                    nickname,
                                    "#" + nostrEvent.PublicKey[^4..],
                                    editedContent,
                                    g[0],
                                    nostrEvent.Id,
                                    nostrEvent.PublicKey,
                                    nostrEvent.Kind
                                    );
                                //dataGridViewNotes.Sort(dataGridViewNotes.Columns["time"], ListSortDirection.Descending);

                                if (dataGridViewNotes.Rows.Count > 0)
                                {
                                    if (g[0] != _geohash)
                                    {
                                        foreach (DataGridViewCell cell in dataGridViewNotes.Rows[0].Cells)
                                        {
                                            cell.Style.ForeColor = Color.Gray;
                                        }
                                    }

                                    // リプライの時は背景色変更
                                    if (isReply)
                                    {
                                        dataGridViewNotes.Rows[0].DefaultCellStyle.BackColor = Tools.HexToColor(Setting.ReplyColor);
                                    }

                                    if (_autoScroll)
                                    {
                                        // 最上行にスクロール
                                        dataGridViewNotes.FirstDisplayedScrollingRowIndex = 0;

                                    }
                                }
                            }
                            else
                            {
                                dataGridViewNotes.Rows.Add(
                                    dto.ToLocalTime(),
                                    //$"{headMark} {userName}",
                                    nickname,
                                    "#" + nostrEvent.PublicKey[^4..],
                                    editedContent,
                                    g[0],
                                    nostrEvent.Id,
                                    nostrEvent.PublicKey,
                                    nostrEvent.Kind
                                    );

                                if (dataGridViewNotes.Rows.Count > 0)
                                {
                                    if (g[0] != _geohash)
                                    {
                                        foreach (DataGridViewCell cell in dataGridViewNotes.Rows[^1].Cells)
                                        {
                                            cell.Style.ForeColor = Color.Gray;
                                        }
                                    }

                                    // リプライの時は背景色変更
                                    if (isReply)
                                    {
                                        dataGridViewNotes.Rows[^1].DefaultCellStyle.BackColor = Tools.HexToColor(Setting.ReplyColor);
                                    }

                                    if (_autoScroll)
                                    {
                                        // 最下行にスクロール
                                        dataGridViewNotes.FirstDisplayedScrollingRowIndex = dataGridViewNotes.Rows.Count - 1;
                                    }
                                }
                            }

                            // 行を装飾
                            await EditRowAsync(nostrEvent, user, nickname);

                            // SSPに送る
                            if (_sendDSSTP && _ds != null)
                            {
                                NIP19.NostrEventNote nostrEventNote = new()
                                {
                                    EventId = nostrEvent.Id,
                                    Relays = [string.Empty],
                                };
                                var nevent = nostrEventNote.ToNIP19();
                                SearchGhost();

                                //string msg = content;
                                Dictionary<string, string> SSTPHeader = new(_baseSSTPHeader)
                                {
                                    { "Reference1", "1" }, // kind
                                    { "Reference2", content }, // content
                                    { "Reference3", user?.Name ?? "???" }, // name
                                    { "Reference4", user?.DisplayName ?? string.Empty }, // display_name
                                    { "Reference5", user?.Picture ?? string.Empty }, // picture
                                    { "Reference6", nevent }, // nevent1...
                                    { "Reference7", nostrEvent.PublicKey.ConvertToNpub() }, // npub1...
                                    { "Reference8", g[0] }, // g
                                    { "Reference9", n != null && n.Length > 0 && n[0] != null ? n[0] : "" }, // n
                                    { "Reference10", t != null && t.Length > 0 && t[0] != null ? t[0] : "" }, // t
                                    { "Script", $"{speaker}{nickname}\\n{editedContent}\\e" }
                                };
                                string sstpmsg = _SSTPMethod + "\r\n" + string.Join("\r\n", SSTPHeader.Select(kvp => kvp.Key + ": " + kvp.Value.Replace("\n", "\\n"))) + "\r\n\r\n";
                                string r = _ds.GetSSTPResponse(_ghostName, sstpmsg);
                                //Debug.WriteLine(r);
                            }

                            // 改行をスペースに置き換えてログ表示
                            Debug.WriteLine($"{nickname}: {content.Replace('\n', ' ')}");
                        }
                        #endregion
                    }
                }
                #endregion
            }
        }
        #endregion

        #region グリッド行装飾
        private async Task EditRowAsync(NostrEvent nostrEvent, User user, string userName)
        {
            var addIndex = 0;
            if (_descendingOrder)
            {
                addIndex = 0;
            }
            else
            {
                addIndex = dataGridViewNotes.Rows.Count - 1;
            }
            // note列のToolTipにcontentを設定
            dataGridViewNotes.Rows[addIndex].Cells["note"].ToolTipText = nostrEvent.Content;

            //// pubkeyColorを取得
            //var pubkeyColor = Tools.HexToColor(nostrEvent.PublicKey[..6]); // [i..j] で「i番目からj番目の範囲」
            //// geohash列の背景色をpubkeyColorに変更
            //dataGridViewNotes.Rows[addIndex].Cells["geohash"].Style.BackColor = pubkeyColor;
            //// name列の文字色をpubkeyColorに変更
            //dataGridViewNotes.Rows[addIndex].Cells["name"].Style.ForeColor = pubkeyColor;

            // クライアントタグによる背景色変更
            var userClient = nostrEvent.GetTaggedData("client");
            if (userClient != null && 0 < userClient.Length)
            {
                Color clientColor = Color.WhiteSmoke;

                // userClient[0]を_clientsから検索して色を取得
                var client = _clients.FirstOrDefault(c => c.Name == userClient[0]);
                if (client != null && client.ColorCode != null)
                {
                    clientColor = Tools.HexToColor(client.ColorCode);
                }
                // ツールチップにcontentを設定
                dataGridViewNotes.Rows[addIndex].Cells["time"].ToolTipText = userClient[0];
                // time列の背景色をclientColorに変更
                dataGridViewNotes.Rows[addIndex].Cells["time"].Style.BackColor = clientColor;
            }

            // content-warning
            string[]? reason = null;
            try
            {
                reason = nostrEvent.GetTaggedData("content-warning"); // reasonが無いと例外吐く
            }
            catch
            {
                reason = [""];
            }
            if (reason != null && 0 < reason.Length)
            {
                dataGridViewNotes.Rows[addIndex].Cells["note"].Value = "CW: " + reason[0];
                // ツールチップにcontentを設定
                dataGridViewNotes.Rows[addIndex].Cells["note"].ToolTipText = nostrEvent.Content;
                // note列の背景色をCWColorに変更
                dataGridViewNotes.Rows[addIndex].Cells["note"].Style.BackColor = Tools.HexToColor(Setting.CWColor);
            }
        }
        #endregion

        #region ユーザー取得
        private async Task<User?> GetUserAsync(string pubkey)
        {
            User? user = null;
            int retryCount = 0;
            while (retryCount < 10)
            {
                Debug.WriteLine($"retryCount = {retryCount} {GetUserName(pubkey)}");
                Users.TryGetValue(pubkey, out user);
                // ユーザーが見つかった場合、ループを抜ける
                if (user != null)
                {
                    break;
                }
                // 一定時間待機してから再試行
                await Task.Delay(100);
                retryCount++;
            }
            return user;
        }
        #endregion

        #region Stopボタン
        // Stopボタン
        private void ButtonStop_Click(object sender, EventArgs e)
        {
            if (NostrAccess.Clients == null)
            {
                return;
            }

            try
            {
                NostrAccess.CloseSubscriptions();
                labelRelays.Text = "Close subscription.";

                _ = NostrAccess.Clients.Disconnect();
                labelRelays.Text = "Disconnect.";
                NostrAccess.Clients.Dispose();
                NostrAccess.Clients = null;

                buttonStart.Enabled = true;
                buttonStart.Focus();
                buttonStop.Enabled = false;
                _formPostBar.textBoxPost.Enabled = false;
                _formPostBar.buttonPost.Enabled = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Could not stop.";
            }
        }
        #endregion

        #region Postボタン
        // Postボタン
        internal void ButtonPost_Click(NostrEvent? rootEvent)
        {
            if (0 == _formSetting.textBoxNsec.TextLength)
            {
                _formPostBar.textBoxPost.PlaceholderText = "Please set nsec.";
                return;
            }
            if (0 == _formPostBar.textBoxPost.TextLength)
            {
                _formPostBar.textBoxPost.PlaceholderText = "Cannot post empty.";
                return;
            }

            try
            {
                _ = PostAsync(rootEvent);

                _formPostBar.textBoxPost.Text = string.Empty;
                _formPostBar.textBoxPost.PlaceholderText = string.Empty;
                _formPostBar.RootEvent = null;
                // デフォルトの色に戻す
                _formPostBar.textBoxPost.BackColor = SystemColors.Window;
                _formPostBar.buttonPost.BackColor = SystemColors.Control;
                // 送信後にチェックを外す
                checkBoxPostBar.Checked = false;
                dataGridViewNotes.Focus();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _formPostBar.textBoxPost.PlaceholderText = "Could not post.";
            }

            if (checkBoxPostBar.Checked)
            {
                _formPostBar.textBoxPost.Focus();
            }
        }
        #endregion

        #region 投稿処理
        /// <summary>
        /// 投稿処理
        /// </summary>
        /// <returns></returns>
        private async Task PostAsync(NostrEvent? rootEvent = null)
        {
            if (NostrAccess.Clients == null)
            {
                return;
            }
            // create tags
            List<NostrEventTag> tags = [];
            tags.Add(new NostrEventTag() { TagIdentifier = "g", Data = [_geohash] });
            tags.Add(new NostrEventTag() { TagIdentifier = "n", Data = [_nickname] });
            if (rootEvent != null)
            {
                tags.Add(new NostrEventTag() { TagIdentifier = "e", Data = [rootEvent.Id, string.Empty] });
                tags.Add(new NostrEventTag() { TagIdentifier = "p", Data = [rootEvent.PublicKey] });
            }
            if (_addTeleport)
            {
                tags.Add(new NostrEventTag() { TagIdentifier = "t", Data = ["teleport"] });
            }
            if (_addClient)
            {
                tags.Add(new NostrEventTag()
                {
                    TagIdentifier = "client",
                    Data = ["omochat", "31990:21ac29561b5de90cdc21995fc0707525cd78c8a52d87721ab681d3d609d1e2df:1756530676223", "wss://relay.nostr.band"]
                });
            }
            // create a new event
            var newEvent = new NostrEvent()
            {
                Kind = 20000,
                Content = _formPostBar.textBoxPost.Text
                            //.Replace("\\n", "\r\n") // 本体の改行をポストバーのマルチラインに合わせる→廃止
                            .Replace("\r\n", "\n"),
                Tags = tags
            };

            try
            {
                // load from an nsec string
                var key = _nsec.FromNIP19Nsec();
                // sign the event
                await newEvent.ComputeIdAndSignAsync(key);
                // send the event
                await NostrAccess.Clients.SendEventsAndWaitUntilReceived([newEvent], CancellationToken.None);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _formPostBar.textBoxPost.PlaceholderText = "Decryption failed.";
            }
        }
        #endregion

        #region Settingボタン
        // Settingボタン
        private async void ButtonSetting_Click(object sender, EventArgs e)
        {
            // 開く前
            Opacity = _tempOpacity;
            _formSetting.checkBoxTopMost.Checked = TopMost;
            _formSetting.trackBarOpacity.Value = (int)(Opacity * 100);
            _formSetting.checkBoxMinimizeToTray.Checked = _minimizeToTray;
            _formSetting.checkBoxWorldView.Checked = _worldView;
            _formSetting.textBoxGeohash.Text = _geohash;
            _formSetting.checkBoxAddTeleport.Checked = _addTeleport;
            _formSetting.textBoxNickname.Text = _nickname;
            _formSetting.checkBoxAutoScroll.Checked = _autoScroll;
            _formSetting.checkBoxDescendingOrder.Checked = _descendingOrder;
            _formSetting.checkBoxSendDSSTP.Checked = _sendDSSTP;
            _formSetting.checkBoxAddClient.Checked = _addClient;
            _formSetting.textBoxNsec.Text = _nsec;
            _formSetting.textBoxNpub.Text = _nsec.GetNpub();

            // 開く
            _formSetting.ShowDialog(this);

            // 閉じた後
            TopMost = _formSetting.checkBoxTopMost.Checked;
            Opacity = _formSetting.trackBarOpacity.Value / 100.0;
            _tempOpacity = Opacity;
            _formPostBar.Opacity = Opacity;
            _minimizeToTray = _formSetting.checkBoxMinimizeToTray.Checked;
            _worldView = _formSetting.checkBoxWorldView.Checked;
            _geohash = _formSetting.textBoxGeohash.Text;
            _addTeleport = _formSetting.checkBoxAddTeleport.Checked;
            _nickname = _formSetting.textBoxNickname.Text;
            if (string.IsNullOrEmpty(_nickname))
            {
                // ニックネームが空欄の時はランダム6文字にする
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                var random = new Random();
                _nickname = new string(Enumerable.Repeat(chars, 6)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
                _formSetting.textBoxNickname.Text = _nickname;
            }
            notifyIcon.Visible = _minimizeToTray;
            _nsec = _formSetting.textBoxNsec.Text;
            _autoScroll = _formSetting.checkBoxAutoScroll.Checked;
            _descendingOrder = _formSetting.checkBoxDescendingOrder.Checked;
            _sendDSSTP = _formSetting.checkBoxSendDSSTP.Checked;
            _addClient = _formSetting.checkBoxAddClient.Checked;
            try
            {
                // 別アカウントログイン失敗に備えてクリアしておく
                _npubHex = string.Empty;
                _formPostBar.textBoxPost.PlaceholderText = "omochat";
                Text = "omochat";
                notifyIcon.Text = "omochat";

                // 秘密鍵と公開鍵取得
                _npubHex = _nsec.GetNpubHex();

                // ログイン済みの時
                if (!string.IsNullOrEmpty(_npubHex))
                {
                    int connectCount = await NostrAccess.ConnectAsync();

                    toolTipRelays.SetToolTip(labelRelays, string.Join("\n", NostrAccess.RelayStatusList));

                    switch (connectCount)
                    {
                        case 0:
                            labelRelays.Text = "No relay enabled.";
                            return;
                        case 1:
                            labelRelays.Text = $"{connectCount} relay";
                            break;
                        default:
                            labelRelays.Text = $"{connectCount} relays";
                            break;
                    }

                    //// フォロイーを購読をする
                    //await NostrAccess.SubscribeFollowsAsync(_npubHex);

                    // タイトルバーにニックネームとジオハッシュを表示
                    if (!string.IsNullOrEmpty(_nickname))
                    {
                        Text = $"@{_nickname}  #{_geohash}{(_addTeleport ? "📍" : "")} {(_worldView ? "🌐" : "")}";
                        notifyIcon.Text = $"omochat - @{_nickname}  #{_geohash}{(_addTeleport ? "📍" : "")} {(_worldView ? "🌐" : "")}";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Decryption failed.";
            }
            // nsecを保存
            SavePubkey(_npubHex);
            SaveNsec(_npubHex, _nsec);

            Setting.TopMost = TopMost;
            Setting.Opacity = Opacity;
            Setting.MinimizeToTray = _minimizeToTray;
            Setting.WorldView = _worldView;
            Setting.Geohash = _geohash;
            Setting.AddTeleport = _addTeleport;
            Setting.Nickname = _nickname;
            Setting.AutoScroll = _autoScroll;
            Setting.DescendingOrder = _descendingOrder;
            Setting.SendDSSTP = _sendDSSTP;
            Setting.AddClient = _addClient;

            Setting.Save(_configPath);
            _clients = Tools.LoadClients();

            dataGridViewNotes.Focus();
        }
        #endregion

        #region 複数リレーからの処理済みイベントを除外
        /// <summary>
        /// 複数リレーからの処理済みイベントを除外
        /// </summary>
        /// <param name="eventId"></param>
        private bool RemoveCompletedEventIds(string eventId)
        {
            if (_displayedEventIds.Contains(eventId))
            {
                return true;
            }
            if (_displayedEventIds.Count >= 4096)
            {
                _displayedEventIds.RemoveFirst();
            }
            _displayedEventIds.AddLast(eventId);
            return false;
        }
        #endregion

        #region 透明解除処理
        // マウス入った時
        private void Control_MouseEnter(object sender, EventArgs e)
        {
            _tempOpacity = Opacity;
            Opacity = 1.00;
        }

        // マウス出た時
        private void Control_MouseLeave(object sender, EventArgs e)
        {
            Opacity = _tempOpacity;
        }
        #endregion

        #region SSPゴースト名を取得する
        /// <summary>
        /// SSPゴースト名を取得する
        /// </summary>
        private void SearchGhost()
        {
            _ds.Update();
            SakuraFMO fmo = (SakuraFMO)_ds.FMO;
            var names = fmo.GetGhostNames();
            if (names.Length > 0)
            {
                _ghostName = names.First(); // とりあえず先頭で
                                            //Debug.WriteLine(_ghostName);
            }
            else
            {
                _ghostName = string.Empty;
                //Debug.WriteLine("ゴーストがいません");
            }
        }
        #endregion

        #region ユーザー名を取得する
        /// <summary>
        /// ユーザー名を取得する
        /// </summary>
        /// <param name="publicKeyHex">公開鍵HEX</param>
        /// <returns>ユーザー名</returns>
        private string? GetName(string publicKeyHex)
        {
            // 情報があればユーザー名を取得
            Users.TryGetValue(publicKeyHex, out User? user);
            string? userName = string.Empty;
            if (user != null)
            {
                userName = user.Name;
                // 取得日更新
                user.LastActivity = DateTime.Now;
                Tools.SaveUsers(Users);
            }
            return userName;
        }
        #endregion

        #region ユーザー表示名を取得する
        /// <summary>
        /// ユーザー表示名を取得する
        /// </summary>
        /// <param name="publicKeyHex">公開鍵HEX</param>
        /// <returns>ユーザー表示名</returns>
        private string GetUserName(string publicKeyHex)
        {
            // 情報があれば表示名を取得
            Users.TryGetValue(publicKeyHex, out User? user);
            string? userName = "???";
            if (user != null)
            {
                userName = user.DisplayName;
                // display_nameが無い場合は@nameとする
                if (userName == null || string.Empty == userName)
                {
                    userName = $"{user.Name}";
                }
                // nicknameがある場合はnicknameとする
                if (!string.IsNullOrEmpty(user?.Nickname))
                {
                    userName = $"{user.Nickname}";
                }
                // 取得日更新
                user.LastActivity = DateTime.Now;
                Tools.SaveUsers(Users);
                //Debug.WriteLine($"名前取得: {user.DisplayName} @{user.Name} 📛{user.PetName}");
            }
            return userName;
        }
        #endregion

        #region ミュートされているか確認する
        /// <summary>
        /// ミュートされているか確認する
        /// </summary>
        /// <param name="publicKeyHex">公開鍵HEX</param>
        /// <returns>ミュートフラグ</returns>
        private bool IsMuted(string publicKeyHex)
        {
            if (Users.TryGetValue(publicKeyHex, out User? user))
            {
                if (user != null)
                {
                    return user.Mute;
                }
            }
            return false;
        }
        #endregion

        #region 閉じる
        // 閉じる
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_minimizeToTray && !_reallyClose && e.CloseReason == CloseReason.UserClosing)
            {
                // 閉じるボタンが押されたときは最小化
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                Hide(); // フォームを非表示にします（タスクトレイに格納）
            }
            else
            {
                // ホットキーの登録を解除
                UnregisterHotKey(this.Handle, HOTKEY_ID);

                NostrAccess.CloseSubscriptions();
                NostrAccess.DisconnectAndDispose();

                if (FormWindowState.Normal != WindowState)
                {
                    // 最小化最大化状態の時、元の位置と大きさを保存
                    Setting.Location = RestoreBounds.Location;
                    Setting.Size = RestoreBounds.Size;
                }
                else
                {
                    Setting.Location = Location;
                    Setting.Size = Size;
                }
                Setting.PostBarLocation = _formPostBar.Location;
                Setting.PostBarSize = _formPostBar.Size;
                if (FormWindowState.Normal != _formWeb.WindowState)
                {
                    // 最小化最大化状態の時、元の位置と大きさを保存
                    Setting.WebViewLocation = _formWeb.RestoreBounds.Location;
                    Setting.WebViewSize = _formWeb.RestoreBounds.Size;
                }
                else
                {
                    Setting.WebViewLocation = _formWeb.Location;
                    Setting.WebViewSize = _formWeb.Size;
                }
                Setting.NameColumnWidth = dataGridViewNotes.Columns["name"].Width;
                Setting.Save(_configPath);
                Tools.SaveUsers(Users);

                _ds?.Dispose();      // FrmMsgReceiverのThread停止せず1000ms待たされるうえにプロセス残るので…
                Application.Exit(); // ←これで殺す。SSTLibに手を入れた方がいいが、とりあえず。
            }
        }
        #endregion

        #region ロード時
        // ロード時
        private void FormMain_Load(object sender, EventArgs e)
        {
            // Ctrl + Shift + Z をホットキーとして登録
            RegisterHotKey(this.Handle, HOTKEY_ID, MOD_CONTROL | MOD_SHIFT, (int)Keys.Z);

            _formPostBar.ShowDialog();

            try
            {
                _npubHex = LoadPubkey();
                _nsec = LoadNsec();
                _formSetting.textBoxNsec.Text = _nsec;
                _formSetting.textBoxNpub.Text = _nsec.GetNpub();
                if (!string.IsNullOrEmpty(_formSetting.textBoxNpub?.Text))
                {
                    _formSetting.textBoxNsec.Enabled = false;
                }

                dataGridViewNotes.Columns["geohash"].Visible = false;
                dataGridViewNotes.Columns["hash"].Visible = false;

                ButtonStart_Click(sender, e);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                labelRelays.Text = "Decryption failed.";
            }
        }
        #endregion

        #region ポストバー表示切り替え
        // ポストバー表示切り替え
        private void CheckBoxPostBar_CheckedChanged(object sender, EventArgs e)
        {
            _formPostBar.Visible = checkBoxPostBar.Checked;
            if (_formPostBar.Visible)
            {
                _formPostBar.textBoxPost.Focus();
            }
            else
            {
                dataGridViewNotes.Focus();
            }
        }
        #endregion

        #region 画面表示切替
        // 画面表示切替
        private void FormMain_KeyDown(object sender, KeyEventArgs e)
        {
            // ポストバー表示切替
            if (e.KeyCode == Keys.F11 || e.KeyCode == Keys.F12 || e.KeyCode == Keys.F1)
            {
                checkBoxPostBar.Checked = !checkBoxPostBar.Checked;
            }
            // F2キーでtime列の表示切替
            if (e.KeyCode == Keys.F2)
            {
                dataGridViewNotes.Columns["time"].Visible = !dataGridViewNotes.Columns["time"].Visible;
            }
            // F3キーでname列の表示切替
            if (e.KeyCode == Keys.F3)
            {
                dataGridViewNotes.Columns["name"].Visible = !dataGridViewNotes.Columns["name"].Visible;
            }
            // F4キーでhash列の表示切替
            if (e.KeyCode == Keys.F4)
            {
                dataGridViewNotes.Columns["hash"].Visible = !dataGridViewNotes.Columns["hash"].Visible;
            }
            // F5キーでgeohash列の表示切替
            if (e.KeyCode == Keys.F5)
            {
                dataGridViewNotes.Columns["geohash"].Visible = !dataGridViewNotes.Columns["geohash"].Visible;
            }

            if (e.KeyCode == Keys.Escape)
            {
                ButtonSetting_Click(sender, e);
            }

            if (e.KeyCode == Keys.F10)
            {
                var ev = new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0);
                FormMain_MouseClick(sender, ev);
            }

            if (e.KeyCode == Keys.F9)
            {
                var ev = new MouseEventArgs(MouseButtons.Left, 2, 0, 0, 0);
                FormMain_MouseDoubleClick(sender, ev);
            }
        }
        #endregion

        #region マニアクス表示
        private void FormMain_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (_formManiacs == null || _formManiacs.IsDisposed)
                {
                    _formManiacs = new FormManiacs
                    {
                        MainForm = this
                    };
                }
                if (!_formManiacs.Visible)
                {
                    _formManiacs.Show(this);
                }
            }
        }
        #endregion

        #region リレーリスト表示
        private void ButtonRelayList_Click(object sender, EventArgs e)
        {
            _formRelayList = new FormRelayList();
            if (_formRelayList.ShowDialog(this) == DialogResult.OK)
            {
                ButtonStop_Click(sender, e);
                ButtonStart_Click(sender, e);
            }
            _formRelayList.Dispose();
            dataGridViewNotes.Focus();
        }
        #endregion

        #region セルダブルクリック
        private void DataGridViewNotes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            // マウスカーソルがデフォルトじゃない時は無視
            if (Cursor.Current != Cursors.Default)
            {
                return;
            }
            // ヘッダー行がダブルクリックされた場合は無視
            if (e.RowIndex < 0)
            {
                return;
            }
            DataGridViewRow selectedRow = dataGridViewNotes.Rows[e.RowIndex];
            string name = (string)selectedRow.Cells["name"].Value;
            string hash = (string)selectedRow.Cells["hash"].Value;

            if (!checkBoxPostBar.Checked)
            {
                checkBoxPostBar.Checked = true;
            }
            _formPostBar.Focus();
            _formPostBar.textBoxPost.Text = $"@{name}{hash} ";
            // キャレットを末尾に移動
            _formPostBar.textBoxPost.SelectionStart = _formPostBar.textBoxPost.TextLength;
        }
        #endregion

        #region グリッドキー入力
        private void DataGridViewNotes_KeyDown(object sender, KeyEventArgs e)
        {
            // Cキーで_formWebを閉じる
            if (e.KeyCode == Keys.C)
            {
                if (_formWeb != null && !_formWeb.IsDisposed)
                {
                    _formWeb.Close();
                }
            }
            // Wキーで選択行を上に
            if (e.KeyCode == Keys.W)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index > 0)
                {
                    dataGridViewNotes.Rows[dataGridViewNotes.SelectedRows[0].Index - 1].Selected = true;
                    dataGridViewNotes.CurrentCell = dataGridViewNotes["note", dataGridViewNotes.SelectedRows[0].Index];
                }
            }
            // Sキーで選択行を下に
            if (e.KeyCode == Keys.S)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index < dataGridViewNotes.Rows.Count - 1)
                {
                    dataGridViewNotes.Rows[dataGridViewNotes.SelectedRows[0].Index + 1].Selected = true;
                    dataGridViewNotes.CurrentCell = dataGridViewNotes["note", dataGridViewNotes.SelectedRows[0].Index];
                }
            }
            // Shift + Wキーで選択行を最上部に
            if (e.KeyCode == Keys.W && e.Shift)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index > 0)
                {
                    dataGridViewNotes.Rows[0].Selected = true;
                    dataGridViewNotes.CurrentCell = dataGridViewNotes["note", 0];
                }
            }
            // Shift + Sキーで選択行を最下部に
            if (e.KeyCode == Keys.S && e.Shift)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index < dataGridViewNotes.Rows.Count - 1)
                {
                    dataGridViewNotes.Rows[^1].Selected = true; // インデックス演算子 [^i] で「後ろからi番目の要素」
                    dataGridViewNotes.CurrentCell = dataGridViewNotes["note", dataGridViewNotes.Rows.Count - 1];
                }
            }
            // メンション入力
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.F)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index >= 0)
                {
                    // 画面外に出た時サイズ変更用カーソルを記憶しているのでデフォルトに戻す
                    Cursor.Current = Cursors.Default;
                    var ev = new DataGridViewCellEventArgs(3, dataGridViewNotes.SelectedRows[0].Index);
                    DataGridViewNotes_CellDoubleClick(sender, ev);
                }
            }
            // Webビュー表示
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index >= 0)
                {
                    var mev = new MouseEventArgs(MouseButtons.Right, 1, 0, 0, 0);
                    var ev = new DataGridViewCellMouseEventArgs(0, dataGridViewNotes.SelectedRows[0].Index, 0, 0, mev);
                    DataGridViewNotes_CellMouseClick(sender, ev);
                }
            }
            // DキーでGoogle翻訳
            if (e.KeyCode == Keys.D)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index >= 0)
                {
                    var note = (string)dataGridViewNotes.Rows[dataGridViewNotes.SelectedRows[0].Index].Cells["note"].Value;
                    var systemLang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                    //var url = $"https://translate.google.com/?sl=auto&tl={systemLang}&text=" + Uri.EscapeDataString(note) + "&op=translate";
                    var url = $"https://translate.google.com/m?sl=auto&tl={systemLang}&q=" + Uri.EscapeDataString(note);

                    if (_formWeb == null || _formWeb.IsDisposed)
                    {
                        _formWeb = new FormWeb
                        {
                            Location = _formWebLocation,
                            Size = _formWebSize
                        };
                    }
                    if (!_formWeb.Visible)
                    {
                        _formWeb.Location = _formWebLocation;
                        _formWeb.Size = _formWebSize;
                        _formWeb.Show(this);
                    }
                    if (_formWeb.WindowState == FormWindowState.Minimized)
                    {
                        _formWeb.WindowState = FormWindowState.Normal;
                    }
                    Setting.WebViewLocation = _formWeb.Location;
                    Setting.WebViewSize = _formWeb.Size;

                    try
                    {
                        _ = _formWeb.NavigateAndSimplifyAsync(url);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        _formWeb.Close();
                    }
                    Focus();
                }
            }
            // Rキーで返信
            if (e.KeyCode == Keys.R)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index >= 0)
                {
                    var rootEvent = new NostrEvent()
                    {
                        Id = (string)dataGridViewNotes.Rows[dataGridViewNotes.SelectedRows[0].Index].Cells["id"].Value,
                        PublicKey = (string)dataGridViewNotes.Rows[dataGridViewNotes.SelectedRows[0].Index].Cells["pubkey"].Value
                    };
                    _formPostBar.RootEvent = rootEvent;
                    _formPostBar.textBoxPost.PlaceholderText = $"Reply to {GetUserName(rootEvent.PublicKey)}";
                    if (!checkBoxPostBar.Checked)
                    {
                        checkBoxPostBar.Checked = true;
                    }
                    _formPostBar.Focus();
                }
            }
            // Hキーでhug
            if (e.KeyCode == Keys.H)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index >= 0)
                {
                    var name = (string)dataGridViewNotes.Rows[dataGridViewNotes.SelectedRows[0].Index].Cells["name"].Value;
                    var hash = (string)dataGridViewNotes.Rows[dataGridViewNotes.SelectedRows[0].Index].Cells["hash"].Value;
                    _formPostBar.textBoxPost.Text = $"* 🫂 {_nickname} hugs {name}{hash} *";

                    _ = PostAsync();

                    _formPostBar.textBoxPost.Text = string.Empty;
                    _formPostBar.textBoxPost.PlaceholderText = string.Empty;
                    _formPostBar.RootEvent = null;
                    // デフォルトの色に戻す
                    _formPostBar.textBoxPost.BackColor = SystemColors.Window;
                    _formPostBar.buttonPost.BackColor = SystemColors.Control;
                }
            }
            // Tキーでslap
            if (e.KeyCode == Keys.T)
            {
                if (dataGridViewNotes.SelectedRows.Count > 0 && dataGridViewNotes.SelectedRows[0].Index >= 0)
                {
                    var name = (string)dataGridViewNotes.Rows[dataGridViewNotes.SelectedRows[0].Index].Cells["name"].Value;
                    var hash = (string)dataGridViewNotes.Rows[dataGridViewNotes.SelectedRows[0].Index].Cells["hash"].Value;
                    _formPostBar.textBoxPost.Text = $"* 🐟 {_nickname} slaps {name}{hash} around a bit with a large trout *";

                    _ = PostAsync();

                    _formPostBar.textBoxPost.Text = string.Empty;
                    _formPostBar.textBoxPost.PlaceholderText = string.Empty;
                    _formPostBar.RootEvent = null;
                    // デフォルトの色に戻す
                    _formPostBar.textBoxPost.BackColor = SystemColors.Window;
                    _formPostBar.buttonPost.BackColor = SystemColors.Control;
                }
            }

            // Zキーでnote列の折り返し切り替え
            if (e.KeyCode == Keys.Z)
            {
                var ev = new MouseEventArgs(MouseButtons.Left, 2, 0, 0, 0);
                FormMain_MouseDoubleClick(sender, ev);
            }
        }
        #endregion

        #region フォームマウスダブルクリック
        private void FormMain_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (dataGridViewNotes.Columns["note"].DefaultCellStyle.WrapMode != DataGridViewTriState.True)
            {
                dataGridViewNotes.Columns["note"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }
            else
            {
                dataGridViewNotes.Columns["note"].DefaultCellStyle.WrapMode = DataGridViewTriState.NotSet;
            }
        }
        #endregion

        #region セル右クリック
        private void DataGridViewNotes_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                dataGridViewNotes.Rows[e.RowIndex].Selected = true;
                dataGridViewNotes.Rows[e.RowIndex].Cells["note"].Selected = true;

                if (_formWeb == null || _formWeb.IsDisposed)
                {
                    _formWeb = new FormWeb
                    {
                        Location = _formWebLocation,
                        Size = _formWebSize
                    };
                }
                if (!_formWeb.Visible)
                {
                    _formWeb.Location = _formWebLocation;
                    _formWeb.Size = _formWebSize;
                    _formWeb.Show(this);
                }
                if (_formWeb.WindowState == FormWindowState.Minimized)
                {
                    _formWeb.WindowState = FormWindowState.Normal;
                }
                Setting.WebViewLocation = _formWeb.Location;
                Setting.WebViewSize = _formWeb.Size;

                var id = dataGridViewNotes.Rows[e.RowIndex].Cells["id"].Value.ToString() ?? string.Empty;
                NIP19.NostrEventNote nostrEventNote = new()
                {
                    EventId = id,
                    //Relays = [string.Empty],
                    Relays = [],
                };
                var nevent = nostrEventNote.ToNIP19();
                try
                {
                    _formWeb.webView2.Source = new Uri(Setting.WebViewUrl + nevent);

                    //var app = new ProcessStartInfo
                    //{
                    //    FileName = Setting.WebViewUrl + nevent,
                    //    UseShellExecute = true
                    //};
                    //Process.Start(app);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    _formWeb.Close();
                }
                Focus();
            }
        }
        #endregion

        #region フォーム最初の表示時
        private void FormMain_Shown(object sender, EventArgs e)
        {
            dataGridViewNotes.Focus();
        }
        #endregion

        #region パスワード管理
        private static void SavePubkey(string pubkey)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("pubkey");
            config.AppSettings.Settings.Add("pubkey", pubkey);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static string LoadPubkey()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            return config.AppSettings.Settings["pubkey"]?.Value ?? string.Empty;
        }

        private static void SaveNsec(string pubkey, string nsec)
        {
            // 前回のトークンを削除
            DeletePreviousTarget();

            // 新しいtargetを生成して保存
            string target = Guid.NewGuid().ToString();
            Tools.SavePassword("omochat_" + target, pubkey, nsec);
            SaveTarget(target);
        }

        private static void SaveTarget(string target)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove("target");
            config.AppSettings.Settings.Add("target", target);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static void DeletePreviousTarget()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var previousTarget = config.AppSettings.Settings["target"]?.Value;
            if (!string.IsNullOrEmpty(previousTarget))
            {
                Tools.DeletePassword("omochat_" + previousTarget);
                config.AppSettings.Settings.Remove("target");
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
        }

        private static string LoadNsec()
        {
            string target = LoadTarget();
            if (!string.IsNullOrEmpty(target))
            {
                return Tools.LoadPassword("omochat_" + target);
            }
            return string.Empty;
        }

        private static string LoadTarget()
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            return config.AppSettings.Settings["target"]?.Value ?? string.Empty;
        }
        #endregion

        #region グローバルホットキー
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                this.Activate(); // FormMainをアクティブにする
            }
            base.WndProc(ref m);
        }
        #endregion

        #region タスクトレイ最小化
        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            // 右クリック時は抜ける
            if (e is MouseEventArgs me && me.Button == MouseButtons.Right)
            {
                return;
            }

            // 最小化時は通常表示に戻す
            if (WindowState == FormWindowState.Minimized)
            {
                Show();
                WindowState = FormWindowState.Normal;
            }
            else if (WindowState == FormWindowState.Normal)
            {
                WindowState = FormWindowState.Minimized;
            }
        }

        private void SettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // 設定画面がすでに開かれている場合は抜ける
            if (_formSetting.Visible)
            {
                return;
            }
            Show();
            WindowState = FormWindowState.Normal;
            ButtonSetting_Click(sender, e);
        }

        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _reallyClose = true;
            Close();
        }

        private void FormMain_SizeChanged(object sender, EventArgs e)
        {
            // 最小化時はタスクトレイに格納
            if (_minimizeToTray && WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }
        #endregion
    }
}
