using System;
using System.Text.RegularExpressions;
using System.Windows.Documents.DocumentStructures;
using GalaSoft.MvvmLight;

namespace PReviewer.Domain
{
    public class PullRequestLocator : ViewModelBase, IEquatable<PullRequestLocator>
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
            return string.Format("https://github.com/{0}/{1}/pull/{2}",
                Owner, Repository, PullRequestNumber);
        }

        public bool Equals(PullRequestLocator other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return String.Compare(Owner, other.Owner, StringComparison.InvariantCultureIgnoreCase) == 0
                   &&String.Compare(Repository, other.Repository, StringComparison.InvariantCultureIgnoreCase) == 0
                   && this.PullRequestNumber == other.PullRequestNumber;
        }

        public override int GetHashCode()
        {
            return (Owner ?? "" + (Repository ?? "") + PullRequestNumber).GetHashCode();
        }

        public override string ToString()
        {
            return ToUrl();
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Owner) &&
                   !string.IsNullOrWhiteSpace(Repository)
                   && PullRequestNumber != 0;
        }
    }
}