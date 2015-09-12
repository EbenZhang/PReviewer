using System.Text.RegularExpressions;

namespace PReviewer.Service.DiffHelper
{
    public class DiffLineNumAnalyzer
    {
        public delegate void EvLineNumAnalyzed(DiffLineNum diffLineNum);

        public event EvLineNumAnalyzed OnLineNumAnalyzed;

        protected void FireLineAnalyzedEvent(DiffLineNum diffline)
        {
            var handler = OnLineNumAnalyzed;
            if (handler != null) handler(diffline);
        }

        public void Start(string diffContent)
        {
            DoAnalyze(diffContent);
        }

        private void DoAnalyze(string diffContent)
        {
            var lineNumInDiff = 0;
            var leftLineNum = DiffLineNum.NotApplicableLineNum;
            var rightLineNum = DiffLineNum.NotApplicableLineNum;
            foreach (var line in diffContent.Split('\n'))
            {
                lineNumInDiff++;
                if (line.StartsWith("@"))
                {
                    var meta = new DiffLineNum
                    {
                        LineNumInDiff = lineNumInDiff,
                        LeftLineNum = DiffLineNum.NotApplicableLineNum,
                        RightLineNum = DiffLineNum.NotApplicableLineNum
                    };
                    var regex =
                        new Regex(
                            @"\-(?<leftStart>\d{1,})\,{0,}(?<leftCount>\d{0,})\s\+(?<rightStart>\d{1,})\,{0,}(?<rightCount>\d{0,})",
                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

                    var lineNumbers = regex.Match(line);
                    leftLineNum = int.Parse(lineNumbers.Groups["leftStart"].Value);
                    rightLineNum = int.Parse(lineNumbers.Groups["rightStart"].Value);

                    FireLineAnalyzedEvent(meta);
                }
                else if (line.StartsWith("-"))
                {
                    var meta = new DiffLineNum
                    {
                        LineNumInDiff = lineNumInDiff,
                        LeftLineNum = leftLineNum,
                        RightLineNum = DiffLineNum.NotApplicableLineNum
                    };
                    FireLineAnalyzedEvent(meta);

                    leftLineNum++;
                }
                else if (line.StartsWith("+"))
                {
                    var meta = new DiffLineNum
                    {
                        LineNumInDiff = lineNumInDiff,
                        LeftLineNum = DiffLineNum.NotApplicableLineNum,
                        RightLineNum = rightLineNum
                    };
                    FireLineAnalyzedEvent(meta);
                    rightLineNum++;
                }
                else
                {
                    var meta = new DiffLineNum
                    {
                        LineNumInDiff = lineNumInDiff,
                        LeftLineNum = leftLineNum,
                        RightLineNum = rightLineNum
                    };
                    FireLineAnalyzedEvent(meta);

                    leftLineNum++;
                    rightLineNum++;
                }
            }
        }
    }

    public class DiffLineNum
    {
        public enum DiffLineStyle
        {
            Header,
            Plus,
            Minus,
            Context
        }

        public static readonly int NotApplicableLineNum = -1;
        public int LineNumInDiff { get; set; }
        public int LeftLineNum { get; set; }
        public int RightLineNum { get; set; }

        public DiffLineStyle Style
        {
            get
            {
                if (RightLineNum == NotApplicableLineNum
                    && LeftLineNum == NotApplicableLineNum)
                {
                    return DiffLineStyle.Header;
                }
                if (RightLineNum == NotApplicableLineNum
                    && LeftLineNum != NotApplicableLineNum)
                {
                    return DiffLineStyle.Minus;
                }
                if (RightLineNum != NotApplicableLineNum
                    && LeftLineNum == NotApplicableLineNum)
                {
                    return DiffLineStyle.Plus;
                }
                return DiffLineStyle.Context;
            }
        }
    }
}