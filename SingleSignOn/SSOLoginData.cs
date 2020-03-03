using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SingleSignOn
{
    public class SSOLoginData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }

		public String UserName { get; set; }
		public string Country { get; set; }
        public string City { get; set; }
        public string Department { get; set; }
        public string PhoneNumber { get; set; }
        public string GroupMembership { get; set; }
        public string ErrorUrl { get; set; }
    }
}