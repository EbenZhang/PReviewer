using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PReviewer.Service
{
    public interface IBackgroundTaskRunner
    {
        void RunInBackground(Action action);
    }

    public class BackgroundTaskRunner : IBackgroundTaskRunner
    {
        public void RunInBackground(Action action)
        {
            Task.Run(action);
        }
    }
}
