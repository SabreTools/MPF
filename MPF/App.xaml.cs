using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Windows.Themes;

namespace MPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

#if NET40_OR_GREATER || NETCOREAPP
        public App()
        {
            InitializeComponent();

            AddDropShadowChrome("ComboBoxTemplate");
            AddDropShadowChrome("ComboBoxEditableTemplate");
        }

        private void AddDropShadowChrome(string resourceName)
        {
            // Get the template resource
            if (Resources[resourceName] is not ControlTemplate controlTemplate)
                return;

            // Get the popup
            if (controlTemplate.Resources["PART_Popup"] is not Popup popup)
                return;

            // Create the various nested items
            var chrome = new SystemDropShadowChrome
            {
                Name = "shadow",
                Color = Colors.Transparent,
            };
            var border = new Border
            {
                Name = "dropDownBorder",
                BorderThickness = new Thickness(1),
            };
            var scrollViewer = new ScrollViewer { Name = "DropDownScrollViewer" };
            var grid = new Grid { Name = "grid" };
            RenderOptions.SetClearTypeHint(grid, ClearTypeHint.Enabled);
            var canvas = new Canvas
            {
                Name = "canvas",
                HorizontalAlignment = HorizontalAlignment.Left,
                Height = 0,
                VerticalAlignment = VerticalAlignment.Top,
                Width = 0,
            };
            var rectangle = new Rectangle { Name = "opaqueRect" };
            var itemsPresenter = new ItemsPresenter { Name = "ItemsPresenter" };
            KeyboardNavigation.SetDirectionalNavigation(itemsPresenter, KeyboardNavigationMode.Contained);

            // Set the bindings
            BindingOperations.SetBinding(chrome, SystemDropShadowChrome.MaxHeightProperty, new Binding
            {
                Source = new TemplateBindingExtension(ComboBox.MaxDropDownHeightProperty),
            });
            BindingOperations.SetBinding(chrome, SystemDropShadowChrome.MinWidthProperty, new Binding
            {
                Source = ComboBox.ActualWidthProperty,
                ElementName = "templateRoot",
            });

            BindingOperations.SetBinding(border, Border.BorderBrushProperty, new Binding
            {
                Source = new DynamicResourceExtension(new StaticResourceExtension(SystemColors.WindowFrameBrushKey)),
            });
            BindingOperations.SetBinding(border, Border.BackgroundProperty, new Binding
            {
                Source = new DynamicResourceExtension(new StaticResourceExtension(SystemColors.WindowBrushKey)),
            });

            BindingOperations.SetBinding(rectangle, Rectangle.FillProperty, new Binding
            {
                Source = Border.BackgroundProperty,
                ElementName = "dropDownBorder",
            });
            BindingOperations.SetBinding(rectangle, Rectangle.HeightProperty, new Binding
            {
                Source = Border.ActualHeightProperty,
                ElementName = "dropDownBorder",
            });
            BindingOperations.SetBinding(rectangle, Rectangle.WidthProperty, new Binding
            {
                Source = Border.ActualWidthProperty,
                ElementName = "dropDownBorder",
            });

            BindingOperations.SetBinding(itemsPresenter, ItemsPresenter.SnapsToDevicePixelsProperty, new Binding
            {
                Source = new TemplateBindingExtension(ComboBox.SnapsToDevicePixelsProperty),
            });

            // Build the item tree
            canvas.Children.Add(rectangle);
            grid.Children.Add(canvas);
            grid.Children.Add(itemsPresenter);
            scrollViewer.Content = grid;
            border.Child = scrollViewer;
            chrome.Child = border;

            // Add the tree to the popup
            popup.Child = chrome;

            // Get the triggers
            var triggers = controlTemplate.Triggers;
            if (triggers == null)
                return;

            // Create the new triggers
            var hasDropShadow = new Trigger
            {
                Property = Popup.HasDropShadowProperty,
                SourceName = "PART_Popup",
                Value = "true",
            };
            var hasItems = new Trigger
            {
                Property = ComboBox.HasItemsProperty,
                Value = "false",
            };
            var canContentScroll = new Trigger
            {
                Property = ScrollViewer.CanContentScrollProperty,
                SourceName = "DropDownScrollViewer",
                Value = "false",
            };

            // Create and add the setters
            hasDropShadow.Setters.Add(new Setter
            {
                Property = SystemDropShadowChrome.MarginProperty,
                TargetName = "shadow",
                Value = "0,0,5,5",
            });
            hasDropShadow.Setters.Add(new Setter
            {
                Property = SystemDropShadowChrome.ColorProperty,
                TargetName = "shadow",
                Value = "#71000000",
            });

            hasItems.Setters.Add(new Setter
            {
                Property = Border.HeightProperty,
                TargetName = "dropDownBorder",
                Value = "95",
            });

            canContentScroll.Setters.Add(new Setter
            {
                Property = Canvas.TopProperty,
                TargetName = "opaqueRect",
                Value = new Binding
                {
                    Source = ScrollViewer.VerticalOffsetProperty,
                    ElementName = "DropDownScrollViewer",
                },
            });
            canContentScroll.Setters.Add(new Setter
            {
                Property = Canvas.LeftProperty,
                TargetName = "opaqueRect",
                Value = new Binding
                {
                    Source = ScrollViewer.HorizontalOffsetProperty,
                    ElementName = "DropDownScrollViewer",
                },
            });

            // Add the new triggers
            triggers.Add(hasDropShadow);
            triggers.Add(hasItems);
            triggers.Add(canContentScroll);
        }
#endif
    }
}
