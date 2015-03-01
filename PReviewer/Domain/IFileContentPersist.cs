using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PReviewer.Domain
{
    public interface IFileContentPersist
    {
        string SaveContent(string content);
    }
}
