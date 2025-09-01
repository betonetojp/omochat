using System.Diagnostics;

namespace omochat
{
    public partial class FormWeb : Form
    {
        private bool _simplifyScriptRegistered;
        private bool _webMessageHooked;

        private const string SimplifiedMessage = "simplified";

        public FormWeb()
        {
            InitializeComponent();
        }

        private void FormWeb_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Owner == null) return;

            var mainForm = (FormMain)Owner;
            if (FormWindowState.Normal != WindowState)
            {
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
        /// 指定URLを読み込み 翻訳結果(.result-container)のみ表示。処理完了まで元画面を一切見せない。
        /// </summary>
        public async Task NavigateAndSimplifyAsync(string url)
        {
            try
            {
                if (webView2.CoreWebView2 == null)
                {
                    await webView2.EnsureCoreWebView2Async();
                }

                var core = webView2.CoreWebView2;

                // WebMessage hook (一度だけ)
                if (!_webMessageHooked)
                {
                    core.WebMessageReceived += (s, e) =>
                    {
                        try
                        {
                            if (e.TryGetWebMessageAsString() == SimplifiedMessage)
                            {
                                // 準備できたら表示
                                if (!webView2.Visible)
                                {
                                    webView2.Visible = true;
                                }
                            }
                        }
                        catch { }
                    };
                    _webMessageHooked = true;
                }

                // 初期スクリプト（早期に全体非表示）と本体スクリプト（結果整形）を一度だけ登録
                if (!_simplifyScriptRegistered)
                {
                    // 1. 極小スクリプト: 即座に不可視化
                    await core.AddScriptToExecuteOnDocumentCreatedAsync(@"
try { document.documentElement.style.visibility='hidden'; } catch(_) {}
");

                    // 2. 本体
                    var script = @"
(() => {
  // Google翻訳モバイル or translate? 判定軽め
  try {
    const h = location.hostname;
    if (!/translate\.google\./.test(h)) {
      document.documentElement.style.visibility='visible';
      window.chrome?.webview?.postMessage('" + SimplifiedMessage + @"');
      return;
    }
  } catch {
    window.chrome?.webview?.postMessage('" + SimplifiedMessage + @"');
    return;
  }

  const install = () => {
    try {
      // CSS 注入: 結果のみ残す
      const css = `
        html,body{ background:#fff !important; margin:0 !important; padding:12px !important;
          font:16px/1.55 -apple-system,BlinkMacSystemFont,'Segoe UI','Helvetica Neue',Arial,'Noto Sans JP',sans-serif; }
        body > * { display:none !important; }
        body > .result-container {
          display:block !important;
          white-space:pre-wrap;
          word-break:break-word;
          font-size:1.05rem;
        }
      `;
      if(!document.getElementById('__only_result_style__')){
        const st = document.createElement('style');
        st.id='__only_result_style__';
        st.textContent = css;
        (document.head||document.documentElement).appendChild(st);
      }

      const expose = () => {
        // 最後に表示
        document.documentElement.style.visibility='visible';
        window.chrome?.webview?.postMessage('" + SimplifiedMessage + @"');
      };

      const waitResult = () => {
        const r = document.querySelector('.result-container');
        if (!r) { setTimeout(waitResult, 120); return; }

        // 不要な子要素のクリーニング（再翻訳で挿入されても維持）
        const clean = () => {
          r.querySelectorAll('form, input, button, a').forEach(e => e.remove());
        };
        clean();
        const mo = new MutationObserver(() => clean());
        mo.observe(r, { childList:true, subtree:true });

        window.scrollTo(0,0);
        expose();
      };
      waitResult();
    } catch {
      // 失敗しても露出だけはする
      try { document.documentElement.style.visibility='visible'; } catch{}
      window.chrome?.webview?.postMessage('" + SimplifiedMessage + @"');
    }
  };

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', install, { once:true });
  } else {
    install();
  }
})();
";
                    await core.AddScriptToExecuteOnDocumentCreatedAsync(script);
                    _simplifyScriptRegistered = true;
                }

                // 再ナビゲート前に不可視化
                webView2.Visible = false;

                core.Navigate(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                // エラー時は表示を戻しておく
                if (!webView2.Visible) webView2.Visible = true;
            }
        }
    }
}
