using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StegoApp
{
    public static class Message
    {

        public static string MakeXml(User from,string message)
        {

            string formattedMsg;

            using (StringWriter strWriter = new StringWriter())
            using (XmlWriter xmlWriter = XmlWriter.Create(strWriter))
            {

                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("message");
                xmlWriter.WriteAttributeString("from", from.Nickname);
                xmlWriter.WriteAttributeString("sendDateTime",
                    XmlConvert.ToString(DateTime.Now, XmlDateTimeSerializationMode.Utc));

                xmlWriter.WriteElementString("content", message);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();

                formattedMsg = strWriter.ToString();
            }

            return formattedMsg;

        }

        public static (string, User, DateTime) ParseXml(string xmlString)
        {

            User user;
            string message;
            DateTime dateTime;

            using (StringReader strReader = new StringReader(xmlString))
            using (XmlReader xmlReader = XmlReader.Create(strReader))
            {


                xmlReader.Read(); // Skip document start
                xmlReader.Read();
                xmlReader.MoveToNextAttribute(); // Get to the "from" attribute
                user = User.AllUsers[xmlReader.Value];
                xmlReader.MoveToNextAttribute(); // Get to the "sendDateTime" attribute
                dateTime = xmlReader.ReadContentAsDateTime();
                xmlReader.ReadToFollowing("content");
                xmlReader.Read(); // Get message
                message = xmlReader.Value;

            }

            return (message, user, dateTime);

        }

    }
}
