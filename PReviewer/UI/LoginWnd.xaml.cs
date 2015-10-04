using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using PReviewer.Domain;
using WpfCommon.Utils;
using Application = System.Windows.Application;

namespace PReviewer.UI
{
    /// <summary>
    ///     Interaction logic for LoginWnd.xaml
    /// </summary>
    public partial class LoginWnd : Window
    {
        private readonly LoginWndVm _viewModel;

        public bool IsChangingAccount = false;

        public LoginWnd()
        {
            InitializeComponent();
            _viewModel = DataContext as LoginWndVm;
            _viewModel.LoadCredential();
            Loaded += LoginWnd_Loaded;
        }

        private void LoginWnd_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsChangingAccount)
            {
                // winforms browser is not able to show in loaded event.
                Dispatcher.CurrentDispatcher.InvokeAsync(TryLogin);
            }
        }

        private void TryLogin()
        {
            if (!_viewModel.HasActiveUser)
            {
                if (!_viewModel.AddNewUser())
                {
                    MessageBoxHelper.ShowError(this, "Failed to authenticate.");
                    return;
                }
            }
            else
            {
                _viewModel.CreateGitHubClientWhenTokenAlreadyGot();
            }

            if (!IsChangingAccount)
            {
                var main = new MainWindow();

                Application.Current.MainWindow = main;

                main.Show();
            }
            Close();
        }

        private void CreateGitHubClient()
        {
            _viewModel.CreateGitHubClientWhenTokenAlreadyGot();
        }

        private void OnAccountSelected(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

        private void OnSelectAccountBtnClicked(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LoginWnd_OnClosed(object sender, EventArgs e)
        {
            CreateGitHubClient();
        }
    }
}