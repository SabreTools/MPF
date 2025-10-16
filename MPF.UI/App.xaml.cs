using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MPF.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

#if NET35_OR_GREATER || NETCOREAPP

        #region ControlTemplates

        /// <summary>
        /// ComboBoxTemplate ControlTemplate XAML (.NET Framework 4.0 and above)
        /// </summary>
        private const string _comboBoxTemplateDefault = @"<ControlTemplate TargetType=""{x:Type ComboBox}"">
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

        /// <summary>
        /// ComboBoxTemplate ControlTemplate XAML (.NET Framework 3.5)
        /// </summary>
        private const string _comboBoxTemplateNet35 = @"<ControlTemplate TargetType=""{x:Type ComboBox}"">
            <Grid x:Name=""templateRoot"" SnapsToDevicePixels=""true"">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width=""*""/>
                    <ColumnDefinition MinWidth=""{DynamicResource {x:Static SystemParameters.VerticalScrollBarWidthKey}}"" Width=""0""/>
                </Grid.ColumnDefinitions>
                <Popup x:Name=""PART_Popup"" AllowsTransparency=""true"" Grid.ColumnSpan=""2"" IsOpen=""{Binding IsDropDownOpen, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}"" Margin=""1"" PopupAnimation=""{DynamicResource {x:Static SystemParameters.ComboBoxPopupAnimationKey}}"" Placement=""Bottom"">
                    <Border x:Name=""dropDownBorder"" BorderBrush=""{DynamicResource {x:Static SystemColors.WindowFrameBrushKey}}"" BorderThickness=""1"" Background=""{DynamicResource {x:Static SystemColors.WindowBrushKey}}"">
                        <ScrollViewer x:Name=""DropDownScrollViewer"">
                            <Grid x:Name=""grid"">
                                <Canvas x:Name=""canvas"" HorizontalAlignment=""Left"" Height=""0"" VerticalAlignment=""Top"" Width=""0"">
                                    <Rectangle x:Name=""opaqueRect"" Fill=""{Binding Background, ElementName=dropDownBorder}"" Height=""{Binding ActualHeight, ElementName=dropDownBorder}"" Width=""{Binding ActualWidth, ElementName=dropDownBorder}""/>
                                </Canvas>
                                <ItemsPresenter x:Name=""ItemsPresenter"" KeyboardNavigation.DirectionalNavigation=""Contained"" SnapsToDevicePixels=""{TemplateBinding SnapsToDevicePixels}""/>
                            </Grid>
                        </ScrollViewer>
                    </Border>
                </Popup>
                <ToggleButton x:Name=""toggleButton"" BorderBrush=""{TemplateBinding BorderBrush}"" BorderThickness=""{TemplateBinding BorderThickness}"" Background=""{TemplateBinding Background}"" Grid.ColumnSpan=""2"" IsChecked=""{Binding IsDropDownOpen, Mode=TwoWay, RelativeSource={RelativeSource TemplatedParent}}"" Style=""{DynamicResource ComboBoxToggleButton}""/>
                <ContentPresenter x:Name=""contentPresenter"" ContentTemplate=""{TemplateBinding SelectionBoxItemTemplate}"" ContentTemplateSelector=""{TemplateBinding ItemTemplateSelector}"" Content=""{TemplateBinding SelectionBoxItem}"" ContentStringFormat=""{TemplateBinding SelectionBoxItemStringFormat}"" HorizontalAlignment=""{TemplateBinding HorizontalContentAlignment}"" IsHitTestVisible=""false"" Margin=""{TemplateBinding Padding}"" SnapsToDevicePixels=""{TemplateBinding SnapsToDevicePixels}"" VerticalAlignment=""{TemplateBinding VerticalContentAlignment}""/>
            </Grid>
            <ControlTemplate.Triggers>
                <Trigger Property=""HasItems"" Value=""false"">
                    <Setter Property=""Height"" TargetName=""dropDownBorder"" Value=""95""/>
                </Trigger>
                <MultiTrigger>
                    <MultiTrigger.Conditions>
                        <Condition Property=""IsGrouping"" Value=""true""/>
                    </MultiTrigger.Conditions>
                    <Setter Property=""ScrollViewer.CanContentScroll"" Value=""false""/>
                </MultiTrigger>
                <Trigger Property=""ScrollViewer.CanContentScroll"" SourceName=""DropDownScrollViewer"" Value=""false"">
                    <Setter Property=""Canvas.Top"" TargetName=""opaqueRect"" Value=""{Binding VerticalOffset, ElementName=DropDownScrollViewer}""/>
                    <Setter Property=""Canvas.Left"" TargetName=""opaqueRect"" Value=""{Binding HorizontalOffset, ElementName=DropDownScrollViewer}""/>
                </Trigger>
            </ControlTemplate.Triggers>
        </ControlTemplate>";

        /// <summary>
        /// ComboBoxEditableTemplate ControlTemplate XAML (.NET Framework 4.0 and above)
        /// </summary>
        private const string _comboBoxEditableTemplateDefault = @"<ControlTemplate TargetType=""{x:Type ComboBox}"">
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

        /// <summary>
        /// ComboBoxEditableTemplate ControlTemplate XAML (.NET Framework 3.5)
        /// </summary>
        private const string _comboBoxEditableTemplateNet35 = @"<ControlTemplate TargetType=""{x:Type ComboBox}"">
            <Grid x:Name=""templateRoot"" SnapsToDevicePixels=""true"">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width=""*""/>
                    <ColumnDefinition MinWidth=""{DynamicResource {x:Static SystemParameters.VerticalScrollBarWidthKey}}"" Width=""0""/>
                </Grid.ColumnDefinitions>
                <Popup x:Name=""PART_Popup"" AllowsTransparency=""true"" Grid.ColumnSpan=""2"" IsOpen=""{Binding IsDropDownOpen, RelativeSource={RelativeSource TemplatedParent}}"" PopupAnimation=""{DynamicResource {x:Static SystemParameters.ComboBoxPopupAnimationKey}}"" Placement=""Bottom"">
                    <Border x:Name=""dropDownBorder"" BorderBrush=""{DynamicResource {x:Static SystemColors.WindowFrameBrushKey}}"" BorderThickness=""1"" Background=""{DynamicResource {x:Static SystemColors.WindowBrushKey}}"">
                        <ScrollViewer x:Name=""DropDownScrollViewer"">
                            <Grid x:Name=""grid"">
                                <Canvas x:Name=""canvas"" HorizontalAlignment=""Left"" Height=""0"" VerticalAlignment=""Top"" Width=""0"">
                                    <Rectangle x:Name=""opaqueRect"" Fill=""{Binding Background, ElementName=dropDownBorder}"" Height=""{Binding ActualHeight, ElementName=dropDownBorder}"" Width=""{Binding ActualWidth, ElementName=dropDownBorder}""/>
                                </Canvas>
                                <ItemsPresenter x:Name=""ItemsPresenter"" KeyboardNavigation.DirectionalNavigation=""Contained"" SnapsToDevicePixels=""{TemplateBinding SnapsToDevicePixels}""/>
                            </Grid>
                        </ScrollViewer>
                    </Border>
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
                <Trigger Property=""HasItems"" Value=""false"">
                    <Setter Property=""Height"" TargetName=""dropDownBorder"" Value=""95""/>
                </Trigger>
                <MultiTrigger>
                    <MultiTrigger.Conditions>
                        <Condition Property=""IsGrouping"" Value=""true""/>
                    </MultiTrigger.Conditions>
                    <Setter Property=""ScrollViewer.CanContentScroll"" Value=""false""/>
                </MultiTrigger>
                <Trigger Property=""ScrollViewer.CanContentScroll"" SourceName=""DropDownScrollViewer"" Value=""false"">
                    <Setter Property=""Canvas.Top"" TargetName=""opaqueRect"" Value=""{Binding VerticalOffset, ElementName=DropDownScrollViewer}""/>
                    <Setter Property=""Canvas.Left"" TargetName=""opaqueRect"" Value=""{Binding HorizontalOffset, ElementName=DropDownScrollViewer}""/>
                </Trigger>
            </ControlTemplate.Triggers>
        </ControlTemplate>";

        #endregion

        #region Styles

        /// <summary>
        /// ComboBoxEditableTextBox Style XAML (.NET Framework 4.0 and above)
        /// </summary>
        private const string _comboBoxEditableTextBoxStyleDefault = @"<Style TargetType=""{x:Type TextBox}"">
            <Setter Property=""OverridesDefaultStyle"" Value=""true""/>
            <Setter Property=""AllowDrop"" Value=""true""/>
            <Setter Property=""MinWidth"" Value=""0""/>
            <Setter Property=""MinHeight"" Value=""0""/>
            <Setter Property=""FocusVisualStyle"" Value=""{x:Null}""/>
            <Setter Property=""ScrollViewer.PanningMode"" Value=""VerticalFirst""/>
            <Setter Property=""Stylus.IsFlicksEnabled"" Value=""False""/>
            <Setter Property=""Template"">
                <Setter.Value>
                    <ControlTemplate TargetType=""{x:Type TextBox}"">
                        <ScrollViewer x:Name=""PART_ContentHost"" Background=""Transparent"" Focusable=""false"" HorizontalScrollBarVisibility=""Hidden"" VerticalScrollBarVisibility=""Hidden""/>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>";

        /// <summary>
        /// ComboBoxEditableTextBox Style XAML (.NET Framework 3.5)
        /// </summary>
        private const string _comboBoxEditableTextBoxStyleNet35 = @"<Style TargetType=""{x:Type TextBox}"">
            <Setter Property=""OverridesDefaultStyle"" Value=""true""/>
            <Setter Property=""AllowDrop"" Value=""true""/>
            <Setter Property=""MinWidth"" Value=""0""/>
            <Setter Property=""MinHeight"" Value=""0""/>
            <Setter Property=""FocusVisualStyle"" Value=""{x:Null}""/>
            <Setter Property=""Stylus.IsFlicksEnabled"" Value=""False""/>
            <Setter Property=""Template"">
                <Setter.Value>
                    <ControlTemplate TargetType=""{x:Type TextBox}"">
                        <ScrollViewer x:Name=""PART_ContentHost"" Background=""Transparent"" Focusable=""false"" HorizontalScrollBarVisibility=""Hidden"" VerticalScrollBarVisibility=""Hidden""/>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
        </Style>";

        /// <summary>
        /// CustomComboBoxStyle Style XAML (.NET Framework 4.0 and above)
        /// </summary>
        private const string _customComboBoxStyleDefault = @"<Style x:Key=""CustomComboBoxStyle"" TargetType=""{x:Type ComboBox}"">
            <Setter Property=""FocusVisualStyle"" Value=""{DynamicResource FocusVisual}""/>
            <Setter Property=""Background"" Value=""{DynamicResource ComboBox.Static.Background}""/>
            <Setter Property=""BorderBrush"" Value=""{DynamicResource ComboBox.Static.Border}""/>
            <Setter Property=""Foreground"" Value=""{DynamicResource {x:Static SystemColors.WindowTextBrushKey}}""/>
            <Setter Property=""BorderThickness"" Value=""1""/>
            <Setter Property=""ScrollViewer.HorizontalScrollBarVisibility"" Value=""Auto""/>
            <Setter Property=""ScrollViewer.VerticalScrollBarVisibility"" Value=""Auto""/>
            <Setter Property=""Padding"" Value=""6,3,5,3""/>
            <Setter Property=""ScrollViewer.CanContentScroll"" Value=""true""/>
            <Setter Property=""ScrollViewer.PanningMode"" Value=""VerticalFirst""/>
            <Setter Property=""Stylus.IsFlicksEnabled"" Value=""False""/>
            <Setter Property=""Template"" Value=""{DynamicResource ComboBoxTemplate}""/>
            <Style.Triggers>
                <Trigger Property=""IsEditable"" Value=""true"">
                    <Setter Property=""IsTabStop"" Value=""false""/>
                    <Setter Property=""Padding"" Value=""2""/>
                    <Setter Property=""Template"" Value=""{DynamicResource ComboBoxEditableTemplate}""/>
                </Trigger>
            </Style.Triggers>
        </Style>";

        /// <summary>
        /// CustomComboBoxStyle Style XAML (.NET Framework 3.5)
        /// </summary>
        private const string _customComboBoxStyleNet35 = @"<Style TargetType=""{x:Type ComboBox}"">
            <Setter Property=""FocusVisualStyle"" Value=""{DynamicResource FocusVisual}""/>
            <Setter Property=""Background"" Value=""{DynamicResource ComboBox.Static.Background}""/>
            <Setter Property=""BorderBrush"" Value=""{DynamicResource ComboBox.Static.Border}""/>
            <Setter Property=""Foreground"" Value=""{DynamicResource {x:Static SystemColors.WindowTextBrushKey}}""/>
            <Setter Property=""BorderThickness"" Value=""1""/>
            <Setter Property=""ScrollViewer.HorizontalScrollBarVisibility"" Value=""Auto""/>
            <Setter Property=""ScrollViewer.VerticalScrollBarVisibility"" Value=""Auto""/>
            <Setter Property=""Padding"" Value=""6,3,5,3""/>
            <Setter Property=""ScrollViewer.CanContentScroll"" Value=""true""/>
            <Setter Property=""Stylus.IsFlicksEnabled"" Value=""False""/>
            <Setter Property=""Template"" Value=""{DynamicResource ComboBoxTemplate}""/>
            <Style.Triggers>
                <Trigger Property=""IsEditable"" Value=""true"">
                    <Setter Property=""IsTabStop"" Value=""false""/>
                    <Setter Property=""Padding"" Value=""2""/>
                    <Setter Property=""Template"" Value=""{DynamicResource ComboBoxEditableTemplate}""/>
                </Trigger>
            </Style.Triggers>
        </Style>";

       #endregion

        public App()
        {
#if NET40_OR_GREATER || NETCOREAPP
            InitializeComponent();
#endif

            // Assign resource dictionaries
            SetUILanguage();

            // Create control templates
            CreateControlTemplate("ComboBoxTemplate");
            CreateControlTemplate("ComboBoxEditableTemplate");

            // Create styles
            CreateStyle("ComboBoxEditableTextBox");
            CreateStyle("CustomComboBoxStyle");
        }

        /// <summary>
        /// Create an XAML parser context with the required namespaces
        /// </summary>
        private ParserContext CreateParserContext()
        {
            var context = new ParserContext();

            context.XmlnsDictionary[""] = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            context.XmlnsDictionary["x"] = "http://schemas.microsoft.com/winfx/2006/xaml";
#if NETFRAMEWORK
            context.XmlnsDictionary["themes"] = "clr-namespace:Microsoft.Windows.Themes;assembly=PresentationFramework.Aero";
#else
            context.XmlnsDictionary["themes"] = "clr-namespace:Microsoft.Windows.Themes;assembly=PresentationFramework.Aero2";
#endif
            context.XamlTypeMapper = new XamlTypeMapper([]);

            return context;
        }

        /// <summary>
        /// Create a named control template and add it to the current set of resources
        /// </summary>
        private void CreateControlTemplate(string resourceName)
        {
            var parserContext = CreateParserContext();
            var controlTemplate = resourceName switch
            {
#if NET35
                "ComboBoxTemplate" => XamlReader.Parse(_comboBoxTemplateNet35, parserContext) as ControlTemplate,
                "ComboBoxEditableTemplate" => XamlReader.Parse(_comboBoxEditableTemplateNet35, parserContext) as ControlTemplate,
#else
                "ComboBoxTemplate" => XamlReader.Parse(_comboBoxTemplateDefault, parserContext) as ControlTemplate,
                "ComboBoxEditableTemplate" => XamlReader.Parse(_comboBoxEditableTemplateDefault, parserContext) as ControlTemplate,
#endif
                _ => throw new ArgumentException($"'{resourceName}' is not a recognized control template", nameof(resourceName)),
            };

            // Add the control template
            Resources[resourceName] = controlTemplate;
        }

        /// <summary>
        /// Create a strings resource dictionary for use as the UI language
        /// </summary>
        private void SetUILanguage()
        {
            // Add English strings to merged dictionary as default, add translations later
            var baselineDictionary = new ResourceDictionary();
            baselineDictionary.Source = new Uri("Resources/Strings.xaml", UriKind.Relative);
            Resources.MergedDictionaries.Add(baselineDictionary);

            // Get current region code 
            string region = "";
            try
            {
                // Can throw exception
                region = new RegionInfo(culture.Name).TwoLetterISORegionName;
            }
            catch { }

            // Select startup language based on current system locale
            var translatedDictionary = new ResourceDictionary();
            switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
            {
                case "en":
                    // Don't add any translated text
                    break;
                
                case "ko":
                    // Translate UI elements to Korean
                    translatedDictionary.Source = new Uri("Resources/Strings.ko.xaml", UriKind.Relative);
                    break;
                
                case "zh":
                    // Check if region uses Traditional Chinese
                    string[] traditionalRegions = { "TW", "HK", "MO" };
                    if (Array.Exists(traditionalRegions, r => r.Equals(region, StringComparison.OrdinalIgnoreCase)))
                    {
                        // TODO: Translate UI elements to Traditional Chinese
                        translatedDictionary.Source = new Uri("Resources/Strings.xaml", UriKind.Relative);
                    }
                    else
                    {
                        // TODO: Translate UI elements to Simplified Chinese
                        translatedDictionary.Source = new Uri("Resources/Strings.xaml", UriKind.Relative);
                    }
                
                default:
                    // Unsupported language
                    break;
            }

            // Add the strings resource to app resources
            Resources.MergedDictionaries.Add(translatedDictionary);
        }

        /// <summary>
        /// Create a named style and add it to the current set of resources
        /// </summary>
        private void CreateStyle(string resourceName)
        {
            var parserContext = CreateParserContext();
            var style = resourceName switch
            {
#if NET35
                "ComboBoxEditableTextBox" => XamlReader.Parse(_comboBoxEditableTextBoxStyleNet35, parserContext) as Style,
                "CustomComboBoxStyle" => XamlReader.Parse(_customComboBoxStyleNet35, parserContext) as Style,
#else
                "ComboBoxEditableTextBox" => XamlReader.Parse(_comboBoxEditableTextBoxStyleDefault, parserContext) as Style,
                "CustomComboBoxStyle" => XamlReader.Parse(_customComboBoxStyleDefault, parserContext) as Style,
#endif
                _ => throw new ArgumentException($"'{resourceName}' is not a recognized style", nameof(resourceName)),
            };

            // Add the style
            Resources[resourceName] = style;
        }
#endif
    }
}
