namespace PReviewer.Domain
{
    public interface IDiffToolLauncher
    {
        void Open(string @base, string head);
    }
}