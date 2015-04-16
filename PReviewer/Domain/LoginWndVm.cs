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

namespace PReviewer.Domain
{
    public class LoginWndVm : ViewModelBase
    {
        private readonly IGitHubClientFactory _gitHubClientFactory;
        private readonly ICredentialPersisit _credentialPersisit;

        public LoginWndVm(IGitHubClientFactory gitHubClientFactory, ICredentialPersisit credentialPersisit)
        {
            _gitHubClientFactory = gitHubClientFactory;
            _credentialPersisit = credentialPersisit;
        }

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

        public void LoadCredential()
        {
            var credential = _credentialPersisit.Load();
            if (string.IsNullOrWhiteSpace(credential.UserName))
            {
                return;
            }
            UserName = credential.UserName;
            Password = credential.Password;
        }

        public void SaveCredential()
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                return;
            }
            _credentialPersisit.Save(this.GetCredential());
        }

        public async Task<IGitHubClient> Login()
        {
            IsProcessing = true;
            try
            {
                var client = await _gitHubClientFactory.Login(this.UserName, this.Password);
                this.SaveCredential();
                return client;
            }
            finally
            {
                IsProcessing = false;
            }
        }
    }
}
