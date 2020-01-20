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
    public partial class Test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            SSOLoginData ssoLoginData = new SSOLoginData()
            {
                Email = "mytestemail@gmail.com",
                City = "New York",
                Country = "US",
                Department = "IT",
                ErrorUrl = "https://errorsite.com/errorpage.aspx",
                FirstName = "John",
                LastName = "Smith",
                PhoneNumber = "001354534643",
                GroupMembership = ""
            };
            var samlRequestData = SAML2Helper.GetSamlBase64StringToGetToken(ssoLoginData);
            Response.Clear();
            string postbackUrl = "https://localhost:44391/api/values";
            var RelayState = "example.org";
            var id = Guid.NewGuid().ToString();
            StringBuilder sb = new StringBuilder();

            sb.Append("<html>");
            sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
            sb.AppendFormat("<form name='form' action='{0}' method='POST'>", postbackUrl);
            sb.AppendFormat("<input type='hidden' id='SAMLRequest' name='SAMLRequest' value='{0}'>", samlRequestData);
            sb.AppendFormat("<input type='hidden' id='RelayState' name='RelayState' value='{0}'>", RelayState);
            sb.AppendFormat("<input type='hidden' id='id' name='id' value='{0}'>", id);
            // Other params go here
            sb.Append("</form>");
            sb.Append("</body>");
            sb.Append("</html>");

            Response.Write(sb.ToString());

            Response.End();
        }
    }
}