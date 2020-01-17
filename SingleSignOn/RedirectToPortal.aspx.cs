using SingleSignOn.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SingleSignOn
{
    public partial class RedirectToPortal : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {

                //var userData = db.GetSSOLoginDetail(LoggedInAgentID).FirstOrDefault();
                //if (userData == null)
                //    return;

                var ssoLoginData = new SSOLoginData();
                //ssoLoginData.Email = userData.PrimaryEmail;
                //ssoLoginData.AdditionalEmail1 = userData.SecondaryEmail;
                //ssoLoginData.AdditionalEmail2 = userData.SalesRadixEmail;
                //ssoLoginData.FirstName = userData.FirstName;
                //ssoLoginData.LastName = userData.LastName;
                //ssoLoginData.RoleId = userData.Role_ID;
                //ssoLoginData.GlobalUserId = userData.GlobalUserId;
                //ssoLoginData.ManagerGlobalUserId = userData.ManagerGlobalUserId;

                var tokenData = GetAuthenticationToken(ssoLoginData);

                if (!string.IsNullOrWhiteSpace(tokenData))
                {
                    var encoding = new UnicodeEncoding();
                    var bytes = encoding.GetBytes(tokenData);
                   // PortalTokenData.Value = Convert.ToBase64String(bytes);
                }


            }
            catch (Exception ex)
            {
                // Logger.Log(ex);
            }
            finally
            {
                string portalAuthUrl = $"{ConfigurationManager.AppSettings["PortalURL"]}/ExternalLogin/Authenticate";
                form1.Action = portalAuthUrl;
            }
        }

        private string GetAuthenticationToken(SSOLoginData ssoLoginData)
        {

            string portalAPIUrl = $"{ConfigurationManager.AppSettings["PortalAPIURL"]}/api/auth/token";

            var samlRequestData = SAML2Helper.GetSamlBase64StringToGetToken(ssoLoginData);

            var client = new HttpClient();
            var postData = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("grant_type", "password"),
                       // new KeyValuePair<string, string>("username", ssoLoginData.GlobalUserId.ToString()),
                        new KeyValuePair<string, string>("password", samlRequestData),
                        new KeyValuePair<string, string>("scope", "ssorequest")
                    };

            var response = client.PostAsync(portalAPIUrl, new FormUrlEncodedContent(postData)).Result;

            if (!response.IsSuccessStatusCode)
                return null;

            return response.Content.ReadAsStringAsync().Result;

        }
    }
}