using System.Threading.Tasks;

namespace PReviewer.Core
{
    public interface IPatchService
    {
        Task RevertViaPatch(string content, string patch, string patchTo);
    }
}
