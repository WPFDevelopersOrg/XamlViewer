using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;

namespace XamlEditor.Utils
{
    internal class XsdParser
    {
        private XmlSchema _schema = null;

        private readonly string _fileName = null;
        private readonly List<XmlSchemaSimpleType> _simpleTypes = null;

        public XsdParser()
        {
            _fileName = AppDomain.CurrentDomain.BaseDirectory + "Assets\\XamlPresentation2006.xsd";
            _simpleTypes = new List<XmlSchemaSimpleType>();
        }

        public bool TryParse()
        {
            if (!File.Exists(_fileName))
                return false;

            try
            {
                var schemaSet = new XmlSchemaSet();
                schemaSet.ValidationEventHandler += (s, e) => { };

                _schema = schemaSet.Add(null, _fileName);
                schemaSet.Compile();

                foreach (var item in _schema.Items)
                {
                    var simpleType = item as XmlSchemaSimpleType;

                    if (simpleType != null)
                        _simpleTypes.Add(simpleType);
                }

                return true;
            }
            catch
            {
                _schema = null;
                return false;
            }
        }

        public IEnumerable<string> GetElements()
        {
            if (_schema == null)
                return null;

            var results = new List<string>();

            foreach (XmlSchemaElement element in _schema.Elements.Values)
            {
                if (!string.IsNullOrEmpty(element.Name) && !char.IsLower(element.Name, 0))
                    results.Add(element.Name);
            }

            return results.OrderBy(s => s);
        }

        public IEnumerable<string> GetAttributes(string elementName)
        {
            var results = new List<XmlSchemaAttribute>();

            var element = GetElement(elementName);
            if (element != null)
                GetAttributes(element, results);

            return results.Select(a => a.Name).OrderBy(n => n);
        }

        //x:...
        public IEnumerable<string> GetNamespaceAttributes()
        {
            return new List<string> { "Class", "Key", "Name", "Subclass" };
        }

        //="{...
        public IEnumerable<string> GetReferenceTypeValues()
        {
            return new List<string> { "StaticResource", "DynamicResource", "Binding", "x:Static", "x:Null" };
        }

        public IEnumerable<string> GetValues(string elementName, string attributeName)
        {
            var results = new List<string>();

            var element = GetElement(elementName);
            if (element != null)
            {
                var attribute = GetAttribute(element, attributeName);
                if (attribute != null)
                    GetSimpleTypeValues(attribute.AttributeSchemaType.Content, results);
            }

            return results.OrderBy(s => s);
        }


        private void GetSimpleTypeValues(XmlSchemaSimpleTypeContent content, List<string> resultValues)
        {
            var restriction = content as XmlSchemaSimpleTypeRestriction;
            if (restriction != null)
            {
                //just get XmlSchemaEnumerationFacet
                foreach (var facet in restriction.Facets)
                {
                    var enumFacet = facet as XmlSchemaEnumerationFacet;
                    if (enumFacet != null)
                        resultValues.Add(enumFacet.Value);
                }
            }
            else
            {
                var union = content as XmlSchemaSimpleTypeUnion;
                if (union != null)
                {
                    foreach (var member in union.MemberTypes)
                    {
                        var simpleType = _simpleTypes.FirstOrDefault(t => t.Name == member.Name);
                        if (simpleType != null)
                        {
                            GetSimpleTypeValues(simpleType.Content, resultValues);
                        }

                    }
                }
            }
        }

        private XmlSchemaElement GetElement(string elementName)
        {
            foreach (XmlSchemaElement element in _schema.Elements.Values)
            {
                if (!string.IsNullOrEmpty(element.Name) && element.Name == elementName)
                {
                    return element;
                }
            }

            return null;
        }

        private XmlSchemaAttribute GetAttribute(XmlSchemaElement element, string attributeName)
        {
            var results = new List<XmlSchemaAttribute>();
            GetAttributes(element, results);

            return results.FirstOrDefault(a => a.Name == attributeName);
        }

        private void GetAttributes(XmlSchemaElement element, List<XmlSchemaAttribute> resultAttributes)
        {
            if (element == null)
                return;

            var complexType = element.ElementSchemaType as XmlSchemaComplexType;
            if (complexType != null)
            {
                GetAttributes(complexType.Attributes, resultAttributes);
            }
        }

        private void GetAttributes(string attributeGroupName, List<XmlSchemaAttribute> resultAttributes)
        {
            if (string.IsNullOrWhiteSpace(attributeGroupName))
                return;

            foreach (XmlSchemaAttributeGroup attributeGroup in _schema.AttributeGroups.Values)
            {
                if (attributeGroup.Name == attributeGroupName)
                {
                    GetAttributes(attributeGroup.Attributes, resultAttributes);
                    break;
                }
            }
        }

        private void GetAttributes(XmlSchemaObjectCollection xmlAttributes, List<XmlSchemaAttribute> resultAttributes)
        {
            if (xmlAttributes == null)
                return;

            for (int i = 0; i < xmlAttributes.Count; i++)
            {
                var obj = xmlAttributes[i];
                var attribute = obj as XmlSchemaAttribute;

                if (attribute == null)
                {
                    var attributeGroupRef = obj as XmlSchemaAttributeGroupRef;
                    if (attributeGroupRef != null && !string.IsNullOrEmpty(attributeGroupRef.RefName.Name))
                        GetAttributes(attributeGroupRef.RefName.Name, resultAttributes);
                }
                else if (!string.IsNullOrEmpty(attribute.Name))
                {
                    resultAttributes.Add(attribute);
                }
            }
        }
    }
}
