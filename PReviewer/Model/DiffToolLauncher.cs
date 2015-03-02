using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PReviewer.Domain;

namespace PReviewer.Model
{
    public class InvalidDiffToolSettings : Exception
    {
        public InvalidDiffToolSettings(Exception ex) : base("Invalid difftool settings." , ex)
        {
        }
    }
    public class DiffToolLauncher : IDiffToolLauncher
    {
        private readonly ICompareToolSettingsPersist _compareToolSettingsPersist;
        private readonly IDiffToolParamBuilder _diffToolParamBuilder;

        public DiffToolLauncher(ICompareToolSettingsPersist compareToolSettingsPersist,
            IDiffToolParamBuilder diffToolParamBuilder)
        {
            _compareToolSettingsPersist = compareToolSettingsPersist;
            _diffToolParamBuilder = diffToolParamBuilder;
        }

        public void Open(string @base, string head)
        {
            var toolContainer = _compareToolSettingsPersist.Load();
            var tool = toolContainer.GetCompareTools().ElementAt(toolContainer.CurrentActiveIndex);
            var p = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = tool.ExePath,
                    Arguments = _diffToolParamBuilder.Build(tool.Parameters, @base, head),
                    WorkingDirectory = Path.GetDirectoryName(tool.ExePath),
                }
            };
            try
            {
                p.Start();
            }
            catch(Exception ex)
            {
                throw new InvalidDiffToolSettings(ex);
            }
        }
    }
}