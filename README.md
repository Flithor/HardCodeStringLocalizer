# 硬编码字符串本地化工具
一个依托Visual Studio来提取和替换项目文件中硬编码字符串来快速实现本地化的工具  
[English ReadMe here](https://github.com/Flithor/HardCodeStringLocalizer/blob/master/README_en.md) 

目前支持`.cs`文件和`.xaml`文件的本地化

## 注意！
此工具不是开箱即用的，你必须通过修改代码来实现你自己项目的本地化文本替换。

## 本项目提供的功能
* 通过代码中定义的规则获取文件中的需要本地化的文本
** 不同项目使用的本地化方法不同，请自行修改替换规则
* 可快速在Visual Studio中定位代码
* 选择目标resx资源文件后，只需要输入一个本地化键名称即可将其写入资源文件，并将原字符串替换为本地化代码
* 只要你可以解析特定文件中的代码

## 我应该如何使用此工具本地化我的项目？
这个工具很容易实现自定义，简单来说你只需要继承并实现[`FileLocalizerBase`](https://github.com/Flithor/HardCodeStringLocalizer/blob/master/HardCodeStringLocalizer/FileProcesser/FileLocalizerBase.cs)和[`HardCodeStringInfo`](https://github.com/Flithor/HardCodeStringLocalizer/blob/master/HardCodeStringLocalizer/FileProcesser/HardCodeStringInfo.cs)这2个类就行了,其他的代码框架可以自己搞定  
阅读该文件
[CSharpFileLocalizer.cs](https://github.com/Flithor/HardCodeStringLocalizer/blob/master/HardCodeStringLocalizer/FileProcesser/LocalizeProcessers/CSharpFileLocalizer.cs)的注释可了解如何实现

## 使用本开源项目的义务：
* 请星标该项目。
* 请在你的项目中任意位置(可以是许可文件,可以是鸣谢界面,也可以是软件的任意可访问位置)添加可导航至本项目(https://github.com/Flithor/HardCodeStringLocalizer)的标识(超链接或可复制的字符串)。

欢迎PR补充功能
