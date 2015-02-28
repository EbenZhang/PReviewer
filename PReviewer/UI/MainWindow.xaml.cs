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
    }
}
