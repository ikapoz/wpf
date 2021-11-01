# CSS in WPF

## Introduction
In HTML world we have CSS (Cascade Style Sheets) to style the web page.
In WPF we have resource dictionary to define styling.
In following table you can compare the two styling:

|   Selector   |   CSS    |   WPF    |
|--------------|----------|----------|
| Element      |   Yes	  |	  Yes    |
| Id		   |   Yes    |   Yes    |
| Class        |   Yes    |   No     |

In this article we will show how to add class selectors to WPF styling by using attached behavior.

## Prerequisite
If you are not familiar with attached behavior in WPF my recommendation is to read first:

[Intoduction to Attached Behavior in WPF](https://www.codeproject.com/Articles/28959/Introduction-to-Attached-Behaviors-in-WPF)

Visual Studio 2022 WPF Designer correctly shows attached properties.
Visual Studio 2019 WPF Designer does not correctly shows attached properties.
## Creating Styles
### Variable(s)

In SASS/CSS
``` SCSS
$FontStyle: 'Poppins', sans-serif;

.base-font {
    font-family: $FontStyle;
    font-size: 12em;
    font-weight: 400;
}
```

In WPF
```XML
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:s="clr-namespace:System;assembly=mscorlib">
    <FontFamily x:Key="$FontStyle">Resources/Poppins/#Poppins</FontFamily>

    <ResourceDictionary x:Key="BaseFont">
        <s:Double x:Key="FontSize">12</s:Double>
        <FontWeight x:Key="FontWeight">Normal</FontWeight>
        <FontFamily x:Key="FontFamily">$FontStyle</FontFamily>
    </ResourceDictionary>
</ResourceDictionary>
```

### Extend/Inheritance
In SASS/CSS
``` SCSS
%center {
    text-align: center;
}

%base-font {
    font-size: 12em;
    font-weight: 400;
}

.title-text {
    @extend %center;
    @extend %base-font;
    font-size: 28em;
}
```

In WPF
``` XML
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                    xmlns:s="clr-namespace:System;assembly=mscorlib"
                    xmlns:ui="clr-namespace:CSS.UI">
    <ResourceDictionary x:Key="Center">
        <HorizontalAlignment x:Key="HorizontalAlignment">Center</HorizontalAlignment>
        <VerticalAlignment x:Key="VerticalAlignment">Center</VerticalAlignment>
    </ResourceDictionary>

    <ResourceDictionary x:Key="BaseFont">
        <s:Double x:Key="FontSize">12</s:Double>
        <FontWeight x:Key="FontWeight">Normal</FontWeight>
    </ResourceDictionary>

    <ResourceDictionary x:Key="TitleText">
        <ui:CSS x:Key="BaseFont Center"/>
        <s:Double x:Key="FontSize">28</s:Double>
    </ResourceDictionary>
</ResourceDictionary> 
```

## Using the style in code
``` XML
<Window x:Class="CSS.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:CSS"
        mc:Ignorable="d"
        xmlns:ui="clr-namespace:CSS.UI"
        Title="MainWindow" Height="450" Width="800">
    <StackPanel Orientation="Vertical">
        <TextBlock Text="Title" ui:X.CSS="TitleText"/>
        <TextBlock Text="Title Centered" ui:X.CSS="TitleText Center"/>
        <TextBlock Text="Body text" ui:X.CSS="BaseFont"/>
    </StackPanel>
</Window>
```
Important to remember
* Key name is property name you want to set locally.
* CSS inheritance is applied first than local property.

# Licence

This article, along with any associated source code and files, 
is licensed under [MIT License](https://choosealicense.com/licenses/mit/)
