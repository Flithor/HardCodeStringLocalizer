# Hardcoded string localizer
A tool that relies on Visual Studio to extract and replace hard-coded strings in project files for quick localization.  
[中文版点这](https://github.com/Flithor/HardCodeStringLocalizer/blob/master/README.md)

Localization of `.cs` files and `.xaml` files is currently supported.

## Attention!
This tool does not work out-of-the-box, you must modify the code to implement localized text replacement for your own projects.

## Features provided by this project
* Get the text that needs to be localized in the file through the rules defined in the code
  * Different projects use different localization methods, please modify the replacement rules by yourself
* Quickly locate code in Visual Studio
* After selecting the target resx resource file, you only need to enter a localization key name to write it into the resource file, and replace the original string with the localization code
* as long as you can parse the code in a specific file

## How should I use this tool to localize my project?
This tool is easy to customize, in simple terms you just need to inherit and implement [`FileLocalizerBase`](https://github.com/Flithor/HardCodeStringLocalizer/blob/master/HardCodeStringLocalizer/FileProcesser/FileLocalizerBase.cs) and [` HardCodeStringInfo`](https://github.com/Flithor/HardCodeStringLocalizer/blob/master/HardCodeStringLocalizer/FileProcesser/HardCodeStringInfo.cs) These two classes are enough, other works  can be done by framework themselves.  
read the document
Comments at [CSharpFileLocalizer.cs](https://github.com/Flithor/HardCodeStringLocalizer/blob/master/HardCodeStringLocalizer/FileProcesser/LocalizeProcessers/CSharpFileLocalizer.cs) to learn how to implement.

## Obligations to use this open source project:
* Please star this project.
* Please add a mark (it can be a hyperlink or copyable string) can navigate to this project (https://github.com/Flithor/HardCodeStringLocalizer) anywhere in your project (it can be a license file, acknowledgement, or any accessible place in your software).

Welcome PR
