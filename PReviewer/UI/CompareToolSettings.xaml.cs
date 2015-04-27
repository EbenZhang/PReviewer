using System;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using PReviewer.Domain;
using WpfCommon.Utils;

namespace PReviewer.UI
{
    /// <summary>
    /// Interaction logic for CompareToolSettings.xaml
    /// </summary>
    public partial class CompareToolSettings : Window
    {
        private CompareToolSettingsVm _viewModel;
        public CompareToolSettings()
        {
            InitializeComponent();
            _viewModel = DataContext as CompareToolSettingsVm;
            this.Loaded += OnLoaded;
            this.Closing += OnClosing;
        }

        void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!NeedToConfirmBeforeClose)
            {
                return;
            }

            if (!_viewModel.HasChanges())
            {
                return;
            }

            e.Cancel = MessageBoxHelper.ShowConfirmation(this, "Are you sure to cancel?") == MessageBoxResult.No;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.Load();
        }

        public ICommand CancelCmd
        {
            get { return new RelayCommand(Cancel); }
        }

        public ICommand SaveCmd
        {
            get { return new RelayCommand(Save); }
        }

        public ICommand BrowserCmd
        {
            get
            {
                return new RelayCommand(Browser);
            }
        }

        private async void Save()
        {
            try
            {
                await _viewModel.Save();
                NeedToConfirmBeforeClose = false;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(this, "Unable to save settings.\r\n" + ex);
            }
        }

        public bool NeedToConfirmBeforeClose = true;

        private void Cancel()
        {
            this.Close();
        }

        private void Browser()
        {
            var dlg = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "Exe file (*.exe)|*.exe",
                Multiselect = false
            };
            var ok = dlg.ShowDialog();
            if (ok != null && ok.Value)
            {
                _viewModel.SelectedComapreToolSettings.ExePath = dlg.FileName;
            }
        }
    }
}
