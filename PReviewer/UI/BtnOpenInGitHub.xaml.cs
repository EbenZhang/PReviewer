using System.Diagnostics;
using System.Windows;

namespace PReviewer.UI
{
    /// <summary>
    /// Interaction logic for BtnOpenInGitHub.xaml
    /// </summary>
    public partial class BtnOpenInGitHub
    {
        public BtnOpenInGitHub()
        {
            InitializeComponent();
        }

        private void OnBtnClicked(object sender, RoutedEventArgs e)
        {
            this.IsOpen = false;
            Process.Start(Url);
        }

        public string Url { get; set; }
    }
}
