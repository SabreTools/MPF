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
        public void ScanForDisk()
        {
            BTN_Search.IsEnabled = false;
            CB_DriveLetter.Items.Clear();
            foreach (var d in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom))
            {
                if (d.IsReady == true)
                {
                    TXT_OutputFilename.Text = d.VolumeLabel;
                    
                    if (TXT_OutputFilename.Text == "")
                    {
                        TXT_OutputFilename.Text = "unknown";
                    }
                    CB_DriveLetter.Items.Add(d.Name.Replace(":\\", ""));
                    CB_DriveLetter.SelectedIndex = 0;
                    TXT_OutputDirectory.Text = "ISO" + "\\" + TXT_OutputFilename.Text + "\\";
                    LBL_Status.Content = "CD or DVD found ! Choose your Disc Type";
                    BTN_Start.IsEnabled = true;
                    CB_DriveSpeed.Text = "8";
                }
                else
                {
                    LBL_Status.Content = "No CD or DVD found !";
                }
            BTN_Search.IsEnabled = true;
            }
        }

        public void BrowseFolder()
        {
            WinForms.FolderBrowserDialog folderDialog = new WinForms.FolderBrowserDialog { ShowNewFolderButton = false, SelectedPath = System.AppDomain.CurrentDomain.BaseDirectory };
            WinForms.DialogResult result = folderDialog.ShowDialog();

            if (result == WinForms.DialogResult.OK)
            {
                String sPath = folderDialog.SelectedPath;
                TXT_OutputDirectory.Text = sPath;
            }
        }

        public async void StartDumping()
        {
            String VAR_Type = "";
            String VAR_Switches = "";
            String VAR_DriveLetter = CB_DriveLetter.Text;
            String VAR_OutputDirectory = TXT_OutputDirectory.Text;
            String VAR_OutputFilename = TXT_OutputFilename.Text;
            String VAR_DriveSpeed = CB_DriveSpeed.Text;
            Boolean VAR_IsPSX = false;
            Boolean VAR_IsXBOXorPS4 = false;
            BTN_Start.IsEnabled = false;

            switch (Convert.ToString(CB_DiscType.Text))
            {
                #region Consoles

                case "Bandai Playdia Quick Interactive System":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "Bandai / Apple Pippin":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "Commodore Amiga CD / CD32 / CDTV":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "Mattel HyperScan":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "Microsoft XBOX":
                    // Placeholder for later use
                    break;
                case "Microsoft XBOX 360":
                    // Placeholder for later use
                    break;
                case "Microsoft XBOX One":
                    VAR_Type = "bd";
                    VAR_Switches = "";
                    VAR_IsXBOXorPS4 = true;
                    break;
                case "NEC PC-Engine / TurboGrafx CD":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "NEC PC-FX / PC-FXGA":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
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
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "Philips CD-i":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "Sega CD / Mega CD":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "Sega Dreamcast":
                    // Placeholder for later use
                    break;
                case "Sega Saturn":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "SNK Neo Geo CD":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "Sony PlayStation":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    VAR_IsPSX = true;
                    break;
                case "Sony PlayStation 2 (CD-Rom)":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "Sony PlayStation 2 (DVD-Rom)":
                    VAR_Type = "dvd";
                    VAR_Switches = "";
                    break;
                case "Sony PlayStation 3":
                    // Placeholder for later use
                    break;
                case "Sony PlayStation 4":
                    VAR_Type = "bd";
                    VAR_Switches = "";
                    VAR_IsXBOXorPS4 = true;
                    break;
                case "Sony PlayStation Portable":
                    // No-op - PSP can't be dumped with DIC
                    break;
                case "VM Labs NUON":
                    // Placeholder for later use
                    break;
                case "VTech V.Flash - V.Smile Pro":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
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
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "Apple Macintosh (DVD-Rom)":
                    VAR_Type = "dvd";
                    VAR_Switches = "";
                    break;
                case "Fujitsu FM Towns series":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "IBM PC Compatible (CD-Rom)":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2 /ns /sf /ss";
                    break;
                case "IBM PC Compatible (DVD-Rom)":
                    VAR_Type = "dvd";
                    VAR_Switches = "";
                    break;
                case "NEC PC-88":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "NEC PC-98":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
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
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
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
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "BD-Video":
                    VAR_Type = "bd";
                    VAR_Switches = "";
                    break;
                case "DVD-Video":
                    VAR_Type = "dvd";
                    break;
                case "PalmOS":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "Philips CD-i Digital Video":
                    // Placeholder for later use
                    break;
                case "Photo CD":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "PlayStation GameShark Updates":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "Tao iKTV":
                    // Placeholder for later use
                    break;
                case "Tomy Kiss-Site":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;
                case "Video CD":
                    VAR_Type = "cd";
                    VAR_Switches = "/c2";
                    break;

                #endregion

                case "Unknown":
                default:
                    VAR_Type = "";
                    VAR_Switches = "";
                    break;
            }

            await Task.Run(
                () =>
                {
                    Process process = new Process();
                    process.StartInfo.FileName = "Release_ANSI\\DiscImageCreator.exe";
                    process.StartInfo.Arguments = VAR_Type + " " + VAR_DriveLetter + " \"" + VAR_OutputDirectory + "\\" + VAR_OutputFilename + "\" " + VAR_DriveSpeed + " " + VAR_Switches;
                    Console.WriteLine(process.StartInfo.Arguments);
                    process.Start();
                    process.WaitForExit();
                });

            if (VAR_IsXBOXorPS4 == true)
            {
                using (StreamWriter writetext = new StreamWriter("PS4orXBOXONE.bat"))
                {
                    writetext.WriteLine("sg_raw.exe -v -r 4100 -R " + VAR_DriveLetter + ":" + "ad 01 00 00 00 00 00 00 10 04 00 00 -o \"PIC.bin\"");
                }
                Process processps4orxboxone = new Process();
                processps4orxboxone.StartInfo.FileName = "PS4orXBOXONE.bat";
                processps4orxboxone.Start();
                processps4orxboxone.WaitForExit();
            }
            if (VAR_IsPSX == true)
            {
                using (StreamWriter writetext = new StreamWriter("PSX.bat"))
                {
                    writetext.WriteLine("edccchk" + " " + "\"" + VAR_OutputDirectory + "\\" + VAR_OutputFilename + ".bin" + "\" > " + "\"" + VAR_OutputDirectory + "\\" + "edccchk1.txt");
                    writetext.WriteLine("edccchk" + " " + "\"" + VAR_OutputDirectory + "\\" + VAR_OutputFilename + " (Track 1).bin" + "\" > " + "\"" + VAR_OutputDirectory + "\\" + "edccchk1.txt");
                    writetext.WriteLine("edccchk" + " " + "\"" + VAR_OutputDirectory + "\\" + VAR_OutputFilename + " (Track 01).bin" + "\" > " + "\"" + VAR_OutputDirectory + "\\" + "edccchk1.txt");
                    writetext.WriteLine("psxt001z" + " " + "\"" + VAR_OutputDirectory + "\\" + VAR_OutputFilename + ".bin" + "\" > " + "\"" + VAR_OutputDirectory + "\\" + "psxt001z1.txt");
                    writetext.WriteLine("psxt001z" + " " + "\"" + VAR_OutputDirectory + "\\" + VAR_OutputFilename + " (Track 1).bin" + "\" > " + "\"" + VAR_OutputDirectory + "\\" + "psxt001z2.txt");
                    writetext.WriteLine("psxt001z" + " " + "\"" + VAR_OutputDirectory + "\\" + VAR_OutputFilename + " (Track 01).bin" + "\" > " + "\"" + VAR_OutputDirectory + "\\" + "psxt001z3.txt");
                    writetext.WriteLine("psxt001z" + " " + "--libcrypt " + "\"" + VAR_OutputDirectory + "\\" + VAR_OutputFilename + ".sub\" > " + "\"" + VAR_OutputDirectory + "\\" + "libcrypt.txt");
                    writetext.WriteLine("psxt001z" + " " + "--libcryptdrvfast " + VAR_DriveLetter + " > " + "\"" + VAR_OutputDirectory + "\\" + "libcryptdrv.log");
                }
                Process processpsx = new Process();
                processpsx.StartInfo.FileName = "PSX.bat";
                processpsx.Start();
                processpsx.WaitForExit();
            }
            BTN_Start.IsEnabled = true;



        }

        public MainWindow()
        {
            InitializeComponent();
            ScanForDisk();
        }

        private void BTN_Start_Click(object sender, RoutedEventArgs e)
        {
            StartDumping();  
        }

        private void BTN_OutputDirectoryBrowse_Click(object sender, RoutedEventArgs e)
        {
            BrowseFolder();
        }

        private void BTN_Search_Click(object sender, RoutedEventArgs e)
        {
            ScanForDisk();
        }

        private void CB_DiscType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LBL_Status.Content = "Ready to dump";
        }

        private void CB_DriveSpeed_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CB_DriveSpeed.SelectedIndex == 4)
            {
                CB_DriveSpeed.Items.Clear();
                CB_DriveSpeed.IsEditable = true;
                
            }
        }
    }
}
