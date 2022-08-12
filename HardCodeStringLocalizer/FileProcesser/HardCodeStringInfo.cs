
using System.Xml.Linq;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace HardCodeStringLocalizer.FileProcesser
{
    /// <summary>
    /// 硬编码字符串信息
    /// </summary>
    abstract class HardCodeStringInfo
    {
        public HardCodeStringInfo(string filePath, string orignalText)
        {
            FilePath = filePath;
            OrignalText = orignalText;
        }
        /// <summary>
        /// 文件路径
        /// </summary>
        public string FilePath { get; private set; }
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName => Path.GetFileName(FilePath ?? "");
        /// <summary>
        /// 原始文本
        /// </summary>
        public string OrignalText { get; private set; }
        /// <summary>
        /// 文本位置
        /// </summary>
        public FileLinePositionSpan TextRange { get; protected set; }
        /// <summary>
        /// 从原始文本获取资源值
        /// </summary>
        public abstract string GetResourceValue();

        /// <summary>
        /// 构建替换字符串
        /// </summary>
        public abstract string BuildReplaceString(string resourceName);

    }
}
