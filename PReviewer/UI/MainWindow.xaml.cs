using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GalaSoft.MvvmLight.CommandWpf;
using Mantin.Controls.Wpf.Notification;
using Octokit;
using PReviewer.Domain;
using PReviewer.Model;
using PReviewer.UI;
using WpfCommon.Utils;

namespace PReviewer.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowVm _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = DataContext as MainWindowVm;
            this.Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.TxtPullRequest.Focus();
        }

        public ICommand ShowChangesCmd
        {
            get
            {
                return new RelayCommand(ShowChanges);
            }
        }

        public ICommand ShowSettingsCmd
        {
            get { return new RelayCommand(ShowSettings);}
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
                MessageBoxHelper.ShowError(this, "Unable to find the pull request.\r\nEither the information you provided is incorrect or you don't have permission to view the pull request.");
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
            if (_viewModel.SelectedDiffFile.Status == GitFileStatus.Renamed
                && _viewModel.SelectedDiffFile.Changes == 0
                && _viewModel.SelectedDiffFile.Additions == 0
                && _viewModel.SelectedDiffFile.Deletions == 0)
            {
                var ballon = new Balloon(sender as Control, "No changes found. It's just renamed", BalloonType.Information)
                {
                    ShowCloseButton = true
                };
                ballon.Show();
                return;
            }
            if (_viewModel.SelectedDiffFile.Status == GitFileStatus.Removed)
            {
                var ballon = new Balloon(sender as Control, "This file has been deleted in the pull request.", BalloonType.Information)
                {
                    ShowCloseButton = true
                };
                ballon.Show();
                return;
            }
            await _viewModel.PrepareDiffContent();
        }
    }
}
