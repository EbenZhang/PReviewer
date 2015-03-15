using GalaSoft.MvvmLight;

namespace PReviewer.Domain
{
    public class CommitVm : ViewModelBase
    {
        private string _display;
        private string _sha;
        private string _wholeMessage;

        public string Display
        {
            get { return _display; }
            set
            {
                _display = value;
                RaisePropertyChanged();
            }
        }

        public string WholeMessage
        {
            get { return _wholeMessage; }
            set
            {
                _wholeMessage = value; 
                RaisePropertyChanged();
            }
        }

        public string Sha
        {
            get { return _sha; }
            set
            {
                _sha = value;
                RaisePropertyChanged();
            }
        }

        public static readonly int MaxMessageLength = 40;
        public static readonly int MaxShaLength = 7;

        public CommitVm(string message, string sha)
        {
            WholeMessage = message;
            if (message.Length > MaxMessageLength)
            {
                message = message.Substring(0, MaxMessageLength) + "...";
            }
            var displayableSha = sha.Length > MaxShaLength ? sha.Substring(0, MaxShaLength) : sha;
            _display = "#" + displayableSha + " " + message;
            _sha = sha;
        }
    }
}