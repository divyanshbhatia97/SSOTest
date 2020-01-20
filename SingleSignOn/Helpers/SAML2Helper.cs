using Microsoft.IdentityModel.SecurityTokenService;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml2;
using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;

namespace SingleSignOn.Helpers
{

    public class SAML2Helper
    {
        const string SAML2_Protocol = "urn:oasis:names:tc:SAML:2.0:protocol";
        const string SAML2_Assertion = "urn:oasis:names:tc:SAML:2.0:assertion";
        const string DateTimeFormat = "yyyy-MM-ddThh:mm:ss.fffZ";

        static string certPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigurationManager.AppSettings["SSOCertificatePath"]);
        static string certPwd = ConfigurationManager.AppSettings["SSOCertificatePassword"];
        static string SSOEncryptionKey = ConfigurationManager.AppSettings["SSOEncryptionKey"];


        #region To Genrate SAML for Token Request
        public static string GetSamlBase64StringToGetToken(SSOLoginData ssoLoginData)
        {
            var xmlDoc = GetSamlXmlDocToGetToken(ssoLoginData);
          //  EncryptAssertionInDoc(xmlDoc);
            return GetBase64String(xmlDoc);
        }

        private static XmlDocument GetSamlXmlDocToGetToken(SSOLoginData ssoLoginData)
        {         
            var xmlDoc = new XmlDocument();

            var requestId = Guid.NewGuid().ToString();
            var elemReq = CreateRequestNode(xmlDoc, requestId);
            xmlDoc.AppendChild(elemReq);

            var samlAssertion = CreateSamlAssertionsForTokenRequest(requestId, ssoLoginData);

            AppendSamlAssertion(xmlDoc, elemReq, samlAssertion);

            return xmlDoc;
        }

        private static Saml2Assertion CreateSamlAssertionsForTokenRequest(string requestId, SSOLoginData ssoLoginData)
        {
            if (!File.Exists(certPath))
            {
                throw new Exception($"Unable to find Certificate.  Path: {certPath}");
            }

            var assertion = new Saml2Assertion(new Saml2NameIdentifier("SSO"));
            assertion.Subject = new Saml2Subject(new Saml2NameIdentifier($"AxcoRequest : {requestId}"));

            assertion.Conditions = new Saml2Conditions()
            {
                NotBefore = DateTime.Now.AddSeconds(-30),
                NotOnOrAfter = DateTime.Now.AddSeconds(30),
            };

            var statement = new Saml2AttributeStatement();
            AddAttributeToStatement("UserName", ssoLoginData.Email, statement);
            AddAttributeToStatement("FirstName", ssoLoginData.FirstName, statement);
            AddAttributeToStatement("LastName", ssoLoginData.LastName, statement);
            AddAttributeToStatement("Country", ssoLoginData.Country, statement);
            AddAttributeToStatement("City", ssoLoginData.City, statement);
            AddAttributeToStatement("Department", ssoLoginData.Department, statement);
            AddAttributeToStatement("PhoneNumber", ssoLoginData.PhoneNumber, statement);
            AddAttributeToStatement("GroupMembership", ssoLoginData.GroupMembership, statement);
            AddAttributeToStatement("ErrorUrl", ssoLoginData.ErrorUrl, statement);

            assertion.Statements.Add(statement);

            var x509 = new X509Certificate2();
            x509.Import(certPath, certPwd, X509KeyStorageFlags.MachineKeySet);

            var clientSigningCreds = new System.IdentityModel.Tokens.X509SigningCredentials(x509);        
            assertion.SigningCredentials = clientSigningCreds;

            return assertion;
        }
        #endregion

        #region Genrate SAML to Send to Portal UI
      
        #endregion

        #region
        private static XmlElement CreateRequestNode(XmlDocument doc, string requestId)
        {
            var elem = doc.CreateElement("Request", SAML2_Protocol);
            elem.SetAttribute("id", requestId);
            elem.SetAttribute("IssueInstant", DateTime.UtcNow.ToString(DateTimeFormat));
            elem.SetAttribute("Version", "2.0");
            return elem;
        }
        private static string GetBase64String(XmlDocument xmlDocument)
        {
            var xmlStr = ConvertXmlDocToString(xmlDocument);
            var encoding = new UnicodeEncoding();
            var bytes = encoding.GetBytes(xmlStr);
            return Convert.ToBase64String(bytes);
        }
        private static string ConvertXmlDocToString(XmlDocument xmlDocument)
        {
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings();
            using (var writer = XmlWriter.Create(sb, settings))
            {
                xmlDocument.WriteTo(writer);
            }
            var xmlStr = sb.ToString();
            return xmlStr;
        }
        private static void AddAttributeToStatement(string attrName,string attrValue, Saml2AttributeStatement statement)
        {
            var attribute = new Saml2Attribute(attrName);
            attribute.Values.Add(attrValue);
            statement.Attributes.Add(attribute);
        }
        private static void AppendSamlAssertion(XmlDocument xmlDocument, XmlElement xmlElementToAppendTo, Saml2Assertion samlAssertion)
        {
            var samlXmlDoc = CreateXmlDocFromSamlAssertion(samlAssertion);
            var deep = true;
            var xmlAssertion = (XmlElement)xmlDocument.ImportNode(samlXmlDoc.DocumentElement, deep);          
            xmlElementToAppendTo.AppendChild(xmlAssertion);
        }
        private static XmlDocument CreateXmlDocFromSamlAssertion(Saml2Assertion assertion)
        {
            var serializer = new Saml2Serializer();
            var sb = new StringBuilder();
            var settings = new XmlWriterSettings();
            using (var writer = XmlWriter.Create(sb, settings))
            {
                serializer.WriteSaml2Assertion(writer, assertion);
            }

            var doc = new XmlDocument();
            doc.LoadXml(sb.ToString());
            return doc;
        }

        #endregion

        #region Encryption Related Methods
        private static void EncryptAssertionInDoc(XmlDocument xmlDoc)
        {
            var rijndaelManagedAlgo = new RijndaelManaged();
            rijndaelManagedAlgo.Key = Encoding.ASCII.GetBytes(SSOEncryptionKey);
            rijndaelManagedAlgo.Padding = PaddingMode.None;
            EncryptElement(xmlDoc, "Assertion", rijndaelManagedAlgo);
        }
        private static void EncryptElement(XmlDocument Doc, string ElementName, SymmetricAlgorithm Key)
        {
            XmlElement elementToEncrypt = Doc.GetElementsByTagName(ElementName)[0] as XmlElement;
            EncryptedXml eXml = new EncryptedXml();
            byte[] encryptedElement = eXml.EncryptData(elementToEncrypt, Key, false);
            EncryptedData edElement = new EncryptedData();
            edElement.Type = EncryptedXml.XmlEncElementUrl;
            string encryptionMethod = null;

            if (Key is Rijndael)
            {
                switch (Key.KeySize)
                {
                    case 128:
                        encryptionMethod = EncryptedXml.XmlEncAES128Url;

                        break;
                    case 192:
                        encryptionMethod = EncryptedXml.XmlEncAES192Url;
                        break;
                    case 256:
                        encryptionMethod = EncryptedXml.XmlEncAES256Url;
                        break;
                }
            }
            else
            {
                throw new CryptographicException("The specified algorithm is not supported for XML Encryption.");
            }

            edElement.EncryptionMethod = new EncryptionMethod(encryptionMethod);
            edElement.CipherData.CipherValue = encryptedElement;
            EncryptedXml.ReplaceElement(elementToEncrypt, edElement, false);
        }
        #endregion

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