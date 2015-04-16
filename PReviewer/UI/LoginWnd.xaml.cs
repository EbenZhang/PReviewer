using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using Octokit;
using PReviewer.Domain;
using PReviewer.UI;
using WpfCommon.Utils;
using Application = System.Windows.Application;

namespace PReviewer
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

        public ICommand LoginCmd
        {
            get { return new RelayCommand(Login); }
        }

        private void LoginWnd_Loaded(object sender, RoutedEventArgs e)
        {
            TxtUserName.Focus();
            if (!IsChangingAccount)
            {
                Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    if (!string.IsNullOrWhiteSpace(_viewModel.UserName))
                    {
                        Login();
                    }
                }));
            }
        }

        private async void Login()
        {
            if (!this.ValidateTextBoxes())
            {
                return;
            }

            try
            {
                var client = await _viewModel.Login();

                if (client == null) return;

                if (!IsChangingAccount)
                {
                    var main = new MainWindow();
                    Application.Current.MainWindow = main;
                    main.Show();
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(this, ex.ToString());
            }
        }
    }
}