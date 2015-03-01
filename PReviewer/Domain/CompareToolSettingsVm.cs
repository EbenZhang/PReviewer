using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ExtendedCL;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using PReviewer.Model;

namespace PReviewer.Domain
{
    public class CompareToolSettingsVm : ViewModelBase
    {
        private readonly ICompareToolSettingsPersist _compareToolSettingsPersist;
        private CompareToolSettings _SelectedComapreToolSettings = new CompareToolSettings();
        private bool _IsProcessing;

        private CompareToolSettingsContainer _orgContainer;

        public CompareToolSettingsVm(ICompareToolSettingsPersist compareToolSettingsPersist)
        {
            _compareToolSettingsPersist = compareToolSettingsPersist;
            CompareTools = new ObservableCollection<CompareToolSettings>();
            if (IsInDesignMode)
            {
                CompareTools.Add(new CompareToolSettings()
                {
                    Name = "Beyond Compare3",
                });

                CompareTools.Add(new CompareToolSettings()
                {
                    Name = "Kdiff3",
                });
            }
        }

        public ObservableCollection<CompareToolSettings> CompareTools { get; set; }

        public CompareToolSettings SelectedComapreToolSettings
        {
            get { return _SelectedComapreToolSettings; }
            set
            {
                _SelectedComapreToolSettings = value;
                RaisePropertyChanged();
            }
        }

        public bool IsProcessing
        {
            get { return _IsProcessing; }
            set
            {
                _IsProcessing = value; 
                RaisePropertyChanged();
            }
        }

        public async Task Load()
        {
            try
            {
                IsProcessing = true;
                var tools = await Task.Run(() => _compareToolSettingsPersist.Load());
                _orgContainer = tools;
                CompareTools.Assign(tools.GetCompareTools());
                SelectedComapreToolSettings = CompareTools.ElementAtOrDefault(tools.CurrentActiveIndex);
            }
            finally
            {
                IsProcessing = false;
            }
        }

        public async Task Save()
        {
            try
            {
                IsProcessing = true;

                CorrectsCompareToolsWithSelection();

                var toRemove = CompareTools.Where(r => !IsValidSetting(r)).ToList();
                foreach (var rm in toRemove)
                {
                    CompareTools.Remove(rm);
                }
                
                // Reverse before distinct so the later one will be kept.
                var toKeep = CompareTools.Reverse().Distinct().ToArray();
                foreach (var tool in CompareTools.ToList())
                {
                    if (!toKeep.Contains(tool))
                    {
                        CompareTools.Remove(tool);
                    }
                }
                var newIndx = -1;
                if (SelectedComapreToolSettings != null)
                {
                    newIndx = CompareTools.IndexOf(SelectedComapreToolSettings);
                }

                var serializable = new CompareToolSettingsContainer(CompareTools, newIndx);
                await Task.Run(() => _compareToolSettingsPersist.Save(serializable));
            }
            finally
            {
                IsProcessing = false;
            }
        }

        private bool IsValidSetting(CompareToolSettings settings)
        {
            return settings != null && settings.IsValidSetting();
        }

        private void CorrectsCompareToolsWithSelection()
        {
            if (!IsValidSetting(SelectedComapreToolSettings))
            {
                if (_orgContainer.CurrentActiveIndex != -1)
                {
                    SelectedComapreToolSettings = _orgContainer.GetCompareTools()
                        .ElementAt(_orgContainer.CurrentActiveIndex);
                }
                return;
            }

            var exists =
                CompareTools.FirstOrDefault(
                    x =>
                        String.Equals(x.Name, SelectedComapreToolSettings.Name,
                            StringComparison.InvariantCultureIgnoreCase));
            if (exists == null)
            {
                CompareTools.Insert(0, SelectedComapreToolSettings);
            }
            else
            {
                exists.Parameters = SelectedComapreToolSettings.Parameters;
                exists.ExePath = SelectedComapreToolSettings.ExePath;
                SelectedComapreToolSettings = exists;
            }
        }

        public ICommand PrepareNewToolCmd
        {
            get
            {
                return new RelayCommand(PrepareNewTool);
            }
        }

        public void PrepareNewTool()
        {
            this.SelectedComapreToolSettings = new CompareToolSettings();
            CompareTools.Add(this.SelectedComapreToolSettings);
        }

        public bool HasChanges()
        {
            var valids = CompareTools.Where(IsValidSetting).ToList();
            var containerChanged = !valids.SequenceEqual(_orgContainer.GetCompareTools());
            var indexChanged = IsValidSetting(SelectedComapreToolSettings) && valids.IndexOf(SelectedComapreToolSettings) != _orgContainer.CurrentActiveIndex;

            return containerChanged || indexChanged;
        }
    }

    public interface ICompareToolSettingsPersist
    {
        CompareToolSettingsContainer Load();
        void Save(CompareToolSettingsContainer container);
    }
}
