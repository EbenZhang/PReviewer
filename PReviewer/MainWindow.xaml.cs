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
using Octokit;
using PReviewer.Model;

namespace GitReviewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void OnBtnTestClicked(object sender, RoutedEventArgs e)
        {
            var github = await GitHubClientFactory.GetClient("EbenZhangEmbed", "Kaspersky");

            var commitsClient = github.Repository.Commits;
            var diff = await commitsClient.Compare("EmbedCard", "ECS7", 
                "be16c51eca417c7c306949e2f9db27c2bada0a91",
                "9352125b64db53e3ba15083215a8abf7983215a5");
        }
    }
}
