using System.Text.RegularExpressions;
using GalaSoft.MvvmLight;

namespace PReviewer.Domain
{
    public class PullRequestLocator : ViewModelBase
    {
        public static readonly PullRequestLocator Empty = new PullRequestLocator();
        private string _Repository;
        private string _Owner;
        private int _PullRequestNumber;

        public string Repository
        {
            get { return _Repository; }
            set
            {
                _Repository = value;
                RaisePropertyChanged();
            }
        }

        public string Owner
        {
            get { return _Owner; }
            set
            {
                _Owner = value;
                RaisePropertyChanged();
            }
        }

        public int PullRequestNumber
        {
            get { return _PullRequestNumber; }
            set
            {
                _PullRequestNumber = value;
                RaisePropertyChanged();
            }
        }

        public static PullRequestLocator FromUrl(string url)
        {
            var ret = new PullRequestLocator();
            FromUrl(ret, url);
            return ret;
        }

        public void UpdateWith(string url)
        {
            FromUrl(this, url);
        }

        private static void FromUrl(PullRequestLocator locator, string url)
        {
            url = Regex.Replace(url, @"HTTPS://|http://", "", RegexOptions.IgnoreCase);
            var splited = url.Split(new char[] { '/' });
            locator.Owner = splited[1];
            locator.Repository = splited[2];
            locator.PullRequestNumber = int.Parse(splited[4]);
        }

        public string ToUrl()
        {
            return string.Format("https//github.com/{0}/{1}/pull/{2}",
                Owner, Repository, PullRequestNumber);
        }
    }
}