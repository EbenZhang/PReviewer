using System.Runtime.Serialization;

namespace PReviewer.User
{
    public interface IUser
    {
        string Key { get; }
        bool Login();
        void Logout();
        void Delete();
        bool IsActiveUser { get; set; }
        string Token { get; }
    }
}