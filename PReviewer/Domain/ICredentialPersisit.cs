using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ExtendedCL;

namespace PReviewer.Domain
{
    [Serializable]
    public struct LoginCredential
    {
        public string UserName;
        public string Password;
    }

    public interface ICredentialPersisit
    {
        LoginCredential Load();
        void Save(LoginCredential credential);
    }

    public class CredentialPersisit : ICredentialPersisit
    {
        private static readonly string LoginCredentialFile = Path.Combine(PathHelper.ProcessAppDir, "LoginCredential.bin");
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
                    var credential = (LoginCredential)fmt.Deserialize(stream);
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
                formatter.Serialize(stream, credential);
            }
        }
    }
}
