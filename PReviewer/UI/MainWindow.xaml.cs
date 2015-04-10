using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using ExtendedCL;
using GalaSoft.MvvmLight.CommandWpf;
using Mantin.Controls.Wpf.Notification;
using MarkdownSharp;
using Microsoft.Win32;
using Octokit;
using PReviewer.Domain;
using PReviewer.Model;
using WpfCommon.Controls;
using WpfCommon.Utils;
using Clipboard = System.Windows.Clipboard;
using Control = System.Windows.Controls.Control;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace PReviewer.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowVm _viewModel;
        private Window _previewWnd;
        private MarkdownView _previewBrowser;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as MainWindowVm;
            Loaded += OnLoaded;
            DiffViewer.TextChanged += DiffViewerOnTextChanged;
            SetupWindowClosingActions();
            _viewModel.PropertyChanged += OnPrDescriptionChanged;
            TxtPrDescription.Navigating += WebBrowser_OnNavigating;

            InputManager.Current.PreNotifyInput += ProcessF5RefreshHotKey;
        }

        private void ProcessF5RefreshHotKey(object sender, NotifyInputEventArgs e)
        {
            if (OwnedWindows.Count != 0)
            {
                return;
            }
            if (e.StagingItem.Input.RoutedEvent != Keyboard.KeyDownEvent)
                return;

            var args = e.StagingItem.Input as KeyEventArgs;
            if (args == null || args.Key != Key.F5)
            {
                return;
            }
            args.Handled = true;
            if (ShowChangesCmd.CanExecute(null))
            {
                ShowChangesCmd.Execute(null);
            }
        }

        void OnPrDescriptionChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != PropertyName.Get((MainWindowVm x) => x.PrDescription))
            {
                return;
            }
            
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                var html = _viewModel.PrDescription;
                TxtPrDescription.DocumentText = MarkdownToHtml(html);
            }));

        }

        private static string MarkdownToHtml(string mdText)
        {
            var md = new Markdown();
            // replace \r\n with \r\n\r\n as required by the Transformer to generate <ul><li>
            var transformed = md.Transform(mdText.Replace("\r\n", "\r\n\r\n"));
            var text = "<html><body>" + transformed.Replace("\n", "") + "</body></html>";
            return text.Replace("<p>", "<p style='line-height:15%'>")
                .Replace("<code>", "<code style='background-color:#e0eaf1'>");
        }

        private void WebBrowser_OnNavigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            if (!e.Url.ToString().StartsWith("about:blank"))
            {
                e.Cancel = true;
                Process.Start(e.Url.ToString());
            }
        }

        private void DiffViewerOnTextChanged(object sender, EventArgs eventArgs)
        {
            var markerStrategy = DiffViewer.Document.MarkerStrategy;
            markerStrategy.RemoveAll(m => true);

            if (DiffViewer.Text == "") return;
            new PatchHighlighter(DiffViewer.Document).Highlight();
        }

        private void SetupWindowClosingActions()
        {
            this.Closed += async (sender, arg) => await _viewModel.SaveComments();
            SystemEvents.SessionEnding += async (sender, arg) => await _viewModel.SaveComments();
        }

        public ICommand ShowChangesCmd
        {
            get { return new RelayCommand(ShowChanges); }
        }

        public ICommand ShowSettingsCmd
        {
            get { return new RelayCommand(ShowSettings); }
        }

        public ICommand SubmitCommentsCmd
        {
            get { return new RelayCommand(SubmitComments); }
        }

        public ICommand AboutCmd
        {
            get
            {
                return new RelayCommand(() =>
                {
                    var licMarkdown = File.ReadAllText(Path.Combine(PathHelper.ProcessDir, "LICENSE.md"));
                    var markdown = new MarkdownSharp.Markdown();
                    var html = "<html><body>" + markdown.Transform(licMarkdown) + "</body></html>";
                    var dlg = new WpfCommon.Controls.AboutDialog
                    {
                        Owner = this,
                        HtmlDescription = html,
                    };
                    dlg.ShowDialog();
                });
            }
        }

        public ICommand OpenInBrowserCmd
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        if (_viewModel.IsUrlMode)
                        {
                            if (Regex.IsMatch(_viewModel.PullRequestUrl, @"^https://|^http://", RegexOptions.IgnoreCase))
                            {
                                Process.Start(_viewModel.PullRequestUrl);
                            }
                            else
                            {
                                Process.Start("https://" + _viewModel.PullRequestUrl);
                            }
                        }
                        else
                        {
                            Process.Start(_viewModel.PullRequestLocator.ToUrl());
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBoxHelper.ShowError(this, "Unable to open, please check you pull request URL.\r\n\r\n" + ex);
                    }
                });
            }
        }

        public ICommand ClearCommentsCmd
        {
            get { return new RelayCommand(ClearComments); }
        }

        private async void ClearComments()
        {
            if (MessageBoxHelper.ShowConfirmation(this, "Are you sure to clear all the comments locally.") ==
                MessageBoxResult.No)
            {
                return;
            }

            try
            {
                await _viewModel.ClearComments();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(this, "Unable to delete local comments.\r\n\r\n" + ex);
            }
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            TxtPullRequest.Focus();

            await _viewModel.LoadRepoHistory();
        }

        private async void SubmitComments()
        {
            if (!_viewModel.HasComments())
            {
                MessageBoxHelper.ShowError(this, "You haven't commented on anything yet.");
                return;
            }
            var choice = MessageBoxHelper.ShowConfirmation(this, "Are you sure to submit?");
            if (choice != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await _viewModel.SubmitComments();
            }
            catch (Exception exception)
            {
                MessageBoxHelper.ShowError(this, "Unable to comment.\r\n\r\n" + exception);
            }
        }

        private void ShowSettings()
        {
            var settings = new CompareToolSettings {Owner = this};
            settings.ShowDialog();
        }

        private async void ShowChanges()
        {
            if (!this.ValidateAutoCompleteBoxes())
            {
                return;
            }
            if (!this.ValidateTextBoxes())
            {
                return;
            }

            try
            {
                await _viewModel.RetrieveDiffs();
            }
            catch (NotFoundException)
            {
                MessageBoxHelper.ShowError(this,
                    "Unable to find the pull request.\r\nEither the information you provided is incorrect or you don't have permission to view the pull request.");
            }

            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(this, "Unable to get changes.\r\n" + ex);
            }
        }

        private async void OnFileDoubleClicked(object sender, MouseButtonEventArgs e)
        {
            await OpenInDiffTool(sender);
        }

        private async Task OpenInDiffTool(object sender)
        {
            if (_viewModel.SelectedDiffFile == null)
            {
                return;
            }
            if (_viewModel.SelectedDiffFile.GitHubCommitFile.Status == GitFileStatus.Renamed
                && _viewModel.SelectedDiffFile.IsTheSameAsOrgFile())
            {
                var ballon = new Balloon(sender as Control, "No changes found. It's just renamed",
                    BalloonType.Information)
                {
                    ShowCloseButton = true
                };
                ballon.Show();
                _viewModel.SelectedDiffFile.ReviewStatus = ReviewStatus.Reviewed;
                return;
            }
            if (_viewModel.SelectedDiffFile.GitHubCommitFile.Status == GitFileStatus.Removed)
            {
                var ballon = new Balloon(sender as Control, "This file has been deleted in the pull request.",
                    BalloonType.Information)
                {
                    ShowCloseButton = true
                };
                ballon.Show();
                _viewModel.SelectedDiffFile.ReviewStatus = ReviewStatus.Reviewed;
                return;
            }
            try
            {
                await _viewModel.PrepareDiffContent();
            }
            catch (InvalidDiffToolSettings)
            {
                MessageBoxHelper.ShowError(this, "Unable to launch the diff tool.\r\nPlease check your settings.");
                ShowSettingsCmd.Execute(null);
            }
            catch (FailedToSaveContent ex)
            {
                MessageBoxHelper.ShowError(this, "Unable to save content.\r\n\r\n" + ex);
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(this, "Unable to get diff content.\r\n\r\n" + ex);
            }
        }

        private void OnBtnMenuClicked(object sender, RoutedEventArgs e)
        {
            MainMenu.Margin = new Thickness(0, BtnMenu.ActualHeight + 4, 0, 0);
            MainMenu.Visibility = MainMenu.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private void OnMenuClicked(object sender, MouseButtonEventArgs e)
        {
            MainMenu.Visibility = Visibility.Collapsed;
        }
        
        private void OnPreviewBtnReviewStatusDbClicked(object sender, MouseButtonEventArgs e)
        {
            var item = sender as FrameworkElement;
            if (item != null)
            {
                _viewModel.SelectedDiffFile = item.DataContext as CommitFileVm;
            }
            e.Handled = true;
        }

        private void OnPreviewBtnReviewStatusMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as FrameworkElement;
            if (item != null)
            {
                _viewModel.SelectedDiffFile = item.DataContext as CommitFileVm;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void DiffListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewModel.SelectedDiffFile == null
                ||_viewModel.SelectedDiffFile.GitHubCommitFile == null
                ||string.IsNullOrWhiteSpace(_viewModel.SelectedDiffFile.GitHubCommitFile.Patch))
            {
                DiffViewer.Text = "";
                DiffViewer.Refresh();
                return;
            }

            DiffViewer.Text = _viewModel.SelectedDiffFile.GitHubCommitFile.Patch;
            DiffViewer.Refresh();
        }

        public ICommand CopyFileNameCmd
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Clipboard.SetText(Path.GetFileName(_viewModel.SelectedDiffFile.GitHubCommitFile.Filename));
                });
            }
        }

        public ICommand CopyFilePathCmd
        {
            get
            {
                return new RelayCommand(() =>
                {
                    Clipboard.SetText(_viewModel.SelectedDiffFile.GitHubCommitFile.Filename);
                });
            }
        }

        public ICommand OpenInDiffToolCmd
        {
            get { return new RelayCommand(async () => await OpenInDiffTool(DiffListView.SelectedItem)); }
        }

        public ICommand FlagAsReviewedCmd
        {
            get { return new RelayCommand(() => MarkReviewingStatus(ReviewStatus.Reviewed)); }
        }

        public ICommand FlagAsBackLaterCmd
        {
            get { return new RelayCommand(() => MarkReviewingStatus(ReviewStatus.ConfirmLater)); }
        }

        public ICommand FlagAsFreshCmd
        {
            get { return new RelayCommand(() => MarkReviewingStatus(ReviewStatus.HasntBeenReviewed)); }
        }

        public ICommand AddToGitExtCmd
        {
            get { return new RelayCommand(AddToGitExt); }
        }

        public ICommand PreviewCommentsCmd
        {
            get { return new RelayCommand(PreviewComments); }
        }

        private void PreviewComments()
        {
            if (!_viewModel.HasComments())
            {
                MessageBoxHelper.ShowError(this, "You haven't commented on anything yet.");
                return;
            }
            var comments = _viewModel.GenerateComments();
            InitPreviewWnd();
            
            var html = MarkdownToHtml(comments);

            _previewBrowser.HtmlContent = html;
            _previewWnd.Show();
        }

        private void InitPreviewWnd()
        {
            if (_previewWnd != null)
            {
                return;
            }
            _previewWnd = new Window
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Height = 600,
                Width = 800,
                ShowInTaskbar = true,
                Title = "Previewing"
            };

            _previewWnd.Closing += (sender, args) =>
            {
                _previewWnd.Hide();
                args.Cancel = true;
                Activate();
            };

            _previewBrowser = new MarkdownView();
            _previewWnd.Content = _previewBrowser;
            _previewBrowser.OpenLinkInExternalBrowser = true;
        }

        private void AddToGitExt()
        {
            var dialog = new FolderBrowserDialog
            {
                Description = "Select the GitExtensions folder",
                ShowNewFolderButton = false
            };
            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;

            var targetDir = dialog.SelectedPath;
            if (!targetDir.ToUpperInvariant().EndsWith(@"\PLUGINS"))
            {
                targetDir = Path.Combine(targetDir, "plugins");
            }
            const string fileName = "PReviewer.gitext.dll";
            var src = Path.Combine(PathHelper.ProcessDir, fileName);
            var dst = Path.Combine(targetDir, fileName);
            try
            {
                File.Copy(src, dst, overwrite: true);
                MessageBoxHelper.ShowInfo(this, "Succeed");
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(this, "Unable to install to GitExtensions.\r\n" + ex);
            }
        }

        private void MarkReviewingStatus(ReviewStatus status)
        {
            foreach (var item in DiffListView.SelectedItems.Cast<CommitFileVm>())
            {
                item.ReviewStatus = status;
            }
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e)
        {
            if (TxtPrDescription.Visible)
            {
                TxtPrDescription.Hide();
            }
        }

        private void OnDragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (PrDescExpander.IsExpanded)
            {
                TxtPrDescription.Show();
                TxtPrDescription.Height += (int)e.VerticalChange;
            }
        }
        private async void OnFileKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await OpenInDiffTool(sender);
            }
        }
    }
}