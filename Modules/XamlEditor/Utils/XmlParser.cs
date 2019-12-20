using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace XamlEditor.Utils
{
    internal class XmlParser
    {
        XmlSchema schema = null;
        string fileName = AppDomain.CurrentDomain.BaseDirectory + "Modules\\Assets\\XamlPresentation2006.xsd";

        public void Parse()
        {
            XmlSchemaSet set = new XmlSchemaSet();

            using (var reader = new StreamReader(fileName, true))
            {
                using (var xmlReader = new XmlTextReader(string.Empty, reader))
                {
                    xmlReader.XmlResolver = null;
                    schema = XmlSchema.Read(xmlReader, SchemaValidation);
                }
            }
            set.Add(schema);
            set.Compile();
        }

        void SchemaValidation(object source, ValidationEventArgs e)
        {
            // Do nothing.
        }
    }
}
