using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ExtendedCL;
using GalaSoft.MvvmLight.CommandWpf;
using Mantin.Controls.Wpf.Notification;
using Microsoft.Win32;
using Octokit;
using PReviewer.Domain;
using PReviewer.Model;
using WpfCommon.Utils;

namespace PReviewer.UI
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowVm _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as MainWindowVm;
            Loaded += OnLoaded;
            DiffViewer.TextChanged += DiffViewerOnTextChanged;
            SetupWindowClosingActions();
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
                    var licMarkdown = File.ReadAllText(Path.Combine(PathHelper.ProcessDir, "LICENSE"));
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
    }
}