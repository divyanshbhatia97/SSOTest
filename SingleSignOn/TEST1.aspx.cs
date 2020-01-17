using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SingleSignOn
{
    public partial class TEST1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var data = HttpContext.Current.Request.Form["SAMLRequest"];
        }
    }
}