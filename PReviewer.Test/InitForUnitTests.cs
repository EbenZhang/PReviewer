using GalaSoft.MvvmLight.Threading;
using NUnit.Framework;

namespace PReviewer.Test
{
    [SetUpFixture]
    public class InitForUnitTests
    {
        [SetUp]
        public void SetUp()
        {
            DispatcherHelper.Initialize();
        }
    }
}
