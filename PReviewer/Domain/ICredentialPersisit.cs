using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using ExtendedCL;

namespace PReviewer.Domain
{
    [Serializable]
    public struct LoginCredential
    {
        public string Password;
        public string UserName;
    }

    public interface ICredentialPersisit
    {
        LoginCredential Load();
        void Save(LoginCredential credential);
    }

    public class CredentialPersisit : ICredentialPersisit
    {
        private static readonly string LoginCredentialFile = Path.Combine(PathHelper.ProcessAppDir,
            "LoginCredential.bin");

        public LoginCredential Load()
        {
            if (!File.Exists(LoginCredentialFile))
            {
                return new LoginCredential();
            }
            try
            {
                using (var stream = File.OpenRead(LoginCredentialFile))
                {
                    var fmt = new BinaryFormatter();
                    var credential = (LoginCredential) fmt.Deserialize(stream);
                    credential.Password = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(credential.Password));
                    return credential;
                }
            }
            catch
            {
                try
                {
                    File.Delete(LoginCredentialFile);
                }
                catch
                {
                    // ignored
                }
            }
            return new LoginCredential();
        }

        public void Save(LoginCredential credential)
        {
            var formatter = new BinaryFormatter();
            using (var stream = File.OpenWrite(LoginCredentialFile))
            {
                credential.Password = Convert.ToBase64String(Encoding.UTF8.GetBytes(credential.Password));
                formatter.Serialize(stream, credential);
            }
        }
    }
}