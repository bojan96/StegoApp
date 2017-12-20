using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace StegoApp
{
    class User
    {

        public string FullName { get; }
        public string Nickname { get; }
        public string CertSerial { get; }
        public string PasswHash { get; }

        public static Dictionary<string, User> AllUsers { get; private set } = null;

        const string XML_PATH = "users.xml";
        const string XML_SCHEMA = "users.xsd";

        User(string fullName, string nickname, string certSerial, string passwHash)
        {

            FullName = fullName;
            Nickname = nickname;
            CertSerial = certSerial;
            PasswHash = passwHash;
            
        }

        // Loads users from XML_PATH, performs validation against XML_SCHEMA
        public static void LoadUsers()
        {

            Dictionary<string, User> allUsers = new Dictionary<string, User>();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(XML_PATH);

            ValidateXmlDoc(xmlDoc);
            var rootElem = xmlDoc.DocumentElement;

            foreach(XmlNode childNode in rootElem.ChildNodes)
            {

                var user = GetUserFromXmlNode(childNode);
                allUsers.Add(user.Nickname, user);

            }

            AllUsers = allUsers;
        }

        static User GetUserFromXmlNode(XmlNode xmlNode)
        {

            string fullName;
            string nickname;
            string certSerial;
            string passwHash;

            fullName = xmlNode.ChildNodes[0].FirstChild.Value;
            nickname = xmlNode.ChildNodes[1].FirstChild.Value;
            certSerial = xmlNode.ChildNodes[2].FirstChild.Value;
            passwHash = xmlNode.ChildNodes[3].FirstChild.Value;

            return new User(fullName, nickname, certSerial, passwHash);
        }


        static void ValidateXmlDoc(XmlDocument xmlDoc)
        {

            XmlSchema schema = new XmlSchema();
            xmlDoc.Schemas = new XmlSchemaSet();
            xmlDoc.Schemas.Add(null, XML_SCHEMA);
            
            try
            {

                xmlDoc.Validate(null);

            }
            catch (XmlSchemaValidationException ex)
            {
                
                //Todo: Add code to indentify which element produced error
                throw new XmlSchemaValidationException($"Schema validation error: {ex.Message}");

            }


        }

    }
}
