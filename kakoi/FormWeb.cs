using System.Diagnostics;

namespace omochat
{
    public partial class FormWeb : Form
    {
        private bool _simplifyScriptRegistered;

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
        /// 指定 URL を読み込んで翻訳結果 (.result-container) 以外を非表示にする。
        /// </summary>
        public async Task NavigateAndSimplifyAsync(string url)
        {
            try
            {
                if (webView2.CoreWebView2 == null)
                {
                    await webView2.EnsureCoreWebView2Async();
                }

                // CoreWebView2 が null の場合は処理しない
                if (webView2.CoreWebView2 == null)
                {
                    Debug.WriteLine("CoreWebView2 is still null after initialization.");
                    return;
                }

                // スクリプト（1度だけ登録）
                if (!_simplifyScriptRegistered)
                {
                    var script = @"
        (() => {
          const install = () => {
            // CSS で結果以外を一括非表示
            const css = `
              body { margin:12px !important; background:#fff !important;
                     font: 16px/1.55 -apple-system, BlinkMacSystemFont, 'Segoe UI', 'Helvetica Neue', Arial, 'Noto Sans JP', sans-serif; }
              body > * { display:none !important; }
              body > .result-container {
                display:block !important;
                white-space:pre-wrap;
                word-break:break-word;
                font-size:1.05rem;
              }
            `;
            if (!document.getElementById('__only_result_style__')) {
              const st = document.createElement('style');
              st.id = '__only_result_style__';
              st.textContent = css;
              document.head ? document.head.appendChild(st) : document.documentElement.appendChild(st);
            }

            const extract = () => {
              const r = document.querySelector('.result-container');
              if (!r) {
                // 再試行
                setTimeout(extract, 200);
                return;
              }
              // 不要な子要素が追加された場合のクリーンアップ（念のため）
              const clean = () => {
                r.querySelectorAll('form, input, button, a').forEach(e => e.remove());
              };
              clean();
              // 変化監視（再翻訳時にも維持）
              const mo = new MutationObserver(() => clean());
              mo.observe(r, { childList:true, subtree:true });
              window.scrollTo(0, 0);
            };
            extract();
          };

          // Google 翻訳ページ以外は何もしない軽い判定（必要に応じて調整）
          try {
            const h = location.hostname;
            if (!/translate\.google\./.test(h)) return;
          } catch { return; }

          if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', install, { once:true });
          } else {
            install();
          }
        })();
        ";
                    await webView2.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(script);
                    _simplifyScriptRegistered = true;
                }

                webView2.CoreWebView2.Navigate(url);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
