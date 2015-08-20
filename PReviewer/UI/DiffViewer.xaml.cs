using ExtendedCL;
using GalaSoft.MvvmLight.CommandWpf;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Search;
using PReviewer.Domain;
using PReviewer.Service;
using PReviewer.Service.DiffHelper;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WpfCommon.Utils;

namespace PReviewer.UI
{
    /// <summary>
    /// Interaction logic for DiffViewer.xaml
    /// </summary>
    public partial class DiffViewer : UserControl
    {
        private readonly BtnOpenInGitHub _btnOpenLineInGitHub = new BtnOpenInGitHub();
        private DiffViewerLineNumberCtrl _lineNumbersControl;
        private readonly SearchPanel _searchPanel;
        private readonly TextEditorOptionsVm _optionsVm = new TextEditorOptionsVm();

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(PropertyName.Get<DiffViewer, string>(x => x.Text), 
                typeof(string), typeof(DiffViewer),
                new PropertyMetadata(default(string), TextPropertyChangedCallback));

        private static void TextPropertyChangedCallback(DependencyObject dependencyObject, 
            DependencyPropertyChangedEventArgs args)
        {
            ((DiffViewer)dependencyObject).ScrollToHome();
            ((DiffViewer) dependencyObject).UpdateDiffViewerWith(args.NewValue as string);
        }

        public string Text
        {
            get { return GetValue(TextProperty) as string; }
            set { SetValue(TextProperty, value); }
        }

        private MainWindowVm ViewModel
        {
            get
            {
                return DataContext as MainWindowVm;
            }
        }

        public TextEditorOptionsVm OptionsVm
        {
            get
            {
                return _optionsVm;
            }
        }

        public DiffViewer()
        { 
            InitializeComponent();

            var binding = new Binding
            {
                Source = OptionsVm,
                Path = new PropertyPath(PropertyName.Get<TextEditorOptionsVm, TextEditorOptions>(x => x.Options))
            };

            _diffViewer.SetBinding(TextEditor.OptionsProperty, binding);

            _diffViewer.TextArea.TextView.BackgroundRenderers.Add(new Highlighter(_diffViewer.TextArea.TextView));
            _diffViewer.TextArea.TextView.ColumnRulerPen = new Pen(Brushes.Gray, 1);

            _searchPanel = SearchPanel.Install(_diffViewer);
        }

        private void DiffViewer_OnPreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_lineNumbersControl == null)
            {
                return;
            }

            if (ViewModel.SelectedDiffFile == null)
            {
                return;
            }

            var textViewer = _diffViewer.TextArea.TextView;
            var lineDesc = _lineNumbersControl.GetLineDesc(textViewer.HighlightedLine);
            if (lineDesc == null)
            {
                return;
            }
            var y = textViewer.GetVisualTopByDocumentLine(textViewer.HighlightedLine) - textViewer.VerticalOffset;
            var pos = new Point(0, y);
            pos = textViewer.PointToScreen(pos);

            _btnOpenLineInGitHub.Placement = PlacementMode.AbsolutePoint;
            _btnOpenLineInGitHub.PlacementRectangle = new Rect(pos, new Size(100, 100));

            _btnOpenLineInGitHub.Url = ViewModel.PullRequestLocator.ToUrl()
                                       + "/files#diff-"
                                       +
                                       MD5.Create()
                                           .GetMd5HashString(ViewModel.SelectedDiffFile.GitHubCommitFile.Filename)
                                       + lineDesc;

            _btnOpenLineInGitHub.IsOpen = true;
        }

        private void DiffViewer_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _btnOpenLineInGitHub.IsOpen = false;
        }

        private void DiffViewer_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            _btnOpenLineInGitHub.IsOpen = false;
        }

        private async void UpdateDiffViewerWith(string text)
        {
            if (text == "")
            {
                _diffViewer.Text = text;
                return;
            }
            if (_lineNumbersControl != null)
            {
                _diffViewer.TextArea.LeftMargins.Remove(_lineNumbersControl);
            }
            _lineNumbersControl = new DiffViewerLineNumberCtrl();
            await Task.Run(() =>
            {
                var diffLineNumAnalyzer = new DiffLineNumAnalyzer();
                diffLineNumAnalyzer.OnLineNumAnalyzed += line =>
                {
                    _lineNumbersControl.AddDiffLineNum(line);
                };
                diffLineNumAnalyzer.Start(text);
            });
            _diffViewer.TextArea.LeftMargins.Add(_lineNumbersControl);

            _diffViewer.Text = text;
        }

        private void ScrollToHome()
        {
            _diffViewer.ScrollToHome();
        }

        public ICommand ToggleSearchPanelCmd
        {
            get { return new RelayCommand(ToggleSearchPanel, () => _searchPanel != null); }
        }

        private void ToggleSearchPanel()
        {
            if (_searchPanel.IsClosed)
            {
                _searchPanel.Open();
            }
            else
            {
                _searchPanel.Close();
            }
        }

        public ICommand ShowDiffInWindowCmd
        {
            get
            {
                return new RelayCommand(ShowDiffInWindow);
            }
        }

        private static Window _diffWindow;
        private void ShowDiffInWindow()
        {
            if (_diffWindow == null)
            {
                _diffWindow = new Window();
                _diffWindow.Closing += (sender, args) =>
                {
                    ((Window) sender).Hide();
                    args.Cancel = true;
                };
                var diffViewer = new DiffViewer {DataContext = this.DataContext};
                var binding = new Binding(PropertyName.Get((MainWindowVm x) => x.TextForDiffViewer))
                {
                    Source = this.DataContext
                };
                diffViewer.SetBinding(DiffViewer.TextProperty, binding);
                diffViewer.SetupToolbarItemForWindowMode();
                _diffWindow.Content = diffViewer;
            }
            if (!_diffWindow.IsVisible)
            {
                _diffWindow.Show();
            }
        }

        private void SetupToolbarItemForWindowMode()
        {
            btnShowDiffInWindow.Visibility = Visibility.Collapsed;
        }
    }
}
