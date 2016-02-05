using System;
using System.Web;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.AccountManagement;

namespace Zendesk
{
    public class JWTLogin : IHttpHandler
    {
        private string SHARED_KEY = ConfigurationManager.AppSettings["ZenDeskToken"];
        private string SUBDOMAIN = ConfigurationManager.AppSettings["ZenDeskSubdomain"];
        private string EMAILDOMAIN = ConfigurationManager.AppSettings["EmailDomain"];

        public void ProcessRequest(HttpContext context)
        {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            int timestamp  = (int) t.TotalSeconds;

            var userInformation =
              GetUserNameAndEmailAddress(HttpContext.Current.Request.LogonUserIdentity.Name);

            var payload = new Dictionary<string, object>() {
                { "iat", timestamp },
                { "jti", System.Guid.NewGuid().ToString() },
                { "name", userInformation.Item1 },
                { "email", userInformation.Item2 }
            };

            string token = JWT.JsonWebToken.Encode(payload, SHARED_KEY, JWT.JwtHashAlgorithm.HS256);
            string redirectUrl = "https://" + SUBDOMAIN + ".zendesk.com/access/jwt?jwt=" + token;

            string returnTo = context.Request.QueryString["return_to"];

            if(returnTo != null) {
              redirectUrl += "&return_to=" + HttpUtility.UrlEncode(returnTo);
            }

            context.Response.Redirect(redirectUrl);
        }

        public Tuple<string, string> GetUserNameAndEmailAddress(string username)
        {
           using (var pctx = new PrincipalContext(ContextType.Domain))
           {
               using (UserPrincipal up = UserPrincipal.FindByIdentity(pctx, username))
               {
                  string emailAddress = up != null && !String.IsNullOrEmpty(up.EmailAddress) ? up.EmailAddress.ToLower() : username.Split('\\')[1] + "@" + EMAILDOMAIN;
                  string name = up != null && !String.IsNullOrEmpty(up.Name) ? up.Name : username.Split('\\')[1];

                  return new Tuple<string, string>(name, emailAddress);
               }
           }
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}
