using SingleSignOn.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SingleSignOn
{
    public partial class RedirectUsingSSO : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SSOLoginData ssoLoginData = new SSOLoginData()
            {
            };
            var samlRequestData = SAML2Helper.GetSamlBase64StringToGetToken(ssoLoginData);
            Response.Clear();
            string postbackUrl = "";
            var SAMLRequest = "";
            var RelayState = "";
            var id = "";
            StringBuilder sb = new StringBuilder();

            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='post'>", postbackUrl);
            sb.AppendFormat("<input type='hidden' name='SAMLRequest' value='{0}'>", SAMLRequest);
            sb.AppendFormat("<input type='hidden' name='RelayState' value='{0}'>", RelayState);
            sb.AppendFormat("<input type='hidden' name='id' value='{0}'>", id);
            // Other params go here
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            Response.Write(sb.ToString());

            Response.End();
        }
    }
}