using GalaSoft.MvvmLight;

namespace PReviewer.Domain
{
    public class PullRequestLocator : ViewModelBase
    {
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
    }
}