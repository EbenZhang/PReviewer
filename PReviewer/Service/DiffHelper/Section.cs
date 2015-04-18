using System.Collections.Generic;

namespace PReviewer.Service.DiffHelper
{
    public class Section
    {
        public int Start;
        public int End;
        public int DelimiterLength;

        public Section(int start, int end, int delimiterLength)
        {
            Start = start;
            End = end;
            DelimiterLength = delimiterLength;
        }

        public override string ToString()
        {
            return string.Format("Start {0}, End {1}, Len {2}, EOL {3}, Total: {4}",
                Start, End, LenWithoutEol, DelimiterLength, LenWithEol);
        }

        public int LenWithEol
        {
            get { return End - Start + DelimiterLength; }
        }

        public int LenWithoutEol
        {
            get { return End - Start; }
        }

        public class SectionLocator : IComparer<Section> 
        {
            public int Compare(Section x, Section y)
            {
                if (y.Start >= x.Start && y.Start < x.End)
                {
                    return 0;
                }
                if (y.Start < x.Start)
                {
                    return 1;
                }
                // (y.Start >= x.End)
                return -1;
            }
        }
    }
}