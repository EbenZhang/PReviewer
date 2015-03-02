using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using Mantin.Controls.Wpf.Notification;
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

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            TxtPullRequest.Focus();
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
                return;
            }
            try
            {
                await _viewModel.PrepareDiffContent();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(this, "Unable to get diff content.\r\n\r\n" + ex);
            }
        }
    }
}