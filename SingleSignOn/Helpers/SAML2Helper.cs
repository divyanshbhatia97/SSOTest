using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace SingleSignOn.Helpers
{

	public class SAML2Helper
	{
		const string SAML2_Protocol = "urn:oasis:names:tc:SAML:2.0:protocol";
		const string SAML2_Basic = "urn:oasis:names:tc:SAML:2.0:attrname-format:basic";
		const string SAML2_Assertion = "urn:oasis:names:tc:SAML:2.0:assertion";
		const string SAML2_Bearer = "urn:oasis:names:tc:SAML:2.0:cm:bearer";
		const string SAML2_Password = "urn:oasis:names:tc:SAML:2.0:ac:classes:Password";
		const string DateTimeFormat = "yyyy-MM-ddThh:mm:ss.fffZ";
		const string SAML2_Success = "urn:oasis:names:tc:SAML:2.0:status:Success";

		static readonly string certPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["SSOCertificatePath"]);
		static readonly string certPwd = ConfigurationManager.AppSettings["SSOCertificatePassword"];
		static readonly string recepient = "https://dev.axco.co.uk/Axco.sso/saml2/v1/signin/AssurexGlobal";

		public static string GetSamlBase64StringToGetToken(SSOLoginData ssoLoginData)
		{
			var xmlDoc = GetDoc(ssoLoginData);
			return GetBase64String(xmlDoc);
		}

		private static XmlDocument GetDoc(SSOLoginData ssoLoginData)
		{
			var id = "_" + Guid.NewGuid().ToString().Replace("-", "");

			var xmlDoc = new XmlDocument();

			var response = xmlDoc.CreateElement("samlp", "Response", SAML2_Protocol);
			response.SetAttribute("ID", id);
			response.SetAttribute("Version", "2.0");
			response.SetAttribute("IssueInstant", DateTime.UtcNow.ToString(DateTimeFormat));
			response.SetAttribute("Destination", "https://dev.axco.co.uk/Axco.sso/saml2/v1/signin/AssurexGlobal");

			var docIssuer = xmlDoc.CreateElement("saml", "Issuer", SAML2_Assertion);
			docIssuer.InnerText = "Assurex Global Test System";
			response.AppendChild(docIssuer);

			var status = xmlDoc.CreateElement("samlp", "Status");
			var statuscode = xmlDoc.CreateElement("samlp", "StatusCode");
			statuscode.SetAttribute("Value", SAML2_Success);
			status.AppendChild(statuscode);
			response.AppendChild(status);

			var xmlAssertion = GetAssertion(xmlDoc, ssoLoginData);
			response.AppendChild(xmlAssertion);

			xmlDoc.AppendChild(response);

			xmlDoc.DocumentElement.AppendChild(GetSignature(xmlDoc, id));

			return xmlDoc;
		}

		private static XmlElement GetSignature(XmlDocument xmlDocument, string id)
		{
			var x509 = new X509Certificate2();
			x509.Import(certPath, certPwd, X509KeyStorageFlags.MachineKeySet);

			SignedXml signedXml = new SignedXml(xmlDocument)
			{
				SigningKey = x509.PrivateKey
			};
			//signedXml.SignedInfo.CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";
			signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

			Reference reference = new Reference("#" + id);

			reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());

			reference.AddTransform(new XmlDsigExcC14NTransform("#default samlp saml ds xs xsi"));

			signedXml.AddReference(reference);

			KeyInfo keyInfo = new KeyInfo();

			keyInfo.AddClause(new KeyInfoX509Data(x509));

			signedXml.KeyInfo = keyInfo;

			signedXml.ComputeSignature();

			return signedXml.GetXml();
		}

		private static XmlNode GetAssertion(XmlDocument xmlDoc, SSOLoginData ssoLoginData)
		{
			if (File.Exists(certPath))
			{
				var assertion = new Saml2Assertion(new Saml2NameIdentifier("Assurex Global Test System"));

				assertion.Subject = new Saml2Subject(new Saml2NameIdentifier("ckillilea@assurexglobal.com"));

				assertion.Subject.SubjectConfirmations.Add(new Saml2SubjectConfirmation(new Uri(SAML2_Bearer))
				{ SubjectConfirmationData = new Saml2SubjectConfirmationData() { Recipient = new Uri(recepient) } });

				assertion.Statements.Add(new Saml2AuthenticationStatement(new Saml2AuthenticationContext(new Uri(SAML2_Password))));

				List<Saml2Attribute> attributes = new List<Saml2Attribute>()
				{
					GetAttribute("UserName", ssoLoginData.Email),
					GetAttribute("FirstName", ssoLoginData.FirstName),
					GetAttribute("LastName", ssoLoginData.LastName),
					GetAttribute("Country", ssoLoginData.Country),
					GetAttribute("City", ssoLoginData.City),
					GetAttribute("Department", ssoLoginData.Department),
					GetAttribute("PhoneNumber", ssoLoginData.PhoneNumber),
					GetAttribute("GroupMembership", ssoLoginData.GroupMembership),
					GetAttribute("ErrorUrl", ssoLoginData.ErrorUrl),
				};

				assertion.Statements.Add(new Saml2AttributeStatement(attributes));

				var samlAssertion = assertion;

				var serializer = new Saml2Serializer();
				var sb = new StringBuilder();
				var settings = new XmlWriterSettings();
				using (var writer = XmlWriter.Create(sb, settings))
				{
					serializer.WriteSaml2Assertion(writer, samlAssertion);
				}

				var samlXmlDoc = new XmlDocument();
				samlXmlDoc.LoadXml(sb.ToString());

				var xmlAssertion = xmlDoc.ImportNode(samlXmlDoc.DocumentElement, true);

				return xmlAssertion;
			}
			else
			{
				throw new Exception($"Unable to find Certificate.  Path: {certPath}");
			}
		}

		private static string GetBase64String(XmlDocument xmlDocument)
		{
			var sb = new StringBuilder();
			var settings = new XmlWriterSettings();
			using (var writer = XmlWriter.Create(sb, settings))
			{
				xmlDocument.WriteTo(writer);
			}
			var xmlStr = sb.ToString();
			//return xmlStr;
			var encoding = new UnicodeEncoding();
			var bytes = encoding.GetBytes(xmlStr);
			return Convert.ToBase64String(bytes);
		}

		private static Saml2Attribute GetAttribute(string attrName, string attrValue)
		{
			var attribute = new Saml2Attribute(attrName, attrValue)
			{
				NameFormat = new Uri(SAML2_Basic)
			};
			return attribute;
		}

	}

	public class Saml2Serializer : Saml2SecurityTokenHandler
	{
		public Saml2Serializer()
		{
			Configuration = new SecurityTokenHandlerConfiguration()
			{

			};
		}

		public void WriteSaml2Assertion(XmlWriter writer, Saml2Assertion data)
		{
			base.WriteAssertion(writer, data);
		}
	}
}