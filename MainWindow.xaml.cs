using System;
using System.Linq;
using System.Windows;
using System.IO;
using WinForms = System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;


namespace DICUI
{

    public partial class MainWindow : Window
    {
        public String discType;
        public String processArguments;
        public String dicPath = "Programs\\DiscImageCreator.exe";
        public String driveLetter;
        public String outputDirectory;
        public String outputFileName;
        public String driveSpeed;
        public Boolean isPSX = false;
        public Boolean isXboneOrPS4 = false;

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

            driveLetter = cmb_DriveLetter.Text;
            outputDirectory = txt_OutputDirectory.Text;
            outputFileName = txt_OutputFilename.Text;
            driveSpeed = cmb_DriveSpeed.Text;
            btn_Start.IsEnabled = false;

            switch (Convert.ToString(cmb_DiscType.Text))
            {
                #region Consoles

                case "Bandai Playdia Quick Interactive System":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Bandai / Apple Pippin":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Commodore Amiga CD / CD32 / CDTV":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Mattel HyperScan":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Microsoft XBOX":
                    // Placeholder for later use
                    break;
                case "Microsoft XBOX 360":
                    // Placeholder for later use
                    break;
                case "Microsoft XBOX One":
                    discType = "bd";
                    processArguments = "";
                    isXboneOrPS4 = true;
                    break;
                case "NEC PC-Engine / TurboGrafx CD":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "NEC PC-FX / PC-FXGA":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Nintendo GameCube":
                    // Placeholder for later use
                    break;
                case "Nintendo Wii":
                    // Placeholder for later use
                    break;
                case "Nintendo Wii U":
                    // Placeholder for later use
                    break;
                case "Panasonic 3DO":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Philips CD-i":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Sega CD / Mega CD":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Sega Dreamcast":
                    // Placeholder for later use
                    break;
                case "Sega Saturn":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "SNK Neo Geo CD":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Sony PlayStation":
                    discType = "cd";
                    processArguments = "/c2 20";
                    isPSX = true;
                    break;
                case "Sony PlayStation 2 (CD-Rom)":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Sony PlayStation 2 (DVD-Rom)":
                    discType = "dvd";
                    processArguments = "";
                    break;
                case "Sony PlayStation 3":
                    // Placeholder for later use
                    break;
                case "Sony PlayStation 4":
                    discType = "bd";
                    processArguments = "";
                    isXboneOrPS4 = true;
                    break;
                case "Sony PlayStation Portable":
                    // No-op - PSP can't be dumped with DIC
                    break;
                case "VM Labs NUON":
                    // Placeholder for later use
                    break;
                case "VTech V.Flash - V.Smile Pro":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "ZAPiT Games Game Wave Family Entertainment System":
                    // Placeholder for later use
                    break;

                #endregion

                #region Computers

                case "Acorn Archimedes":
                    // Placeholder for later use
                    break;
                case "Apple Macintosh (CD-Rom)":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Apple Macintosh (DVD-Rom)":
                    discType = "dvd";
                    processArguments = "";
                    break;
                case "Fujitsu FM Towns series":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "IBM PC Compatible (CD-Rom)":
                    discType = "cd";
                    processArguments = "/c2 20 /ns /sf /ss";
                    break;
                case "IBM PC Compatible (DVD-Rom)":
                    discType = "dvd";
                    processArguments = "";
                    break;
                case "NEC PC-88":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "NEC PC-98":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Sharp X68000":
                    // Placeholder for later use
                    break;

                #endregion

                #region Arcade

                case "Namco / Sega / Nintendo Triforce":
                    // Placeholder for later use
                    break;
                case "Sega Chihiro":
                    // Placeholder for later use
                    break;
                case "Sega Lindbergh":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Sega Naomi":
                    // Placeholder for later use
                    break;
                case "Sega Naomi 2":
                    // Placeholder for later use
                    break;
                case "TAB-Austria Quizard":
                    // Placeholder for later use
                    break;
                case "Tandy / Memorex Visual Information System":
                    // Placeholder for later use
                    break;

                #endregion

                #region Others

                case "Audio CD":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "BD-Video":
                    discType = "bd";
                    processArguments = "";
                    break;
                case "DVD-Video":
                    discType = "dvd";
                    break;
                case "PalmOS":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Philips CD-i Digital Video":
                    // Placeholder for later use
                    break;
                case "Photo CD":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "PlayStation GameShark Updates":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Tao iKTV":
                    // Placeholder for later use
                    break;
                case "Tomy Kiss-Site":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;
                case "Video CD":
                    discType = "cd";
                    processArguments = "/c2 20";
                    break;

                #endregion

                case "Unknown":
                default:
                    discType = "";
                    processArguments = "";
                    break;
            }

            await Task.Run(
                () =>
                {
                    Process process = new Process();
                    process.StartInfo.FileName = dicPath; // TODO: Make this configurable in UI
                    process.StartInfo.Arguments = discType + " " + driveLetter + " \"" + outputDirectory + "\\" + outputFileName + "\" " + driveSpeed + " " + processArguments;
                    Console.WriteLine(process.StartInfo.Arguments);
                    process.Start();
                    process.WaitForExit();
                });

            if (isXboneOrPS4 == true)
            {
                // TODO: Add random string / GUID to end of batch file name so that multiple instances can run at once
                using (StreamWriter writetext = new StreamWriter("XboneOrPS4.bat"))
                {
                    writetext.WriteLine("sg_raw.exe -v -r 4100 -R " + driveLetter + ": " + "ad 01 00 00 00 00 00 00 10 04 00 00 -o \"PIC.bin\"");
                }
                Process processXboneOrPS4 = new Process();
                processXboneOrPS4.StartInfo.FileName = "XboneOrPS4.bat";
                processXboneOrPS4.Start();
                processXboneOrPS4.WaitForExit();
            }
            if (isPSX == true)
            {
                // TODO: Add random string / GUID to end of batch file name so that multiple instances can run at once
                using (StreamWriter writetext = new StreamWriter("PSX.bat"))
                {
                    writetext.WriteLine("edccchk" + " " + "\"" + outputDirectory + "\\" + outputFileName + ".bin" + "\" > " + "\"" + outputDirectory + "\\" + "edccchk1.txt");
                    writetext.WriteLine("edccchk" + " " + "\"" + outputDirectory + "\\" + outputFileName + " (Track 1).bin" + "\" > " + "\"" + outputDirectory + "\\" + "edccchk1.txt");
                    writetext.WriteLine("edccchk" + " " + "\"" + outputDirectory + "\\" + outputFileName + " (Track 01).bin" + "\" > " + "\"" + outputDirectory + "\\" + "edccchk1.txt");
                    writetext.WriteLine("psxt001z" + " " + "\"" + outputDirectory + "\\" + outputFileName + ".bin" + "\" > " + "\"" + outputDirectory + "\\" + "psxt001z1.txt");
                    writetext.WriteLine("psxt001z" + " " + "\"" + outputDirectory + "\\" + outputFileName + " (Track 1).bin" + "\" > " + "\"" + outputDirectory + "\\" + "psxt001z2.txt");
                    writetext.WriteLine("psxt001z" + " " + "\"" + outputDirectory + "\\" + outputFileName + " (Track 01).bin" + "\" > " + "\"" + outputDirectory + "\\" + "psxt001z3.txt");
                    writetext.WriteLine("psxt001z" + " " + "--libcrypt " + "\"" + outputDirectory + "\\" + outputFileName + ".sub\" > " + "\"" + outputDirectory + "\\" + "libcrypt.txt");
                    writetext.WriteLine("psxt001z" + " " + "--libcryptdrvfast " + driveLetter + " > " + "\"" + outputDirectory + "\\" + "libcryptdrv.log");
                }
                Process processpsx = new Process();
                processpsx.StartInfo.FileName = "PSX.bat";
                processpsx.Start();
                processpsx.WaitForExit();
            }
            btn_Start.IsEnabled = true;
        }

        public MainWindow()
        {
            InitializeComponent();
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



        private void cmb_DiscType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {


            lbl_Status.Content = "Ready to dump";
        }

        private void cmb_DriveSpeed_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
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
            switch (Convert.ToString(cmb_DiscType.Text))
            {

                case "Sony PlayStation 4":
                    cmb_DriveSpeed.Items.Clear();
                    cmb_DriveSpeed.IsEnabled = false;
                    break;
                case "Microsoft XBOX One":
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
