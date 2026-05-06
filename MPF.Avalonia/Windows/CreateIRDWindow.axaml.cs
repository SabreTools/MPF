using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MPF.Avalonia.Services;
using MPF.Frontend.ViewModels;

namespace MPF.Avalonia.Windows
{
    public partial class CreateIRDWindow : WindowBase
    {
        public CreateIRDViewModel CreateIRDViewModel => DataContext as CreateIRDViewModel ?? new CreateIRDViewModel();

        public CreateIRDWindow()
        {
            InitializeWindow();
        }

        public CreateIRDWindow(MainWindow parent)
        {
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            InitializeComponent();
            DataContext = new CreateIRDViewModel();
            Opened += (_, _) => WireEvents();
        }

        private void WireEvents()
        {
            this.FindControl<Button>("CreateIRDButton")!.Click += OnCreateIRDClick;
            this.FindControl<Button>("CancelButton")!.Click += OnCancelClick;
            this.FindControl<Button>("InputPathBrowseButton")!.Click += InputPathBrowseButtonClick;
            this.FindControl<Button>("LogPathBrowseButton")!.Click += LogPathBrowseButtonClick;
            this.FindControl<Button>("KeyPathBrowseButton")!.Click += KeyPathBrowseButtonClick;
            this.FindControl<Button>("PICPathBrowseButton")!.Click += PICPathBrowseButtonClick;

            this.FindControl<TextBox>("InputPathTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangeInputPath();
            this.FindControl<TextBox>("LogPathTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangeLogPath();
            this.FindControl<TextBox>("KeyPathTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangeKeyPath();
            this.FindControl<TextBox>("KeyTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangeKey();
            this.FindControl<TextBox>("DiscIDTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangeDiscID();
            this.FindControl<TextBox>("PICPathTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangePICPath();
            this.FindControl<TextBox>("PICTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangePIC();
            this.FindControl<TextBox>("LayerbreakTextBox")!.TextChanged += (_, _) => CreateIRDViewModel.ChangeLayerbreak();
        }

        private async Task<string?> OpenFileAsync(string title, params string[] patterns)
        {
            return await DialogService.OpenFileAsync(this, title, new List<FilePickerFileType>
            {
                new(title) { Patterns = patterns },
                FilePickerFileTypes.All,
            });
        }

        private async void OnCreateIRDClick(object? sender, RoutedEventArgs e)
        {
            string? outputPath = await DialogService.SaveFileAsync(this, "Save IRD", "game.ird", new List<FilePickerFileType>
            {
                new("IRD File") { Patterns = new[] { "*.ird" } },
            });

            if (string.IsNullOrWhiteSpace(outputPath))
                return;

            string? result = CreateIRDViewModel.CreateIRD(outputPath);
            if (string.IsNullOrWhiteSpace(result))
                DisplayUserMessage("IRD Create", "An IRD has been created successfully.", 1, false);
            else
                DisplayUserMessage("Failed to create IRD", result, 1, false);
        }

        private void OnCancelClick(object? sender, RoutedEventArgs e)
            => Close(false);

        public async void InputPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            string? path = await OpenFileAsync("ISO", "*.iso");
            if (!string.IsNullOrWhiteSpace(path))
                CreateIRDViewModel.InputPath = path;
        }

        public async void LogPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            string? path = await OpenFileAsync("GetKey Log", "*.getkey.log");
            if (!string.IsNullOrWhiteSpace(path))
                CreateIRDViewModel.LogPath = path;
        }

        public async void KeyPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            string? path = await OpenFileAsync("Key", "*.key");
            if (!string.IsNullOrWhiteSpace(path))
                CreateIRDViewModel.KeyPath = path;
        }

        public async void PICPathBrowseButtonClick(object? sender, RoutedEventArgs e)
        {
            string? path = await OpenFileAsync("PIC", "*.physical", "*_PIC.bin", "*.PIC");
            if (!string.IsNullOrWhiteSpace(path))
                CreateIRDViewModel.PICPath = path;
        }
    }
}
