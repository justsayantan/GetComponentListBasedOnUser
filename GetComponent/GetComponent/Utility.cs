using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Web;
using System.Xml;
using Tridion.ContentManager.CoreService.Client;

namespace GetComponent
{
    public static class Utility
    {
        private static CoreServiceClient _coreServiceSource;

        public static CoreServiceClient CoreServiceSource
        {
            get
            {
                if (_coreServiceSource != null) return _coreServiceSource;
                _coreServiceSource = new CoreServiceClient("sourceEndpoint");

                _coreServiceSource.ClientCredentials.UserName.UserName = ConfigurationManager.AppSettings["SourceUserName"];
                _coreServiceSource.ClientCredentials.UserName.Password = ConfigurationManager.AppSettings["SourceUserPassword"];
                _coreServiceSource.ClientCredentials.Windows.ClientCredential.UserName = ConfigurationManager.AppSettings["SourceUserName"];
                _coreServiceSource.ClientCredentials.Windows.ClientCredential.Password = ConfigurationManager.AppSettings["SourceUserPassword"];
                _coreServiceSource.ClientCredentials.Windows.ClientCredential.Domain = ConfigurationManager.AppSettings["SourceUserDomain"];

                return _coreServiceSource;
            }
        }
    }
}