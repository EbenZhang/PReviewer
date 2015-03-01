namespace PReviewer.Domain
{
    public interface IDiffToolParamBuilder
    {
        string Build(string configuredParams, string @base, string head);
    }
}