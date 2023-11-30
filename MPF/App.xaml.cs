using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

#if NET40_OR_GREATER || NETCOREAPP

        private const string _comboBoxTemplate = @"<ControlTemplate x:Key=""ComboBoxTemplate"" TargetType=""{x:Type ComboBox}"">
            <Grid x:Name=""templateRoot"" SnapsToDevicePixels=""true"">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width=""*""/>
                    <ColumnDefinition MinWidth=""{DynamicResource {x:Static SystemParameters.VerticalScrollBarWidthKey}}"" Width=""0""/>
                </Grid.ColumnDefinitions>
                <Popup x:Name=""PART_Popup"" AllowsTransparency=""true"" Grid.ColumnSpan=""2"" IsOpen=""{Binding IsDropDownOpen, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}"" Margin=""1"" PopupAnimation=""{DynamicResource {x:Static SystemParameters.ComboBoxPopupAnimationKey}}"" Placement=""Bottom"">
                    <themes:SystemDropShadowChrome x:Name=""shadow"" Color=""Transparent"" MaxHeight=""{TemplateBinding MaxDropDownHeight}"" MinWidth=""{Binding ActualWidth, ElementName=templateRoot}"">
                        <Border x:Name=""dropDownBorder"" BorderBrush=""{DynamicResource {x:Static SystemColors.WindowFrameBrushKey}}"" BorderThickness=""1"" Background=""{DynamicResource {x:Static SystemColors.WindowBrushKey}}"">
                            <ScrollViewer x:Name=""DropDownScrollViewer"">
                                <Grid x:Name=""grid"" RenderOptions.ClearTypeHint=""Enabled"">
                                    <Canvas x:Name=""canvas"" HorizontalAlignment=""Left"" Height=""0"" VerticalAlignment=""Top"" Width=""0"">
                                        <Rectangle x:Name=""opaqueRect"" Fill=""{Binding Background, ElementName=dropDownBorder}"" Height=""{Binding ActualHeight, ElementName=dropDownBorder}"" Width=""{Binding ActualWidth, ElementName=dropDownBorder}""/>
                                    </Canvas>
                                    <ItemsPresenter x:Name=""ItemsPresenter"" KeyboardNavigation.DirectionalNavigation=""Contained"" SnapsToDevicePixels=""{TemplateBinding SnapsToDevicePixels}""/>
                                </Grid>
                            </ScrollViewer>
                        </Border>
                    </themes:SystemDropShadowChrome>
                </Popup>
                <ToggleButton x:Name=""toggleButton"" BorderBrush=""{TemplateBinding BorderBrush}"" BorderThickness=""{TemplateBinding BorderThickness}"" Background=""{TemplateBinding Background}"" Grid.ColumnSpan=""2"" IsChecked=""{Binding IsDropDownOpen, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}"" Style=""{DynamicResource ComboBoxToggleButton}""/>
                <ContentPresenter x:Name=""contentPresenter"" ContentTemplate=""{TemplateBinding SelectionBoxItemTemplate}"" ContentTemplateSelector=""{TemplateBinding ItemTemplateSelector}"" Content=""{TemplateBinding SelectionBoxItem}"" ContentStringFormat=""{TemplateBinding SelectionBoxItemStringFormat}"" HorizontalAlignment=""{TemplateBinding HorizontalContentAlignment}"" IsHitTestVisible=""false"" Margin=""{TemplateBinding Padding}"" SnapsToDevicePixels=""{TemplateBinding SnapsToDevicePixels}"" VerticalAlignment=""{TemplateBinding VerticalContentAlignment}""/>
            </Grid>
            <ControlTemplate.Triggers>
                <Trigger Property=""HasDropShadow"" SourceName=""PART_Popup"" Value=""true"">
                    <Setter Property=""Margin"" TargetName=""shadow"" Value=""0,0,5,5""/>
                    <Setter Property=""Color"" TargetName=""shadow"" Value=""#71000000""/>
                </Trigger>
                <Trigger Property=""HasItems"" Value=""false"">
                    <Setter Property=""Height"" TargetName=""dropDownBorder"" Value=""95""/>
                </Trigger>
                <MultiTrigger>
                    <MultiTrigger.Conditions>
                        <Condition Property=""IsGrouping"" Value=""true""/>
                        <Condition Property=""VirtualizingPanel.IsVirtualizingWhenGrouping"" Value=""false""/>
                    </MultiTrigger.Conditions>
                    <Setter Property=""ScrollViewer.CanContentScroll"" Value=""false""/>
                </MultiTrigger>
                <Trigger Property=""ScrollViewer.CanContentScroll"" SourceName=""DropDownScrollViewer"" Value=""false"">
                    <Setter Property=""Canvas.Top"" TargetName=""opaqueRect"" Value=""{Binding VerticalOffset, ElementName=DropDownScrollViewer}""/>
                    <Setter Property=""Canvas.Left"" TargetName=""opaqueRect"" Value=""{Binding HorizontalOffset, ElementName=DropDownScrollViewer}""/>
                </Trigger>
            </ControlTemplate.Triggers>
        </ControlTemplate>";

        private const string _comboBoxEditableTemplate = @"<ControlTemplate x:Key=""ComboBoxEditableTemplate"" TargetType=""{x:Type ComboBox}"">
            <Grid x:Name=""templateRoot"" SnapsToDevicePixels=""true"">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width=""*""/>
                    <ColumnDefinition MinWidth=""{DynamicResource {x:Static SystemParameters.VerticalScrollBarWidthKey}}"" Width=""0""/>
                </Grid.ColumnDefinitions>
                <Popup x:Name=""PART_Popup"" AllowsTransparency=""true"" Grid.ColumnSpan=""2"" IsOpen=""{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}}"" PopupAnimation=""{DynamicResource {x:Static SystemParameters.ComboBoxPopupAnimationKey}}"" Placement=""Bottom"">
                    <themes:SystemDropShadowChrome x:Name=""shadow"" Color=""Transparent"" MaxHeight=""{TemplateBinding MaxDropDownHeight}"" MinWidth=""{Binding ActualWidth, ElementName=templateRoot}"">
                        <Border x:Name=""dropDownBorder"" BorderBrush=""{DynamicResource {x:Static SystemColors.WindowFrameBrushKey}}"" BorderThickness=""1"" Background=""{DynamicResource {x:Static SystemColors.WindowBrushKey}}"">
                            <ScrollViewer x:Name=""DropDownScrollViewer"">
                                <Grid x:Name=""grid"" RenderOptions.ClearTypeHint=""Enabled"">
                                    <Canvas x:Name=""canvas"" HorizontalAlignment=""Left"" Height=""0"" VerticalAlignment=""Top"" Width=""0"">
                                        <Rectangle x:Name=""opaqueRect"" Fill=""{Binding Background, ElementName=dropDownBorder}"" Height=""{Binding ActualHeight, ElementName=dropDownBorder}"" Width=""{Binding ActualWidth, ElementName=dropDownBorder}""/>
                                    </Canvas>
                                    <ItemsPresenter x:Name=""ItemsPresenter"" KeyboardNavigation.DirectionalNavigation=""Contained"" SnapsToDevicePixels=""{TemplateBinding SnapsToDevicePixels}""/>
                                </Grid>
                            </ScrollViewer>
                        </Border>
                    </themes:SystemDropShadowChrome>
                </Popup>
                <ToggleButton x:Name=""toggleButton"" BorderBrush=""{TemplateBinding BorderBrush}"" BorderThickness=""{TemplateBinding BorderThickness}"" Background=""{TemplateBinding Background}"" Grid.ColumnSpan=""2"" IsChecked=""{Binding IsDropDownOpen, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}"" Style=""{DynamicResource ComboBoxToggleButton}""/>
                <Border x:Name=""border"" Background=""{DynamicResource TextBox.Static.Background}"" Margin=""{TemplateBinding BorderThickness}"">
                    <TextBox x:Name=""PART_EditableTextBox"" HorizontalContentAlignment=""{TemplateBinding HorizontalContentAlignment}"" IsReadOnly=""{Binding IsReadOnly, RelativeSource={RelativeSource TemplatedParent}}"" Margin=""{TemplateBinding Padding}"" Style=""{DynamicResource ComboBoxEditableTextBox}"" VerticalContentAlignment=""{TemplateBinding VerticalContentAlignment}""/>
                </Border>
            </Grid>
            <ControlTemplate.Triggers>
                <Trigger Property=""IsEnabled"" Value=""false"">
                    <Setter Property=""Opacity"" TargetName=""border"" Value=""0.56""/>
                </Trigger>
                <Trigger Property=""IsKeyboardFocusWithin"" Value=""true"">
                    <Setter Property=""Foreground"" Value=""Black""/>
                </Trigger>
                <Trigger Property=""HasDropShadow"" SourceName=""PART_Popup"" Value=""true"">
                    <Setter Property=""Margin"" TargetName=""shadow"" Value=""0,0,5,5""/>
                    <Setter Property=""Color"" TargetName=""shadow"" Value=""#71000000""/>
                </Trigger>
                <Trigger Property=""HasItems"" Value=""false"">
                    <Setter Property=""Height"" TargetName=""dropDownBorder"" Value=""95""/>
                </Trigger>
                <MultiTrigger>
                    <MultiTrigger.Conditions>
                        <Condition Property=""IsGrouping"" Value=""true""/>
                        <Condition Property=""VirtualizingPanel.IsVirtualizingWhenGrouping"" Value=""false""/>
                    </MultiTrigger.Conditions>
                    <Setter Property=""ScrollViewer.CanContentScroll"" Value=""false""/>
                </MultiTrigger>
                <Trigger Property=""ScrollViewer.CanContentScroll"" SourceName=""DropDownScrollViewer"" Value=""false"">
                    <Setter Property=""Canvas.Top"" TargetName=""opaqueRect"" Value=""{Binding VerticalOffset, ElementName=DropDownScrollViewer}""/>
                    <Setter Property=""Canvas.Left"" TargetName=""opaqueRect"" Value=""{Binding HorizontalOffset, ElementName=DropDownScrollViewer}""/>
                </Trigger>
            </ControlTemplate.Triggers>
        </ControlTemplate>";
        public App()
        {
            InitializeComponent();

            CreateControlTemplate("ComboBoxTemplate");
            CreateControlTemplate("ComboBoxEditableTemplate");
        }

        private void CreateControlTemplate(string resourceName)
        {
            var parserContext = new ParserContext();
            parserContext.XmlnsDictionary[""] = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            parserContext.XmlnsDictionary["x"] = "http://schemas.microsoft.com/winfx/2006/xaml";
#if NETFRAMEWORK
            parserContext.XmlnsDictionary["themes"] = "clr-namespace:Microsoft.Windows.Themes;assembly=PresentationFramework.Aero";
#else
            parserContext.XmlnsDictionary["themes"] = "clr-namespace:Microsoft.Windows.Themes;assembly=PresentationFramework.Aero2";
#endif
            parserContext.XamlTypeMapper = new XamlTypeMapper([]);

            var controlTemplate = resourceName switch
            {
                "ComboBoxTemplate" => XamlReader.Parse(_comboBoxTemplate, parserContext) as ControlTemplate,
                "ComboBoxEditableTemplate" => XamlReader.Parse(_comboBoxEditableTemplate, parserContext) as ControlTemplate,
                _ => throw new ArgumentException(nameof(resourceName)),
            };

            // Add the control template
            Resources[resourceName] = controlTemplate;
        }
#endif
        }
}
