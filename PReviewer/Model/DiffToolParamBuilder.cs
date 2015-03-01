using System.Text.RegularExpressions;
using PReviewer.Domain;

namespace PReviewer.Model
{
    public class DiffToolParamBuilder : IDiffToolParamBuilder
    {
        public string Build(string configuredParams, string @base, string head)
        {
            var baseReplaced = Regex.Replace(configuredParams, @"\$Base", "\"" + @base + "\"", RegexOptions.IgnoreCase);
            var headReplaced = Regex.Replace(baseReplaced, @"\$Head", "\"" + head + "\"", RegexOptions.IgnoreCase);
            return headReplaced;
        }
    }
}