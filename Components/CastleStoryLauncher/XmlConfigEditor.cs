using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace CastleStoryLauncher
{
    public class XmlConfigEditor
    {
        private XDocument document;
        private string filePath;

        public XmlConfigEditor(string filePath)
        {
            this.filePath = filePath;
            if (File.Exists(filePath))
            {
                LoadXmlFile(filePath);
            }
            else
            {
                document = new XDocument(new XElement("root"));
            }
        }

        public bool LoadXmlFile(string path)
        {
            try
            {
                document = XDocument.Load(path);
                filePath = path;
                return true;
            }
            catch
            {
                document = new XDocument(new XElement("root"));
                return false;
            }
        }

        public bool SaveXmlFile(string? path = null, bool indent = true)
        {
            try
            {
                string savePath = path ?? filePath;
                
                var settings = new XmlWriterSettings
                {
                    Indent = indent,
                    IndentChars = "  ",
                    OmitXmlDeclaration = false
                };

                using (var writer = XmlWriter.Create(savePath, settings))
                {
                    document.Save(writer);
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public XElement? GetElement(string xpath)
        {
            try
            {
                return document.XPathSelectElement(xpath);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<XElement> GetElements(string xpath)
        {
            try
            {
                return document.XPathSelectElements(xpath);
            }
            catch
            {
                return Enumerable.Empty<XElement>();
            }
        }

        public string? GetValue(string xpath)
        {
            try
            {
                var element = document.XPathSelectElement(xpath);
                return element?.Value;
            }
            catch
            {
                return null;
            }
        }

        public string? GetAttribute(string xpath, string attributeName)
        {
            try
            {
                var element = document.XPathSelectElement(xpath);
                return element?.Attribute(attributeName)?.Value;
            }
            catch
            {
                return null;
            }
        }

        public bool SetValue(string xpath, string value)
        {
            try
            {
                var element = document.XPathSelectElement(xpath);
                if (element != null)
                {
                    element.Value = value;
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool SetAttribute(string xpath, string attributeName, string value)
        {
            try
            {
                var element = document.XPathSelectElement(xpath);
                if (element != null)
                {
                    element.SetAttributeValue(attributeName, value);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool AddElement(string parentXpath, string elementName, string? value = null)
        {
            try
            {
                var parent = document.XPathSelectElement(parentXpath);
                if (parent != null)
                {
                    var newElement = new XElement(elementName);
                    if (value != null)
                    {
                        newElement.Value = value;
                    }
                    parent.Add(newElement);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveElement(string xpath)
        {
            try
            {
                var element = document.XPathSelectElement(xpath);
                if (element != null)
                {
                    element.Remove();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public Dictionary<string, string> GetAllAttributes(string xpath)
        {
            var attributes = new Dictionary<string, string>();
            
            try
            {
                var element = document.XPathSelectElement(xpath);
                if (element != null)
                {
                    foreach (var attr in element.Attributes())
                    {
                        attributes[attr.Name.LocalName] = attr.Value;
                    }
                }
            }
            catch
            {
                // Return empty dictionary on error
            }
            
            return attributes;
        }

        public List<string> GetAllElementPaths()
        {
            var paths = new List<string>();
            if (document.Root != null)
            {
                CollectPaths(document.Root, "/" + document.Root.Name.LocalName, paths);
            }
            return paths;
        }

        private void CollectPaths(XElement element, string currentPath, List<string> paths)
        {
            paths.Add(currentPath);

            foreach (var child in element.Elements())
            {
                string childPath = currentPath + "/" + child.Name.LocalName;
                CollectPaths(child, childPath, paths);
            }
        }

        public bool ValidateAgainstSchema(string schemaPath, out List<string> errors)
        {
            errors = new List<string>();
            var errorList = errors; // Create local variable for use in lambda
            
            try
            {
                var schemas = new XmlSchemaSet();
                schemas.Add("", schemaPath);

                document.Validate(schemas, (sender, e) =>
                {
                    errorList.Add(e.Message);
                });

                return errorList.Count == 0;
            }
            catch (Exception ex)
            {
                errorList.Add(ex.Message);
                errors = errorList;
                return false;
            }
        }

        public List<string> SearchElements(string searchTerm, bool searchValues = true, bool searchAttributes = true)
        {
            var matches = new List<string>();

            if (document.Root != null)
            {
                SearchElementsRecursive(document.Root, "/" + document.Root.Name.LocalName, searchTerm, searchValues, searchAttributes, matches);
            }

            return matches;
        }

        private void SearchElementsRecursive(XElement element, string currentPath, string searchTerm, bool searchValues, bool searchAttributes, List<string> matches)
        {
            // Search element name
            if (element.Name.LocalName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(currentPath + " (element name)");
            }

            // Search element value
            if (searchValues && !string.IsNullOrWhiteSpace(element.Value))
            {
                if (element.Value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add(currentPath + " (value)");
                }
            }

            // Search attributes
            if (searchAttributes)
            {
                foreach (var attr in element.Attributes())
                {
                    if (attr.Name.LocalName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                        attr.Value.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    {
                        matches.Add(currentPath + "/@" + attr.Name.LocalName);
                    }
                }
            }

            // Recurse to children
            foreach (var child in element.Elements())
            {
                string childPath = currentPath + "/" + child.Name.LocalName;
                SearchElementsRecursive(child, childPath, searchTerm, searchValues, searchAttributes, matches);
            }
        }

        public string GetFormattedXml(bool indent = true)
        {
            var settings = new XmlWriterSettings
            {
                Indent = indent,
                IndentChars = "  ",
                OmitXmlDeclaration = false
            };

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                document.Save(xmlWriter);
                return stringWriter.ToString();
            }
        }

        public Dictionary<string, string> GetFlatKeyValuePairs(string separator = "/")
        {
            var result = new Dictionary<string, string>();

            if (document.Root != null)
            {
                FlattenElement(document.Root, document.Root.Name.LocalName, separator, result);
            }

            return result;
        }

        private void FlattenElement(XElement element, string currentPath, string separator, Dictionary<string, string> result)
        {
            // Add element value if it has no children (leaf node)
            if (!element.HasElements && !string.IsNullOrWhiteSpace(element.Value))
            {
                result[currentPath] = element.Value;
            }

            // Add attributes
            foreach (var attr in element.Attributes())
            {
                result[currentPath + separator + "@" + attr.Name.LocalName] = attr.Value;
            }

            // Recurse to children
            foreach (var child in element.Elements())
            {
                string childPath = currentPath + separator + child.Name.LocalName;
                FlattenElement(child, childPath, separator, result);
            }
        }

        public bool MergeWith(XmlConfigEditor other, bool overwriteExisting = true)
        {
            try
            {
                if (document.Root == null || other.document.Root == null)
                    return false;

                MergeElements(document.Root, other.document.Root, overwriteExisting);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void MergeElements(XElement target, XElement source, bool overwriteExisting)
        {
            // Merge attributes
            foreach (var attr in source.Attributes())
            {
                if (overwriteExisting || target.Attribute(attr.Name) == null)
                {
                    target.SetAttributeValue(attr.Name, attr.Value);
                }
            }

            // Merge child elements
            foreach (var sourceChild in source.Elements())
            {
                var targetChild = target.Elements(sourceChild.Name).FirstOrDefault();
                
                if (targetChild != null)
                {
                    // Recursively merge existing child
                    MergeElements(targetChild, sourceChild, overwriteExisting);
                }
                else
                {
                    // Add new child
                    target.Add(new XElement(sourceChild));
                }
            }
        }

        public bool TransformWith(string xsltPath, string outputPath)
        {
            try
            {
                var transform = new System.Xml.Xsl.XslCompiledTransform();
                transform.Load(xsltPath);
                
                using (var writer = XmlWriter.Create(outputPath))
                {
                    transform.Transform(document.CreateReader(), writer);
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public int CountElements(string xpath)
        {
            try
            {
                return document.XPathSelectElements(xpath).Count();
            }
            catch
            {
                return 0;
            }
        }

        public XDocument GetDocument()
        {
            return new XDocument(document);
        }

        public void Clear()
        {
            document = new XDocument(new XElement("root"));
        }
    }
}

