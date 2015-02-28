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
        private MainWindowVm _viewModel;
        public MainWindow()
        {
            _viewModel = DataContext as MainWindowVm;
            InitializeComponent();
        }

        public ICommand ShowChangesCmd
        {
            get
            {
                return new RelayCommand(ShowChanges);
            }
        }

        private void ShowChanges()
        {
            if (!this.ValidateAutoCompleteBoxes())
            {
                return;
            }
            if (!this.ValidateTextBoxes())
            {
                return;
            }

            _viewModel.RetrieveDiffs();
        }
    }
}
