using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ExtendedCL;

namespace PReviewer
{
    /// <summary>
    /// Interaction logic for App.xaml
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
