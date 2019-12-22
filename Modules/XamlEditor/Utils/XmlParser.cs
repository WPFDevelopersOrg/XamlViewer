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
        private XmlSchema _schema = null;
        private string _fileName = AppDomain.CurrentDomain.BaseDirectory + "Modules\\Assets\\XamlPresentation2006.xsd";

        public bool Parse()
        {
            if (!File.Exists(_fileName))
                return false;

            try
            {  
                var schemaSet = new XmlSchemaSet();
                schemaSet.ValidationEventHandler += (s, e) => {  };

                _schema = schemaSet.Add(null, _fileName);
                schemaSet.Compile(); 

                return true;
            }
            catch
            {
                return false;
            }
        }


    }
}
