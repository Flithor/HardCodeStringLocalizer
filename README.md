# 硬编码字符串本地化工具(Hardcoded String Localizer)
一个依托Visual Studio来提取和替换项目文件中硬编码字符串来快速实现本地化的工具  

目前代码内已实现`.cs`文件和`.xaml`文件的解析。

## 注意！
此工具不是开箱即用的，你必须通过修改代码来实现你自己项目的本地化文本替换。

## 本项目提供的功能
* 通过代码中定义的规则获取文件中的需要本地化的文本。
  * 不同项目使用的本地化方法不同，请自行修改替换规则。
* 可快速在Visual Studio中定位代码。
* 选择目标resx资源文件后，只需要输入一个本地化键名称即可将其写入资源文件，并将原字符串替换为本地化代码。
  * 如果资源文件中已存在相同的文本，可以选择直接使用已有项的键，也可以创建新的资源项。

## 我应该如何使用此工具本地化我的项目？
1. 先检查是否一切就绪
    * 确保你已经在项目中实现了适用于你项目的通用本地化框架。
    * 确保你已经将本项目中预定义的本地化替换文本修改为适用于你项目的本地化代码。
    * 确认是否已经打开Visual Studio并加载了解决方案。
    * 在正式进行本地化修改前，先测试一遍看看是否有问题。
2. 加载你需要本地化的目录。
    * 注意代码中扫描文件部分关于排除的目录的定义，要么将需要排除的从目录中移除，要么将这些目录加入排除列表中。
3. 加载你需要写入资源键的资源文件。
    * 如果你在软件外部修改了资源文件，请点击重载资源文件来同步修改。
4. 在界面中筛选你需要进行本地化的字符串。
    * 可根据文件名或字符串文本进行正则匹配筛选。
5. 点击“位置”可在Visual Studio中打开该文件并选中文本
    * 此操作用于确认本地化前替换的文本位置是否正确。
    * 如果你在软件外部修改了文件(如：格式化、撤销)，请记得*保存*并点击软件中该文件字符串项的“重载该文件”。
    * 若位置出现偏请尝试重载文件（重载文件前请记得保存文件），若无效则请检查代码实现是否有问题，务必修正偏移。
6. 点击“转本地化”按钮，并在弹出窗口中输入本地化资源的键名。
    * 若资源中已包含相同的资源文本，则会询问是否使用已有的键，但是你也可以选择创建新的键。
7. 确认，软件将替换硬编码字符串并应用本地化代码。

## 我想本地化其他类型的文件要怎么做？
这个工具很容易扩展，只要你可以解析你需要本地化的其它文本中的字符串就行。  
你只需要继承并实现[`FileLocalizerBase`](https://github.com/Flithor/HardCodeStringLocalizer/blob/master/HardCodeStringLocalizer/FileProcesser/FileLocalizerBase.cs)和[`HardCodeStringInfo`](https://github.com/Flithor/HardCodeStringLocalizer/blob/master/HardCodeStringLocalizer/FileProcesser/HardCodeStringInfo.cs)这2个类,其他的代码框架可以自己搞定  
阅读文件
[CSharpFileLocalizer.cs](https://github.com/Flithor/HardCodeStringLocalizer/blob/master/HardCodeStringLocalizer/FileProcesser/LocalizeProcessers/CSharpFileLocalizer.cs)的注释可了解如何实现

## 使用本开源项目的义务：
* 请星标该项目。
* 请在你的项目中任意位置(可以是许可文件,可以是鸣谢界面,也可以是软件的任意可访问位置)添加可导航至本项目(https://github.com/Flithor/HardCodeStringLocalizer) 的标识(超链接或可复制的字符串)。

欢迎PR补充功能

## 项目引用
[ookii-dialogs-wpf](https://github.com/ookii-dialogs/ookii-dialogs-wpf)
