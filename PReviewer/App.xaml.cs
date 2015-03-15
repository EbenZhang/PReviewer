using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ExtendedCL;
using GalaSoft.MvvmLight.Threading;
using Microsoft.Win32;
using PReviewer.Domain;
using PReviewer.Model;

namespace PReviewer
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherHelper.Initialize();
            ViewModelLocator.BootStrap();
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