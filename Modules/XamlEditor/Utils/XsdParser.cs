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
        private XmlSchemaSet _schemaSet = null;

        private readonly string _fileName = null;
        private readonly List<XmlSchemaSimpleType> _simpleTypes = null;

        public XsdParser()
        {
            _fileName = AppDomain.CurrentDomain.BaseDirectory + "Modules\\Assets\\XamlPresentation2006.xsd";
            _simpleTypes = new List<XmlSchemaSimpleType>();
        }

        public bool TryParse()
        {
            if (!File.Exists(_fileName))
                return false;

            try
            {
                _schemaSet = new XmlSchemaSet();
                //_schemaSet.ValidationEventHandler += (s, e) => { };

                _schema = _schemaSet.Add(null, _fileName);
                _schemaSet.Compile();

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

        public void Dispose()
        {
            if (_schemaSet != null && _schema != null)
            {
                _schemaSet.RemoveRecursive(_schema);

                _schema = null;
                _schemaSet = null;
            }
        }

        public List<string> GetElements()
        {
            if (_schema == null)
                return null;

            var results = new List<string>();

            foreach (XmlSchemaElement element in _schema.Elements.Values)
            {
                if (!string.IsNullOrEmpty(element.Name) && char.IsUpper(element.Name, 0))
                    results.Add(element.Name);
            }

            return results.OrderBy(s => s).ToList();
        }

        public List<string> GetChildElements(string parentElementName)
        {
            var results = new List<string>();

            if (parentElementName.Contains("."))
            {
                var element = GetElement(parentElementName.Split('.')[0]);

                var elements = new List<XmlSchemaElement>();
                GetAttributesFromElements(element, elements);

                element = elements.FirstOrDefault(e => e.Name == parentElementName);
                if (element != null)
                {
                    var complexType = element.ElementSchemaType as XmlSchemaComplexType;
                    if (complexType != null)
                    {
                        var choice = complexType.Particle as XmlSchemaChoice;
                        GetChildElements(choice, results);
                    }
                }
            }
            else
            {
                var element = GetElement(parentElementName);
                GetChildElements(element, results);
            }

            return results.OrderBy(s => s).ToList();
        }

        public List<string> GetAttributes(string elementName)
        {
            var element = GetElement(elementName);

            var attributes = new List<XmlSchemaAttribute>();
            GetAttributes(element, attributes);

            var elements = new List<XmlSchemaElement>();
            GetAttributesFromElements(element, elements);

            if (elements.Count == 0)
                return attributes.Select(a => a.Name.Contains(".") ? a.Name.Split('.')[1] : a.Name).Distinct().OrderBy(n => n).ToList();

            var resultsFromElemts = elements.Select(a => a.Name.Contains(".") ? a.Name.Split('.')[1] : a.Name).ToList();
            resultsFromElemts.AddRange(attributes.Select(a => a.Name.Contains(".") ? a.Name.Split('.')[1] : a.Name));

            return resultsFromElemts.Distinct().OrderBy(n => n).ToList();
        }

        //x:...
        public List<string> GetNamespaceAttributes()
        {
            return new List<string> { "Class", "Key", "Name", "Subclass" };
        }

        //="{...
        public List<string> GetReferenceTypeValues()
        {
            return new List<string> { "StaticResource", "DynamicResource", "Binding", "x:Static", "x:Null" };
        }

        public List<string> GetValues(string elementName, string attributeName)
        {
            var results = new List<string>();

            var element = GetElement(elementName);
            if (element != null)
            {
                var attribute = GetAttribute(element, attributeName);
                if (attribute != null)
                    GetSimpleTypeValues(attribute.AttributeSchemaType.Content, results);
            }

            return results.OrderBy(s => s).ToList();
        }

        private void GetChildElements(XmlSchemaElement element, List<string> resultElements)
        {
            if (element == null)
                return;

            var complexType = element.ElementSchemaType as XmlSchemaComplexType;
            if (complexType == null)
                return;

            //just for Sequence
            var sequence = complexType.Particle as XmlSchemaSequence;
            if (sequence == null)
                return;

            foreach (var item in sequence.Items)
            {
                var group = item as XmlSchemaGroupRef;
                if (group == null || !group.RefName.Name.StartsWith("c"))
                    continue;

                var choice = group.Particle as XmlSchemaChoice;
                GetChildElements(choice, resultElements);
            }
        }

        private void GetChildElements(XmlSchemaChoice choice, List<string> resultElements)
        {
            if (choice == null)
                return;

            foreach (var c in choice.Items)
            {
                var xse = c as XmlSchemaElement;
                if (xse != null)
                {
                    var name = xse.RefName.Name;
                    if (!string.IsNullOrEmpty(name))
                    {
                        if (char.IsUpper(name, 0))
                            resultElements.Add(name);
                        else if (name.StartsWith("sg"))
                            resultElements.Add(name.Substring(2));
                    }

                    continue;
                }

                var xsgr = c as XmlSchemaChoice;
                if (xsgr != null)
                    GetChildElements(xsgr, resultElements);
            }
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

        private void GetAttributesFromElements(XmlSchemaElement element, List<XmlSchemaElement> resultAttributes)
        {
            if (element == null)
                return;

            var complexType = element.ElementSchemaType as XmlSchemaComplexType;
            if (complexType == null)
                return;

            //just for Sequence
            var sequence = complexType.Particle as XmlSchemaSequence;
            if (sequence != null)
            {
                //just parse first peXXXX
                foreach (var item in sequence.Items)
                {
                    var group = item as XmlSchemaGroupRef;
                    if (group == null || !group.RefName.Name.StartsWith("pe"))
                        continue;

                    var choice = group.Particle as XmlSchemaChoice;
                    if (choice == null)
                        break;

                    foreach (var c in choice.Items)
                    {
                        var xse = c as XmlSchemaElement;
                        if (xse != null && !string.IsNullOrEmpty(xse.Name))
                            resultAttributes.Add(xse);
                    }

                    break;
                }
            }
        }

        private void GetAttributes(XmlSchemaElement element, List<XmlSchemaAttribute> resultAttributes)
        {
            if (element == null)
                return;

            var complexType = element.ElementSchemaType as XmlSchemaComplexType;
            if (complexType == null)
                return;

            GetAttributes(complexType.Attributes, resultAttributes);
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
