using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HardCodeStringLocalizer.FileProcesser
{
    /// <summary>
    /// 文件本地化处理
    /// </summary>
    internal abstract class FileLocalizerBase
    {
        /// <summary>
        /// 处理类的缓存列表
        /// </summary>
        private static List<FileLocalizerBase> localisersCache;

        /// <summary>
        /// 支持的文件扩展名,如"cs","xaml"
        /// </summary>
        public abstract string SupportedFileExtension { get; }
        /// <summary>
        /// 扫描目录下的字符串
        /// </summary>
        /// <param name="baseFolder">扫描文件的目录</param>
        /// <param name="ignoreFolders">忽略该目录下的文件夹</param>
        /// <returns></returns>
        private IEnumerable<(string fileFullPath, List<HardCodeStringInfo> hardCodeStrings)> ScanInFolder(
            string baseFolder, params string[] ignoreFolders)
        {
            var allCsFile = GetFiles(baseFolder, $"*.{SupportedFileExtension}", SearchOption.AllDirectories, toIgnore: ignoreFolders);
            foreach (var csFile in allCsFile)
            {
                var fullFilePath = System.IO.Path.Combine(baseFolder, csFile);
                yield return (fullFilePath, LoadStringNodesInFile(fullFilePath));
            }
        }
        /// <summary>
        /// 获取所有文件并排除忽略的目录
        /// </summary>
        /// <param name="path">扫描路径</param>
        /// <param name="searchPattern">查找匹配条件</param>
        /// <param name="searchOption">搜索范围</param>
        /// <param name="toIgnore">忽略的目录</param>
        /// <returns></returns>
        private static IEnumerable<string> GetFiles(string path, string searchPattern = "*",
            SearchOption searchOption = SearchOption.AllDirectories, params string[] toIgnore)
        {
            var hash = new HashSet<string>(toIgnore.Select(folder => System.IO.Path.Combine(path, folder)), StringComparer.InvariantCultureIgnoreCase);
            var topDerctoryFiles = Directory.EnumerateFiles(path, searchPattern, SearchOption.TopDirectoryOnly);
            if (searchOption == SearchOption.AllDirectories)//扫描子级
                return topDerctoryFiles
                    .Concat(Directory.EnumerateDirectories(path, "*", searchOption)
                        .Where(folder => !toIgnore.Any(str => folder.Contains(str)))
                        .SelectMany(x => Directory.EnumerateFiles(x, searchPattern, searchOption)));
            //仅返回1级目录下的文件
            else return topDerctoryFiles;
        }

        /// <summary>
        /// 实现从文件提取需要本地化字符串的业务
        /// </summary>
        /// <param name="fullFilePath">文件完整路径</param>
        protected abstract List<HardCodeStringInfo> LoadStringNodesInFile(string fullFilePath);
        /// <summary>
        /// 自动读取继承类型并扫描目录下的文件
        /// </summary>
        public static List<(string fileFullPath, List<HardCodeStringInfo> hardCodeStrings)> ScanInFolder(string baseFolder)
        {
            var baseType = typeof(FileLocalizerBase);
            InitLocalisersCache(baseType);
            return localisersCache
                .SelectMany(flp => flp.ScanInFolder(baseFolder, "obj", "bin"))
                .ToList();
        }
        /// <summary>
        /// 初始化本地化处理类的实例
        /// </summary>
        /// <param name="baseType"></param>
        private static void InitLocalisersCache(Type baseType)
        {
            if (localisersCache == null)
            {
                var localisersTypes = baseType.Assembly.GetTypes()
                    .Where(type => type.Namespace == "HardCodeStringLocalizer.FileProcesser.LocalizeProcessers" && baseType.IsAssignableFrom(type))
                    .ToList();
                localisersCache = localisersTypes
                    .AsParallel()
                    .Select(type => Activator.CreateInstance(type) as FileLocalizerBase)
                    .ToList();
            }
        }

        /// <summary>
        /// 重载文件
        /// </summary>
        /// <param name="filePath"></param>
        public static List<HardCodeStringInfo> ReloadStringNodeInFile(string filePath)
        {
            var exteionsion = System.IO.Path.GetExtension(filePath).Trim('.');
            foreach (var ltype in localisersCache)
            {
                if (ltype.SupportedFileExtension.Equals(exteionsion, StringComparison.OrdinalIgnoreCase))
                {
                    return ltype.LoadStringNodesInFile(filePath);
                }
            }
            return null;
        }
    }
}
