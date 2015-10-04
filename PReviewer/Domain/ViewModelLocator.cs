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
using PReviewer.Model;
using PReviewer.Service;
using PReviewer.UI;
using PReviewer.User;

namespace PReviewer.Domain
{
    /// <summary>
    ///     This class contains static references to all the view models in the
    ///     application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private static readonly ContainerBuilder _builder = new ContainerBuilder();
        private static IContainer _container;

        public LoginWndVm LoginWndVm
        {
            get { return _container.Resolve<LoginWndVm>(); }
        }

        public MainWindowVm MainWindowVm
        {
            get
            {
                var client = _container.Resolve<IGitHubClientFactory>().GetClient();
                return _container.Resolve<MainWindowVm>(TypedParameter.From(client));
            }
        }

        public CompareToolSettingsVm CompareToolSettingsVm
        {
            get { return _container.Resolve<CompareToolSettingsVm>(); }
        }

        public static T Resolve<T>()
        {
            return _container.Resolve<T>();
        }

        public static void BootStrap()
        {
            // Usually you're only interested in exposing the type
            // via its interface:
            _builder.RegisterType<LoginWndVm>().AsSelf();
            _builder.RegisterType<MainWindowVm>().AsSelf();
            _builder.RegisterType<CompareToolSettingsVm>().AsSelf();
            _builder.RegisterInstance(new GitHubClientFactory()).As<IGitHubClientFactory>();
            var compareToolSettingsPersist = new CompareToolSettingsPersist();
            _builder.RegisterInstance(compareToolSettingsPersist).As<ICompareToolSettingsPersist>();
            _builder.RegisterInstance(new FileContentPersist()).As<IFileContentPersist>();
            _builder.RegisterInstance(new DiffToolLauncher(compareToolSettingsPersist, new DiffToolParamBuilder()))
                .As<IDiffToolLauncher>();

            _builder.RegisterInstance(new CommentsBuilder()).As<ICommentsBuilder>();

            _builder.RegisterInstance(new PatchService()).As<IPatchService>();

            _builder.RegisterInstance(new CommentsPersist()).As<ICommentsPersist>();

            _builder.RegisterInstance(new RepoHistoryPersist()).As<IRepoHistoryPersist>();

            _builder.RegisterInstance(new BackgroundTaskRunner()).As<IBackgroundTaskRunner>();

            _builder.RegisterType<GitHubOAuthView>().AsSelf();

            _builder.RegisterType<GitHubOAuth>().As<IGitHubOAuth>();

            _builder.RegisterType<GitHubUserProvider>().AsSelf();

            _builder.RegisterType<GitHubOAuth>().As<IGitHubOAuth>();

            _builder.RegisterType<GitHubUserPersister>().As<IGitHubUserPersister>();

            _builder.RegisterType<UserManager>().As<IUserManager>();

            _container = _builder.Build();
        }

        public static void RegisterInstance<T, TBase>(T inst) where T : class
        {
            _builder.RegisterInstance(inst).As<TBase>();
        }

        public static void Cleanup()
        {
            _container.Dispose();
        }
    }
}