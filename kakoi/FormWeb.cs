using Microsoft.Web.WebView2.Core;
using System.Diagnostics;

namespace omochat
{
    public partial class FormWeb : Form
    {
        public FormWeb()
        {
            InitializeComponent();
        }

        private void FormWeb_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Owner == null)
            {
                return;
            }

            var mainForm = (FormMain)Owner;
            if (FormWindowState.Normal != WindowState)
            {
                // 最小化最大化状態の時、元の位置と大きさを保存
                mainForm._formWebLocation = RestoreBounds.Location;
                mainForm._formWebSize = RestoreBounds.Size;
            }
            else
            {
                mainForm._formWebLocation = Location;
                mainForm._formWebSize = Size;
            }
        }

        /// <summary>
        /// 指定 URL を読み込んで、翻訳表示以外の部分を非表示にするスクリプトを注入する。
        /// </summary>
        public async Task NavigateAndSimplifyAsync(string url)
        {
            try
            {
                // Core がまだ準備できていなければ初期化
                if (webView2.CoreWebView2 == null)
                {
                    await webView2.EnsureCoreWebView2Async();
                }

                // ナビゲート
                webView2.CoreWebView2.Navigate(url);

                // ナビゲーション完了時に一度だけスクリプトを注入
                void Handler(object? s, CoreWebView2NavigationCompletedEventArgs args)
                {
                    webView2.NavigationCompleted -= Handler;

                    // ページ上のヘッダー／フッター等を非表示にする CSS を追加
                    // 必要に応じてセレクタを調整してください
                    var script = @"
(function(){
  try {
    const css = `
      header, footer, [role=banner], [role=navigation],
      .gb_uc, .gb_2c, .gb_ucc, .yDmH0d, .k6, .m-header, .appbar, .topbar,
      .widget-consent, .cookie-consent, .widget, .HjZz4d { display: none !important; }
      body { margin: 8px !important; background: #fff !important; }
    `;
    const style = document.createElement('style');
    style.type = 'text/css';
    style.appendChild(document.createTextNode(css));
    document.head && document.head.appendChild(style);

    // モバイル版 (/m) の場合、余計な要素をさらに隠す処理
    try {
      document.querySelectorAll('[role=heading], .header, .top, .nav').forEach(e=>e.style.display='none');
    } catch(e){}

    // 余計なバナー等がまだ残っていれば attempt to close/hide
    try {
      document.querySelectorAll('[aria-label*=close], .close, .consent, .cookie').forEach(e=>e.style.display='none');
    } catch(e){}

    // スクロール位置を先頭に（視覚的調整）
    window.scrollTo(0,0);
  } catch(err){}
})();
";
                    _ = webView2.ExecuteScriptAsync(script);
                }

                webView2.NavigationCompleted += Handler;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
