using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using PropertyChanged;

namespace PReviewer.Core
{
    [AddINotifyPropertyChangedInterface]
    public class PullRequestLocator :  IEquatable<PullRequestLocator>
    {
        public static readonly PullRequestLocator Empty = new PullRequestLocator();
        public PullRequestLocator()
        {
        }

        public PullRequestLocator(PullRequestLocator copyFrom)
        {
            this.Repository = copyFrom.Repository;
            this.Owner = copyFrom.Owner;
            this.PullRequestNumber = copyFrom.PullRequestNumber;
        }

        public string Repository { get; set; }

        public string Owner { get; set; }

        public int PullRequestNumber { get; set; }

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
            if (!url.Contains("/"))
            {
                throw new UriFormatException(url);
            }
            var splited = url.Split(new char[] { '/' });
            locator.Owner = splited[1];
            locator.Repository = splited[2];
            locator.PullRequestNumber = int.Parse(splited[4]);
        }

        public string ToUrl()
        {
            return $"https://github.com/{Owner}/{Repository}/pull/{PullRequestNumber}";
        }

        public bool Equals(PullRequestLocator other)
        {
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Compare(Owner, other.Owner, StringComparison.InvariantCultureIgnoreCase) == 0
                   &&string.Compare(Repository, other.Repository, StringComparison.InvariantCultureIgnoreCase) == 0
                   && PullRequestNumber == other.PullRequestNumber;
        }

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
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