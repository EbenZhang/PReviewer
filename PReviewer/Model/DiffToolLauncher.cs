using System.Diagnostics;
using System.Linq;
using PReviewer.Domain;

namespace PReviewer.Model
{
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
                    Arguments = _diffToolParamBuilder.Build(tool.Parameters, @base, head)
                }
            };
            p.Start();
        }
    }
}