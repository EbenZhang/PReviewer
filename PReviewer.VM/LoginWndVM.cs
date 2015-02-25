﻿using System;
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
    [Serializable]
    public class LoginWndVm : ViewModelBase
    {
        private static readonly string LoginCredentialFile = Path.Combine(PathHelper.ProcessAppDir, "LoginCredential.bin");
        private string _UserName;
        private string _Password;

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

        public static LoginWndVm LoadFromFile()
        {
            if (!File.Exists(LoginCredentialFile))
            {
                return new LoginWndVm();
            }
            using (var stream = File.OpenRead(LoginCredentialFile))
            {
                var fmt = new BinaryFormatter();
                return fmt.Deserialize(stream) as LoginWndVm;
            }
        }

        public void SaveToFile()
        {
            var formatter = new BinaryFormatter();
            using (var stream = File.OpenWrite(LoginCredentialFile))
            {
                formatter.Serialize(stream, this);
            }
        }

        public async Task<IGitHubClient> Login()
        {
            var client = await GitHubClientFactory.GetClient(this.UserName, this.Password);
            return client;
        }
    }
}
