using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PReviewer.Domain
{
    public interface IPatchService
    {
        Task RevertViaPatch(string content, string patch, string patchTo);
    }
}
