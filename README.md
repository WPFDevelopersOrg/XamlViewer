# XAML Viewer

XAML Viewer is a lightweight XAML editor. 在编写文本的同时，能够实时显示相应的设计预览。不仅提供方便的文档管理，更具人性化的智能提示。当你在学习或尝试一些XAML效果时，一定会是你的不错选择。

![Preview](images/XAMLViewer.png)

## Document Manager
通过工具栏按钮（或快捷键）提供如下操作：  
1. 支持创建，打开，保存，关闭等操作；
2. 支持拖动页签进行位置交换；
3. 支持在 Active Files 下拉列表中针对已打开的文件进行快速选择。  

<font color=red>_注意：在关闭软件时，只会自动保存已经存储在本地的文档，请务必在此之前，将需要保留的临时文档保存到本地。_</font>

## Automitic Compile
通过 [Setting] >> [Compile] 修改相应的设置：  
1. Automitic Compile CheckBox，开启或关闭自动编译功能，但手动编译[F5]，一直生效；
2. Auto-Compile Delay Slider，在无任何输入的指定时间后自动执行编译。

## Custom Controls
通过 [Setting] >> [Reference] >> [Add] 添加自定义控件库，可以在XAML中直接引用其中控件。  
请注意以下两点：
1. 当前软件基于.Net Framework 4.5，只要系统中包含.Net Framework 4.X（X >= 5），即可引用基于4.0--4.X任意版本的控件库；
2. 引用自定义控件库时，命名空间的申明只能是以下形式方能生效：</br>
``` xml
xmlns:controls="clr-namespace:MyControl.Controls;assembly=MyControl"
```

## Third-Party Notices
Library|Version|License
--|:--:|--:
[Prism](https://github.com/PrismLibrary/Prism)|7.2.0.1367|[MIT](https://github.com/PrismLibrary/Prism/blob/master/LICENSE)
[AvalonEdit](https://github.com/icsharpcode/AvalonEdit)|6.0|[MIT](https://github.com/icsharpcode/AvalonEdit/blob/master/LICENSE)
[Json.NET](https://github.com/JamesNK/Newtonsoft.Json)|12.0.3|[MIT](https://github.com/JamesNK/Newtonsoft.Json/blob/master/LICENSE.md)
                   
