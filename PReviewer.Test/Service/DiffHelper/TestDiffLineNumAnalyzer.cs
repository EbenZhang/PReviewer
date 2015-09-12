using System.IO;
using ExtendedCL;
using NSubstitute;
using NUnit.Framework;
using PReviewer.Service.DiffHelper;

namespace PReviewer.Test.Service.DiffHelper
{
    public interface IDiffLineNumRecv
    {
        void OnLineNumAnalyzed(DiffLineNum lineNum);
    }

    [TestFixture]
    public class TestDiffLineNumAnalyzer
    {
        private static readonly string TestDataDir = Path.Combine(PathHelper.GetCallingAssemblyDir(), "Service", "DiffHelper");
        private readonly string _sampleDiff;
        private DiffLineNumAnalyzer _lineNumAnalyzer;
        private IDiffLineNumRecv _lineNumMetaRecv;

        public TestDiffLineNumAnalyzer()
        {
            // File copied from https://github.com/libgit2/libgit2sharp/pull/1034/files
            _sampleDiff = File.ReadAllText(Path.Combine(TestDataDir, "Sample.diff"));
        }

        [SetUp]
        public void SetUp()
        {
            _lineNumAnalyzer = new DiffLineNumAnalyzer();
            _lineNumMetaRecv = Substitute.For<IDiffLineNumRecv>();
            _lineNumAnalyzer.OnLineNumAnalyzed += _lineNumMetaRecv.OnLineNumAnalyzed;
        }

        [Test]
        public void CanGetHeaders()
        {
            _lineNumAnalyzer.Start(_sampleDiff);
            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 1
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == DiffLineNum.NotApplicableLineNum));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 13
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == DiffLineNum.NotApplicableLineNum));
        }

        [Test]
        public void CanGetContextLines()
        {   
            _lineNumAnalyzer.Start(_sampleDiff);

            // header1
            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 2
                && line.LeftLineNum == 9
                && line.RightLineNum == 9));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 10
                && line.LeftLineNum == 15
                && line.RightLineNum == 16));

            // header2
            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 14
                && line.LeftLineNum == 33
                && line.RightLineNum == 34));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 21
                && line.LeftLineNum == 39
                && line.RightLineNum == 40));
        }

        [Test]
        public void CanGetMinusLines()
        {
            _lineNumAnalyzer.Start(_sampleDiff);

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 5
                && line.LeftLineNum == 12
                && line.RightLineNum == DiffLineNum.NotApplicableLineNum));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 17
                && line.LeftLineNum == 36
                && line.RightLineNum == DiffLineNum.NotApplicableLineNum));
        }

        [Test]
        public void CanGetPlusLines()
        {
            _lineNumAnalyzer.Start(_sampleDiff);

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 8
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == 14));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 9
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == 15));

            _lineNumMetaRecv.Received(1).OnLineNumAnalyzed(Arg.Is<DiffLineNum>(line => line.LineNumInDiff == 18
                && line.LeftLineNum == DiffLineNum.NotApplicableLineNum
                && line.RightLineNum == 37));
        }
    }
}
