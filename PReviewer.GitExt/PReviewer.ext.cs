using System;
using System.Diagnostics;
using System.IO;
using GitUIPluginInterfaces;

namespace PReviewer
{
    public class PReviewerForGitExt : GitPluginBase
    {
        public override string Description
        {
            get { return "PReviewer"; }
        }

        public override bool Execute(GitUIBaseEventArgs gitUiCommands)
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var path = Path.Combine(userProfile, @"Start Menu\Programs\Nicologies\PReviewer.appref-ms");
            if (File.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                Process.Start("https://raw.github.com/EbenZhang/PReviewer/master/dist/setup.exe");
            }
            return true;
        }
    }
}
