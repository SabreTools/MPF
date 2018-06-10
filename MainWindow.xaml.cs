using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using WinForms = System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DICUI
{
    public partial class MainWindow : Window
    {
        private const string dicPath = "Programs\\DiscImageCreator.exe"; // TODO: Make configurable in UI or in Settings
        private List<Tuple<string, KnownSystem?, DiscType?>> _systems { get; set; }

        public void ScanForDisk()
        {
            btn_Search.IsEnabled = false;
            cmb_DriveLetter.Items.Clear();
            foreach (var d in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom))
            {
                if (d.IsReady == true)
                {
                    txt_OutputFilename.Text = d.VolumeLabel;
                    
                    if (txt_OutputFilename.Text == "")
                    {
                        txt_OutputFilename.Text = "unknown";
                    }
                    cmb_DriveLetter.Items.Add(d.Name.Replace(":\\", ""));
                    cmb_DriveLetter.SelectedIndex = 0;
                    txt_OutputDirectory.Text = "ISO" + "\\" + txt_OutputFilename.Text + "\\";
                    lbl_Status.Content = "CD or DVD found ! Choose your Disc Type";
                    btn_Start.IsEnabled = true;
                    cmb_DriveSpeed.Text = "8";
                    break;
                }
                else
                {
                    lbl_Status.Content = "No CD or DVD found !";
                }

                btn_Search.IsEnabled = true;
            }
        }

        public void BrowseFolder()
        {
            WinForms.FolderBrowserDialog folderDialog = new WinForms.FolderBrowserDialog { ShowNewFolderButton = false, SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory };
            WinForms.DialogResult result = folderDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                String sPath = folderDialog.SelectedPath;
                txt_OutputDirectory.Text = sPath;
            }
        }

        public async void StartDumping()
        {
            // Local variables
            string driveLetter = cmb_DriveLetter.Text;
            string outputDirectory = txt_OutputDirectory.Text;
            string outputFileName = txt_OutputFilename.Text;
            string driveSpeed = cmb_DriveSpeed.Text;
            btn_Start.IsEnabled = false;

            // Get the discType and processArguments from a given system and disc combo
            var selected = cmb_DiscType.SelectedValue as Tuple<string, KnownSystem?, DiscType?>;
            string discType = Utilities.GetBaseCommand(selected.Item3);
            string processArguments = string.Join(" ", Utilities.GetDefaultParameters(selected.Item2, selected.Item3));

            await Task.Run(
                () =>
                {
                    Process process = new Process();
                    process.StartInfo.FileName = dicPath;
                    process.StartInfo.Arguments = discType + " " + driveLetter + " \"" + outputDirectory + "\\" + outputFileName + "\" " + driveSpeed + " " + processArguments;
                    Console.WriteLine(process.StartInfo.Arguments);
                    process.Start();
                    process.WaitForExit();
                });

            // Special cases
            string guid = Guid.NewGuid().ToString();
            switch (selected.Item2)
            {
                case KnownSystem.MicrosoftXBOXOne:
                case KnownSystem.SonyPlayStation4:
                    // TODO: Direct invocation of program instead of via Batch File
                    using (StreamWriter writetext = new StreamWriter("XboneOrPS4" + guid + ".bat"))
                    {
                        writetext.WriteLine("sg_raw.exe -v -r 4100 -R " + driveLetter + ": " + "ad 01 00 00 00 00 00 00 10 04 00 00 -o \"PIC.bin\"");
                    }
                    Process processXboneOrPS4 = new Process();
                    processXboneOrPS4.StartInfo.FileName = "XboneOrPS4" + guid + ".bat";
                    processXboneOrPS4.Start();
                    processXboneOrPS4.WaitForExit();
                    break;
                case KnownSystem.SonyPlayStation:
                    // TODO: Direct invocation of program instead of via Batch File
                    using (StreamWriter writetext = new StreamWriter("PSX" + guid + ".bat"))
                    {
                        writetext.WriteLine("psxt001z" + " " + "\"" + outputDirectory + "\\" + outputFileName + ".bin" + "\" > " + "\"" + outputDirectory + "\\" + "psxt001z1.txt");
                        writetext.WriteLine("psxt001z" + " " + "\"" + outputDirectory + "\\" + outputFileName + " (Track 1).bin" + "\" > " + "\"" + outputDirectory + "\\" + "psxt001z2.txt");
                        writetext.WriteLine("psxt001z" + " " + "\"" + outputDirectory + "\\" + outputFileName + " (Track 01).bin" + "\" > " + "\"" + outputDirectory + "\\" + "psxt001z3.txt");
                        writetext.WriteLine("psxt001z" + " " + "--libcrypt " + "\"" + outputDirectory + "\\" + outputFileName + ".sub\" > " + "\"" + outputDirectory + "\\" + "libcrypt.txt");
                        writetext.WriteLine("psxt001z" + " " + "--libcryptdrvfast " + driveLetter + " > " + "\"" + outputDirectory + "\\" + "libcryptdrv.log");
                    }
                    Process processpsx = new Process();
                    processpsx.StartInfo.FileName = "PSX" + guid + ".bat";
                    processpsx.Start();
                    processpsx.WaitForExit();
                    break;
            }

            btn_Start.IsEnabled = true;
        }

        public MainWindow()
        {
            InitializeComponent();

            // Populate the list of systems and add it to the combo box
            _systems = Utilities.CreateListOfSystems();
            cmb_DiscType.ItemsSource = _systems;
            cmb_DiscType.DisplayMemberPath = "Item1";
            cmb_DiscType.SelectedIndex = 0;
            cmb_DiscType_SelectionChanged(null, null);

            ScanForDisk();
        }

        private void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            StartDumping();  
        }

        private void BTN_OutputDirectoryBrowse_Click(object sender, RoutedEventArgs e)
        {
            BrowseFolder();
        }

        private void btn_Search_Click(object sender, RoutedEventArgs e)
        {
            ScanForDisk();
        }

        private void cmb_DiscType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If we're on a separator, go to the next item
            var tuple = cmb_DiscType.SelectedItem as Tuple<string, KnownSystem?, DiscType?>;
            if (tuple.Item2 == null && tuple.Item3 == null)
            {
                cmb_DiscType.SelectedIndex++;
                tuple = cmb_DiscType.SelectedItem as Tuple<string, KnownSystem?, DiscType?>;
            }

            // If we're on an unsupported type, update the status accordingly
            switch (tuple.Item3)
            {
                case DiscType.NONE:
                    lbl_Status.Content = "Please select a valid disc type";
                    break;
                case DiscType.GDROM:
                    lbl_Status.Content = string.Format("{0} discs are partially supported by DIC", Utilities.DiscTypeToString(tuple.Item3));
                    break;
                case DiscType.GameCubeGameDisc:
                case DiscType.HDDVD:
                case DiscType.UMD:
                    lbl_Status.Content = string.Format("{0} discs are not currently supported by DIC", Utilities.DiscTypeToString(tuple.Item3));
                    break;
                default:
                    lbl_Status.Content = string.Format("{0} ready to dump", Utilities.DiscTypeToString(tuple.Item3));
                    break;
            }
        }

        private void cmb_DriveSpeed_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Figure out how to keep the list of items while also allowing the custom input
            if (cmb_DriveSpeed.SelectedIndex == 4)
            {
                cmb_DriveSpeed.Items.Clear();
                cmb_DriveSpeed.IsEditable = true;
            }
        }

        private void cmb_DiscType_DropDownClosed(object sender, EventArgs e)
        {
			var tuple = cmb_DiscType.SelectedItem as Tuple<string, KnownSystem?, DiscType?>;
			switch (tuple.Item2)
            {
				case KnownSystem.MicrosoftXBOXOne:
				case KnownSystem.SonyPlayStation4:
                    cmb_DriveSpeed.Items.Clear();
                    cmb_DriveSpeed.IsEnabled = false;
                    break;
                default:
                    cmb_DriveSpeed.IsEnabled = true;
                    cmb_DriveSpeed.Items.Clear();
                    cmb_DriveSpeed.Items.Add("4");
                    cmb_DriveSpeed.Items.Add("8");
                    cmb_DriveSpeed.Items.Add("16");
                    cmb_DriveSpeed.Items.Add("48");
                    cmb_DriveSpeed.Items.Add("Custom");
                    cmb_DriveSpeed.SelectedIndex = 1;
                    break;
            }

        }
    }
}
