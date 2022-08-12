using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Xml.XPath;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;

namespace HardCodeStringLocalizer.FileProcesser.LocalizeProcessers
{
    /// <summary>
    /// Xaml文件本地化
    /// </summary>
    internal class XamlFileLocalizer : FileLocalizerBase
    {
        public override string SupportedFileExtension => "xaml";

        protected override List<HardCodeStringInfo> LoadStringNodesInFile(string fullFilePath)
        {
            var group = new List<HardCodeStringInfo>();
            var xdoc = XDocument.Load(fullFilePath, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo);
            var elemtns = xdoc.XPathSelectElements("//*[@Title or @Text or @Value or @Content or @Header or @ToolTip]");
            var listAcceptAttrNames = new HashSet<string> { "Title", "Text", "Value", "Content", "Header", "ToolTip" };
            foreach (var element in elemtns)
            {
                var lineInfo = (IXmlLineInfo)element;

                foreach (var attr in element.Attributes().Where(attr => listAcceptAttrNames.Contains(attr.Name.LocalName)))
                {
                    group.Add(new XamlHardCodeStringInfo(fullFilePath, attr));
                    var attrLineInfo = (IXmlLineInfo)attr;
                }
            }
            return group;
        }

        /// <summary>
        /// Xaml文件中的硬编码字符串
        /// </summary>
        class XamlHardCodeStringInfo : HardCodeStringInfo
        {
            public XAttribute XAttribute { get; private set; }

            public XamlHardCodeStringInfo(string filePath, XAttribute xAttr) : base(filePath, xAttr.ToString())
            {
                XAttribute = new XAttribute(xAttr);
                var lineInfo = (IXmlLineInfo)xAttr;
                var startPosition = new LinePosition(lineInfo.LineNumber - 1, lineInfo.LinePosition - 1);
                var endPosition = new LinePosition(lineInfo.LineNumber - 1, lineInfo.LinePosition - 1 + xAttr.ToString().Length);
                TextRange = new FileLinePositionSpan("", startPosition, endPosition);
            }
            public override string GetResourceValue()
            {
                return XAttribute.Value;
            }
            public override string BuildReplaceString(string resourceName)
            {
                return $"{XAttribute.Name}=\"{{cc:LocalizedHelper {resourceName}}}\"";
            }

        }
    }
}
