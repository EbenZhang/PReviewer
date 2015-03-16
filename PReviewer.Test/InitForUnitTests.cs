using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
