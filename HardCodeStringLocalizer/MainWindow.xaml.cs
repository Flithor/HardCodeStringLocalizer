using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xaml;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

using ConsoleLauncherApp;

using EnvDTE;

using HardCodeStringLocalizer.FileProcesser;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.OLE.Interop;

using Ookii.Dialogs.Wpf;

using static System.Net.WebRequestMethods;

using static System.Windows.Forms.LinkLabel;

namespace HardCodeStringLocalizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        bool filterName;
        public string NameRegexFilter
        {
            get { return (string)GetValue(NameRegexFilterProperty); }
            set { SetValue(NameRegexFilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NameRegexFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NameRegexFilterProperty =
            DependencyProperty.Register("NameRegexFilter", typeof(string), typeof(MainWindow), new PropertyMetadata("", NameRegexFilterValueChanged), RegexPatternValidation);

        private static void NameRegexFilterValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = d as MainWindow;
            var newVal = e.NewValue as string;
            if (_this.filterName = !string.IsNullOrWhiteSpace(newVal))
                _this.DataGrid_AllHardCodeStrings.Items.Filter = (obj) => _this.FilterItem(obj as HardCodeStringInfo);
            else
                _this.DataGrid_AllHardCodeStrings.Items.Filter = null;
        }
        bool filterText;
        public string TextRegexFilter
        {
            get { return (string)GetValue(TextRegexFilterProperty); }
            set { SetValue(TextRegexFilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TextRegexFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextRegexFilterProperty =
            DependencyProperty.Register("TextRegexFilter", typeof(string), typeof(MainWindow), new PropertyMetadata("", TextRegexFilterValueChanged), RegexPatternValidation);

        private static void TextRegexFilterValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = d as MainWindow;
            var newVal = e.NewValue as string;
            if (_this.filterText = !string.IsNullOrWhiteSpace(newVal))
                _this.DataGrid_AllHardCodeStrings.Items.Filter = (obj) => _this.FilterItem(obj as HardCodeStringInfo);
            else
                _this.DataGrid_AllHardCodeStrings.Items.Filter = null;
        }
        bool FilterItem(HardCodeStringInfo hcsi)
        {
            return (!filterName || Regex.IsMatch(hcsi.FileName, NameRegexFilter))
                && (!filterText || Regex.IsMatch(hcsi.OrignalText, TextRegexFilter));
        }

        private static bool RegexPatternValidation(object value)
        {
            try
            {
                if (value is string pattern)
                {
                    if (string.IsNullOrEmpty(pattern))
                        return true;
                    Regex.Match("", pattern);
                    return true;
                }
            }
            catch { }
            return false;
        }

        Dictionary<string, List<HardCodeStringInfo>> hardCodeStringsDic;
        string targetResourceFilePath;
        Dictionary<string, string> existedResource = new Dictionary<string, string>();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                hardCodeStringsDic = new Dictionary<string, List<HardCodeStringInfo>>();
                var dirDlg = new VistaFolderBrowserDialog();
                if (dirDlg.ShowDialog() == true)
                {
                    string selectedPath = dirDlg.SelectedPath;
                    foreach (var (fullFilePath, hardCodeStrings) in FileLocalizerBase.ScanInFolder(selectedPath))
                    {
                        hardCodeStringsDic[fullFilePath] = hardCodeStrings;
                    };
                }
                DataGrid_AllHardCodeStrings.ItemsSource = hardCodeStringsDic.SelectMany(kv => kv.Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 重载文件
        /// </summary>
        public void ReloadStringNodeInFile(string filePath)
        {
            try
            {
                var hcsis = FileLocalizerBase.ReloadStringNodeInFile(filePath);
                if (hcsis != null)
                {
                    hardCodeStringsDic[filePath] = hcsis;
                    DataGrid_AllHardCodeStrings.ItemsSource = hardCodeStringsDic.SelectMany(kv => kv.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void GotoFileLine(object sender, RoutedEventArgs e)
        {
            try
            {
                var hyperlink = sender as Hyperlink;
                var hcsi = hyperlink.DataContext as HardCodeStringInfo;

                SelectTextInFile(hcsi);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 在Visual Studio中打开文件并选中文本区域
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        private static EnvDTE.TextSelection SelectTextInFile(HardCodeStringInfo hcsi)
        {
            var textSelection = VisualStudioInterop.OpenFileInVisualStudio(hcsi.FilePath);
            if (textSelection != null)
            {
                textSelection.MoveToLineAndOffset(hcsi.TextRange.StartLinePosition.Line + 1, hcsi.TextRange.StartLinePosition.Character + 1);
                textSelection.MoveToLineAndOffset(hcsi.TextRange.EndLinePosition.Line + 1, hcsi.TextRange.EndLinePosition.Character + 1, true);
            }
            else
                throw new InvalidOperationException("未能在Visual Studio中打开文件");
            return textSelection;
        }

        private void btn_ReloadResourceFile(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(targetResourceFilePath))
                {
                    MessageBox.Show("请先选择目标资源文件");
                    return;
                }
                LoadResourceFile(targetResourceFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>
        /// 转本地化操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TransLocalization(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            var hcsi = hyperlink?.DataContext as HardCodeStringInfo;
            if (hcsi != null)
                TransLocalization(hcsi);
        }
        /// <summary>
        /// 转本地化操作
        /// </summary>
        private void TransLocalization(HardCodeStringInfo hcsi)
        {
            try
            {
                if (string.IsNullOrEmpty(targetResourceFilePath))
                {
                    MessageBox.Show("请先选择目标资源文件");
                    return;
                }


                //获取资源值
                var resourceString = hcsi.GetResourceValue();

                if (string.IsNullOrEmpty(resourceString)) return;

                bool createNew = true;
                if (ResourceExists(resourceString, out string name))
                {
                    var mr = MessageBox.Show("同值键已存在,是否使用？", "提示", MessageBoxButton.YesNoCancel);
                    if (mr == MessageBoxResult.Yes)
                        createNew = false;
                    else if (mr == MessageBoxResult.Cancel)
                        return;
                }
                if (createNew)
                {
                    if (TextWindow.GetText(this, out name))
                    {
                        //MessageBox.Show($"原始文本：{hcsi.OrignalText}\r\n替换文本：{replaceText}\r\n资源文本：{formatString}");
                        AddNewResourceKey(name, resourceString);
                    }
                    else return;
                }

                string replaceText = hcsi.BuildReplaceString(name);

                using (MessageFilter.Using())
                {
                    var ts = SelectTextInFile(hcsi);
                    ts.Insert(replaceText);
                    ts.Parent.Parent.Save();
                }

                ReloadStringNodeInFile(hcsi.FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("操作被拒绝，请重试\r\n异常详情：\r\n" + ex.ToString());
            }
        }

        /// <summary>
        /// 同值资源已存在
        /// </summary>
        private bool ResourceExists(string resourceString, out string name)
        {
            foreach (var kv in existedResource)
            {
                if (kv.Value == resourceString)
                {
                    name = kv.Key;
                    return true;
                }
            }
            name = null;
            return false;
        }
        /// <summary>
        /// 选择加载资源文件
        /// </summary>
        private void btn_SelectResourceFile(object sender, RoutedEventArgs e)
        {
            try
            {
                VistaOpenFileDialog dialog = new VistaOpenFileDialog() { Filter = "资源文件(*.resx)|*.resx" };
                if (dialog.ShowDialog() == true)
                {
                    string fileName = dialog.FileName;
                    LoadResourceFile(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        /// <summary>
        /// 加载资源文件,并读缓存有已有的文本资源定义
        /// </summary>
        /// <param name="fileName"></param>
        private void LoadResourceFile(string fileName)
        {
            var xmlDoc = new XmlDocument();
            existedResource.Clear();
            xmlDoc.Load(fileName);
            foreach (XmlNode node in xmlDoc.SelectNodes("root/data"))
            {
                existedResource.Add(node.Attributes["name"].Value, node["value"].InnerText);
            }
            targetResourceFilePath = fileName;
            TextBlock_TargetResourceFilePath.Text = targetResourceFilePath;
        }
        /// <summary>
        /// 向资源文件写入键值对并加入缓存
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="keyValue"></param>
        private void AddNewResourceKey(string keyName, string keyValue)
        {
            var doc = new XmlDocument();
            doc.Load(targetResourceFilePath);
            var root = doc.GetElementsByTagName("root")[0];

            //相同键已存在
            var existed = doc.SelectNodes($"root/data[@name='{keyName}']");
            if (existed.Count > 0) return;

            var dataNode = doc.CreateNode(XmlNodeType.Element, "data", null);
            var nameAttr = doc.CreateAttribute("name");
            nameAttr.Value = keyName;
            dataNode.Attributes.Append(nameAttr);
            var spaceAttr = doc.CreateAttribute("xml:space");
            spaceAttr.Value = "preserve";
            dataNode.Attributes.Append(spaceAttr);
            dataNode.InnerXml = $"<value>{keyValue}</value>";
            root.AppendChild(dataNode);
            doc.Save(targetResourceFilePath);
            existedResource.Add(keyName, keyValue);
        }
        /// <summary>
        /// 重载当前文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReloadCurrentFile(object sender, RoutedEventArgs e)
        {
            var hyperlink = sender as Hyperlink;
            var hcsi = hyperlink.DataContext as HardCodeStringInfo;

            ReloadStringNodeInFile(hcsi.FilePath);
        }

        private void TextBox_GotFocusSelectAll(object sender, RoutedEventArgs e)
        {
            (sender as TextBox)?.SelectAll();
        }
    }
}
