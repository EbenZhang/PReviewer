using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using ExtendedCL;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Win32;
using NBug.Core.Submission.Custom;
using NBug.Events;
using Octokit;
using PReviewer.Domain;
using PReviewer.Model;
using Application = System.Windows.Application;

namespace PReviewer
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly string LogDir = Path.Combine(PathHelper.ProcessAppDir, "Logs");

        public App()
        { 
            NBug.Settings.CustomSubmissionEvent += OnCustomSubmissionEvent;
            NBug.Settings.Destinations.Add(new Custom());
            AppDomain.CurrentDomain.UnhandledException += NBug.Handler.UnhandledException;
            Current.DispatcherUnhandledException += NBug.Handler.DispatcherUnhandledException;
            NBug.Settings.StoragePath = LogDir;
            DispatcherHelper.Initialize();
            ViewModelLocator.BootStrap();
        }

        private static async void OnCustomSubmissionEvent(object sender, CustomSubmissionEventArgs customSubmissionEventArgs)
        {
            customSubmissionEventArgs.Result = false;
            var factory = ViewModelLocator.Resolve<IGitHubClientFactory>();
            var client = factory?.GetClient();
            if (client == null)
            {
                return;
            }
            try
            {
                var subject = "Product environment exception";
                if (!string.IsNullOrWhiteSpace(customSubmissionEventArgs.Exception.Message))
                {
                    subject = customSubmissionEventArgs.Exception.Message;
                }
                await
                    client.Issue.Create("EbenZhang", "PReviewer",
                        new NewIssue(subject)
                        {
                            Body = customSubmissionEventArgs.Report.ToString() + customSubmissionEventArgs.Exception.ToString(),
                            Assignee = "EbenZhang",
                            Labels = { "Production Env Error" },
                        });
                customSubmissionEventArgs.File.Dispose();

                File.Delete(Path.Combine(LogDir, customSubmissionEventArgs.FileName));
                customSubmissionEventArgs.Result = true;
            }
            catch
            {
                // ignored
            }
        }

        private static string FindAppPath(RegistryKey root, string keyPath, string itemName)
        {
            try
            {
                using (var key = root.OpenSubKey(keyPath, writable: false))
                {
                    return (string)key.GetValue(itemName);
                }
            }
            catch
            {
                return "";
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            PathHelper.CreateProcessAppDir();

            // should be ok to run as background task, as it's slow to login to github.
            Task.Run(() =>
            {
                var compareToolPersist = ViewModelLocator.Resolve<ICompareToolSettingsPersist>();
                var settingsContainer = compareToolPersist.Load();
                var settings = settingsContainer.GetCompareTools();
                if (!settings.Any())
                {
                    TryAddDifftool("BeyondCompare", @"Software\Scooter Software\Beyond Compare", 
                        "ExePath", settingsContainer);
                    TryAddDifftool("KDiff", @"Software\KDiff3\diff-ext", "diffcommand", settingsContainer);
                    TryAddDifftool("WinMerge", @"Software\Thingamahoochie\WinMerge", "Executable", settingsContainer);
                    if (settingsContainer.CompareToolSettingses.Any())
                    {
                        settingsContainer.CurrentActiveIndex = 0;
                        compareToolPersist.Save(settingsContainer);
                    }
                }
            });
            base.OnStartup(e);
        }

        private void TryAddDifftool(string appName, string keyPath, 
            string exeItemName, 
            CompareToolSettingsContainer settingsContainer)
        {
            var appPath = FindAppPath(Registry.CurrentUser, keyPath, exeItemName);
            if (string.IsNullOrWhiteSpace(appPath))
            {
                appPath = FindAppPath(Registry.LocalMachine, keyPath, exeItemName);
            }
            if (string.IsNullOrWhiteSpace(appPath))
            {
                return;
            }

            var bcSettings = new CompareToolSettings()
            {
                ExePath = appPath,
                Name = appName,
            };
            settingsContainer.CompareToolSettingses.Add(bcSettings.ToSerializable());
        }
    }
}