using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using ExtendedCL;
using GalaSoft.MvvmLight;
using Microsoft.SqlServer.Server;
using Octokit;
using PReviewer.Model;

namespace PReviewer.VM
{
    public class LoginWndVm : ViewModelBase
    {
        [Serializable]
        private struct LoginCredential
        {
            public string UserName;
            public string Password;
        }
        private static readonly string LoginCredentialFile = Path.Combine(PathHelper.ProcessAppDir, "LoginCredential.bin");
     
        private string _UserName;

        private string _Password;
        private bool _IsProcessing;

        public string UserName
        {
            get { return _UserName; }
            set
            {
                _UserName = value;
                RaisePropertyChanged();
            }
        }

        public string Password
        {
            get { return _Password; }
            set
            {
                _Password = value;
                RaisePropertyChanged();
            }
        }

        public bool IsProcessing
        {
            get { return _IsProcessing; }
            set
            {
                _IsProcessing = value;
                RaisePropertyChanged();
            }
        }

        private LoginCredential GetCredential()
        {
            return new LoginCredential()
            {
                UserName = this.UserName,
                Password = this.Password
            };
        }

        public static LoginWndVm LoadFromFile()
        {
            if (!File.Exists(LoginCredentialFile))
            {
                return new LoginWndVm();
            }
            try
            {
                using (var stream = File.OpenRead(LoginCredentialFile))
                {
                    var fmt = new BinaryFormatter();
                    var credential = (LoginCredential) fmt.Deserialize(stream);
                    return new LoginWndVm()
                    {
                        UserName = credential.UserName,
                        Password = credential.Password,
                    };
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
            return new LoginWndVm();
        }

        public void SaveToFile()
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                return;
            }
            var formatter = new BinaryFormatter();
            using (var stream = File.OpenWrite(LoginCredentialFile))
            {
                formatter.Serialize(stream, GetCredential());
            }
        }

        public async Task<IGitHubClient> Login()
        {
            IsProcessing = true;
            try
            {
                var client = await GitHubClientFactory.GetClient(this.UserName, this.Password);
                this.SaveToFile();
                return client;
            }
            catch
            {
                IsProcessing = false;
                throw;
            }
        }
    }
}
