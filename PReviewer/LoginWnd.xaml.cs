using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Command;
using GitReviewer;
using Octokit;
using PReviewer.Domain;
using WpfCommon;
using WpfCommon.Utils;

namespace PReviewer
{
    /// <summary>
    /// Interaction logic for LoginWnd.xaml
    /// </summary>
    public partial class LoginWnd : Window
    {
        private readonly LoginWndVm _viewModel;
        public LoginWnd()
        {
            InitializeComponent();
            _viewModel = DataContext as LoginWndVm;
            _viewModel.LoadFromFile();
            this.Loaded += LoginWnd_Loaded;
        }

        void LoginWnd_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                if (!string.IsNullOrWhiteSpace(_viewModel.UserName))
                {
                    Login();
                }
            }));
        }

        public ICommand LoginCmd
        {
            get { return new RelayCommand(Login); }
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

                ViewModelLocator.RegisterInstance<IGitHubClient, IGitHubClient>(client);

                var main = new MainWindow();
                App.Current.MainWindow = main;
                main.Show();

                Close();
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError(this, ex.ToString());
            }
        }
    }
}
