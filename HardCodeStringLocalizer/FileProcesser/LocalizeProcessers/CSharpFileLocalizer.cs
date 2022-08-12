using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace HardCodeStringLocalizer.FileProcesser.LocalizeProcessers
{
    /// <summary>
    /// CSharp文件本地化
    /// </summary>
    internal class CSharpFileLocalizer : FileLocalizerBase
    {
        public override string SupportedFileExtension => "cs";

        protected override List<HardCodeStringInfo> LoadStringNodesInFile(string fullFilePath)
        {
            var group = new List<HardCodeStringInfo>();
            var fileContent = System.IO.File.ReadAllText(fullFilePath);
            //使用C#编译器Roslyn来解析获取语法树
            SyntaxTree tree = CSharpSyntaxTree.ParseText(fileContent);
            //获取语法树根节点
            CompilationUnitSyntax root = tree.GetCompilationUnitRoot();
            //展平所有节点并从中筛选出字符串表达式和插值字符串表达式
            List<SyntaxNode> nodes = root.DescendantNodes().Where(node => node.IsKind(SyntaxKind.StringLiteralExpression) || node.IsKind(SyntaxKind.InterpolatedStringExpression)).ToList();
            foreach (var node in nodes)
            {
                //将节点转换为硬编码字符串信息
                group.Add(new CSharpHardCodeStringInfo(fullFilePath, node));
            }
            return group;
        }


        /// <summary>
        /// C#文件中的硬编码字符串信息
        /// </summary>
        class CSharpHardCodeStringInfo : HardCodeStringInfo
        {
            public CSharpHardCodeStringInfo(string filePath, SyntaxNode orignalNode) : base(filePath, orignalNode.ToString())
            {
                OrignalNode = orignalNode;
                IsRawString = orignalNode.IsKind(SyntaxKind.StringLiteralExpression);
                //一定要注意设置文本在文件中的位置,否则无法定位文本
                TextRange = orignalNode.SyntaxTree.GetLineSpan(orignalNode.Span);
            }
            /// <summary>
            /// 语法树节点
            /// </summary>
            public SyntaxNode OrignalNode { get; private set; }
            /// <summary>
            /// 是原始字符串,为false表示插值字符串,需要特殊处理
            /// </summary>
            public bool IsRawString { get; private set; }

            public override string GetResourceValue()
            {
                //原始字符串去除前后引号直接返回
                if (IsRawString)
                {
                    return OrignalText.Trim('"');
                }
                else
                {
                    //特殊处理插值字符串
                    var sb = new StringBuilder();
                    var interpolatedString = OrignalNode as InterpolatedStringExpressionSyntax;
                    var interpolateIndex = 0;
                    foreach (var childNode in interpolatedString.ChildNodes())
                    {
                        //例子: $"插值字符串的{"插值部分"}与纯文本部分"
                        switch (childNode.Kind())
                        {
                            //插值字符串中的纯文本部分直接拼接
                            //就是例子中的"插值字符串的"和"与纯文本部分"
                            case SyntaxKind.InterpolatedStringText:
                                sb.Append(childNode.ToString());
                                break;
                            //插值字符串中的插值标记转换为插值索引
                            //就是例子中的{"插值部分"}
                            case SyntaxKind.Interpolation:
                                sb.Append($"{{{interpolateIndex}}}");
                                interpolateIndex++;
                                break;
                        }
                    }
                    //处理结果为: "插值字符串的{0}与纯文本部分"
                    return sb.ToString();
                }
            }
            //替换硬编码字符串的代码，请根据项目中应用的实际本地化代码编写
            public override string BuildReplaceString(string resourceName)
            {
                if (IsRawString)
                    return $"Properties.Resources.{resourceName}/*{OrignalText}*/";
                else
                    return $"Properties.Resources.{resourceName}.FormatToString({OrignalText})";
            }

        }
    }
}
