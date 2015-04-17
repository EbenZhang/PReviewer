using GalaSoft.MvvmLight;
using ICSharpCode.AvalonEdit;
using PReviewer.Model;

namespace PReviewer.Domain
{
    public class TextEditorOptionsVm : ViewModelBase
    {
        private readonly TextEditorOptions _options =new TextEditorOptions();

        public TextEditorOptionsVm()
        {
            _options.ShowEndOfLine = _options.ShowSpaces = _options.ShowTabs = ShowWhiteSpaces;
        }

        public TextEditorOptions Options
        {
            get { return _options; }
        }

        private readonly UserPreferences _preferences = UserPreferences.Instance;

        public bool ShowWhiteSpaces
        {
            get { return _preferences.ShowWhiteSpaces; }
            set
            {
                _preferences.ShowWhiteSpaces = value;
                _options.ShowEndOfLine = _options.ShowSpaces = _options.ShowTabs = value;
                RaisePropertyChanged();
                _preferences.Save();
            }
        }
    }
}
