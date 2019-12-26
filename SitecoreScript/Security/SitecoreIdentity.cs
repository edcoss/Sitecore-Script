using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Sitecore.Script.Security
{
    public class SitecoreIdentity : IIdentity
    {
        private global::Sitecore.Security.Accounts.User user;

        public SitecoreIdentity()
        {
            user = global::Sitecore.Context.User;
        }

        #region IIdentity Members

        public string AuthenticationType
        {
            get { return global::Sitecore.Context.Domain.Name; }
        }

        public bool IsAuthenticated
        {
            get { return user.Identity.IsAuthenticated; }
        }

        public string Name
        {
            get { return user.Name; }
        }

        #endregion
    }
}