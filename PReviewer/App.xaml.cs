using System.Windows;
using ExtendedCL;
using PReviewer.Domain;

namespace PReviewer
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ViewModelLocator.BootStrap();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            PathHelper.CreateProcessAppDir();
            base.OnStartup(e);
        }
    }
}