/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:PReviewer"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

using Autofac;
using PReviewer.Domain;

namespace PReviewer
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private static readonly ContainerBuilder _builder = new ContainerBuilder();
        private static IContainer _container;

        public static void BootStrap()
        {
            // Usually you're only interested in exposing the type
            // via its interface:
            _builder.RegisterType<LoginWndVm>().AsSelf();
            _container = _builder.Build();
        }

        public static void RegisterInstance<T, TBase>(T inst) where T : class
        {
            _builder.RegisterInstance(inst).As<TBase>();
        }

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ////if (ViewModelBase.IsInDesignModeStatic)
            ////{
            ////    // Create design time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DesignDataService>();
            ////}
            ////else
            ////{
            ////    // Create run time view services and models
            ////    SimpleIoc.Default.Register<IDataService, DataService>();
            ////}
        }

        public LoginWndVm LoginWndVm
        {
            get { return _container.Resolve<LoginWndVm>(); }
        }
        
        public static void Cleanup()
        {
            _container.Dispose();
        }
    }
}