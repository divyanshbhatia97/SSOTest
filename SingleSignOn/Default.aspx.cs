using ComponentSpace.Saml2;
using SingleSignOn.Helpers;
using System;
using System.Text;
using System.Web.UI;

namespace SingleSignOn
{
	public partial class _Default : Page
	{
		protected void btnTestSSO_Click(object sender, EventArgs e)
		{
			SSOLoginData ssoLoginData = new SSOLoginData()
			{
				Email = "ckillilea@assurexglobal.com",
				UserName= "ckillilea@assurexglobal.com",
				City = "New York",
				Country = "US",
				Department = "IT",
				ErrorUrl = "https://errorsite.com/errorpage.aspx",
				FirstName = "John",
				LastName = "Smith",
				PhoneNumber = "001354534643",
				GroupMembership = "InsightCompliance"
			};
			var SAMLResponse = SAML2Helper.GetSamlBase64StringToGetToken(ssoLoginData);

			Response.Clear();
			string postbackUrl = "https://dev.axco.co.uk/Axco.sso/saml2/v1/signin/AssurexGlobal";

			var RelayState = "";
			StringBuilder sb = new StringBuilder();

			sb.Append("<html>");
			sb.AppendFormat(@"<body onload='document.forms[""form""].submit()'>");
			sb.AppendFormat("<form name='form' action='{0}' method='POST'>", postbackUrl);
			sb.AppendFormat("<input type='hidden' id='SAMLResponse' name='SAMLResponse' value='{0}'>", SAMLResponse);
			sb.AppendFormat("<input type='hidden' id='RelayState' name='RelayState' value='{0}'>", RelayState);
			sb.Append("</form>");
			sb.Append("</body>");
			sb.Append("</html>");

			Response.Write(sb.ToString());

			Response.End();
		}
	}
}