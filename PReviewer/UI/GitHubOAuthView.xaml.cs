using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Cyotek.ApplicationServices.Windows.Forms;
using PReviewer.Domain;

namespace PReviewer.UI
{
    /// <summary>
    /// </summary>
    public partial class GitHubOAuthView : Window
    {
        private readonly IGitHubClientFactory _gitHubClientFactory;
        private const string ClientId = "bf1e8f76de041537f93e";
        private const string ClientSecret = "bfe6a01cebe6b96455dedc5572aefcfe7033c9bb";
        private const string AuthUrl = "https://github.com/login/oauth/authorize?client_id=" + ClientId + "&scope=repo,public_repo,user:email";

        public string Token { get; private set; }
        public string UserName { get; private set; }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(nameof(IsLoading),
            typeof(bool),
            typeof(GitHubOAuthView), new PropertyMetadata(false));

        public bool IsLoading
        {
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

        public GitHubOAuthView(IGitHubClientFactory gitHubClientFactory)
        {
            _gitHubClientFactory = gitHubClientFactory;

            InitializeComponent();

            this.Loaded += (sender, args) =>
            {
                if (!InternetExplorerBrowserEmulation.IsBrowserEmulationSet())
                {
                    InternetExplorerBrowserEmulation.SetBrowserEmulationVersion();
                }

                IsLoading = true;
                WebBrowser.Navigate(AuthUrl);
            };
        }

        private static void DeleteCookie()
        {
            var dir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.Cookies));
            foreach (
                var file in
                    dir.EnumerateFiles("*.*", SearchOption.TopDirectoryOnly).OrderByDescending(r => r.LastWriteTime))
            {
                using (var reader = new StreamReader(file.FullName))
                {
                    var content = reader.ReadToEnd();
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        continue;
                    }
                    if (!content.Contains("github.com")) continue;
                    reader.Dispose();
                    file.Delete();
                    return;
                }
            }
        }

        private async void WebBrowser_OnNavigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            IsLoading = true;
            await TryParseAuthResult(e.Url);
        }

        private async Task TryParseAuthResult(Uri uri)
        {
            if (uri == null)
            {
                return;
            }
            if (!string.IsNullOrWhiteSpace(Token))
            {
                return;
            }
            var retUrl = uri.ToString();
            var retCodeQueryParam = "?code=";
            if (!retUrl.Contains(retCodeQueryParam)) return;
            WebBrowser.Stop();
            WebBrowser.Visible = false;

            var code = retUrl.Substring(retUrl
                .IndexOf(retCodeQueryParam, StringComparison.InvariantCulture) + retCodeQueryParam.Length);

            var ret = await _gitHubClientFactory.RecreateClient(ClientId, ClientSecret, code);
            var token = ret?.Item2;
            if (token == null)
                return;

            var client = ret.Item1;
            var curUser = await client.User.Current();
            UserName = curUser.Login;

            Token = token;
            DeleteCookie();
            Close();
        }

        private async void WebBrowser_OnNavigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            await TryParseAuthResult(e.Url);
        }

        private void WebBrowser_OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            IsLoading = false;
        }
    }
}
