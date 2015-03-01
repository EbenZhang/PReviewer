namespace PReviewer.Domain
{
    public interface IDiffToolProvider
    {
        void Open(string @base, string head);
    }
}