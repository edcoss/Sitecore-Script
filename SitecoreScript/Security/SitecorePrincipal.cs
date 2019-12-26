using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;

namespace Sitecore.Script.Security
{
    public class SitecorePrincipal : IPrincipal
    {
        private List<string> roles = new List<string>();
        private IIdentity identity;
        private global::Sitecore.Security.Accounts.User user;

        public SitecorePrincipal(IIdentity identity)
        {
            this.identity = identity;
            user = global::Sitecore.Context.User;
            if (global::Sitecore.Context.User.IsAdministrator)
                roles.Add("Administrator");

            foreach (global::Sitecore.Security.Accounts.Role userRole in user.Roles)
            {
                roles.Add(userRole.Name);
            }
        }

        #region IPrincipal Members
        public IIdentity Identity { get { return identity; } }
        public bool IsInRole(string role) { return roles.Contains(role); }
        #endregion
    }
}