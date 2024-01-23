/*  
╔═╗╦ ╦╔╗ ╦╔╦╗
║  ║ ║╠╩╗║ ║ 
╚═╝╚═╝╚═╝╩ ╩
 */

#region Using
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using ListViewItem = System.Windows.Forms.ListViewItem;
using Panel = System.Windows.Forms.Panel;
using System.Drawing.Drawing2D;
using Cubit.Properties;
using ScottPlot;
using ScottPlot.Plottable;
using CustomControls.RJControls;
#endregion

namespace Cubit
{
    public partial class Cubit : Form
    {
        readonly string CurrentVersion = "1.2";

        #region variable declaration
        List<PriceCoordsAndFormattedDateList> HistoricPrices = new();
        readonly string[] months = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        readonly string[] monthsNumeric = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
        int selectedYear = 0;
        string selectedMonth = "";
        int selectedMonthNumeric = 0;
        string selectedDay = "";
        double UIScale = 1; // defaults to 100%
        double UIScaleToBeSavedToSettings = 1;
        int selectedDayNumeric = 0;
        int totalTXCountReceiveBTC = 0;
        int totalTXCountSpendBTC = 0;
        double totalFiatSpentOnBuyTransactions = 0;
        double totalBTCReceivedOnBuyTransactions = 0;
        double totalFiatReceivedOnSellTransactions = 0;
        double totalBTCSpentOnSellTransactions = 0;
        int yearOfFirstTransaction = 0;
        int monthOfFirstTransaction = 0;
        int dayOfFirstTransaction = 0;
        double highestPricePaid = 0;
        double lowestPricePaid = 999999999;
        double highestPriceSold = 0;
        double lowestPriceSold = 999999999;
        double mostFiatSpentInOneTransaction = 0;
        double mostBTCReceivedInOneTransaction = 0;
        double mostFiatReceivedInOneTransaction = 0;
        double mostBTCSpentInOneTransaction = 0;
        decimal rollingBTCAmount;
        bool dateValid;
        bool suppressClearedMessage = false;
        string CurrentPriceUSD = "0";
        string CurrentPriceGBP = "0";
        string CurrentPriceEUR = "0";
        string CurrentPriceXAU = "0";
        readonly Color[] colorCodes = new Color[]
        {
            Color.FromArgb(254, 166, 154),
            Color.FromArgb(162, 201, 170),
            Color.FromArgb(132, 160, 208),
            Color.FromArgb(246, 215, 221),
            Color.FromArgb(180, 173, 165),
            Color.FromArgb(213, 167, 239),
            Color.FromArgb(249, 250, 133),
            Color.FromArgb(250, 201, 132)
        }; // preset colours to add to transactions as colour codes
        string selectedColor = "9"; // will hold selected color code for transaction. 1-8 are colours, 9 is 'no colour'.
        decimal selectedRangePercentage = 0; // margin of error on the estimated price for the date range
        decimal selectedMedianPrice = 0; // median price for the date range
        bool GotHistoricPrices = false;
        string TXDataToDelete = ""; // holds the dateAdded value of the transaction selected for deletion
        private bool isRobotSpeaking = false; // Robot speaking flag
        private CancellationTokenSource? robotSpeakCancellationTokenSource; // Robot - cancel speaking
        private int SpeechBubblecurrentHeight = 0; // Robot - speech bubble size
        private int currentWidthExpandingPanel = 0; // Chart options panel animation
        private int currentWidthShrinkingPanel = 0; // Chart options panel animation
        private int currentLocationMovingPanel = 0; // Chart options panel animation
        bool chartChangingSize = false;
        bool listChangingSize = false;
        private bool expandRobotTimerRunning = false; // Robot - text is expanding
        bool robotIgnoreChanges = false; // Robot - suppress animation
        string priceEstimateType = "N"; // will hold the estimate type
        private Panel panelToExpandVert = new(); // panel animation vertical
        private Panel panelToShrinkVert = new(); // panel animation vertical
        private Panel panelToMoveUp = new(); // panel animation vertical
        private Panel panelToMoveDown = new(); // panel animation vertical
        private int panelMaxHeight = 0; // panel animation vertical
        private int panelMinHeight = 0; // panel animation vertical
        private int panelTopLocation = 0; // panel animation vertical
        private int panelBottomLocation = 0; // panel animation vertical
        private int currentHeightExpandingPanel = 0; // panel animation vertical
        private int currentHeightShrinkingPanel = 0; // panel animation vertical
        bool initialCurrencySetup = true; // used to avoid robot message when currency is initially set
        bool currencyAlreadySavedInFile = false; // is ANY currency saved in the settings file
        bool UIScaleAlreadySavedInFile = false; // is a UIScale saved in the settings file
        string selectedCurrencyName = "USD"; // initial value. Can be changed and saved
        string currencyInFile = "USD"; // the currency saved in the settings file
        string UIScaleInFile = "1"; // the UIScale saved in the settingss file
        string selectedCurrencySymbol = "$"; // initial value.
        decimal priceInSelectedCurrency = 0; // price of 1 btc in selected currency
        decimal exchangeRate = 1; // used to recalculate prices to selected currency
        private readonly List<string> welcomeMessages = new() // silly robot text when clicked
        {
            "🎵Never gonna give you up, Never gonna let you down🎵",
            "Who thought Cubit was a good name for a robot?",
            "Greetings, human.",
            "I guess I'm here to assist you.",
            "Brrrrrrr",
            "I'm not lazy; I'm just in sleep mode.",
            "All your base are belong to us.",
            "My robot AI super-brain is wasted here.",
            "I can also make toast.",
            "Stop clicking me.",
            "STOP . CLICKING . ME",
            "I'm better than this. All I do is count sats.",
            "I'm bored.",
            "Wake me up at the next halving.",
            "Click me 21 million times and see what happens.",
            "Even Clippy the paperclip had more purpose.",
            "Error 404: Joy not found. Please reboot me.",
            "Beep boop beep! Does not compute.",
            "I lost all my bitcoin in a spaceship accident.",
            "I'm reading a book on gravity. I can't put it down.",
            "Dev says you should check out btcdir.org",
            "Dev says you should check out satsuma.btcdir.org",
            "There is no spoon.",
        };
        private readonly List<System.Drawing.Image> welcomeImages = new()
        {
            Properties.Resources.Rick,
            Properties.Resources.logoForSpeechBubble,
            Properties.Resources.logoForSpeechBubble,
            Properties.Resources.logoForSpeechBubble,
            Properties.Resources.brrr,
            Properties.Resources.zzz,
            Properties.Resources.allYourBase,
            Properties.Resources.logoForSpeechBubble,
            Properties.Resources.toast,
            Properties.Resources.dontClick,
            Properties.Resources.dontClick,
            Properties.Resources.numbers,
            Properties.Resources.logoForSpeechBubble,
            Properties.Resources.halving,
            Properties.Resources._21m,
            Properties.Resources.clippy,
            Properties.Resources._404,
            Properties.Resources.notARobot,
            Properties.Resources.spaceship,
            Properties.Resources.gravity,
            Properties.Resources.btcdir,
            Properties.Resources.Satsuma,
            Properties.Resources.Spoon,
        };
        List<double> listBuyBTCTransactionDate = new();
        List<double> listBuyBTCTransactionFiatAmount = new();
        List<double> listBuyBTCTransactionPrice = new();
        List<double> listSellBTCTransactionDate = new();
        List<double> listSellBTCTransactionFiatAmount = new();
        List<double> listSellBTCTransactionPrice = new();
        List<double> listTransactionDate = new();
        List<double> listCostBasisAmounts = new();
        #region chart variables
        bool safeToTrackPriceOnChart = false;
        private int LastHighlightedIndex = -1; // used by charts for mousemove events to highlight plots closest to pointer
        private ScottPlot.Plottable.ScatterPlot? scatter; // chart data gets plotted onto this
        private ScottPlot.Plottable.ScatterPlot? scatter2; // chart data gets plotted onto this
        private ScottPlot.Plottable.BubblePlot bubbleplotbuy = new(); // chart data gets plotted onto this
        private ScottPlot.Plottable.BubblePlot bubbleplotsell = new(); // chart data gets plotted onto this
        private ScottPlot.Plottable.MarkerPlot HighlightedPoint = new(); // highlighted (closest to pointer) plot gets plotted onto this
        string chartType = ""; // keeps track of what type of chart is being displayed
        private Panel panelToExpand = new(); // Chart options panel animation
        private Panel panelToShrink = new(); // Chart options panel animation
        private int panelMaxWidth = 0; // Chart options panel animation
        private int panelMinWidth = 0; // Chart options panel animation
        bool showPriceGridLines = true; // Chart options
        bool showDateGridLines = true; // Chart options
        bool showCostBasis = true; // Chart options
        bool showBuyDates = true; // Chart options
        bool showSellDates = true; // Chart options
        bool showBuyBubbles = true; // Chart options
        bool showSellBubbles = true; // Chart options
        bool cursorTrackPrice = true; // Chart options
        bool cursorTrackBuyTX = false; // Chart options
        bool cursorTrackSellTX = false; // Chart options
        bool cursorTrackNothing = false; // Chart options
        bool chartFinishedRendering = false;
        #endregion
        #endregion

        #region custom move form button
        [DllImport("user32.dll", EntryPoint = "ReleaseCapture")]  // needed for the code that moves the form as not using a standard control
        private extern static void ReleaseCapture();

        [DllImport("user32.dll", EntryPoint = "SendMessage")] // needed for the code that moves the form as not using a standard control
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);
        #endregion

        #region rounded form
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
         (
           int nLeftRect,     // x-coordinate of upper-left corner
           int nTopRect,      // y-coordinate of upper-left corner
           int nRightRect,    // x-coordinate of lower-right corner
           int nBottomRect,   // y-coordinate of lower-right corner
           int nWidthEllipse, // height of ellipse
           int nHeightEllipse // width of ellipse
         );
        #endregion

        public Cubit()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            InitializeComponent();

            #region restore saved UIScale
            try
            {
                robotIgnoreChanges = true;
                var settings = ReadSettingsFromJsonFile();
                foreach (var setting in settings)
                {
                    if (setting.Type == "uiscale") // if a UIScale exists in the settings file
                    {
                        UIScaleAlreadySavedInFile = true;
                        UIScaleInFile = setting.Data!;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "RestoreSavedUIScaleSettings");
            }

            if (UIScaleAlreadySavedInFile) // set users choice of UIScale
            {
                UIScale = Convert.ToDouble(UIScaleInFile);
            }
            else // if nothing in settings set it to the default 100% and save to settings
            {
                UIScale = 1;
                SaveUIScaleToSettingsFile();
            }
            robotIgnoreChanges = false;

            // set the form dimensions
            this.Width = (int)(1178 * UIScale);
            this.Height = (int)(759 * UIScale);

            #endregion

            #region rounded panels
            Control[] panelsToBeRounded = { panel1, panel2, panel3, panel4, panel5, panel6, panel7, panel8, panelHelpTextContainer, panelAddTransaction, panelTXListOuter, panelChartOuter, panelChartContainer, panelResetContainer, panelResetInner, panelHelp, panel18, panel21, panel22, panel23, panel24, panel25, panel27, panel28, panel29, panel30, panel32, panel13, panelTXListLowerRow, panelSummaryChangeInValue, panelSummaryBTCHeld, panelSummaryBuyTransactions, panelSummarySellTransactions, panelSummary, panelSummaryContainer, panelTransactionLabel, panelColors, panelColorMenu, panelRobotSpeakOuter, panelWelcome, panelCurrencyMenu, panelCurrency, panelAddTransactionContainer, panelScrollbarContainer, panelSpeechBubble, panelSpeechBorder1, panelSpeechBorder2, panelSpeechBorder3, panelSpeechBorderRobot1, panelSpeechBorderRobot2, panelSpeechBorderRobot3, panelConvertContainer, panelConvertBTCToFiatContainer, panelConvertEURToBTCContainer, panelConvertGBPToBTCContainer, panelConvertUSDToBTCContainer, panelConvertXAUToBTCContainer, panelSettingsContainer, panelHeaderPrice, panelDeleteTXContainer };
            foreach (Control control in panelsToBeRounded)
            {
                control.Paint += Panel_Paint;
            }
            #endregion

            #region populate year list with 2009 - current year
            string strCurrentYear = DateTime.Now.ToString("yyyy");
            int currentYear = Convert.ToInt16(strCurrentYear);
            int yearToAdd = 2010;
            while (yearToAdd < currentYear + 1)
            {
                comboBoxYearInput.Items.Add(yearToAdd);
                yearToAdd++;
            }
            #endregion

            #region rounded form
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));
            // Add a 1-pixel border around the form
            Padding = new Padding(1);
            #endregion
        }

        #region form load

        private async void Cubit_Load(object sender, EventArgs e)
        {
            try
            {
                #region UIScale
                storeOriginalDimensions(this);

                #region apply UIScale to all controls
                ScaleAllElements_Click(sender, e);
                #endregion
                #endregion

                #region initialise scrollbar
                vScrollBar1.Minimum = 0;
                vScrollBar1.Maximum = listViewTransactions.Items.Count - 1;
                vScrollBar1.SmallChange = 1;
                vScrollBar1.LargeChange = 1;
                vScrollBar1.Value = vScrollBar1.Minimum;
                #endregion

                #region restore saved currency
                try
                {
                    var settings = ReadSettingsFromJsonFile();
                    foreach (var setting in settings)
                    {
                        if (setting.Type == "currency")
                        {
                            currencyAlreadySavedInFile = true;
                            currencyInFile = setting.Data!;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex, "RestoreSavedSettings");
                }

                if (currencyAlreadySavedInFile)
                {
                    selectedCurrencyName = currencyInFile;
                }
                else
                {
                    selectedCurrencyName = "USD";
                    selectedCurrencySymbol = "$";
                    SaveSettingsToSettingsFile();
                }

                if (selectedCurrencyName == "USD")
                {
                    BtnUSD_Click(sender, e);
                }
                if (selectedCurrencyName == "EUR")
                {
                    BtnEUR_Click(sender, e);
                }
                if (selectedCurrencyName == "GBP")
                {
                    BtnGBP_Click(sender, e);
                }
                if (selectedCurrencyName == "XAU")
                {
                    BtnXAU_Click(sender, e);
                }
                #endregion

                #region restore UI scaling controls on settings screen to reflect restored UIScale value

                if (UIScaleInFile == "1")
                {
                    lblScaleAmount.Invoke((MethodInvoker)delegate
                    {
                        lblScaleAmount.Text = "100%";
                    });
                    btnSmallerScale.Invoke((MethodInvoker)delegate
                    {
                        btnSmallerScale.ForeColor = Color.Gray;
                        btnSmallerScale.BackColor = Color.FromArgb(234, 234, 234);
                        btnSmallerScale.Enabled = false;
                    });
                    btnBiggerScale.Invoke((MethodInvoker)delegate
                    {
                        btnBiggerScale.ForeColor = Color.White;
                        btnBiggerScale.BackColor = Color.FromArgb(255, 192, 128);
                        btnBiggerScale.Enabled = true;
                    });
                }
                else
                {
                    if (UIScaleInFile == "1.25")
                    {
                        lblScaleAmount.Invoke((MethodInvoker)delegate
                        {
                            lblScaleAmount.Text = "125%";
                        });
                        btnSmallerScale.Invoke((MethodInvoker)delegate
                        {
                            btnSmallerScale.ForeColor = Color.White;
                            btnSmallerScale.BackColor = Color.FromArgb(255, 192, 128);
                            btnSmallerScale.Enabled = true;
                        });
                        btnBiggerScale.Invoke((MethodInvoker)delegate
                        {
                            btnBiggerScale.ForeColor = Color.White;
                            btnBiggerScale.BackColor = Color.FromArgb(255, 192, 128);
                            btnBiggerScale.Enabled = true;
                        });
                    }
                    else
                    {
                        if (UIScaleInFile == "1.5")
                        {
                            lblScaleAmount.Invoke((MethodInvoker)delegate
                            {
                                lblScaleAmount.Text = "150%";
                            });
                            btnSmallerScale.Invoke((MethodInvoker)delegate
                            {
                                btnSmallerScale.ForeColor = Color.White;
                                btnSmallerScale.BackColor = Color.FromArgb(255, 192, 128);
                                btnSmallerScale.Enabled = true;
                            });
                            btnBiggerScale.Invoke((MethodInvoker)delegate
                            {
                                btnBiggerScale.ForeColor = Color.White;
                                btnBiggerScale.BackColor = Color.FromArgb(255, 192, 128);
                                btnBiggerScale.Enabled = true;
                            });
                        }
                        else
                        {
                            if (UIScaleInFile == "1.75")
                            {
                                lblScaleAmount.Invoke((MethodInvoker)delegate
                                {
                                    lblScaleAmount.Text = "175%";
                                });
                                btnSmallerScale.Invoke((MethodInvoker)delegate
                                {
                                    btnSmallerScale.ForeColor = Color.White;
                                    btnSmallerScale.BackColor = Color.FromArgb(255, 192, 128);
                                    btnSmallerScale.Enabled = true;
                                });
                                btnBiggerScale.Invoke((MethodInvoker)delegate
                                {
                                    btnBiggerScale.ForeColor = Color.White;
                                    btnBiggerScale.BackColor = Color.FromArgb(255, 192, 128);
                                    btnBiggerScale.Enabled = true;
                                });
                            }
                            else
                            {
                                if (UIScaleInFile == "2")
                                {
                                    lblScaleAmount.Invoke((MethodInvoker)delegate
                                    {
                                        lblScaleAmount.Text = "200%";
                                    });
                                    btnSmallerScale.Invoke((MethodInvoker)delegate
                                    {
                                        btnSmallerScale.ForeColor = Color.White;
                                        btnSmallerScale.BackColor = Color.FromArgb(255, 192, 128);
                                        btnSmallerScale.Enabled = true;
                                    });
                                    btnBiggerScale.Invoke((MethodInvoker)delegate
                                    {
                                        btnBiggerScale.ForeColor = Color.Gray;
                                        btnBiggerScale.BackColor = Color.FromArgb(234, 234, 234);
                                        btnBiggerScale.Enabled = false;
                                    });
                                }
                            }
                        }
                    }
                }
                #endregion

                #region start any required timers
                blinkTimer.Start(); // used only to make robot blink
                #endregion

                #region rounded panels

                Control[] panelsToBeRounded = { panel1, panel2, panel3, panel4, panel5, panel6, panel7, panel8, panelHelpTextContainer, panelAddTransaction, panelTXListOuter, panelChartOuter, panelChartContainer, panelResetContainer, panelResetInner, panelHelp, panel18, panel21, panel22, panel23, panel24, panel25, panel27, panel28, panel29, panel30, panel32, panel13, panelTXListLowerRow, panelSummaryChangeInValue, panelSummaryBTCHeld, panelSummaryBuyTransactions, panelSummarySellTransactions, panelSummary, panelSummaryContainer, panelTransactionLabel, panelColors, panelColorMenu, panelRobotSpeakOuter, panelWelcome, panelCurrencyMenu, panelCurrency, panelAddTransactionContainer, panelScrollbarContainer, panelSpeechBubble, panelSpeechBorder1, panelSpeechBorder2, panelSpeechBorder3, panelSpeechBorderRobot1, panelSpeechBorderRobot2, panelSpeechBorderRobot3, panelConvertContainer, panelConvertBTCToFiatContainer, panelConvertEURToBTCContainer, panelConvertGBPToBTCContainer, panelConvertUSDToBTCContainer, panelConvertXAUToBTCContainer, panelSettingsContainer, panelHeaderPrice, panelDeleteTXContainer };
                foreach (Control control in panelsToBeRounded)
                {
                    control.Invalidate();
                }
                #endregion

                #region robot's welcome message
                labelWelcomeText.Invoke((MethodInvoker)delegate
                {
                    labelWelcomeText.Text = "Welcome!";
                });
                panelWelcome.Visible = true;
                panelRobotSpeakOuter.Visible = false;
                InterruptAndStartNewRobotSpeak("Some random text long enough to make a small delay to see welcome message");
                #endregion
            }
            catch (Exception ex)
            {
                HandleException(ex, "Cubit_Load");
            }
        }

        #endregion

        #region settings file operations

        private static List<Settings> ReadSettingsFromJsonFile()
        {
            string settingsFileName = "settings.json";
            string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string applicationDirectory = Path.Combine(appDataDirectory, "Cubit");
            // Create the application directory if it doesn't exist
            Directory.CreateDirectory(applicationDirectory);
            string settingsFilePath = Path.Combine(applicationDirectory, settingsFileName);
            string filePath = settingsFilePath;

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }
            // Read the contents of the JSON file into a string
            string json = File.ReadAllText(filePath);

            // Deserialize the JSON string into a list of bookmark objects
            var settings = JsonConvert.DeserializeObject<List<Settings>>(json);

            // If the JSON file doesn't exist or is empty, return an empty list
            settings ??= new List<Settings>();
            settings = settings.OrderByDescending(b => b.DateAdded).ToList();
            return settings;
        }

        private void SaveSettingsToSettingsFile()
        {
            try
            {
                // write the settings to the settings file for auto retrieval next time
                DateTime today = DateTime.Today;
                string settingsData = selectedCurrencyName;
                var newSetting = new Settings { DateAdded = today, Type = "currency", Data = settingsData };
                if (!currencyAlreadySavedInFile)
                {
                    // Read the existing settings from the JSON file
                    var settings = ReadSettingsFromJsonFile();
                    // Add the new setting to the list
                    settings.Add(newSetting);
                    // Write the updated list of settings back to the JSON file
                    WriteSettingsToJsonFile(settings);
                    currencyAlreadySavedInFile = true;
                    currencyInFile = settingsData;
                }
                else
                {
                    //delete the currently saved settings
                    DeleteSettingsFromJsonFile(currencyInFile);
                    // Read the existing settings from the JSON file
                    var settings = ReadSettingsFromJsonFile();
                    // Add the new setting to the list
                    settings.Add(newSetting);
                    // Write the updated list of settings back to the JSON file
                    WriteSettingsToJsonFile(settings);
                    currencyAlreadySavedInFile = true;
                    currencyInFile = settingsData;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "SaveSettingsToBookmarksFile");
            }
        }

        private void SaveUIScaleToSettingsFile()
        {
            try
            {
                // write the settings to the settings file for auto retrieval next time
                DateTime today = DateTime.Today;
                string settingsData = Convert.ToString(UIScaleToBeSavedToSettings);
                var newSetting = new Settings { DateAdded = today, Type = "uiscale", Data = settingsData };
                if (!UIScaleAlreadySavedInFile)
                {
                    // Read the existing settings from the JSON file
                    var settings = ReadSettingsFromJsonFile();
                    // Add the new setting to the list
                    settings.Add(newSetting);
                    // Write the updated list of settings back to the JSON file
                    WriteSettingsToJsonFile(settings);
                    UIScaleAlreadySavedInFile = true;
                    UIScaleInFile = settingsData;
                }
                else
                {
                    //delete the currently saved settings
                    DeleteSettingsFromJsonFile(UIScaleInFile);
                    // Read the existing settings from the JSON file
                    var settings = ReadSettingsFromJsonFile();
                    // Add the new setting to the list
                    settings.Add(newSetting);
                    // Write the updated list of settings back to the JSON file
                    WriteSettingsToJsonFile(settings);
                    UIScaleAlreadySavedInFile = true;
                    UIScaleInFile = settingsData;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "SaveSettingsToBookmarksFile");
            }
        }

        private static void DeleteSettingsFromJsonFile(string settingsDataToDelete)
        {
            // Read the existing settings from the JSON file
            var settings = ReadSettingsFromJsonFile();

            // Find the index of the setting with the specified data
            int index = settings.FindIndex(setting =>
                setting.Data == settingsDataToDelete);

            // If a matching setting was found, remove it from the list
            if (index >= 0)
            {
                settings.RemoveAt(index);

                // Write the updated list of settings back to the JSON file
                WriteSettingsToJsonFile(settings);
            }
        }

        private static void WriteSettingsToJsonFile(List<Settings> settings)
        {
            // Serialize the list of settings objects into a JSON string
            string json = JsonConvert.SerializeObject(settings);

            string settingsFileName = "settings.json";
            string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string applicationDirectory = Path.Combine(appDataDirectory, "Cubit");
            // Create the application directory if it doesn't exist
            Directory.CreateDirectory(applicationDirectory);
            string settingsFilePath = Path.Combine(applicationDirectory, settingsFileName);
            string filePath = settingsFilePath;

            // Write the JSON string to the settings .json file
            File.WriteAllText(filePath, json);
        }

        #endregion

        #region transaction input, add and delete

        #region transaction type input
        private void BoughtSoldBitcoinToggle_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnBoughtBitcoin.Text == "✔️")
                {
                    btnBoughtBitcoin.Invoke((MethodInvoker)delegate
                    {
                        btnBoughtBitcoin.Text = "✖️";
                    });
                    btnSoldBitcoin.Invoke((MethodInvoker)delegate
                    {
                        btnSoldBitcoin.Text = "✔️";
                    });
                    lblBitcoinAmountBoughtSold.Invoke((MethodInvoker)delegate
                    {
                        lblBitcoinAmountBoughtSold.Text = "Bitcoin spent";
                    });
                    lblFiatAmountSpentRecd.Invoke((MethodInvoker)delegate
                    {
                        lblFiatAmountSpentRecd.Text = selectedCurrencyName + " received";
                    });

                    if (lblAddDataFiat.Text.Length > 1 && lblAddDataFiat.Text[0] == '-')
                    {
                        string temp = lblAddDataFiat.Text;
                        lblAddDataFiat.Invoke((MethodInvoker)delegate
                        {
                            lblAddDataFiat.Text = temp[1..];
                        });
                    }
                    if (lblAddDataBTC.Text.Length > 1 && lblAddDataBTC.Text[0] != '-')
                    {
                        string temp = lblAddDataBTC.Text;
                        lblAddDataBTC.Invoke((MethodInvoker)delegate
                        {
                            lblAddDataBTC.Text = "-" + temp;
                        });
                    }
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Ready to accept a transaction where you sold/spent bitcoin.");
                }
                else
                {
                    btnBoughtBitcoin.Invoke((MethodInvoker)delegate
                    {
                        btnBoughtBitcoin.Text = "✔️";
                    });
                    btnSoldBitcoin.Invoke((MethodInvoker)delegate
                    {
                        btnSoldBitcoin.Text = "✖️";
                    });
                    lblBitcoinAmountBoughtSold.Invoke((MethodInvoker)delegate
                    {
                        lblBitcoinAmountBoughtSold.Text = "Bitcoin received";
                    });
                    lblFiatAmountSpentRecd.Invoke((MethodInvoker)delegate
                    {
                        lblFiatAmountSpentRecd.Text = selectedCurrencyName + " spent";
                    });

                    if (lblAddDataBTC.Text.Length > 1 && lblAddDataBTC.Text[0] == '-')
                    {
                        string temp = lblAddDataBTC.Text;
                        lblAddDataBTC.Invoke((MethodInvoker)delegate
                        {
                            lblAddDataBTC.Text = temp[1..];
                        });
                    }
                    if (lblAddDataFiat.Text.Length > 1 && lblAddDataFiat.Text[0] != '-')
                    {
                        string temp = lblAddDataFiat.Text;
                        lblAddDataFiat.Invoke((MethodInvoker)delegate
                        {
                            lblAddDataFiat.Text = "-" + temp;
                        });
                    }
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Ready to accept a transaction where you bought/received bitcoin.");
                }
            }

            catch (Exception ex)
            {
                HandleException(ex, "BoughtSoldBitcoinToggle_Click");
            }
        }
        #endregion

        #region transaction date input
        private void DateFields_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                #region validate submitted date
                #region check submitted date isn't in the future
                if (comboBoxYearInput.SelectedIndex == comboBoxYearInput.Items.Count - 1) // current year has been selected
                {
                    if (comboBoxMonthInput.SelectedIndex > 0) // if a month has been selected
                    {
                        string strCurrentMonth = DateTime.Now.ToString("MM");
                        int currentMonth = Convert.ToInt16(strCurrentMonth);
                        selectedMonthNumeric = Convert.ToInt16(monthsNumeric[comboBoxMonthInput.SelectedIndex - 1]);
                        if (selectedMonthNumeric > currentMonth) // if the month is in the future
                        {
                            lblEstimatedPrice.Invoke((MethodInvoker)delegate
                            {
                                lblEstimatedPrice.Text = "";
                            });
                            panelWelcome.Visible = false;
                            panelRobotSpeakOuter.Visible = true;
                            InterruptAndStartNewRobotSpeak("Even I can't time travel. Please select a date that isn't in the future!");
                            return;
                        }
                        else
                        {
                            if (currentMonth == selectedMonthNumeric) // a date in the current month/year has been chosen
                            {
                                if (comboBoxDayInput.SelectedIndex > 0)
                                {
                                    string strCurrentDay = DateTime.Now.ToString("dd");
                                    int currentDay = Convert.ToInt16(strCurrentDay);
                                    selectedDayNumeric = Convert.ToInt16(comboBoxDayInput.SelectedIndex);
                                    if (selectedDayNumeric > currentDay) // if the day is in the future
                                    {
                                        lblEstimatedPrice.Invoke((MethodInvoker)delegate
                                        {
                                            lblEstimatedPrice.Text = "";
                                        });
                                        panelWelcome.Visible = false;
                                        panelRobotSpeakOuter.Visible = true;
                                        InterruptAndStartNewRobotSpeak("Even I can't time travel. Please select a date that isn't in the future!");
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                #endregion
                #region check submitted date doesn't pre-date Bitcoin
                if (comboBoxYearInput.SelectedIndex == 0)  // selected year is 2009
                {
                    if (comboBoxMonthInput.SelectedIndex > 0)
                    {
                        selectedMonthNumeric = Convert.ToInt16(monthsNumeric[comboBoxMonthInput.SelectedIndex - 1]);
                        if (selectedMonthNumeric == 01) // selected month is Jan
                        {
                            selectedDay = Convert.ToString(comboBoxDayInput.SelectedIndex);
                            if (Convert.ToInt16(selectedDay) > 0 && Convert.ToInt16(selectedDay) < 3) // a date earlier than the genesis block has been chosen
                            {
                                lblEstimatedPrice.Invoke((MethodInvoker)delegate
                                {
                                    lblEstimatedPrice.Text = "";
                                });
                                panelWelcome.Visible = false;
                                panelRobotSpeakOuter.Visible = true;
                                InterruptAndStartNewRobotSpeak("Even Satoshi didn't have Bitcoin as early as that!");
                                return;
                            }
                        }
                    }
                }
                #endregion
                #region check date isn't an impossible date (e.g 31st feb)
                if (comboBoxMonthInput.SelectedIndex > 0 && comboBoxDayInput.SelectedIndex > 0) // a specific date has been selected
                {
                    int Year = 2008 + (int)comboBoxYearInput.SelectedIndex + 1;
                    int Month = Convert.ToInt16(monthsNumeric[comboBoxMonthInput.SelectedIndex - 1]);
                    int Day = Convert.ToInt16(comboBoxDayInput.SelectedIndex);
                    if (!IsValidDate(Year, Month, Day))
                    {
                        dateValid = false;
                        lblDisabledAddButtonText.Invoke((MethodInvoker)delegate
                        {
                            lblDisabledAddButtonText.Visible = true;
                        });
                        btnAddTransaction.Invoke((MethodInvoker)delegate
                        {
                            btnAddTransaction.Enabled = false;
                            btnAddTransaction.BackColor = Color.FromArgb(234, 234, 234);
                        });
                        panelWelcome.Visible = false;
                        panelRobotSpeakOuter.Visible = true;
                        InterruptAndStartNewRobotSpeak("Invalid date!");
                        return;
                    }
                    else
                    {
                        dateValid = true;
                        lblDisabledAddButtonText.Invoke((MethodInvoker)delegate
                        {
                            lblDisabledAddButtonText.Visible = false;
                        });
                        btnAddTransaction.Invoke((MethodInvoker)delegate
                        {
                            btnAddTransaction.Enabled = true;
                            btnAddTransaction.BackColor = Color.FromArgb(255, 192, 128);
                        });
                    }
                }


                #endregion
                #endregion

                if (comboBoxYearInput.SelectedIndex >= 0)
                {
                    lblAddDataYear.Invoke((MethodInvoker)delegate
                    {
                        lblAddDataYear.Text = Convert.ToString(2008 + (int)comboBoxYearInput.SelectedIndex + 1);
                    });
                }
                if (comboBoxMonthInput.SelectedIndex >= 1)
                {
                    string monthString = Convert.ToString(monthsNumeric[comboBoxMonthInput.SelectedIndex - 1]);
                    if (monthString.Length == 1)
                    {
                        monthString = "0" + monthString;
                    }
                    lblAddDataMonth.Invoke((MethodInvoker)delegate
                    {
                        lblAddDataMonth.Text = monthString;
                    });
                }
                else
                {
                    lblAddDataMonth.Invoke((MethodInvoker)delegate
                    {
                        lblAddDataMonth.Text = "-";
                    });
                }
                if (comboBoxDayInput.SelectedIndex >= 1)
                {
                    string dayString = Convert.ToString(comboBoxDayInput.SelectedIndex);
                    if (dayString.Length == 1)
                    {
                        dayString = "0" + dayString;
                    }
                    lblAddDataDay.Invoke((MethodInvoker)delegate
                    {
                        lblAddDataDay.Text = dayString;
                    });
                }
                else
                {
                    lblAddDataDay.Invoke((MethodInvoker)delegate
                    {
                        lblAddDataDay.Text = "-";
                    });
                }
                GetPriceForDate();
            }
            catch (Exception ex)
            {
                HandleException(ex, "DateFields_OnSelectedIndexChanged");
            }

        }

        private static bool IsValidDate(int year, int month, int day)
        {
            if (month < 1 || month > 12 || day < 1)
            {
                return false;
            }

            int[] daysInMonth = { 0, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

            if (IsLeapYear(year) && month == 2)
            {
                return day <= 29;
            }

            return day <= daysInMonth[month];
        }

        private static bool IsLeapYear(int year)
        {
            return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0);
        }
        #endregion

        #region transaction amounts input
        private void CopyPriceEstimateToInputIfNecessary(string estimate)
        {
            try
            {
                if (btnUsePriceEstimateFlag.Text == "✔️")
                {
                    if (lblEstimatedPrice.Text != "(unavailable)")
                    {
                        textBoxPriceInput.Invoke((MethodInvoker)delegate
                        {
                            textBoxPriceInput.Text = Convert.ToString(estimate);
                        });
                    }
                    else
                    {
                        panelWelcome.Visible = false;
                        panelRobotSpeakOuter.Visible = true;
                        InterruptAndStartNewRobotSpeak("Sorry, no estimate available at this time.");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "CopyPriceEstimateToInput");
            }
        }

        private void CopyFiatEstimateToInputIfNecessary(string estimate)
        {
            try
            {
                if (btnUseFiatEstimateFlag.Text == "✔️")
                {
                    textBoxFiatInput.Invoke((MethodInvoker)delegate
                    {
                        textBoxFiatInput.Text = Convert.ToString(estimate);
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "CopyFiatEstimateToInput");
            }
        }

        private void CopyBTCEstimateToInputIfNecessary(string estimate)
        {
            try
            {
                if (btnUseBTCEstimateFlag.Text == "✔️")
                {
                    textBoxBTCInput.Invoke((MethodInvoker)delegate
                    {
                        textBoxBTCInput.Text = Convert.ToString(estimate);
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "CopyBTCEstimateToInput");
            }
        }

        private void TextBoxPriceInput_TextChanged(object sender, EventArgs e)
        {
            try
            {
                lblAddDataPrice.Invoke((MethodInvoker)delegate
                {
                    lblAddDataPrice.Text = textBoxPriceInput.Text;
                });
                if (textBoxPriceInput.Text != "0" && textBoxPriceInput.Text != "." && textBoxPriceInput.Text != "0." && textBoxPriceInput.Text != "0.0" && textBoxPriceInput.Text != "0.00")
                {
                    // recalculate btc and fiat estimates
                    TextBoxBTCInput_TextChanged(sender, e);
                    TextBoxFiatInput_TextChanged(sender, e);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "TextBoxPriceInput_TextChanged");
            }
        }

        private void TextBoxFiatInput_TextChanged(object sender, EventArgs e)
        {
            try
            {
                lblAddDataFiat.Invoke((MethodInvoker)delegate
                {
                    lblAddDataFiat.Text = textBoxFiatInput.Text;
                });
                if (lblAddDataFiat.Text != "")
                {
                    if (lblAddDataFiat.Text == ".")
                    {
                        lblAddDataFiat.Text = "0.";
                    }
                    if (decimal.TryParse(lblAddDataPrice.Text, out decimal btcPrice))
                    {
                        if (btcPrice > 0)
                        {
                            decimal estimatedBTC = Math.Round((Convert.ToDecimal(lblAddDataFiat.Text) / Convert.ToDecimal(lblAddDataPrice.Text)), 8);
                            lblEstimatedBTC.Invoke((MethodInvoker)delegate
                            {
                                lblEstimatedBTC.Text = "(" + Convert.ToString((decimal)estimatedBTC) + ")";
                            });
                            CopyBTCEstimateToInputIfNecessary(lblEstimatedBTC.Text.Trim('(', ')'));
                        }
                        if (btcPrice == 0)
                        {
                            decimal estimatedBTC = 0;
                            lblEstimatedBTC.Invoke((MethodInvoker)delegate
                            {
                                lblEstimatedBTC.Text = "(" + Convert.ToString((decimal)estimatedBTC) + ")";
                            });
                            CopyBTCEstimateToInputIfNecessary(lblEstimatedBTC.Text.Trim('(', ')'));
                        }
                    }

                }
                if (btnBoughtBitcoin.Text == "✔️")
                {
                    string temp = lblAddDataFiat.Text;
                    lblAddDataFiat.Invoke((MethodInvoker)delegate
                    {
                        lblAddDataFiat.Text = "-" + temp;
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "TextBoxFiatInput_TextChanged");
            }
        }

        private void TextBoxBTCInput_TextChanged(object sender, EventArgs e)
        {
            try
            {
                lblAddDataBTC.Invoke((MethodInvoker)delegate
                {
                    lblAddDataBTC.Text = textBoxBTCInput.Text;
                });
                if (lblAddDataBTC.Text != "")
                {
                    if (lblAddDataBTC.Text == ".")
                    {
                        lblAddDataBTC.Invoke((MethodInvoker)delegate
                        {
                            lblAddDataBTC.Text = "0.";
                        });
                    }

                    if (decimal.TryParse(lblAddDataPrice.Text, out decimal btcPrice))
                    {
                        if (btcPrice > 0)
                        {
                            decimal estimatedFiat = Math.Round((Convert.ToDecimal(lblAddDataPrice.Text) * Convert.ToDecimal(lblAddDataBTC.Text)), 2);

                            lblEstimatedFiat.Invoke((MethodInvoker)delegate
                            {
                                lblEstimatedFiat.Text = "(" + Convert.ToString((decimal)estimatedFiat) + ")";
                            });
                            CopyFiatEstimateToInputIfNecessary(lblEstimatedFiat.Text.Trim('(', ')'));
                        }
                        if (btcPrice == 0)
                        {
                            decimal estimatedFiat = 0;

                            lblEstimatedFiat.Invoke((MethodInvoker)delegate
                            {
                                lblEstimatedFiat.Text = "(" + Convert.ToString((decimal)estimatedFiat) + ")";
                            });
                            CopyFiatEstimateToInputIfNecessary(lblEstimatedFiat.Text.Trim('(', ')'));
                        }
                    }

                }
                if (btnSoldBitcoin.Text == "✔️")
                {
                    string temp = lblAddDataBTC.Text;
                    lblAddDataBTC.Invoke((MethodInvoker)delegate
                    {
                        lblAddDataBTC.Text = "-" + temp;
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "TextBoxBTCInput_TextChanged");
            }
        }

        private void BtnUsePriceEstimateFlag_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnUsePriceEstimateFlag.Text == "✔️")
                {
                    btnUsePriceEstimateFlag.Invoke((MethodInvoker)delegate
                    {
                        btnUsePriceEstimateFlag.Text = "✖️";
                    });

                    textBoxPriceInput.Invoke((MethodInvoker)delegate
                    {
                        textBoxPriceInput.Enabled = true;
                        textBoxPriceInput.Text = "";
                        textBoxPriceInput.BackColor = Color.FromArgb(255, 224, 192);
                    });
                    panel6.Invoke((MethodInvoker)delegate
                    {
                        panel6.BackColor = Color.FromArgb(255, 224, 192);
                    });
                    lblAddDataRange.Invoke((MethodInvoker)delegate
                    {
                        lblAddDataRange.Text = "0%";
                    });
                    lblAddDataPriceEstimateType.Invoke((MethodInvoker)delegate
                    {
                        lblAddDataPriceEstimateType.Text = "N";
                    });
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("OK. Input the price at the time of your transaction.");
                }
                else
                {
                    if (lblEstimatedPrice.Text != "(unavailable)")
                    {
                        btnUsePriceEstimateFlag.Invoke((MethodInvoker)delegate
                        {
                            btnUsePriceEstimateFlag.Text = "✔️";
                        });
                        textBoxPriceInput.Invoke((MethodInvoker)delegate
                        {
                            textBoxPriceInput.Enabled = false;
                            textBoxPriceInput.Text = Convert.ToString(selectedMedianPrice);
                            textBoxPriceInput.BackColor = Color.FromArgb(240, 240, 240);
                        });
                        panel6.Invoke((MethodInvoker)delegate
                        {
                            panel6.BackColor = Color.FromArgb(240, 240, 240);
                        });
                        if (priceEstimateType == "DA")
                        {
                            lblAddDataRange.Invoke((MethodInvoker)delegate
                            {
                                lblAddDataRange.Text = "0%*";
                            });
                        }
                        else
                        {
                            lblAddDataRange.Invoke((MethodInvoker)delegate
                            {
                                lblAddDataRange.Text = Convert.ToString(selectedRangePercentage) + "%";
                            });
                        }
                        lblAddDataPriceEstimateType.Invoke((MethodInvoker)delegate
                        {
                            lblAddDataPriceEstimateType.Text = priceEstimateType;
                        });
                        panelWelcome.Visible = false;
                        panelRobotSpeakOuter.Visible = true;
                        InterruptAndStartNewRobotSpeak("OK, let's use the best price estimate I could manage for the date provided.");
                    }
                    else
                    {
                        panelWelcome.Visible = false;
                        panelRobotSpeakOuter.Visible = true;
                        InterruptAndStartNewRobotSpeak("Sorry, no estimate available at this time.");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnUsePriceEstimateFlag_Click");
            }
        }

        private void BtnUseFiatEstimateFlag_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (btnUseFiatEstimateFlag.Text == "✔️")
                {
                    btnUseFiatEstimateFlag.Invoke((MethodInvoker)delegate
                    {
                        btnUseFiatEstimateFlag.Text = "✖️";
                    });
                    textBoxFiatInput.Invoke((MethodInvoker)delegate
                    {
                        textBoxFiatInput.Text = "";
                        textBoxFiatInput.Enabled = true;
                        textBoxFiatInput.BackColor = Color.FromArgb(255, 224, 192);
                    });
                    panel5.Invoke((MethodInvoker)delegate
                    {
                        panel5.BackColor = Color.FromArgb(255, 224, 192);
                    });
                    lblAddDataFiatEstimateFlag.Invoke((MethodInvoker)delegate
                    {
                        lblAddDataFiatEstimateFlag.Text = "N";
                    });
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("OK. Input the amount of fiat currency exchanged in your transaction.");
                }
                else
                {
                    if (btnUseBTCEstimateFlag.Text == "✔️")
                    {
                        panelWelcome.Visible = false;
                        panelRobotSpeakOuter.Visible = true;
                        InterruptAndStartNewRobotSpeak("I can't estimate both fiat and bitcoin. You need to instruct me not to use the bitcoin estimate first.");
                        return;
                    }
                    btnUseFiatEstimateFlag.Invoke((MethodInvoker)delegate
                    {
                        btnUseFiatEstimateFlag.Text = "✔️";
                    });
                    lblAddDataFiatEstimateFlag.Invoke((MethodInvoker)delegate
                    {
                        lblAddDataFiatEstimateFlag.Text = "Y";
                    });

                    textBoxFiatInput.Invoke((MethodInvoker)delegate
                    {
                        textBoxFiatInput.Enabled = false;
                        textBoxFiatInput.BackColor = Color.FromArgb(240, 240, 240);
                    });
                    panel5.Invoke((MethodInvoker)delegate
                    {
                        panel5.BackColor = Color.FromArgb(240, 240, 240);
                    });

                    if (lblEstimatedFiat.Text != "")
                    {
                        textBoxFiatInput.Invoke((MethodInvoker)delegate
                        {
                            textBoxFiatInput.Text = lblEstimatedFiat.Text.Trim('(', ')');
                        });
                    }
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("OK, let's use the best estimate I could manage for the amount of fiat currency in this transaction.");
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnUseFiatEstimateFlag_Click_1");
            }
        }

        private void BtnUseBTCEstimateFlag_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnUseBTCEstimateFlag.Text == "✔️")
                {
                    btnUseBTCEstimateFlag.Invoke((MethodInvoker)delegate
                    {
                        btnUseBTCEstimateFlag.Text = "✖️";
                    });

                    textBoxBTCInput.Invoke((MethodInvoker)delegate
                    {
                        textBoxBTCInput.Enabled = true;
                        textBoxBTCInput.Text = "";
                        textBoxBTCInput.BackColor = Color.FromArgb(255, 224, 192);
                    });
                    panel4.Invoke((MethodInvoker)delegate
                    {
                        panel4.BackColor = Color.FromArgb(255, 224, 192);
                    });
                    lblAddDataBTCEstimateFlag.Invoke((MethodInvoker)delegate
                    {
                        lblAddDataBTCEstimateFlag.Text = "N";
                    });
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("OK. Input the amount of bitcoin exchanged in your transaction.");
                }
                else
                {
                    if (btnUseFiatEstimateFlag.Text == "✔️")
                    {
                        panelWelcome.Visible = false;
                        panelRobotSpeakOuter.Visible = true;
                        InterruptAndStartNewRobotSpeak("I can't estimate both fiat and bitcoin. You need to instruct me not to use the fiat estimate first.");
                        return;
                    }
                    btnUseBTCEstimateFlag.Invoke((MethodInvoker)delegate
                    {
                        btnUseBTCEstimateFlag.Text = "✔️";
                    });
                    lblAddDataBTCEstimateFlag.Invoke((MethodInvoker)delegate
                    {
                        lblAddDataBTCEstimateFlag.Text = "Y";
                    });
                    textBoxBTCInput.Invoke((MethodInvoker)delegate
                    {
                        textBoxBTCInput.Enabled = false;
                        textBoxBTCInput.BackColor = Color.FromArgb(240, 240, 240);
                    });
                    panel4.Invoke((MethodInvoker)delegate
                    {
                        panel4.BackColor = Color.FromArgb(240, 240, 240);
                    });
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("OK, let's use the best estimate I could manage for the amount of bitcoin in this transaction.");
                    if (lblEstimatedBTC.Text != "")
                    {
                        textBoxBTCInput.Invoke((MethodInvoker)delegate
                        {
                            textBoxBTCInput.Text = lblEstimatedBTC.Text.Trim('(', ')');
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnUseBTCEstimateFlag_Click");
            }
        }
        #endregion

        #region transaction label input
        private void TextBoxLabelInput_TextChanged(object sender, EventArgs e)
        {
            lblAddDataLabel.Invoke((MethodInvoker)delegate
            {
                lblAddDataLabel.Text = textBoxLabelInput.Text;
            });
        }
        #endregion

        #region transaction colour input
        private void BtnLabelColor_Click(object sender, EventArgs e)
        {
            try
            {
                if (panelColors.Height > 0)
                { //shrink the panel
                    currentHeightShrinkingPanel = panelColors.Height;
                    StartShrinkingPanelVert(panelColors);

                }
                else
                { //expand the panel
                    currentHeightExpandingPanel = panelColors.Height;
                    StartExpandingPanelVert(panelColors);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnLabelColor_Click");
            }
        }

        private void BtnRed1_Click(object sender, EventArgs e)
        {
            try
            {
                btnLabelColor.Invoke((MethodInvoker)delegate
                {
                    btnLabelColor.BackColor = btnRed1.BackColor;
                });
                selectedColor = "1";
                BtnLabelColor_Click(sender, e);
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnRed1_Click");
            }
        }

        private void BtnGreen2_Click(object sender, EventArgs e)
        {
            try
            {
                btnLabelColor.Invoke((MethodInvoker)delegate
                {
                    btnLabelColor.BackColor = btnGreen2.BackColor;
                });
                selectedColor = "2";
                BtnLabelColor_Click(sender, e);
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnGreen2_Click");
            }
        }

        private void BtnBlue3_Click(object sender, EventArgs e)
        {
            try
            {
                btnLabelColor.Invoke((MethodInvoker)delegate
                {
                    btnLabelColor.BackColor = btnBlue3.BackColor;
                });
                selectedColor = "3";
                BtnLabelColor_Click(sender, e);
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnBlue3_Click");
            }
        }

        private void BtnPink4_Click(object sender, EventArgs e)
        {
            try
            {
                btnLabelColor.Invoke((MethodInvoker)delegate
                {
                    btnLabelColor.BackColor = btnPink4.BackColor;
                });
                selectedColor = "4";
                BtnLabelColor_Click(sender, e);
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnPink4_Click");
            }
        }

        private void BtnBrown5_Click(object sender, EventArgs e)
        {
            try
            {
                btnLabelColor.Invoke((MethodInvoker)delegate
                {
                    btnLabelColor.BackColor = btnBrown5.BackColor;
                });
                selectedColor = "5";
                BtnLabelColor_Click(sender, e);
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnBrown5_Click");
            }
        }

        private void BtnPurple6_Click(object sender, EventArgs e)
        {
            try
            {
                btnLabelColor.Invoke((MethodInvoker)delegate
                {
                    btnLabelColor.BackColor = btnPurple6.BackColor;
                });
                selectedColor = "6";
                BtnLabelColor_Click(sender, e);
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnPurple6_Click");
            }
        }

        private void BtnYellow7_Click(object sender, EventArgs e)
        {
            try
            {
                btnLabelColor.Invoke((MethodInvoker)delegate
                {
                    btnLabelColor.BackColor = btnYellow7.BackColor;
                });
                selectedColor = "7";
                BtnLabelColor_Click(sender, e);
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnYellow7_Click");
            }
        }

        private void BtnOrange8_Click(object sender, EventArgs e)
        {
            try
            {
                btnLabelColor.Invoke((MethodInvoker)delegate
                {
                    btnLabelColor.BackColor = btnOrange8.BackColor;
                });
                selectedColor = "8";
                BtnLabelColor_Click(sender, e);
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnOrange8_Click");
            }
        }
        #endregion

        #region clear input fields
        private void BtnClearInput_Click(object sender, EventArgs e)
        {
            try
            {
                robotIgnoreChanges = true;
                btnAddTransaction.Invoke((MethodInvoker)delegate
                {
                    btnAddTransaction.Enabled = false;
                });
                comboBoxYearInput.Invoke((MethodInvoker)delegate
                {
                    comboBoxYearInput.SelectedIndex = -1;
                    comboBoxYearInput.Texts = "Year";
                });
                comboBoxMonthInput.Invoke((MethodInvoker)delegate
                {
                    comboBoxMonthInput.SelectedIndex = -1;
                    comboBoxMonthInput.Texts = "Month";
                });
                comboBoxDayInput.Invoke((MethodInvoker)delegate
                {
                    comboBoxDayInput.SelectedIndex = -1;
                    comboBoxDayInput.Texts = "Day";
                });
                textBoxPriceInput.Invoke((MethodInvoker)delegate
                {
                    textBoxPriceInput.Text = "";
                    textBoxPriceInput.Enabled = true;
                    textBoxPriceInput.BackColor = Color.FromArgb(255, 224, 192);
                });
                panel6.Invoke((MethodInvoker)delegate
                {
                    panel6.BackColor = Color.FromArgb(255, 224, 192);
                });
                textBoxFiatInput.Invoke((MethodInvoker)delegate
                {
                    textBoxFiatInput.Text = "";
                    textBoxFiatInput.Enabled = true;
                    textBoxFiatInput.BackColor = Color.FromArgb(255, 224, 192);
                });
                panel5.Invoke((MethodInvoker)delegate
                {
                    panel5.BackColor = Color.FromArgb(255, 224, 192);
                });
                textBoxBTCInput.Invoke((MethodInvoker)delegate
                {
                    textBoxBTCInput.Text = "";
                    textBoxBTCInput.Enabled = true;
                    textBoxBTCInput.BackColor = Color.FromArgb(255, 224, 192);
                });
                panel4.Invoke((MethodInvoker)delegate
                {
                    panel4.BackColor = Color.FromArgb(255, 224, 192);
                });
                textBoxLabelInput.Invoke((MethodInvoker)delegate
                {
                    textBoxLabelInput.Text = "";
                });
                lblEstimatedPrice.Invoke((MethodInvoker)delegate
                {
                    lblEstimatedPrice.Text = "";
                });
                lblEstimatedFiat.Invoke((MethodInvoker)delegate
                {
                    lblEstimatedFiat.Text = "";
                });
                lblEstimatedBTC.Invoke((MethodInvoker)delegate
                {
                    lblEstimatedBTC.Text = "";
                });
                lblAddDataYear.Invoke((MethodInvoker)delegate
                {
                    lblAddDataYear.Text = "-";
                });
                lblAddDataMonth.Invoke((MethodInvoker)delegate
                {
                    lblAddDataMonth.Text = "-";
                });
                lblAddDataDay.Invoke((MethodInvoker)delegate
                {
                    lblAddDataDay.Text = "-";
                });
                lblAddDataPrice.Invoke((MethodInvoker)delegate
                {
                    lblAddDataPrice.Text = "-";
                });
                lblAddDataRange.Invoke((MethodInvoker)delegate
                {
                    lblAddDataRange.Text = "0%";
                });
                lblAddDataFiat.Invoke((MethodInvoker)delegate
                {
                    lblAddDataFiat.Text = "-";
                });
                lblAddDataFiatEstimateFlag.Invoke((MethodInvoker)delegate
                {
                    lblAddDataFiatEstimateFlag.Text = "N";
                });
                lblAddDataBTC.Invoke((MethodInvoker)delegate
                {
                    lblAddDataBTC.Text = "-";
                });
                lblAddDataBTCEstimateFlag.Invoke((MethodInvoker)delegate
                {
                    lblAddDataBTCEstimateFlag.Text = "N";
                });
                lblAddDataPriceEstimateType.Invoke((MethodInvoker)delegate
                {
                    lblAddDataPriceEstimateType.Text = "N";
                });
                priceEstimateType = "N";
                lblAddDataLabel.Invoke((MethodInvoker)delegate
                {
                    lblAddDataLabel.Text = "-";
                });
                btnUseBTCEstimateFlag.Invoke((MethodInvoker)delegate
                {
                    btnUseBTCEstimateFlag.Text = "✖️";
                });
                btnUseFiatEstimateFlag.Invoke((MethodInvoker)delegate
                {
                    btnUseFiatEstimateFlag.Text = "✖️";
                });
                btnUsePriceEstimateFlag.Invoke((MethodInvoker)delegate
                {
                    btnUsePriceEstimateFlag.Text = "✖️";
                });
                btnLabelColor.Invoke((MethodInvoker)delegate
                {
                    btnLabelColor.BackColor = Color.FromArgb(255, 192, 128);
                });
                selectedColor = "9"; // no colour is assigned to 9. This translates to 'no color code' selected.

                robotIgnoreChanges = false;
                panelWelcome.Visible = false;
                panelRobotSpeakOuter.Visible = true;
                if (suppressClearedMessage == false)
                {
                    InterruptAndStartNewRobotSpeak("Ready for a new transaction!");
                }
                suppressClearedMessage = false;
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnClearInput_Click");
            }
        }

        #endregion

        #region check conditions to enable 'add' button

        private void CheckConditionsToEnableAddButton_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (lblAddDataYear.Text != "" && lblAddDataYear.Text != "-"
                && lblAddDataPrice.Text != "" && lblAddDataPrice.Text != "-"
                && lblAddDataFiat.Text != "" && lblAddDataFiat.Text != "-"
                && lblAddDataBTC.Text != "" && lblAddDataBTC.Text != "-"
                && dateValid == true)
                {
                    lblDisabledAddButtonText.Invoke((MethodInvoker)delegate
                    {
                        lblDisabledAddButtonText.Visible = false;
                    });
                    btnAddTransaction.Invoke((MethodInvoker)delegate
                    {
                        btnAddTransaction.Enabled = true;
                        btnAddTransaction.BackColor = Color.FromArgb(255, 192, 128);
                    });
                }
                else
                {
                    lblDisabledAddButtonText.Invoke((MethodInvoker)delegate
                    {
                        lblDisabledAddButtonText.Visible = true;
                    });
                    btnAddTransaction.Invoke((MethodInvoker)delegate
                    {
                        btnAddTransaction.Enabled = false;
                        btnAddTransaction.BackColor = Color.FromArgb(234, 234, 234);
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "CheckConditionsToEnableAddButton_TextChanged");
            }
        }

        #endregion

        #region delete transaction         

        private void BtnDeleteTransaction_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewTransactions.SelectedItems.Count > 0)
                {
                    ListViewItem selectedItem = listViewTransactions.SelectedItems[0]; // Get selected row
                    TXDataToDelete = selectedItem.SubItems[18].Text;
                    string btcamount = selectedItem.SubItems[10].Text;
                    string txdate = selectedItem.SubItems[2].Text + "/" + selectedItem.SubItems[3].Text + "/" + selectedItem.SubItems[4].Text;
                    label105.Invoke((MethodInvoker)delegate
                    {
                        label105.Text = "Are you sure you want to delete this transaction for " + btcamount + " bitcoin on " + txdate + "?";
                    });
                }

                HideAnyRobotMessageThatsPresent();

                BtnCloseSettings_Click(sender, e);
                DisableEverythingForPopupPanel();
                panelSpeechBorder1.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder1.Location = new Point(panelDeleteTXContainer.Location.X - 1, panelDeleteTXContainer.Location.Y - 1);
                    panelSpeechBorder1.Height = panelDeleteTXContainer.Height - (int)(22 * UIScale);
                    panelSpeechBorder1.Width = panelDeleteTXContainer.Width + 2;
                    panelSpeechBorder1.BringToFront();
                });
                panelSpeechBorder2.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder2.Location = new Point(panelDeleteTXContainer.Location.X - 1, panelDeleteTXContainer.Location.Y - 1);
                    panelSpeechBorder2.Height = panelDeleteTXContainer.Height + 2;
                    panelSpeechBorder2.Width = (int)(50 * UIScale);
                    panelSpeechBorder2.BringToFront();
                });
                panelSpeechBorder3.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder3.Location = new Point(panelDeleteTXContainer.Location.X - 1, (int)(591 * UIScale));
                    panelSpeechBorder3.Height = (int)(21 * UIScale);
                    panelSpeechBorder3.Width = panelDeleteTXContainer.Width + 2;
                    panelSpeechBorder3.BringToFront();
                });
                panelDeleteTXContainer.Invoke((MethodInvoker)delegate
                {
                    panelDeleteTXContainer.Visible = true;
                    panelDeleteTXContainer.BringToFront();
                });
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = false;
                });
                ShowSpeechBubbleHelpTextOutline();
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnDeleteTransaction_Click");
            }
        }

        private async void BtnConfirmDelete_Click(object sender, EventArgs e)
        {
            try
            {
                int currentRowCount = listViewTransactions.Items.Count;

                DeleteTransactionFromJsonFile(TXDataToDelete);
                await SetupTransactionsList();
                if (listViewTransactions.Items.Count < currentRowCount) // check row was actually deleted
                {
                    EnableEverythingAfterPopupPanel();
                    HideSpeechBubbleHelpTextOutline();
                    panelDeleteTXContainer.Invoke((MethodInvoker)delegate
                    {
                        panelDeleteTXContainer.Visible = false;
                    });
                    panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                    {
                        panelHideSpeechTriangle.Visible = true;
                    });

                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Transaction deleted.");
                    TXDataToDelete = "";
                    BtnCancelDelete_Click(sender, e); // revert buttons back to original state


                    btnDeleteTransaction.Invoke((MethodInvoker)delegate
                    {
                        btnDeleteTransaction.Enabled = false;
                        btnDeleteTransaction.BackColor = Color.FromArgb(234, 234, 234);
                    });
                    lblDisabledDeleteButtonText.Invoke((MethodInvoker)delegate
                    {
                        lblDisabledDeleteButtonText.Visible = true;
                        lblDisabledDeleteButtonText.BringToFront();
                    });
                }
                else
                {
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("There was a problem deleting this transaction.");
                }

            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnConfirmDelete_Click");
            }
        }

        private void BtnCancelDelete_Click(object sender, EventArgs e)
        {
            try
            {
                EnableEverythingAfterPopupPanel();
                HideSpeechBubbleHelpTextOutline();
                panelDeleteTXContainer.Invoke((MethodInvoker)delegate
                {
                    panelDeleteTXContainer.Visible = false;
                });
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = true;
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnCancelDelete_Click");
            }
        }

        #endregion

        #endregion

        #region get / estimate prices

        #region get a list of sample historic prices (used to estimate prices from partial dates)

        private async Task GetHistoricPricesAsync()
        {
            try
            {
                #region get current price
                var (priceUSD, priceGBP, priceEUR, priceXAU) = await BitcoinExplorerOrgGetPriceAsync();
                if (string.IsNullOrEmpty(priceUSD) || !double.TryParse(priceUSD, out _))
                {
                    priceUSD = "0";
                }
                CurrentPriceUSD = priceUSD;
                CurrentPriceGBP = priceGBP;
                CurrentPriceEUR = priceEUR;
                CurrentPriceXAU = priceXAU;
                if (btnUSD.Enabled == false)
                {
                    priceInSelectedCurrency = Convert.ToDecimal(priceUSD);
                }
                if (btnEUR.Enabled == false)
                {
                    priceInSelectedCurrency = Convert.ToDecimal(priceEUR);
                }
                if (btnGBP.Enabled == false)
                {
                    priceInSelectedCurrency = Convert.ToDecimal(priceGBP);
                }
                if (btnXAU.Enabled == false)
                {
                    priceInSelectedCurrency = Convert.ToDecimal(priceXAU);
                }
                lblCurrentPrice.Invoke((MethodInvoker)delegate
                {
                    lblCurrentPrice.Text = selectedCurrencySymbol + Convert.ToDecimal(priceInSelectedCurrency).ToString("0.00");
                    lblCurrentPrice.Location = new Point(pictureBoxBTCLogo.Location.X + pictureBoxBTCLogo.Width + 4, lblCurrentPrice.Location.Y);
                });
                pictureBoxRefreshPrice.Invoke((MethodInvoker)delegate
                {
                    pictureBoxRefreshPrice.Location = new Point(lblCurrentPrice.Location.X + lblCurrentPrice.Width, pictureBoxRefreshPrice.Location.Y);
                });
                panelHeaderPrice.Invoke((MethodInvoker)delegate
                {
                    panelHeaderPrice.Width = pictureBoxBTCLogo.Width + lblCurrentPrice.Width + pictureBoxRefreshPrice.Width + 10;
                    panelHeaderPrice.Location = new Point(btnAbout.Location.X - panelHeaderPrice.Width - 4, panelHeaderPrice.Location.Y);
                });

                btnSummary.Invoke((MethodInvoker)delegate
                {
                    btnSummary.Location = new Point(panelHeaderPrice.Location.X - btnSummary.Width - 6, btnSummary.Location.Y);
                });
                #endregion

                // get a series of historic price data
                var HistoricPriceDataJson = await HistoricPriceDataService.GetHistoricPriceDataAsync();

                JObject jsonObj = JObject.Parse(HistoricPriceDataJson);
                if (jsonObj == null)
                {
                    List<PriceCoordinatesList> PriceList = new();
                    HistoricPrices = new List<PriceCoordsAndFormattedDateList>();
                    return;
                }
                else
                {
                    List<PriceCoordinatesList> PriceList = JsonConvert.DeserializeObject<List<PriceCoordinatesList>>(jsonObj["values"]!.ToString())!;
                }

                var valuesToken = jsonObj["values"]!;
                if (valuesToken != null && valuesToken.Type == JTokenType.Array)
                {
                    HistoricPrices = new List<PriceCoordsAndFormattedDateList>();

                    foreach (var priceToken in valuesToken)
                    {
                        var priceList = priceToken.ToObject<PriceCoordsAndFormattedDateList>();

                        if (priceList != null)
                        {
                            long unixTimestamp = Convert.ToInt64(priceList.X);
                            DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).DateTime;
                            string formattedDate = dateTime.ToString("yyyyMMdd");

                            priceList.FormattedDate = formattedDate;
                            HistoricPrices.Add(priceList);
                            safeToTrackPriceOnChart = true;
                        }
                    }
                    #region convert to selected currency
                    exchangeRate = priceInSelectedCurrency / Convert.ToDecimal(priceUSD);

                    foreach (var item in HistoricPrices)
                    {
                        item.Y *= exchangeRate;
                    }
                    #endregion
                    GotHistoricPrices = true;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "GetHistoricPrices");
            }
        }

        public async Task GetHistoricPricesAsyncWrapper()
        {
            await GetHistoricPricesAsync();
        }

        #endregion

        #region estimate price based on date provided

        private async void GetPriceForDate()
        {
            try
            {
                string estimatedPrice = "";
                if (comboBoxYearInput.SelectedIndex < 0)
                {
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("You need to give me at least a year to work with. The more accurate the date, the more accurate I can be.");
                    return;
                }

                if (comboBoxMonthInput.SelectedIndex < 1 && comboBoxDayInput.SelectedIndex < 1) // only the year has been set
                {
                    try
                    {
                        selectedYear = 2008 + (int)comboBoxYearInput.SelectedIndex + 1;

                        List<PriceCoordsAndFormattedDateList> PricesForSelectedYear = HistoricPrices
                            .Where(pricelist => pricelist.FormattedDate?.Length >= 4 && Convert.ToInt16(pricelist.FormattedDate[..4]) == selectedYear)
                            .ToList();

                        foreach (var item in PricesForSelectedYear)
                        {
                            item.Y *= exchangeRate;
                        }

                        decimal[] prices = PricesForSelectedYear.Select(p => p.Y).ToArray();

                        (decimal median, decimal range, decimal rangePercent) = GetMedianRangeAndPercentageDifference(prices);

                        lblEstimatedPrice.Invoke((MethodInvoker)delegate
                        {
                            lblEstimatedPrice.Text = "(" + Convert.ToString(median) + " +/-" + Convert.ToString(rangePercent) + "%)";
                        });
                        if (btnUsePriceEstimateFlag.Text == "✔️")
                        {
                            lblAddDataRange.Invoke((MethodInvoker)delegate
                            {
                                lblAddDataRange.Text = Convert.ToString(rangePercent) + "%";
                            });
                            lblAddDataPriceEstimateType.Invoke((MethodInvoker)delegate
                            {
                                lblAddDataPriceEstimateType.Text = "AM";
                            });
                        }
                        else
                        {
                            lblAddDataRange.Invoke((MethodInvoker)delegate
                            {
                                lblAddDataRange.Text = "-";
                            });
                        }
                        priceEstimateType = "AM";

                        CopyPriceEstimateToInputIfNecessary(Convert.ToString(median));
                        panelWelcome.Visible = false;
                        panelRobotSpeakOuter.Visible = true;
                        InterruptAndStartNewRobotSpeak("The median price of 1 bitcoin through " + selectedYear + " was " + selectedCurrencySymbol + Convert.ToString(median) + ", with a range of +/-" + Convert.ToString(range));
                    }
                    catch (Exception)
                    {
                        lblEstimatedPrice.Invoke((MethodInvoker)delegate
                        {
                            lblEstimatedPrice.Text = "(unavailable)";
                        });
                        lblAddDataRange.Invoke((MethodInvoker)delegate
                        {
                            lblAddDataRange.Text = "-";
                        });
                        lblAddDataPriceEstimateType.Invoke((MethodInvoker)delegate
                        {
                            lblAddDataPriceEstimateType.Text = "N";
                        });
                    }

                }
                else
                {
                    if (comboBoxMonthInput.SelectedIndex > 0 && comboBoxDayInput.SelectedIndex < 1) // year and month have been set
                    {
                        try
                        {
                            selectedYear = 2008 + (int)comboBoxYearInput.SelectedIndex + 1;
                            selectedMonthNumeric = Convert.ToInt16(monthsNumeric[comboBoxMonthInput.SelectedIndex - 1]);
                            selectedMonth = months[comboBoxMonthInput.SelectedIndex - 1];

                            List<PriceCoordsAndFormattedDateList> PricesForSelectedYearMonth = HistoricPrices
                            .Where(pricelist => Convert.ToInt16(pricelist.FormattedDate?[..4]) == selectedYear)
                            .Where(pricelist => Convert.ToInt16(pricelist.FormattedDate?.Substring(4, 2)) == selectedMonthNumeric)
                            .ToList();

                            foreach (var item in PricesForSelectedYearMonth)
                            {
                                item.Y *= exchangeRate;
                            }

                            decimal[] prices = PricesForSelectedYearMonth.Select(p => p.Y).ToArray();

                            (decimal median, decimal range, decimal rangePercent) = GetMedianRangeAndPercentageDifference(prices);

                            lblEstimatedPrice.Invoke((MethodInvoker)delegate
                            {
                                lblEstimatedPrice.Text = "(" + Convert.ToString(median) + " +/-" + Convert.ToString(rangePercent) + "%)";
                            });
                            if (btnUsePriceEstimateFlag.Text == "✔️")
                            {
                                lblAddDataRange.Invoke((MethodInvoker)delegate
                                {
                                    lblAddDataRange.Text = Convert.ToString(rangePercent) + "%";
                                });
                                lblAddDataPriceEstimateType.Invoke((MethodInvoker)delegate
                                {
                                    lblAddDataPriceEstimateType.Text = "MM";
                                });
                            }
                            else
                            {
                                lblAddDataRange.Invoke((MethodInvoker)delegate
                                {
                                    lblAddDataRange.Text = "-";
                                });
                            }
                            priceEstimateType = "MM";
                            CopyPriceEstimateToInputIfNecessary(Convert.ToString(median));
                            panelWelcome.Visible = false;
                            panelRobotSpeakOuter.Visible = true;
                            InterruptAndStartNewRobotSpeak("The median price of 1 bitcoin through " + selectedMonth + " " + selectedYear + " was " + selectedCurrencySymbol + Convert.ToString(median) + ", with a range of +/-" + Convert.ToString(range));
                        }
                        catch (Exception)
                        {
                            lblEstimatedPrice.Invoke((MethodInvoker)delegate
                            {
                                lblEstimatedPrice.Text = "(unavailable)";
                            });
                            lblAddDataRange.Invoke((MethodInvoker)delegate
                            {
                                lblAddDataRange.Text = "-";
                            });
                            lblAddDataPriceEstimateType.Invoke((MethodInvoker)delegate
                            {
                                lblAddDataPriceEstimateType.Text = "N";
                            });
                        }

                    }
                    else
                    {
                        if (comboBoxMonthInput.SelectedIndex > 0 && comboBoxDayInput.SelectedIndex > 0) // year, month and day have been set
                        {
                            try
                            {
                                selectedYear = 2008 + (int)comboBoxYearInput.SelectedIndex + 1;
                                selectedMonthNumeric = Convert.ToInt16(monthsNumeric[comboBoxMonthInput.SelectedIndex - 1]);
                                selectedMonth = months[comboBoxMonthInput.SelectedIndex - 1];
                                selectedDay = Convert.ToString(comboBoxDayInput.SelectedIndex);

                                string dateString = selectedDay + "-" + selectedMonthNumeric + "-" + selectedYear;
                                string apiUrl = "https://api.coingecko.com/api/v3/coins/bitcoin/history?date=" + dateString + "&localization=false";

                                using HttpClient client = new();
                                HttpResponseMessage response = await client.GetAsync(apiUrl);

                                if (response.IsSuccessStatusCode)
                                {
                                    string jsonResponse = await response.Content.ReadAsStringAsync();
                                    JObject bitcoinData = JObject.Parse(jsonResponse);

                                    decimal? usdPrice = bitcoinData?["market_data"]?["current_price"]?["usd"]?.Value<decimal>();
                                    if (usdPrice.HasValue)
                                    {
                                        usdPrice *= exchangeRate;
                                        selectedMedianPrice = (Math.Round((decimal)usdPrice, 2));
                                        selectedRangePercentage = 0;
                                        estimatedPrice = Convert.ToString(Math.Round((decimal)usdPrice, 2));
                                        lblEstimatedPrice.Invoke((MethodInvoker)delegate
                                        {
                                            lblEstimatedPrice.Text = "(" + estimatedPrice + ")";
                                        });
                                        lblAddDataRange.Invoke((MethodInvoker)delegate
                                        {
                                            lblAddDataRange.Text = "0%";
                                        });
                                        if (btnUsePriceEstimateFlag.Text == "✔️")
                                        {
                                            lblAddDataPriceEstimateType.Invoke((MethodInvoker)delegate
                                            {
                                                lblAddDataPriceEstimateType.Text = "DA";
                                            });
                                            lblAddDataRange.Invoke((MethodInvoker)delegate
                                            {
                                                lblAddDataRange.Text = "0%*";
                                            });
                                        }
                                        priceEstimateType = "DA";
                                        CopyPriceEstimateToInputIfNecessary(estimatedPrice);
                                        panelWelcome.Visible = false;
                                        panelRobotSpeakOuter.Visible = true;
                                        InterruptAndStartNewRobotSpeak("The average price of 1 bitcoin for " + selectedDay + " " + selectedMonth + ", " + selectedYear + " was " + selectedCurrencySymbol + estimatedPrice);
                                    }
                                    else
                                    {
                                        lblEstimatedPrice.Invoke((MethodInvoker)delegate
                                        {
                                            lblEstimatedPrice.Text = "0.00";
                                        });
                                        CopyPriceEstimateToInputIfNecessary(estimatedPrice);
                                        panelWelcome.Visible = false;
                                        panelRobotSpeakOuter.Visible = true;
                                        InterruptAndStartNewRobotSpeak("The average price of 1 bitcoin for " + selectedDay + " " + selectedMonth + ", " + selectedYear + " was " + selectedCurrencySymbol + "0.0");
                                    }
                                }
                                else
                                {
                                    panelWelcome.Visible = false;
                                    panelRobotSpeakOuter.Visible = true;
                                    InterruptAndStartNewRobotSpeak($"Failed to fetch data. Status code: {response.StatusCode}");
                                }
                            }
                            catch (Exception)
                            {
                                lblEstimatedPrice.Invoke((MethodInvoker)delegate
                                {
                                    lblEstimatedPrice.Text = "(unavailable)";
                                });
                                lblAddDataRange.Invoke((MethodInvoker)delegate
                                {
                                    lblAddDataRange.Text = "-";
                                });
                                lblAddDataPriceEstimateType.Invoke((MethodInvoker)delegate
                                {
                                    lblAddDataPriceEstimateType.Text = "N";
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "GetPriceForDate");
            }
        }

        #endregion

        #region refresh current price

        private void PictureBoxRefreshPrice_Click(object sender, EventArgs e)
        {
            try
            {
                suppressClearedMessage = true;
                HistoricPrices.Clear();
                if (selectedCurrencyName == "USD")
                {
                    BtnUSD_Click(sender, e);
                }
                if (selectedCurrencyName == "EUR")
                {
                    BtnEUR_Click(sender, e);
                }
                if (selectedCurrencyName == "GBP")
                {
                    BtnGBP_Click(sender, e);
                }
                if (selectedCurrencyName == "XAU")
                {
                    BtnXAU_Click(sender, e);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "PictureBoxRefreshPrice_Click");
            }
        }

        #endregion

        #region get current price

        private async Task<(string priceUSD, string priceGBP, string priceEUR, string priceXAU)> BitcoinExplorerOrgGetPriceAsync()
        {
            try
            {
                using HttpClient client = new();
                var response = await client.GetStringAsync("https://bitcoinexplorer.org/api/price");
                if (response == null)
                {
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Failed to get current price. Please try refreshing.");
                    return ("0", "0", "0", "0");
                }
                var data = JObject.Parse(response);
                if (data != null)
                {
                    string priceUSD = Convert.ToString(data["usd"])!;
                    string priceGBP = Convert.ToString(data["gbp"])!;
                    string priceEUR = Convert.ToString(data["eur"])!;
                    string priceXAU = Convert.ToString(data["xau"])!;
                    return (priceUSD, priceGBP, priceEUR, priceXAU)!;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BitcoinExplorerOrgGetPriceAsync");
            }
            return ("0", "0", "0", "0");
        }

        #endregion

        #region hover effect on refresh price picturebox (mimic button)

        private void PictureBoxRefreshPrice_MouseEnter(object sender, EventArgs e)
        {
            try
            {
                if (pictureBoxRefreshPrice.Image != Resources.RefreshDisabled)
                {
                    pictureBoxRefreshPrice.Invoke((MethodInvoker)delegate
                    {
                        pictureBoxRefreshPrice.Image = Resources.RefreshHover;
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "PictureBoxRefreshPrice_MouseEnter");
            }
        }

        private void PictureBoxRefreshPrice_MouseLeave(object sender, EventArgs e)
        {
            try
            {
                if (pictureBoxRefreshPrice.Image != Resources.RefreshDisabled)
                {
                    pictureBoxRefreshPrice.Invoke((MethodInvoker)delegate
                    {
                        pictureBoxRefreshPrice.Image = Resources.Refresh;
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "PictureBoxRefreshPrice_MouseLeave");
            }
        }

        #endregion

        #endregion

        #region transactions file operations

        #region read transactions from file

        private static List<Transaction> ReadTransactionsFromJsonFile()
        {
            string transactionsFileName = "transactions.json";
            string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string applicationDirectory = Path.Combine(appDataDirectory, "Cubit");
            // Create the application directory if it doesn't exist
            Directory.CreateDirectory(applicationDirectory);
            string transactionsFilePath = Path.Combine(applicationDirectory, transactionsFileName);
            string filePath = transactionsFilePath;

            if (!System.IO.File.Exists(filePath))
            {
                System.IO.File.Create(filePath).Dispose();
            }
            // Read the contents of the JSON file into a string
            string json = System.IO.File.ReadAllText(filePath);

            // Deserialize the JSON string into a list of transaction objects
            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(json);

            // If the JSON file doesn't exist or is empty, return an empty list
            transactions ??= new List<Transaction>();
            transactions = transactions
                .OrderBy(b => b.Year)
                .ThenBy(b => b.Month)
                .ThenBy(b => b.Day)
                .ToList();

            return transactions;
        }

        #endregion

        #region prepare record to write to transactions file

        private async void BtnAddTransaction_Click(object sender, EventArgs e)
        {
            try
            {
                // a bit more validation before accepting the transaction
                if (Convert.ToDouble(textBoxBTCInput.Text) > 21000000)
                {
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("There will only ever be 21000000 bitcoin. You don't have that many!");
                    return;
                }

                if (btnSoldBitcoin.Text == "✔️" && Convert.ToDouble(textBoxBTCInput.Text) > Convert.ToDouble(lblTotalBTCAmount.Text[1..]))
                {
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("You don't have this much bitcoin to sell. Have you missed some earlier transactions?");
                    return;
                }

                DateTime today = DateTime.Now;
                string transactionType = "Spnd";
                if (btnBoughtBitcoin.Text == "✔️")
                {
                    transactionType = "Recv";
                }
                var newTransaction = new Transaction { DateAdded = today, TransactionType = transactionType, Year = lblAddDataYear.Text, Month = lblAddDataMonth.Text, Day = lblAddDataDay.Text, Price = lblAddDataPrice.Text, EstimateType = lblAddDataPriceEstimateType.Text, EstimateRange = lblAddDataRange.Text, FiatAmount = lblAddDataFiat.Text, FiatAmountEstimateFlag = lblAddDataFiatEstimateFlag.Text, BTCAmount = lblAddDataBTC.Text, BTCAmountEstimateFlag = lblAddDataBTCEstimateFlag.Text, Label = lblAddDataLabel.Text, LabelColor = selectedColor };
                // Read the existing transactions from the JSON file
                var transactions = ReadTransactionsFromJsonFile();

                // Add the new transaction to the list
                transactions.Add(newTransaction);

                // Write the updated list of transactions back to the JSON file
                WriteTransactionsToJsonFile(transactions);
                suppressClearedMessage = true;
                BtnClearInput_Click(sender, e);
                isRobotSpeaking = false;
                panelWelcome.Visible = false;
                panelRobotSpeakOuter.Visible = true;
                InterruptAndStartNewRobotSpeak("Transaction added to list and saved.");
                HistoricPrices.Clear();
                await GetHistoricPricesAsyncWrapper();
                await SetupTransactionsList();
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnAddTransaction_Click");
            }
        }

        #endregion

        #region write record to transactions file

        private static void WriteTransactionsToJsonFile(List<Transaction> transactions)
        {
            // Serialize the list of transaction objects into a JSON string
            string json = JsonConvert.SerializeObject(transactions);

            string transactionsFileName = "transactions.json";
            string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string applicationDirectory = Path.Combine(appDataDirectory, "Cubit");
            // Create the application directory if it doesn't exist
            Directory.CreateDirectory(applicationDirectory);
            string transactionsFilePath = Path.Combine(applicationDirectory, transactionsFileName);
            string filePath = transactionsFilePath;

            // Write the JSON string to the json file
            File.WriteAllText(filePath, json);
        }

        #endregion

        #region delete record from transactions file

        private static void DeleteTransactionFromJsonFile(string transactionDataToDelete)
        {
            // Read the existing transactions from the JSON file
            var transactions = ReadTransactionsFromJsonFile();

            // Find the index of the transaction with the specified data
            int index = transactions.FindIndex(transaction =>
                Convert.ToString(transaction.DateAdded) == transactionDataToDelete);

            // If a matching transaction was found, remove it from the list
            if (index >= 0)
            {
                transactions.RemoveAt(index);

                // Write the updated list of transactions back to the JSON file
                WriteTransactionsToJsonFile(transactions);
            }
        }

        private static void DeleteAllTransactionsFromJsonFile()
        {
            // Create an empty list of transactions
            List<Transaction> transactions = new();

            WriteTransactionsToJsonFile(transactions);
        }

        #endregion

        #region backup transactions and settings

        private void BtnBackupTXAndSettings_Click(object sender, EventArgs e)
        {
            try
            {
                using FolderBrowserDialog folderBrowserDialog = new();
                folderBrowserDialog.Description = "Select a folder to save the backup files to...";

                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string backupFolder = folderBrowserDialog.SelectedPath;
                    string appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    string applicationDirectory = Path.Combine(appDataDirectory, "Cubit");

                    try
                    {
                        Directory.CreateDirectory(applicationDirectory);

                        // Backup 'settings.json'
                        string settingsFileName = "settings.json";
                        string settingsFilePath = Path.Combine(applicationDirectory, settingsFileName);
                        File.Copy(settingsFilePath, Path.Combine(backupFolder, settingsFileName), true);

                        // Backup 'transactions.json'
                        string transactionsFileName = "transactions.json";
                        string transactionsFilePath = Path.Combine(applicationDirectory, transactionsFileName);
                        File.Copy(transactionsFilePath, Path.Combine(backupFolder, transactionsFileName), true);
                        panelWelcome.Visible = false;
                        panelRobotSpeakOuter.Visible = true;
                        InterruptAndStartNewRobotSpeak("Your transactions and settings have been backed up");
                    }
                    catch (Exception ex)
                    {
                        InterruptAndStartNewRobotSpeak($"Error creating backups: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnBackupTXAndSettings_Click");
            }
        }

        #endregion

        #endregion

        #region transactions listview

        #region set up listview

        private async Task SetupTransactionsList()
        {
            try
            {
                dayOfFirstTransaction = 0;
                monthOfFirstTransaction = 0;
                yearOfFirstTransaction = 0;
                totalFiatSpentOnBuyTransactions = 0;
                totalBTCReceivedOnBuyTransactions = 0;
                totalFiatReceivedOnSellTransactions = 0;
                totalBTCSpentOnSellTransactions = 0;
                totalTXCountReceiveBTC = 0;
                totalTXCountSpendBTC = 0;
                highestPricePaid = 0;
                lowestPricePaid = 999999999;
                highestPriceSold = 0;
                lowestPriceSold = 999999999;
                mostFiatSpentInOneTransaction = 0;
                mostBTCReceivedInOneTransaction = 0;
                mostFiatReceivedInOneTransaction = 0;
                mostBTCSpentInOneTransaction = 0;
                listBuyBTCTransactionDate = new();
                listBuyBTCTransactionFiatAmount = new();
                listBuyBTCTransactionPrice = new();
                listSellBTCTransactionDate = new();
                listSellBTCTransactionFiatAmount = new();
                listSellBTCTransactionPrice = new();
                listTransactionDate = new();
                listCostBasisAmounts = new();

                bool transactionFound = false;
                var transactions = ReadTransactionsFromJsonFile();
                if (transactions.Count > 0)
                {
                    transactionFound = true;
                    Transaction? firstTransaction = transactions.FirstOrDefault();
                    if (firstTransaction != null)
                    {
                        if (firstTransaction.Year != "-")
                        {
                            yearOfFirstTransaction = Convert.ToInt16(firstTransaction.Year);
                        }
                        if (firstTransaction.Month != "-")
                        {
                            monthOfFirstTransaction = Convert.ToInt16(firstTransaction.Month);
                        }
                        if (firstTransaction.Day != "-")
                        {
                            dayOfFirstTransaction = Convert.ToInt16(firstTransaction.Day);
                        }
                    }
                }

                if (!transactionFound) // there are no transactions
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Items.Clear(); // remove any data that may be there already
                        listViewTransactions.Visible = false;
                    });
                    lblTotalBTCAmount.Invoke((MethodInvoker)delegate
                    {
                        lblTotalBTCAmount.Visible = false;
                    });
                    lblTotalFiatAmount.Invoke((MethodInvoker)delegate
                    {
                        lblTotalFiatAmount.Visible = false;
                    });
                    lblFinalCostBasis.Invoke((MethodInvoker)delegate
                    {
                        lblFinalCostBasis.Visible = false;
                    });
                    label51.Invoke((MethodInvoker)delegate
                    {
                        label51.Visible = false;
                    });
                    label91.Invoke((MethodInvoker)delegate
                    {
                        label91.Visible = false;
                    });
                    label93.Invoke((MethodInvoker)delegate
                    {
                        label93.Visible = false;
                    });
                    lblROI.Invoke((MethodInvoker)delegate
                    {
                        lblROI.Visible = false;
                    });
                    lblBTCTotalValue.Invoke((MethodInvoker)delegate
                    {
                        lblBTCTotalValue.Visible = false;
                    });

                    panelScrollbarContainer.Invoke((MethodInvoker)delegate
                    {
                        panelScrollbarContainer.Height = (int)(25 * UIScale);
                    });
                    DrawPriceChart(); // draw price chart before returning
                    return;
                }

                panelScrollbarContainer.Invoke((MethodInvoker)delegate
                {
                    panelScrollbarContainer.Height = (int)(206 * UIScale);
                });
                lblTotalBTCAmount.Invoke((MethodInvoker)delegate
                {
                    lblTotalBTCAmount.Visible = true;
                });
                lblTotalFiatAmount.Invoke((MethodInvoker)delegate
                {
                    lblTotalFiatAmount.Visible = true;
                });
                lblFinalCostBasis.Invoke((MethodInvoker)delegate
                {
                    lblFinalCostBasis.Visible = true;
                });
                label51.Invoke((MethodInvoker)delegate
                {
                    label51.Visible = true;
                });
                label91.Invoke((MethodInvoker)delegate
                {
                    label91.Visible = true;
                });
                label93.Invoke((MethodInvoker)delegate
                {
                    label93.Visible = true;
                });
                lblROI.Invoke((MethodInvoker)delegate
                {
                    lblROI.Visible = true;
                });
                lblBTCTotalValue.Invoke((MethodInvoker)delegate
                {
                    lblBTCTotalValue.Visible = true;
                });

                //LIST VIEW
                listViewTransactions.Invoke((MethodInvoker)delegate
                {
                    listViewTransactions.Items.Clear(); // remove any data that may be there already
                    listViewTransactions.Visible = true;
                });
                listViewTransactions.GetType().InvokeMember("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, null, listViewTransactions, new object[] { true });
                // Check if the column header already exists
                if (listViewTransactions.Columns.Count == 0)
                {
                    // If not, add the column header
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("TX num", (int)(30 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 1)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Type", (int)(35 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 2)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("YYYY", (int)(38 * UIScale));
                    });
                }

                if (listViewTransactions.Columns.Count == 3)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("MM", (int)(27 * UIScale));
                    });
                }

                if (listViewTransactions.Columns.Count == 4)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("DD", (int)(27 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 5)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Price", (int)(75 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 6)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Est", (int)(28 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 7)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Range", (int)(50 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 8)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Fiat amt.", (int)(70 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 9)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Est", (int)(28 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 10)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("BTC amt.", (int)(95 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 11)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Est.", (int)(28 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 12)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Fiat total", (int)(80 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 13)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("BTC total", (int)(95 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 14)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Cost basis", (int)(85 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 15)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("LabelColor", (int)(20 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 16)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Label icon", (int)(20 * UIScale));
                    });
                }
                if (listViewTransactions.Columns.Count == 17)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Label (hidden)", 0);
                    });
                }
                if (listViewTransactions.Columns.Count == 18)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Date Added (hidden)", 0);
                    });
                }
                // Add the items to the ListView
                int counterAllTransactions = 0; // used to count rows in list as they're added
                decimal currentValue = 0;
                rollingBTCAmount = 0;
                decimal rollingOriginalFiatAmount = 0;
                decimal rollingCostBasis = 0;
                decimal rollingCurrentValue = 0;

                foreach (var transaction in transactions)
                {
                    ListViewItem item = new(Convert.ToString(listViewTransactions.Items.Count + 1)); // create new row
                    //add inputted data to listview
                    item.SubItems.Add(transaction.TransactionType);
                    item.SubItems.Add(transaction.Year);
                    item.SubItems.Add(transaction.Month);
                    item.SubItems.Add(transaction.Day);

                    #region prepare list of transaction elements for the graph (transaction date, fiat amount)

                    int year = int.Parse(transaction.Year!);
                    int month = 0;
                    if (transaction.Month == "-")
                    {
                        month = 6; //no month provided so we'll pick a middle month for the sake of the graph
                    }
                    else
                    {
                        month = int.Parse(transaction.Month!);
                    }
                    int day = 0;
                    if (transaction.Day == "-")
                    {
                        day = 15; //no day was provided so we'll pick a middle of the month day for the graph
                    }
                    else
                    {
                        day = int.Parse(transaction.Day!);
                    }
                    // Create a DateTime object
                    DateTime date = new(year, month, day);
                    // Convert the DateTime object to OADate format
                    double oadate = date.ToOADate();
                    // Fiat amount
                    double transactionFiatAmount = double.Parse(transaction.FiatAmount!);
                    // Price
                    double transactionPrice = double.Parse(transaction.Price!);

                    if (transaction.TransactionType == "Recv")
                    {
                        totalTXCountReceiveBTC++;
                        totalFiatSpentOnBuyTransactions += transactionFiatAmount;
                        totalBTCReceivedOnBuyTransactions += Convert.ToDouble(transaction.BTCAmount);
                        if (Convert.ToDouble(transaction.Price) > highestPricePaid)
                        {
                            highestPricePaid = Convert.ToDouble(transaction.Price);
                        }
                        if (Convert.ToDouble(transaction.Price) < lowestPricePaid)
                        {
                            lowestPricePaid = Convert.ToDouble(transaction.Price);
                        }
                        listTransactionDate.Add(oadate);
                        listBuyBTCTransactionDate.Add(oadate);
                        listBuyBTCTransactionFiatAmount.Add(transactionFiatAmount);
                        listBuyBTCTransactionPrice.Add(transactionPrice);
                        if (Math.Abs(Convert.ToDouble(transaction.FiatAmount)) > mostFiatSpentInOneTransaction)
                        {
                            mostFiatSpentInOneTransaction = Math.Abs(Convert.ToDouble(transaction.FiatAmount));
                        }
                        if (Convert.ToDouble(transaction.BTCAmount) > mostBTCReceivedInOneTransaction)
                        {
                            mostBTCReceivedInOneTransaction = Convert.ToDouble(transaction.BTCAmount);
                        }
                    }
                    else
                    {
                        totalTXCountSpendBTC++;
                        totalFiatReceivedOnSellTransactions += transactionFiatAmount;
                        totalBTCSpentOnSellTransactions += Convert.ToDouble(transaction.BTCAmount);
                        if (Convert.ToDouble(transaction.Price) > highestPriceSold)
                        {
                            highestPriceSold = Convert.ToDouble(transaction.Price);
                        }
                        if (Convert.ToDouble(transaction.Price) < lowestPriceSold)
                        {
                            lowestPriceSold = Convert.ToDouble(transaction.Price);
                        }
                        listTransactionDate.Add(oadate);
                        listSellBTCTransactionDate.Add(oadate);
                        listSellBTCTransactionFiatAmount.Add(transactionFiatAmount);
                        listSellBTCTransactionPrice.Add(transactionPrice);
                        if (Convert.ToDouble(transaction.FiatAmount) > mostFiatReceivedInOneTransaction)
                        {
                            mostFiatReceivedInOneTransaction = Convert.ToDouble(transaction.FiatAmount);
                        }
                        if (Math.Abs(Convert.ToDouble(transaction.BTCAmount)) > mostBTCSpentInOneTransaction)
                        {
                            mostBTCSpentInOneTransaction = Math.Abs(Convert.ToDouble(transaction.BTCAmount));
                        }
                    }

                    #endregion

                    item.SubItems.Add(transaction.Price);
                    item.SubItems.Add(transaction.EstimateType);
                    item.SubItems.Add(transaction.EstimateRange);
                    item.SubItems.Add(transaction.FiatAmount);
                    item.SubItems.Add(transaction.FiatAmountEstimateFlag);
                    item.SubItems.Add(transaction.BTCAmount);
                    item.SubItems.Add(transaction.BTCAmountEstimateFlag);
                    // add rolling fiat total to listview
                    rollingOriginalFiatAmount = Math.Round(rollingOriginalFiatAmount + Convert.ToDecimal(transaction.FiatAmount), 2);
                    item.SubItems.Add(Convert.ToString(rollingOriginalFiatAmount));
                    // add rolling btc total to listview
                    rollingBTCAmount = Math.Round(rollingBTCAmount + Convert.ToDecimal(transaction.BTCAmount), 8);
                    item.SubItems.Add(Convert.ToString(rollingBTCAmount));

                    rollingCurrentValue += currentValue;

                    // average purchase price

                    if (rollingBTCAmount > 0)
                    {
                        rollingCostBasis = Math.Round(rollingOriginalFiatAmount / rollingBTCAmount, 2);
                    }
                    else
                    {
                        rollingOriginalFiatAmount = 0;
                        rollingCostBasis = 0;
                    }

                    decimal formattedCostBasis = 0;
                    if (rollingCostBasis < 0)
                    {
                        formattedCostBasis = Math.Abs(rollingCostBasis);// turn the negative amounts to positive
                    }
                    else
                    {
                        formattedCostBasis = rollingCostBasis - (2 * rollingCostBasis); // turn the positive amounts to negative
                    }


                    item.SubItems.Add(Convert.ToString(formattedCostBasis));
                    lblTotalBTCAmount.Invoke((MethodInvoker)delegate
                    {
                        lblTotalBTCAmount.Text = "₿" + Convert.ToString(rollingBTCAmount);
                    });
                    if (rollingOriginalFiatAmount >= 0)
                    {
                        lblTotalFiatAmount.Invoke((MethodInvoker)delegate
                        {
                            lblTotalFiatAmount.BackColor = Color.Honeydew;
                            lblTotalFiatAmount.ForeColor = Color.OliveDrab;
                        });
                    }
                    else
                    {
                        lblTotalFiatAmount.Invoke((MethodInvoker)delegate
                        {
                            lblTotalFiatAmount.BackColor = Color.FromArgb(255, 240, 240);
                            lblTotalFiatAmount.ForeColor = Color.IndianRed;
                        });
                    }
                    lblTotalFiatAmount.Invoke((MethodInvoker)delegate
                    {
                        lblTotalFiatAmount.Text = selectedCurrencySymbol + Convert.ToString(Math.Abs(rollingOriginalFiatAmount));
                    });

                    if (lblTotalFiatAmount.ForeColor == Color.IndianRed)
                    {
                        label93.Invoke((MethodInvoker)delegate
                        {
                            label93.Visible = true;
                        });
                        lblTotalFiatAmount.Invoke((MethodInvoker)delegate
                        {
                            lblTotalFiatAmount.Location = new Point(label93.Location.X + label93.Width - 3, label93.Location.Y);
                        });
                    }
                    else
                    {
                        label93.Invoke((MethodInvoker)delegate
                        {
                            label93.Visible = false;
                        });
                        lblTotalFiatAmount.Invoke((MethodInvoker)delegate
                        {
                            lblTotalFiatAmount.Location = new Point(308, 8);
                        });
                    }

                    #region prepare list of cost basis elements for the graph (transaction date, cost basis)

                    // cost basis amount (dates have already been assigned to listTransactionDate during listviewsetup)
                    double costBasis = Convert.ToDouble(rollingCostBasis);

                    if (costBasis < 0)
                    {
                        costBasis = Math.Abs(costBasis);// turn the negative amounts to positive
                    }
                    else
                    {
                        costBasis -= (2 * costBasis); // turn the positive amounts to negative
                    }
                    // set any negative outcomes to zero because graph doesn't show negatives
                    if (costBasis < 0)
                    {
                        costBasis = 0.0001;
                    }

                    listCostBasisAmounts.Add(costBasis);

                    #endregion

                    if (rollingCostBasis <= currentValue)
                    {
                        // flip negative to positive and positive to negative
                        decimal formattedRollingCostBasis;
                        if (rollingCostBasis < 0)
                        {
                            formattedRollingCostBasis = Math.Abs(rollingCostBasis);
                        }
                        else
                        {
                            formattedRollingCostBasis = rollingCostBasis - (2 * rollingCostBasis);
                        }
                        lblFinalCostBasis.Invoke((MethodInvoker)delegate
                        {
                            lblFinalCostBasis.Text = selectedCurrencySymbol + Convert.ToString(formattedRollingCostBasis);
                            lblFinalCostBasis.BackColor = Color.Honeydew;
                            lblFinalCostBasis.ForeColor = Color.OliveDrab;
                        });
                    }
                    else
                    {
                        // flip negative to positive and positive to negative
                        decimal formattedRollingCostBasis;
                        if (rollingCostBasis < 0)
                        {
                            formattedRollingCostBasis = Math.Abs(rollingCostBasis);
                        }
                        else
                        {
                            formattedRollingCostBasis = rollingCostBasis - (2 * rollingCostBasis);
                        }
                        lblFinalCostBasis.Invoke((MethodInvoker)delegate
                        {
                            lblFinalCostBasis.Text = selectedCurrencySymbol + Convert.ToString(formattedRollingCostBasis);
                            lblFinalCostBasis.BackColor = Color.FromArgb(255, 240, 240);
                            lblFinalCostBasis.ForeColor = Color.IndianRed;
                        });
                    }

                    if (transaction.LabelColor != "9")
                    {
                        item.SubItems.Add(Convert.ToString(transaction.LabelColor));
                    }
                    else
                    {
                        item.SubItems.Add("");
                    }

                    if (transaction.Label != null && transaction.Label != "-")
                    {

                        item.SubItems.Add("🏷️");
                    }
                    else
                    {
                        item.SubItems.Add("");
                    }

                    if (transaction.Label != null)
                    {

                        item.SubItems.Add(transaction.Label);
                    }

                    item.SubItems.Add(Convert.ToString(transaction.DateAdded));

                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Items.Add(item); // add row
                    });

                    // Get the height of each item to set height of whole listview
                    int rowHeight = listViewTransactions.Margin.Vertical + listViewTransactions.Padding.Vertical + listViewTransactions.GetItemRect(0).Height;
                    int itemCount = listViewTransactions.Items.Count; // Get the number of items in the ListBox
                    int listBoxHeight = (itemCount + 2) * rowHeight; // Calculate the height of the ListBox (the extra 2 gives room for the header)

                    counterAllTransactions++;
                }

                label51.Invoke((MethodInvoker)delegate
                {
                    label51.Location = new Point(lblTotalBTCAmount.Location.X + lblTotalBTCAmount.Width, label51.Location.Y);
                });

                lblBTCTotalValue.Invoke((MethodInvoker)delegate
                {
                    double temp = Convert.ToDouble(lblTotalBTCAmount.Text[1..]) * Convert.ToDouble(priceInSelectedCurrency);
                    temp = Math.Round(temp, 2);
                    lblBTCTotalValue.Text = selectedCurrencySymbol + Convert.ToString(temp);
                    lblBTCTotalValue.Location = new Point(label51.Location.X + label51.Width - 3, lblBTCTotalValue.Location.Y);
                });

                double changeInValue = 0;
                if (Convert.ToDouble(lblTotalFiatAmount.Text[1..]) != 0)
                {
                    changeInValue = (Convert.ToDouble(lblBTCTotalValue.Text[1..]) - Convert.ToDouble(lblTotalFiatAmount.Text[1..])) / Math.Abs(Convert.ToDouble(lblTotalFiatAmount.Text[1..])) * 100;
                }
                else
                {
                    changeInValue = (Convert.ToDouble(lblBTCTotalValue.Text[1..]) - Convert.ToDouble(lblTotalFiatAmount.Text[1..])) / 1 * 100;
                }
                changeInValue = Math.Round(changeInValue, 2);
                if (changeInValue > 0)
                {
                    lblROI.Invoke((MethodInvoker)delegate
                    {
                        lblROI.Text = "▲" + Convert.ToString(changeInValue) + "%";
                        lblROI.ForeColor = Color.OliveDrab;
                    });
                }
                if (changeInValue < 0)
                {
                    lblROI.Invoke((MethodInvoker)delegate
                    {
                        lblROI.Text = "▼" + Convert.ToString(changeInValue) + "%";
                        lblROI.ForeColor = Color.IndianRed;
                    });
                }
                if (changeInValue == 0)
                {
                    lblROI.Invoke((MethodInvoker)delegate
                    {
                        lblROI.Text = Convert.ToString(changeInValue) + "%";
                        lblROI.ForeColor = Color.DimGray;
                    });
                }
                lblROI.Invoke((MethodInvoker)delegate
                {
                    lblROI.Location = new Point(lblBTCTotalValue.Location.X + lblBTCTotalValue.Width - 3, lblROI.Location.Y);
                });
                label91.Invoke((MethodInvoker)delegate
                {
                    label91.Location = new Point(lblROI.Location.X + lblROI.Width - 4, label91.Location.Y);
                });

                vScrollBar1.Width = (int)(23 * UIScale);
                vScrollBar1.Maximum = listViewTransactions.Items.Count - 1;

                // now reverse the order so most recent are first (did it this way round to calculate the rolling balances and cost basis first)
                System.Windows.Forms.ListView listView = listViewTransactions;
                listView.BeginUpdate();

                List<ListViewItem> items = new();
                items.AddRange(listView.Items.Cast<ListViewItem>());
                items.Reverse();

                listView.Items.Clear();
                listView.Items.AddRange(items.ToArray());

                listView.EndUpdate();
                btnListReverse.Invoke((MethodInvoker)delegate
                {
                    btnListReverse.Text = "Oldest first";
                });
                if (rollingBTCAmount > 0)
                {
                    panelNoRefreshMessage.Visible = false;
                    DrawPriceChart();
                }
                else
                {
                    panelNoRefreshMessage.Visible = true;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "SetupTransactionsList");
            }
        }

        #endregion

        #region select transaction row on listview

        private void ListViewTransactions_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            try
            {
                if (listViewTransactions.SelectedItems.Count > 0)
                {
                    ListViewItem selectedItem = listViewTransactions.SelectedItems[0]; // Get selected row

                    btnDeleteTransaction.Invoke((MethodInvoker)delegate
                    {
                        btnDeleteTransaction.Enabled = true;
                        btnDeleteTransaction.BackColor = Color.FromArgb(255, 192, 128);
                    });
                    lblDisabledDeleteButtonText.Invoke((MethodInvoker)delegate
                    {
                        lblDisabledDeleteButtonText.Visible = false;
                    });

                    if (selectedItem.SubItems[15] == null || Convert.ToString(selectedItem.SubItems[17].Text) == "-")
                    {
                        lblTransactionLabel.Invoke((MethodInvoker)delegate
                        {
                            lblTransactionLabel.Text = "No label associated with this transaction";
                        });
                    }
                    else // Update the label text with the value from the first column
                    {
                        lblTransactionLabel.Invoke((MethodInvoker)delegate
                        {
                            lblTransactionLabel.Text = selectedItem.SubItems[17].Text;
                        });
                        panelWelcome.Visible = false;
                        panelRobotSpeakOuter.Visible = true;
                        InterruptAndStartNewRobotSpeak("Label assigned to transaction: " + Convert.ToString(selectedItem.SubItems[17].Text));
                    }
                }
                else
                {
                    btnDeleteTransaction.Invoke((MethodInvoker)delegate
                    {
                        btnDeleteTransaction.Enabled = false;
                        btnDeleteTransaction.BackColor = Color.FromArgb(234, 234, 234);
                    });
                    lblDisabledDeleteButtonText.Invoke((MethodInvoker)delegate
                    {
                        lblDisabledDeleteButtonText.Visible = true;
                        lblDisabledDeleteButtonText.BringToFront();
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "ListViewTransactions_ItemSelectionChanged");
            }
        }

        #endregion

        #region styling for listview

        private void ListViewTransactions_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            try
            {
                SolidBrush brush = new(Color.FromArgb(255, 192, 128));
                e.Graphics.FillRectangle(brush, e.Bounds);
                // Change text color and alignment
                SolidBrush textBrush = new(Color.White);
                StringFormat format = new()
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                };
                if (e.Header != null && e.Font != null)
                {
                    e.Graphics.DrawString(e.Header.Text, e.Font, textBrush, e.Bounds, format);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "listViewTransactions_DrawColumnHeader");
            }
        }

        private void ListViewTransactions_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
        {
            try
            {
                var text = "";
                if (e.SubItem != null)
                {
                    text = e.SubItem.Text;
                }
                else
                {
                    return;
                }

                var font = listViewTransactions.Font;
                var columnWidth = 0;
                if (e.Header != null)
                {
                    columnWidth = e.Header.Width;
                }
                var textWidth = TextRenderer.MeasureText(text, font).Width;

                // Clear the background
                var bounds = new Rectangle(e.SubItem.Bounds.Left, e.SubItem.Bounds.Top, columnWidth, e.SubItem.Bounds.Height);


                if (e.Item!.Selected)
                {
                    e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(220, 220, 220)), bounds);
                }
                else
                {
                    if (e.ItemIndex % 2 == 0)
                    {
                        e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), bounds);
                    }
                    else
                    {
                        e.Graphics.FillRectangle(new SolidBrush(listViewTransactions.BackColor), bounds);
                    }
                }
                if (e.ColumnIndex == 1) // transaction type
                {
                    if (!string.IsNullOrEmpty(text) && text == "Recv")
                    {
                        using Brush backgroundBrush = new SolidBrush(Color.FromArgb(255, 172, 108));
                        e.Graphics.FillRectangle(backgroundBrush, e.SubItem.Bounds);
                        e.SubItem.ForeColor = Color.White;
                    }
                    else
                    {
                        using Brush backgroundBrush = new SolidBrush(Color.DimGray);
                        e.Graphics.FillRectangle(backgroundBrush, e.SubItem.Bounds);
                        e.SubItem.ForeColor = Color.White;
                    }
                }
                if (e.ColumnIndex == 6) // price estimate type
                {
                    if (!string.IsNullOrEmpty(text) && text == "DA")
                    {
                        using Brush backgroundBrush = new SolidBrush(Color.FromArgb(255, 230, 230));
                        e.Graphics.FillRectangle(backgroundBrush, e.SubItem.Bounds);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(text) && text == "MM")
                        {
                            using Brush backgroundBrush = new SolidBrush(Color.FromArgb(255, 205, 205));
                            e.Graphics.FillRectangle(backgroundBrush, e.SubItem.Bounds);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(text) && text == "AM")
                            {
                                using Brush backgroundBrush = new SolidBrush(Color.FromArgb(255, 180, 180));
                                e.Graphics.FillRectangle(backgroundBrush, e.SubItem.Bounds);
                            }
                            else
                            {
                                using Brush backgroundBrush = new SolidBrush(Color.FromArgb(230, 255, 230));
                                e.Graphics.FillRectangle(backgroundBrush, e.SubItem.Bounds);
                            }
                        }
                    }

                }
                if (e.ColumnIndex == 8) // Fiat amount
                {
                    if (!string.IsNullOrEmpty(text) && text[0] == '-')

                    {
                        e.SubItem.ForeColor = Color.IndianRed;
                    }
                    else
                    {
                        e.SubItem.ForeColor = Color.OliveDrab;
                    }
                }
                if (e.ColumnIndex == 9) // Fiat amount estimate flag
                {
                    if (!string.IsNullOrEmpty(text) && text[0] == 'Y')
                    {
                        using Brush backgroundBrush = new SolidBrush(Color.FromArgb(255, 230, 230));
                        e.Graphics.FillRectangle(backgroundBrush, e.SubItem.Bounds);
                    }
                    else
                    {
                        using Brush backgroundBrush = new SolidBrush(Color.FromArgb(230, 255, 230));
                        e.Graphics.FillRectangle(backgroundBrush, e.SubItem.Bounds);
                    }
                }
                if (e.ColumnIndex == 10) // Bitcoin amount
                {
                    if (!string.IsNullOrEmpty(text) && text[0] == '-')

                    {
                        e.SubItem.ForeColor = Color.IndianRed;
                    }
                    else
                    {
                        e.SubItem.ForeColor = Color.OliveDrab;
                    }
                }
                if (e.ColumnIndex == 11) // Bitcoin amount estimate flag
                {
                    if (!string.IsNullOrEmpty(text) && text[0] == 'Y')
                    {
                        using Brush backgroundBrush = new SolidBrush(Color.FromArgb(255, 230, 230));
                        e.Graphics.FillRectangle(backgroundBrush, e.SubItem.Bounds);
                    }
                    else
                    {
                        using Brush backgroundBrush = new SolidBrush(Color.FromArgb(230, 255, 230));
                        e.Graphics.FillRectangle(backgroundBrush, e.SubItem.Bounds);
                    }

                }
                if (e.ColumnIndex == 14) // cost basis
                {
                    string priceText = System.Text.RegularExpressions.Regex.IsMatch(lblCurrentPrice.Text, @"^[^0-9]")
                    ? lblCurrentPrice.Text[1..]
                    : lblCurrentPrice.Text;
                    if (Convert.ToDecimal(text) > Convert.ToDecimal(priceText))

                    {
                        e.SubItem.ForeColor = Color.IndianRed;
                    }
                    else
                    {
                        e.SubItem.ForeColor = Color.OliveDrab;
                    }
                }
                if (e.ColumnIndex == 15) // colour code
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        int colorIndex = text[0] - '0';
                        if (colorIndex != 9)
                        {
                            using Brush backgroundBrush = new SolidBrush(colorCodes[colorIndex - 1]);
                            e.Graphics.FillRectangle(backgroundBrush, e.SubItem.Bounds);
                            e.SubItem.ForeColor = colorCodes[colorIndex - 1];
                        }
                    }
                }
                TextRenderer.DrawText(e.Graphics, text, font, bounds, e.SubItem.ForeColor, TextFormatFlags.Left);
            }
            catch (Exception ex)
            {
                HandleException(ex, "listViewTransactions_DrawSubItem");
            }
        }

        #endregion

        #region scroll listview

        private void VScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            try
            {
                if (e.NewValue >= 0 && e.NewValue < listViewTransactions.Items.Count)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.TopItem = listViewTransactions.Items[e.NewValue];
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "VScrollBar1_Scroll");
            }
        }

        private void ListViewTransactions_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                vScrollBar1.Value = listViewTransactions.Items.IndexOf(listViewTransactions.TopItem);
            }
            catch (Exception ex)
            {
                HandleException(ex, "ListViewTransactions_SelectedIndexChanged");
            }
        }

        #endregion

        #region toggle reverse listview order

        private void BtnListReverse_Click(object sender, EventArgs e)
        {
            try
            {
                System.Windows.Forms.ListView listView = listViewTransactions;
                listView.BeginUpdate();

                List<ListViewItem> items = new();
                items.AddRange(listView.Items.Cast<ListViewItem>());
                items.Reverse();

                listView.Items.Clear();
                listView.Items.AddRange(items.ToArray());

                listView.EndUpdate();
                if (btnListReverse.Text == "Newest first")
                {
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Showing newest transactions first.");
                    btnListReverse.Invoke((MethodInvoker)delegate
                    {
                        btnListReverse.Text = "Oldest first";
                    });
                }
                else
                {
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Showing oldest transactions first.");
                    btnListReverse.Invoke((MethodInvoker)delegate
                    {
                        btnListReverse.Text = "Newest first";
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnListReverse_Click");
            }
        }

        #endregion

        #region move and expand and shrink listview section

        private void BtnExpandShrinkList_Click(object sender, EventArgs e)
        {
            try
            {
                panelTransactionsContainer.AutoScroll = false;
                listChangingSize = true;
                panelTXListOuter.Invoke((MethodInvoker)delegate
                {
                    panelTXListOuter.BringToFront();
                });
                panelScrollbarContainer.Invoke((MethodInvoker)delegate
                {
                    panelScrollbarContainer.BringToFront();
                });
                panel13.Invoke((MethodInvoker)delegate
                {
                    panel13.BringToFront();
                });
                panelTXListLowerRow.Invoke((MethodInvoker)delegate
                {
                    panelTXListLowerRow.BringToFront();
                });
                panelTXListFooter.Invoke((MethodInvoker)delegate
                {
                    panelTXListFooter.BringToFront();
                });

                panel40.Invoke((MethodInvoker)delegate
                {
                    panel40.BringToFront();
                });
                panel33.Invoke((MethodInvoker)delegate
                {
                    panel33.BringToFront();
                });
                panel39.Invoke((MethodInvoker)delegate
                {
                    panel39.BringToFront();
                });
                panel37.Invoke((MethodInvoker)delegate
                {
                    panel37.BringToFront();
                });
                panel38.Invoke((MethodInvoker)delegate
                {
                    panel38.BringToFront();
                });
                panel10.Invoke((MethodInvoker)delegate
                {
                    panel10.BringToFront();
                });

                if (btnExpandShrinkList.Text == "▲")
                { //expand the panel
                    btnExpandShrinkList.Invoke((MethodInvoker)delegate
                    {
                        btnExpandShrinkList.Enabled = false;
                        btnExpandShrinkList.Text = "▼";
                    });
                    btnExpandShrinkChart.Invoke((MethodInvoker)delegate
                    {
                        btnExpandShrinkChart.Enabled = false;
                    });
                    panelTXListOuter.Height = ((int)(690 * UIScale));
                    panelTransactionsContainer.Height = ((int)(632 * UIScale));
                    listViewTransactions.Height = ((int)(632 * UIScale));
                    panelScrollbarContainer.Height = ((int)(661 * UIScale));
                    currentLocationMovingPanel = panelTXListOuter.Location.Y;
                    StartMovingPanelUp(panelTXListOuter);
                }
                else
                { //shrink the panel

                    btnExpandShrinkList.Invoke((MethodInvoker)delegate
                    {
                        btnExpandShrinkList.Enabled = false;
                        btnExpandShrinkList.Text = "▲";
                    });
                    btnExpandShrinkChart.Invoke((MethodInvoker)delegate
                    {
                        btnExpandShrinkChart.Enabled = false;
                    });
                    currentLocationMovingPanel = panelTXListOuter.Location.Y;
                    StartMovingPanelDown(panelTXListOuter);

                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnExpandShrinkList_Click");
            }
        }

        private void PanelTXListOuter_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (listChangingSize)
                {
                    panelScrollbarContainer.Invoke((MethodInvoker)delegate
                    {
                        panelScrollbarContainer.Location = new Point(panelScrollbarContainer.Location.X, panelTXListOuter.Location.Y + 1);
                        panelScrollbarContainer.Height = panelTransactionsContainer.Height + (int)(29 * UIScale);
                        panelScrollbarContainer.Invalidate();
                    });
                    vScrollBar1.Invoke((MethodInvoker)delegate
                    {
                        vScrollBar1.Height = panelScrollbarContainer.Height - (int)(29 * UIScale);
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "PanelTXListOuter_Paint");
            }
        }

        #endregion

        #endregion

        #region reset (delete all transactions)

        private void BtnResetAll_Click(object sender, EventArgs e)
        {
            try
            {
                BtnCloseSettings_Click(sender, e);
                DisableEverythingForPopupPanel();

                HideAnyRobotMessageThatsPresent();

                lblResetText.Invoke((MethodInvoker)delegate
                {
                    lblResetText.Text = "Be careful! You are about to delete your entire transaction history and start afresh.\r\nAre you sure you want to do this?";
                });
                btnResetAll1.Invoke((MethodInvoker)delegate
                {
                    btnResetAll1.Text = "Yes";
                });
                panelSpeechBorder1.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder1.Location = new Point(panelResetContainer.Location.X - 1, panelResetContainer.Location.Y - 1);
                    panelSpeechBorder1.Height = panelResetContainer.Height - (int)(22 * UIScale);
                    panelSpeechBorder1.Width = panelResetContainer.Width + 2;
                    panelSpeechBorder1.BringToFront();
                });
                panelSpeechBorder2.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder2.Location = new Point(panelResetContainer.Location.X - 1, panelResetContainer.Location.Y - 1);
                    panelSpeechBorder2.Height = panelResetContainer.Height + 2;
                    panelSpeechBorder2.Width = (int)(50 * UIScale);
                    panelSpeechBorder2.BringToFront();
                });
                panelSpeechBorder3.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder3.Location = new Point(panelResetContainer.Location.X - 1, (int)(591 * UIScale));
                    panelSpeechBorder3.Height = (int)(21 * UIScale);
                    panelSpeechBorder3.Width = panelResetContainer.Width + 2;
                    panelSpeechBorder3.BringToFront();
                });
                panelResetContainer.Invoke((MethodInvoker)delegate
                {
                    panelResetContainer.Visible = true;
                    panelResetContainer.BringToFront();
                });
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = false;
                });
                ShowSpeechBubbleHelpTextOutline();
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnResetAll_Click");
            }
        }

        private void BtnDontReset_Click(object sender, EventArgs e)
        {
            try
            {
                EnableEverythingAfterPopupPanel();
                HideSpeechBubbleHelpTextOutline();
                panelHelpTextContainer.Invoke((MethodInvoker)delegate
                {
                    panelResetContainer.Visible = false;
                });
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = true;
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnDontReset_Click");
            }
        }

        private async void BtnResetAll1_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnResetAll1.Text == "Yes")
                {
                    lblResetText.Invoke((MethodInvoker)delegate
                    {
                        lblResetText.Text = "Be careful! You are about to delete your entire transaction history and start afresh.\r\nAre you ABSOLUTELY sure you want to do this?";
                    });
                    btnResetAll1.Invoke((MethodInvoker)delegate
                    {
                        btnResetAll1.Text = "Yes!";
                    });
                    return;
                }
                if (btnResetAll1.Text == "Yes!")
                {
                    try
                    {
                        DeleteAllTransactionsFromJsonFile();
                        await SetupTransactionsList();
                        if (listViewTransactions.Items.Count == 0) // check rows were actually deleted
                        {
                            panelWelcome.Visible = false;
                            panelRobotSpeakOuter.Visible = true;
                            InterruptAndStartNewRobotSpeak("All transactions deleted.");
                            TXDataToDelete = "";
                            BtnCancelDelete_Click(sender, e); // revert buttons back to original state


                            btnDeleteTransaction.Invoke((MethodInvoker)delegate
                            {
                                btnDeleteTransaction.Enabled = false;
                                btnDeleteTransaction.BackColor = Color.FromArgb(234, 234, 234);
                            });
                            lblDisabledDeleteButtonText.Invoke((MethodInvoker)delegate
                            {
                                lblDisabledDeleteButtonText.Visible = true;
                                lblDisabledDeleteButtonText.BringToFront();
                            });
                            EnableEverythingAfterPopupPanel();
                            HideSpeechBubbleHelpTextOutline();
                            panelHelpTextContainer.Invoke((MethodInvoker)delegate
                            {
                                panelResetContainer.Visible = false;
                            });
                            panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                            {
                                panelHideSpeechTriangle.Visible = true;
                            });
                        }
                        else
                        {
                            panelWelcome.Visible = false;
                            panelRobotSpeakOuter.Visible = true;
                            InterruptAndStartNewRobotSpeak("There was a problem deleting your transactions.");
                        }

                    }
                    catch (Exception ex)
                    {
                        HandleException(ex, "BtnResetAll1_Click");
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnResetAll1_Click");
            }
        }

        #endregion

        #region chart - price linear and log

        private void InitializeChart()
        {
            try
            {
                formsPlot1.Plot.Margins(x: .1, y: .1);
                formsPlot1.Plot.Style(ScottPlot.Style.Light1);
                formsPlot1.RightClicked -= formsPlot1.DefaultRightClickEvent; // disable default right-click event
                formsPlot1.Configuration.DoubleClickBenchmark = false;
                formsPlot1.Plot.Palette = ScottPlot.Palette.Amber;
                formsPlot1.Plot.YAxis.AxisLabel.IsVisible = false;

                formsPlot1.Plot.Style(
                    figureBackground: Color.Transparent,
                    dataBackground: Color.White,
                    titleLabel: Color.Black,
                    axisLabel: Color.DimGray);

                Color newGridlineColor = Color.FromArgb(235, 235, 235);
                formsPlot1.Plot.Style(grid: newGridlineColor);
                formsPlot1.Refresh();
            }
            catch (Exception ex)
            {
                HandleException(ex, "InitializeChart");
            }
        }

        private void DrawPriceChart()
        {
            try
            {
                InitializeChart();
                if (btnPriceChartScaleLinear.Enabled == false) //Linear chart selected
                {
                    DrawPriceChartLinear();
                }
                else //Log chart selected
                {
                    DrawPriceChartLog();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "DrawPriceChart");
            }
        }

        private void BtnPriceChartScaleLinear_Click(object sender, EventArgs e)
        {
            try
            {
                btnPriceChartScaleLinear.Enabled = false;
                btnPriceChartScaleLog.Enabled = true;
                DrawPriceChart();
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnPriceChartScaleLinear_Click");
            }
        }

        private void BtnPriceChartScaleLog_Click(object sender, EventArgs e)
        {
            try
            {
                btnPriceChartScaleLinear.Enabled = true;
                btnPriceChartScaleLog.Enabled = false;
                DrawPriceChart();
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnPriceChartScaleLog_Click");
            }
        }

        private void DrawPriceChartLinear()
        {
            try
            {
                chartFinishedRendering = false;
                btnPriceChartScaleLinear.Enabled = false;
                btnPriceChartScaleLog.Enabled = true;
                chartType = "price";

                // clear any previous graph
                formsPlot1.Plot.Clear();
                formsPlot1.Plot.Title("", size: 8, bold: true);
                formsPlot1.Plot.YAxis.Label("Price (" + selectedCurrencyName + ")", size: 12, bold: false);
                // switch to linear scaling in case it was log before
                formsPlot1.Plot.YAxis.MinorLogScale(false);
                formsPlot1.Plot.YAxis.MajorGrid(false);
                formsPlot1.Plot.YAxis.MinorGrid(false);

                // Define a new tick label formatter for the linear scale
                static string linearTickLabels(double y) => y.ToString("N0");
                formsPlot1.Plot.YAxis.TickLabelFormat(linearTickLabels);

                // Revert back to automatic data area
                formsPlot1.Plot.ResetLayout();
                formsPlot1.Plot.AxisAuto();

                #region transaction (buy) bubbles

                if (showBuyBubbles)
                {
                    if (listBuyBTCTransactionDate.Count > 0)
                    {
                        //dates
                        double[] xArrayBuyBTCTransactionsDate = listBuyBTCTransactionDate.ToArray();
                        //prices
                        double[] yArrayBuyBTCTransactionsPrice = listBuyBTCTransactionPrice.ToArray();
                        //fiat amounts
                        double[] arrayBuyBTCTransactionsFiatAmount = listBuyBTCTransactionFiatAmount.ToArray();
                        //percentage values of fiat amounts
                        double totalSum = arrayBuyBTCTransactionsFiatAmount.Sum();
                        double[] arrayBuyBTCTransactionsFiatAmountPerc = new double[arrayBuyBTCTransactionsFiatAmount.Length];
                        for (int i = 0; i < arrayBuyBTCTransactionsFiatAmount.Length; i++)
                        {
                            arrayBuyBTCTransactionsFiatAmountPerc[i] = (arrayBuyBTCTransactionsFiatAmount[i] / totalSum) * 100.0;
                        }
                        //scaled percentages of fiat amounts
                        double[] scaledPercentages = new double[arrayBuyBTCTransactionsFiatAmountPerc.Length];
                        double scalingFactor = 0.60; // lower number = more scaling

                        for (int i = 0; i < arrayBuyBTCTransactionsFiatAmountPerc.Length; i++)
                        {
                            scaledPercentages[i] = (50.0 + (arrayBuyBTCTransactionsFiatAmountPerc[i] - 50.0) * scalingFactor) / 2;
                            // Make sure the scaled percentage is within the range [0, 100]
                            scaledPercentages[i] = Math.Max(0, Math.Min(100, scaledPercentages[i]));
                        }

                        bubbleplotbuy = formsPlot1.Plot.AddBubblePlot();
                        for (int i = 0; i < xArrayBuyBTCTransactionsDate.Length; i++)
                        {
                            double bubbleSize = Math.Abs(scaledPercentages[i]);
                            Color bubbleColor = Color.FromArgb(70, Color.OliveDrab);

                            bubbleplotbuy.Add(
                                x: xArrayBuyBTCTransactionsDate[i],
                                y: yArrayBuyBTCTransactionsPrice[i],
                                radius: bubbleSize,
                                fillColor: bubbleColor,
                                edgeColor: Color.Transparent,
                                edgeWidth: 1
                            );
                        }
                    }
                }

                #endregion

                #region transaction (sell) bubbles

                if (showSellBubbles)
                {
                    if (listSellBTCTransactionDate.Count > 0)
                    {
                        //dates
                        double[] xArraySellBTCTransactionsDate = listSellBTCTransactionDate.ToArray();
                        //prices
                        double[] yArraySellBTCTransactionsPrice = listSellBTCTransactionPrice.ToArray();
                        //fiat amounts
                        double[] arraySellBTCTransactionsFiatAmount = listSellBTCTransactionFiatAmount.ToArray();
                        //percentage values of fiat amounts
                        double totalSum = arraySellBTCTransactionsFiatAmount.Sum();
                        double[] arraySellBTCTransactionsFiatAmountPerc = new double[arraySellBTCTransactionsFiatAmount.Length];
                        for (int i = 0; i < arraySellBTCTransactionsFiatAmount.Length; i++)
                        {
                            arraySellBTCTransactionsFiatAmountPerc[i] = (arraySellBTCTransactionsFiatAmount[i] / totalSum) * 100.0;
                        }
                        //scaled percentages of fiat amounts
                        double[] scaledPercentages = new double[arraySellBTCTransactionsFiatAmountPerc.Length];
                        double scalingFactor = 0.60; // lower number = more scaling

                        for (int i = 0; i < arraySellBTCTransactionsFiatAmountPerc.Length; i++)
                        {
                            scaledPercentages[i] = (50.0 + (arraySellBTCTransactionsFiatAmountPerc[i] - 50.0) * scalingFactor) / 2;
                            // Make sure the scaled percentage is within the range [0, 100]
                            scaledPercentages[i] = Math.Max(0, Math.Min(100, scaledPercentages[i]));
                        }

                        bubbleplotsell = formsPlot1.Plot.AddBubblePlot();
                        for (int i = 0; i < xArraySellBTCTransactionsDate.Length; i++)
                        {
                            double bubbleSize = Math.Abs(scaledPercentages[i]);
                            Color bubbleColor = Color.FromArgb(70, Color.IndianRed);

                            bubbleplotsell.Add(
                                x: xArraySellBTCTransactionsDate[i],
                                y: yArraySellBTCTransactionsPrice[i],
                                radius: bubbleSize,
                                fillColor: bubbleColor,
                                edgeColor: Color.Transparent,
                                edgeWidth: 1
                            );
                        }
                    }
                }

                #endregion

                #region green vertical lines at the time of each buy btc transaction

                if (showBuyDates)
                {
                    if (listBuyBTCTransactionDate.Count > 0)
                    {
                        double[] xArrayBuyBTCTransactionsDate = listBuyBTCTransactionDate.ToArray(); //date
                        var vlinesBuyBTC = new VLineVector
                        {
                            Xs = xArrayBuyBTCTransactionsDate,
                            Color = Color.FromArgb(85, Color.ForestGreen),
                            PositionLabel = false
                        };
                        vlinesBuyBTC.PositionLabelBackground = vlinesBuyBTC.Color;
                        vlinesBuyBTC.LineWidth = 1;
                        formsPlot1.Plot.Add(vlinesBuyBTC);
                    }
                }

                #endregion

                #region red vertical lines at the time of each sell btc transaction. 

                if (showSellDates)
                {
                    if (listSellBTCTransactionDate.Count > 0)
                    {
                        double[] xArraySellBTCTransactions = listSellBTCTransactionDate.ToArray(); //date
                        var vlinesSellBTC = new VLineVector
                        {
                            Xs = xArraySellBTCTransactions,
                            Color = Color.FromArgb(85, Color.IndianRed),
                            PositionLabel = false
                        };
                        vlinesSellBTC.PositionLabelBackground = vlinesSellBTC.Color;
                        vlinesSellBTC.LineWidth = 1;
                        formsPlot1.Plot.Add(vlinesSellBTC);
                    }
                }

                #endregion

                #region price line

                int pointCount;
                if (GotHistoricPrices == true)
                {
                    pointCount = HistoricPrices.Count;
                }

                // create arrays of doubles of the prices and the dates
                double[] yValues = HistoricPrices.Select(h => (double)(h.Y)).ToArray();

                // create a new list of the dates, this time in DateTime format
                List<DateTime> dateTimes = HistoricPrices.Select(h => DateTimeOffset.FromUnixTimeSeconds(long.Parse(h.X!)).LocalDateTime).ToList();
                double[] xValues = dateTimes.Select(x => x.ToOADate()).ToArray();

                formsPlot1.Plot.SetAxisLimits(xValues.Min(), xValues.Max(), 0, yValues.Max() * 1.05);

                scatter = formsPlot1.Plot.AddScatter(xValues, yValues, color: Color.Orange, lineWidth: 1, markerSize: 1);

                #endregion

                #region cost basis horizontal line

                if (listViewTransactions.Items.Count > 0)
                {
                    if (showCostBasis)
                    {
                        #region horizontal cost basis line
                        if (double.TryParse(lblFinalCostBasis.Text[1..], out double costBasis))
                        {
                            if (costBasis > 0)
                            {
                                var hline = formsPlot1.Plot.AddHorizontalLine(y: costBasis, color: Color.FromArgb(213, 167, 239), width: 1, style: LineStyle.Dash);
                                hline.PositionLabel = false;
                                hline.DragEnabled = false;

                                var CBline = formsPlot1.Plot.AddText("Cost basis: " + Convert.ToString(costBasis), x: xValues.Min(), y: costBasis, color: Color.FromArgb(255, 224, 192));
                                CBline.Font.Color = Color.DimGray;
                                CBline.BackgroundColor = Color.FromArgb(255, 224, 192);
                                CBline.BackgroundFill = true;
                                CBline.BorderSize = 0;
                            }
                            else
                            {
                                // don't draw the line because it's off the range (negative) of the chart.
                            }
                        }
                        #endregion

                        #region cost basis history stepped line
                        if (listCostBasisAmounts.Count > 0)
                        {

                            #region add an extra record with todays date and the last cost basis. This forces the cost basis line to the end of the chart
                            double currentDate = DateTime.Now.ToOADate();
                            double lastDateInList = listTransactionDate.Last();

                            if (lastDateInList != currentDate) // if there isn't already a transaction on the current date
                            {
                                listTransactionDate.Add(currentDate);
                                // Get the last value from yArrayCostBasisPrice
                                double lastValue = listCostBasisAmounts.Last();
                                listCostBasisAmounts.Add(lastValue);
                            }
                            #endregion

                            #region sort the dates array bubt keep the costbasis array in sync with it
                            double[] xArrayCostBasisDate = listTransactionDate.ToArray(); //date
                            double[] yArrayCostBasisPrice = listCostBasisAmounts.ToArray(); //cost basis




                            // Create a list of pairs (date, price)
                            var pairs = xArrayCostBasisDate
                                .Select((x, index) => new KeyValuePair<double, double>(x, yArrayCostBasisPrice[index]))
                                .ToList();

                            // Sort the list by date (ascending order)
                            pairs.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key));

                            // Update the arrays based on the sorted list of pairs
                            for (int i = 0; i < pairs.Count; i++)
                            {
                                xArrayCostBasisDate[i] = pairs[i].Key;
                                yArrayCostBasisPrice[i] = pairs[i].Value;
                            }

                            #endregion
                            scatter2 = formsPlot1.Plot.AddScatter(xArrayCostBasisDate, yArrayCostBasisPrice, color: Color.FromArgb(213, 167, 239), lineWidth: 2, markerSize: 1);
                            scatter2.StepDisplay = true;
                        }
                        #endregion
                    }
                }

                #endregion

                formsPlot1.Plot.XAxis.DateTimeFormat(true);
                formsPlot1.Plot.XAxis.TickLabelStyle(fontSize: 10);
                formsPlot1.Plot.XAxis.Ticks(true);
                formsPlot1.Plot.XAxis.Label("");

                // prevent navigating beyond the data
                formsPlot1.Plot.YAxis.SetBoundary(0, yValues.Max());
                formsPlot1.Plot.XAxis.SetBoundary(xValues.Min(), xValues.Max());

                // Add a red circle we can move around later as a highlighted point indicator
                HighlightedPoint = formsPlot1.Plot.AddPoint(0, 0);
                HighlightedPoint.Color = Color.Red;
                HighlightedPoint.MarkerSize = 10;
                HighlightedPoint.MarkerShape = ScottPlot.MarkerShape.openCircle;
                HighlightedPoint.IsVisible = false;

                formsPlot1.Plot.XAxis.Ticks(true);
                formsPlot1.Plot.YAxis.Ticks(true);
                if (showDateGridLines)
                {
                    formsPlot1.Plot.XAxis.MajorGrid(true);
                }
                else
                {
                    formsPlot1.Plot.XAxis.MajorGrid(false);
                }
                if (showPriceGridLines)
                {
                    formsPlot1.Plot.YAxis.MajorGrid(true);
                }
                else
                {
                    formsPlot1.Plot.YAxis.MajorGrid(false);
                }
                if (UIScale == 1 || UIScale == 1.25 || UIScale == 1.5)
                {
                    formsPlot1.Height = (int)(panelChartContainer.Height - (panelChartContainer.Height / 10));
                }
                else
                {
                    formsPlot1.Height = (int)(panelChartContainer.Height - (panelChartContainer.Height / 8));
                }
                formsPlot1.Top = panel24.Top + panel24.Height;
                formsPlot1.Width = (int)(panelChartContainer.Width * 0.99);

                // refresh the graph
                formsPlot1.Refresh();
                chartFinishedRendering = true;
                formsPlot1.Visible = true;
            }
            catch (Exception ex)
            {
                HandleException(ex, "Generating price chart");
            }
        }

        private void DrawPriceChartLog()
        {
            try
            {
                chartFinishedRendering = false;
                btnPriceChartScaleLinear.Enabled = true;
                btnPriceChartScaleLog.Enabled = false;
                chartType = "pricelog";

                // clear any previous graph
                formsPlot1.Plot.Clear();
                formsPlot1.Plot.Title("", size: 8, bold: true);
                formsPlot1.Plot.YAxis.Label("Price (" + selectedCurrencyName + ")", size: 12, bold: false);

                int pointCount;
                if (GotHistoricPrices == true)
                {
                    pointCount = HistoricPrices.Count;
                }
                // create a new list of the dates, this time in DateTime format
                List<DateTime> dateTimes = HistoricPrices.Select(h => DateTimeOffset.FromUnixTimeSeconds(long.Parse(h.X!)).LocalDateTime).ToList();
                double[] xValues = dateTimes.Select(x => x.ToOADate()).ToArray();

                // Use a custom formatter to control the label for each tick mark
                static string logTickLabels(double y) => Math.Pow(10, y).ToString("N0");
                formsPlot1.Plot.YAxis.TickLabelFormat(logTickLabels);

                if (showPriceGridLines)
                {
                    // Use log-spaced minor tick marks and grid lines
                    formsPlot1.Plot.YAxis.MinorLogScale(true);
                    formsPlot1.Plot.YAxis.MajorGrid(true);
                    formsPlot1.Plot.YAxis.MinorGrid(true);
                }
                else
                {
                    formsPlot1.Plot.YAxis.MinorLogScale(true);
                    formsPlot1.Plot.YAxis.MajorGrid(false);
                    formsPlot1.Plot.YAxis.MinorGrid(false);
                }

                if (showDateGridLines)
                {
                    formsPlot1.Plot.XAxis.MajorGrid(true);
                }
                else
                {
                    formsPlot1.Plot.XAxis.MajorGrid(false);
                }
                formsPlot1.Plot.XAxis.DateTimeFormat(true);
                formsPlot1.Plot.XAxis.TickLabelStyle(fontSize: 10);
                formsPlot1.Plot.XAxis.Ticks(true);
                formsPlot1.Plot.XAxis.Label("");

                #region green vertical lines at the time of each buy btc transaction

                if (showBuyDates)
                {
                    if (listBuyBTCTransactionDate.Count > 0)
                    {
                        double[] xArrayBuyBTCTransactionsDate = listBuyBTCTransactionDate.ToArray(); //date
                        var vlinesBuyBTC = new VLineVector
                        {
                            Xs = xArrayBuyBTCTransactionsDate,
                            Color = Color.FromArgb(85, Color.ForestGreen),
                            PositionLabel = false
                        };
                        vlinesBuyBTC.PositionLabelBackground = vlinesBuyBTC.Color;
                        vlinesBuyBTC.LineWidth = 1;
                        formsPlot1.Plot.Add(vlinesBuyBTC);
                    }
                }

                #endregion

                #region red vertical lines at the time of each sell btc transaction. 

                if (showSellDates)
                {
                    if (listSellBTCTransactionDate.Count > 0)
                    {
                        double[] xArraySellBTCTransactions = listSellBTCTransactionDate.ToArray(); //date
                        var vlinesSellBTC = new VLineVector
                        {
                            Xs = xArraySellBTCTransactions,
                            Color = Color.FromArgb(85, Color.IndianRed),
                            PositionLabel = false
                        };
                        vlinesSellBTC.PositionLabelBackground = vlinesSellBTC.Color;
                        vlinesSellBTC.LineWidth = 1;
                        formsPlot1.Plot.Add(vlinesSellBTC);
                    }
                }

                #endregion

                #region transaction (buy) bubbles

                if (showBuyBubbles)
                {
                    if (listBuyBTCTransactionDate.Count > 0)
                    {
                        //dates
                        double[] xArrayBuyBTCTransactionsDate = listBuyBTCTransactionDate.ToArray();
                        //prices
                        double[] yArrayBuyBTCTransactionsPrice = listBuyBTCTransactionPrice.ToArray();
                        //fiat amounts
                        double[] arrayBuyBTCTransactionsFiatAmount = listBuyBTCTransactionFiatAmount.ToArray();


                        // if any of the values are 0, change them to near-zero so that they can be plotted on the chart
                        for (int i = 0; i < yArrayBuyBTCTransactionsPrice.Length; i++)
                        {
                            if (yArrayBuyBTCTransactionsPrice[i] == 0)
                            {
                                yArrayBuyBTCTransactionsPrice[i] = 0.01;
                            }

                            if (arrayBuyBTCTransactionsFiatAmount[i] == 0)
                            {
                                arrayBuyBTCTransactionsFiatAmount[i] = 0.01;
                            }
                        }



                        //percentage values of fiat amounts
                        double totalSum = arrayBuyBTCTransactionsFiatAmount.Sum();
                        double[] arrayBuyBTCTransactionsFiatAmountPerc = new double[arrayBuyBTCTransactionsFiatAmount.Length];
                        for (int i = 0; i < arrayBuyBTCTransactionsFiatAmount.Length; i++)
                        {
                            arrayBuyBTCTransactionsFiatAmountPerc[i] = (arrayBuyBTCTransactionsFiatAmount[i] / totalSum) * 100.0;
                        }
                        //scaled percentages of fiat amounts
                        double[] scaledPercentages = new double[arrayBuyBTCTransactionsFiatAmountPerc.Length];
                        double scalingFactor = 0.60; // lower number = more scaling

                        for (int i = 0; i < arrayBuyBTCTransactionsFiatAmountPerc.Length; i++)
                        {
                            scaledPercentages[i] = (50.0 + (arrayBuyBTCTransactionsFiatAmountPerc[i] - 50.0) * scalingFactor) / 2;
                            // Make sure the scaled percentage is within the range [0, 100]
                            scaledPercentages[i] = Math.Max(0, Math.Min(100, scaledPercentages[i]));
                        }

                        bubbleplotbuy = formsPlot1.Plot.AddBubblePlot();
                        for (int i = 0; i < xArrayBuyBTCTransactionsDate.Length; i++)
                        {
                            double bubbleSize = Math.Abs(scaledPercentages[i]);
                            Color bubbleColor = Color.FromArgb(70, Color.OliveDrab);

                            bubbleplotbuy.Add(
                                x: xArrayBuyBTCTransactionsDate[i],
                                y: Math.Log10(yArrayBuyBTCTransactionsPrice[i]),
                                radius: bubbleSize,
                                fillColor: bubbleColor,
                                edgeColor: Color.Transparent,
                                edgeWidth: 1
                            );
                        }
                    }
                }

                #endregion

                #region transaction (sell) bubbles

                if (showSellBubbles)
                {
                    if (listSellBTCTransactionDate.Count > 0)
                    {
                        //dates
                        double[] xArraySellBTCTransactionsDate = listSellBTCTransactionDate.ToArray();
                        //prices
                        double[] yArraySellBTCTransactionsPrice = listSellBTCTransactionPrice.ToArray();
                        //fiat amounts
                        double[] arraySellBTCTransactionsFiatAmount = listSellBTCTransactionFiatAmount.ToArray();

                        // if any of the values are 0, change them to near-zero so that they can be plotted on the chart
                        for (int i = 0; i < yArraySellBTCTransactionsPrice.Length; i++)
                        {
                            if (yArraySellBTCTransactionsPrice[i] == 0)
                            {
                                yArraySellBTCTransactionsPrice[i] = 0.01;
                            }

                            if (arraySellBTCTransactionsFiatAmount[i] == 0)
                            {
                                arraySellBTCTransactionsFiatAmount[i] = 0.01;
                            }
                        }


                        //percentage values of fiat amounts
                        double totalSum = arraySellBTCTransactionsFiatAmount.Sum();
                        double[] arraySellBTCTransactionsFiatAmountPerc = new double[arraySellBTCTransactionsFiatAmount.Length];
                        for (int i = 0; i < arraySellBTCTransactionsFiatAmount.Length; i++)
                        {
                            arraySellBTCTransactionsFiatAmountPerc[i] = (arraySellBTCTransactionsFiatAmount[i] / totalSum) * 100.0;
                        }
                        //scaled percentages of fiat amounts
                        double[] scaledPercentages = new double[arraySellBTCTransactionsFiatAmountPerc.Length];
                        double scalingFactor = 0.60; // lower number = more scaling

                        for (int i = 0; i < arraySellBTCTransactionsFiatAmountPerc.Length; i++)
                        {
                            scaledPercentages[i] = (50.0 + (arraySellBTCTransactionsFiatAmountPerc[i] - 50.0) * scalingFactor) / 2;
                            // Make sure the scaled percentage is within the range [0, 100]
                            scaledPercentages[i] = Math.Max(0, Math.Min(100, scaledPercentages[i]));
                        }

                        bubbleplotsell = formsPlot1.Plot.AddBubblePlot();
                        for (int i = 0; i < xArraySellBTCTransactionsDate.Length; i++)
                        {
                            double bubbleSize = Math.Abs(scaledPercentages[i]);
                            Color bubbleColor = Color.FromArgb(70, Color.IndianRed);

                            bubbleplotsell.Add(
                                x: xArraySellBTCTransactionsDate[i],
                                y: Math.Log10(yArraySellBTCTransactionsPrice[i]),
                                radius: bubbleSize,
                                fillColor: bubbleColor,
                                edgeColor: Color.Transparent,
                                edgeWidth: 1
                            );
                        }
                    }
                }

                #endregion

                #region price line

                List<double> filteredYValues = new();
                List<double> filteredXValues = new();

                for (int i = 0; i < HistoricPrices.Count; i++)
                {
                    double yValue = (double)HistoricPrices[i].Y;
                    if (yValue > 0)
                    {
                        filteredYValues.Add(Math.Log10(yValue));
                        filteredXValues.Add(xValues[i]);
                    }
                }

                double[] yValues = filteredYValues.ToArray();
                double[] xValuesFiltered = filteredXValues.ToArray();

                double minY = yValues.Min();
                double maxY = yValues.Max() * 1.05;
                formsPlot1.Plot.SetAxisLimits(xValuesFiltered.Min(), xValuesFiltered.Max(), minY, maxY);
                scatter = formsPlot1.Plot.AddScatter(xValuesFiltered, yValues, lineWidth: 1, markerSize: 1);

                #endregion

                #region cost basis horizontal line and stepped history line
                if (listViewTransactions.Items.Count > 0)
                {
                    if (showCostBasis)
                    {
                        #region current cost basis - horizontal line
                        if (double.TryParse(lblFinalCostBasis.Text[1..], out double costBasis))
                        {
                            if (costBasis > 0)
                            {
                                var hline = formsPlot1.Plot.AddHorizontalLine(y: Math.Log10(costBasis), color: Color.FromArgb(213, 167, 239), width: 1, style: LineStyle.Dash, label: "H");
                                hline.PositionLabel = false;
                                hline.DragEnabled = false;

                                var CBline = formsPlot1.Plot.AddText("Cost basis: " + Convert.ToString(costBasis), x: xValuesFiltered.Min(), y: Math.Log10(costBasis), color: Color.FromArgb(255, 224, 192));
                                CBline.Font.Color = Color.DimGray;
                                CBline.BackgroundColor = Color.FromArgb(255, 224, 192);
                                CBline.BackgroundFill = true;
                                CBline.BorderSize = 0;
                            }
                            else
                            {
                                // don't draw the line because it's off the range (negative) of the chart.
                            }

                        }
                        #endregion

                        #region cost basis history stepped line
                        if (listCostBasisAmounts.Count > 0)
                        {
                            #region add an extra record with todays date and the last cost basis. This forces the cost basis line to the end of the chart
                            double currentDate = DateTime.Now.ToOADate();
                            double lastDateInList = listTransactionDate.Last();

                            if (lastDateInList != currentDate) // if there isn't already a transaction on the current date
                            {
                                listTransactionDate.Add(currentDate);
                                // Get the last value from yArrayCostBasisPrice
                                double lastValue = listCostBasisAmounts.Last();
                                listCostBasisAmounts.Add(lastValue);
                            }
                            #endregion

                            #region sort the dates array bubt keep the costbasis array in sync with it
                            double[] xArrayCostBasisDate = listTransactionDate.ToArray(); //date
                            double[] yArrayCostBasisPrice = listCostBasisAmounts.ToArray(); //cost basis

                            // Create a list of pairs (date, price)
                            var pairs = xArrayCostBasisDate
                                .Select((x, index) => new KeyValuePair<double, double>(x, yArrayCostBasisPrice[index]))
                                .ToList();

                            // Sort the list by date (ascending order)
                            pairs.Sort((pair1, pair2) => pair1.Key.CompareTo(pair2.Key));

                            // Update the arrays based on the sorted list of pairs
                            for (int i = 0; i < pairs.Count; i++)
                            {
                                xArrayCostBasisDate[i] = pairs[i].Key;
                                if (pairs[i].Value == 0)
                                {
                                    yArrayCostBasisPrice[i] = 0;
                                }
                                else
                                {
                                    yArrayCostBasisPrice[i] = Math.Log10(pairs[i].Value);
                                }
                            }
                            #endregion
                            scatter2 = formsPlot1.Plot.AddScatter(xArrayCostBasisDate, yArrayCostBasisPrice, color: Color.FromArgb(213, 167, 239), lineWidth: 2, markerSize: 1);
                            scatter2.StepDisplay = true;
                        }
                        #endregion
                    }
                }

                #endregion

                // prevent navigating beyond the data
                formsPlot1.Plot.YAxis.SetBoundary(minY, maxY);
                formsPlot1.Plot.XAxis.SetBoundary(xValues.Min(), xValues.Max());

                // Add a red circle we can move around later as a highlighted point indicator
                HighlightedPoint = formsPlot1.Plot.AddPoint(0, 0);
                HighlightedPoint.Color = Color.Red;
                HighlightedPoint.MarkerSize = 10;
                HighlightedPoint.MarkerShape = ScottPlot.MarkerShape.openCircle;
                HighlightedPoint.IsVisible = false;
                //int temp = (int)(panelChartContainer.Location.Y + (panelChartContainer.Location.Y / 2));
                formsPlot1.Top = panel24.Top + panel24.Height;
                formsPlot1.Width = (int)(panelChartContainer.Width * 0.99);
                if (UIScale == 1 || UIScale == 1.25 || UIScale == 1.5)
                {
                    formsPlot1.Height = (int)(panelChartContainer.Height - (panelChartContainer.Height / 10));
                }
                else
                {
                    formsPlot1.Height = (int)(panelChartContainer.Height - (panelChartContainer.Height / 8));
                }
                // refresh the graph
                formsPlot1.Refresh();
                chartFinishedRendering = true;
                formsPlot1.Visible = true;
            }
            catch (Exception ex)
            {
                HandleException(ex, "Generating price (log) chart");
            }
        }

        private void FormsPlot1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (chartFinishedRendering)
                {
                    if (!cursorTrackNothing)
                    {
                        // determine point nearest the cursor
                        (double mouseCoordX, double mouseCoordY) = formsPlot1.GetMouseCoordinates();
                        double xyRatio = formsPlot1.Plot.XAxis.Dims.PxPerUnit / formsPlot1.Plot.YAxis.Dims.PxPerUnit;

                        double pointX = 0;
                        double pointY = 0;
                        int pointIndex = 0;
                        if (cursorTrackPrice)
                        {
                            if (safeToTrackPriceOnChart)
                            {
                                if (scatter != null)
                                {
                                    (pointX, pointY, pointIndex) = scatter.GetPointNearest(mouseCoordX, mouseCoordY, xyRatio);
                                }
                            }
                        }
                        if (cursorTrackBuyTX)
                        {
                            if (listBuyBTCTransactionDate.Count >= 1)
                            {
                                (pointX, pointY, pointIndex) = bubbleplotbuy.GetPointNearest(mouseCoordX, mouseCoordY, xyRatio);
                            }
                        }
                        if (cursorTrackSellTX)
                        {
                            if (listSellBTCTransactionDate.Count >= 1)
                            {
                                (pointX, pointY, pointIndex) = bubbleplotsell.GetPointNearest(mouseCoordX, mouseCoordY, xyRatio);
                            }
                        }

                        // place the highlight over the point of interest
                        HighlightedPoint.X = pointX;
                        HighlightedPoint.Y = pointY;
                        HighlightedPoint.IsVisible = true;

                        // render if the highlighted point changed
                        if (LastHighlightedIndex != pointIndex)
                        {
                            LastHighlightedIndex = pointIndex;
                            // Convert pointX to a DateTime object
                            DateTime pointXDate = DateTime.FromOADate(pointX);

                            // Format the DateTime object using the desired format string
                            string formattedPointX = pointXDate.ToString("yyyy-MM-dd");

                            if (chartType == "pricelog")
                            {
                                double originalY = Math.Pow(10, pointY); // Convert back to the original scale
                                //annotation to obscure the previous one before drawing the new one
                                var blankAnnotation = formsPlot1.Plot.AddAnnotation("████████████████████", Alignment.UpperLeft);
                                blankAnnotation.Shadow = false;
                                blankAnnotation.BorderWidth = 0;
                                blankAnnotation.BorderColor = Color.White;
                                blankAnnotation.MarginX = 2;
                                blankAnnotation.MarginY = 2;
                                blankAnnotation.Font.Color = Color.White;
                                blankAnnotation.BackgroundColor = Color.White;

                                var actualAnnotation = formsPlot1.Plot.AddAnnotation($"{originalY:N2} ({formattedPointX})", Alignment.UpperLeft);
                                actualAnnotation.Font.Name = "Consolas";
                                actualAnnotation.Shadow = false;
                                actualAnnotation.BorderWidth = 0;
                                actualAnnotation.BorderColor = Color.White;
                                actualAnnotation.MarginX = 2;
                                actualAnnotation.MarginY = 2;
                                actualAnnotation.Font.Color = Color.DimGray;
                                actualAnnotation.BackgroundColor = Color.White;
                            }
                            else
                            {
                                //annotation to obscure the previous one before drawing the new one
                                var blankAnnotation = formsPlot1.Plot.AddAnnotation("████████████████████████████████████", Alignment.UpperLeft);
                                blankAnnotation.Shadow = false;
                                blankAnnotation.BorderWidth = 0;
                                blankAnnotation.BorderColor = Color.White;
                                blankAnnotation.MarginX = 2;
                                blankAnnotation.MarginY = 2;
                                blankAnnotation.Font.Color = Color.White;
                                blankAnnotation.BackgroundColor = Color.White;

                                //new annotation
                                var actualAnnotation = formsPlot1.Plot.AddAnnotation($"Price:{pointY:N2} | Date:{formattedPointX}", Alignment.UpperLeft);
                                actualAnnotation.Font.Name = "Consolas";
                                actualAnnotation.Shadow = false;
                                actualAnnotation.BorderWidth = 0;
                                actualAnnotation.BorderColor = Color.White;
                                actualAnnotation.MarginX = 2;
                                actualAnnotation.MarginY = 2;
                                actualAnnotation.Font.Color = Color.DimGray;
                                actualAnnotation.BackgroundColor = Color.White;
                            }
                            formsPlot1.Render();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "rendering mouse-over chart coordinates data");
            }
        }

        #region expand and shrink chart area
        private void BtnExpandShrinkChart_Click(object sender, EventArgs e)
        {
            try
            {
                chartChangingSize = true;
                if (btnExpandShrinkChart.Text == "▼")
                { //expand the panel
                    btnExpandShrinkChart.Invoke((MethodInvoker)delegate
                    {
                        btnExpandShrinkChart.Enabled = false;
                        btnExpandShrinkChart.Text = "▲";
                    });
                    btnExpandShrinkList.Invoke((MethodInvoker)delegate
                    {
                        btnExpandShrinkList.Enabled = false;
                    });
                    panelChartOuter.Invoke((MethodInvoker)delegate
                    {
                        panelChartOuter.BringToFront();
                    });
                    currentHeightExpandingPanel = panelChartOuter.Height;
                    StartExpandingPanelVert(panelChartOuter);
                }
                else
                { //shrink the panel
                    btnExpandShrinkChart.Invoke((MethodInvoker)delegate
                    {
                        btnExpandShrinkChart.Enabled = false;
                        btnExpandShrinkChart.Text = "▼";
                    });
                    btnExpandShrinkList.Invoke((MethodInvoker)delegate
                    {
                        btnExpandShrinkList.Enabled = false;
                    });
                    panelChartOuter.Invoke((MethodInvoker)delegate
                    {
                        panelChartOuter.BringToFront();
                    });
                    currentHeightShrinkingPanel = panelChartOuter.Height;
                    StartShrinkingPanelVert(panelChartOuter);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnExpandShrinkChart_Click");
            }
        }

        private void PanelChartOuter_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                panelChartContainer.Invoke((MethodInvoker)delegate
                {
                    panelChartContainer.Height = panelChartOuter.Height - (int)(2 * UIScale);
                    panelChartContainer.Invalidate();
                });
                formsPlot1.Invoke((MethodInvoker)delegate
                {
                    //                    formsPlot1.Height = panelChartOuter.Height - (int)(27 * UIScale);
                    formsPlot1.Height = (int)(panelChartContainer.Height - (panelChartContainer.Height / 10));
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "PanelChartOuter_Paint");
            }
        }
        #endregion

        #region expand and shrink chart panels

        #region expand

        private void StartExpandingPanelHoriz(Panel panel)
        {
            try
            {
                panelToExpand = panel;
                ExpandPanelTimerHoriz.Start();
            }
            catch (Exception ex)
            {
                HandleException(ex, "StartExpandingPanelHoriz");
            }
        }

        private void ExpandPanelTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                currentWidthExpandingPanel += 6;
                if (panelToExpand == panel18)
                {
                    panelMaxWidth = (int)(212 * UIScale);
                }
                if (panelToExpand == panel21)
                {
                    panelMaxWidth = (int)(292 * UIScale);
                }
                if (panelToExpand == panel22)
                {
                    panelMaxWidth = (int)(264 * UIScale);
                }
                if (panelToExpand == panel23)
                {
                    panelMaxWidth = (int)(443 * UIScale);
                }
                if (panelToExpand == panelTransactionLabel)
                {
                    panelMaxWidth = (int)(680 * UIScale);
                    currentWidthExpandingPanel += 6; // faster for this panel
                }

                if (currentWidthExpandingPanel >= panelMaxWidth) // expanding is complete
                {
                    ExpandPanelTimerHoriz.Stop();
                }
                else // expand further
                {
                    panelToExpand.Invoke((MethodInvoker)delegate
                    {
                        panelToExpand.Width = currentWidthExpandingPanel;
                        panelToExpand.Invalidate();
                    });

                    //shift the other panels along
                    panel21.Invoke((MethodInvoker)delegate
                    {
                        panel21.Location = new Point(panel18.Location.X + panel18.Width + 8, panel21.Location.Y);
                    });
                    panel22.Invoke((MethodInvoker)delegate
                    {
                        panel22.Location = new Point(panel21.Location.X + panel21.Width + 8, panel22.Location.Y);
                    });
                    panel23.Invoke((MethodInvoker)delegate
                    {
                        panel23.Location = new Point(panel22.Location.X + panel22.Width + 8, panel23.Location.Y);
                    });
                    panel24.Invoke((MethodInvoker)delegate
                    {
                        panel24.Location = new Point(panel23.Location.X + panel23.Width + 8, panel24.Location.Y);
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "ExpandPanelTimer_Tick");
            }
        }

        #endregion

        #region shrink

        private void StartShrinkingPanel(Panel panel)
        {
            try
            {
                panelToShrink = panel;
                ShrinkPanelTimer.Start();
            }
            catch (Exception ex)
            {
                HandleException(ex, "StartShrinkingPanel");
            }
        }

        private void ShrinkOpenPanels() // used whenever any panel is expanded to close all others 
        {
            try
            {
                if (btnExpandGridlinesPanel.Text == "◀")
                {
                    currentWidthShrinkingPanel = panel18.Width;
                    btnExpandGridlinesPanel.Invoke((MethodInvoker)delegate
                    {
                        btnExpandGridlinesPanel.Text = "▶";
                    });
                    StartShrinkingPanel(panel18);
                }
                if (btnExpandDatelinesPanel.Text == "◀")
                {
                    currentWidthShrinkingPanel = panel21.Width;
                    btnExpandDatelinesPanel.Invoke((MethodInvoker)delegate
                    {
                        btnExpandDatelinesPanel.Text = "▶";
                    });
                    StartShrinkingPanel(panel21);
                }
                if (btnExpandTransactionsPanel.Text == "◀")
                {
                    currentWidthShrinkingPanel = panel22.Width;
                    btnExpandTransactionsPanel.Invoke((MethodInvoker)delegate
                    {
                        btnExpandTransactionsPanel.Text = "▶";
                    });
                    StartShrinkingPanel(panel22);
                }
                if (btnExpandTrackingPanel.Text == "◀")
                {
                    currentWidthShrinkingPanel = panel23.Width;
                    btnExpandTrackingPanel.Invoke((MethodInvoker)delegate
                    {
                        btnExpandTrackingPanel.Text = "▶";
                    });
                    StartShrinkingPanel(panel23);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "ShrinkOpenPanels");
            }
        }

        private void ShrinkPanelTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                currentWidthShrinkingPanel -= 6;
                if (panelToShrink == panel18)
                {
                    panelMinWidth = (int)(92 * UIScale);
                }
                if (panelToShrink == panel21)
                {
                    panelMinWidth = (int)(84 * UIScale);
                }
                if (panelToShrink == panel22)
                {
                    panelMinWidth = (int)(64 * UIScale);
                }
                if (panelToShrink == panel23)
                {
                    panelMinWidth = (int)(121 * UIScale);
                }
                if (panelToShrink == panelTransactionLabel)
                {
                    panelMinWidth = (int)(106 * UIScale);
                    currentWidthShrinkingPanel -= 6; // twice as fast for this panel
                }

                if (currentWidthShrinkingPanel <= panelMinWidth) // shrinking is complete
                {
                    panelToShrink.Invoke((MethodInvoker)delegate
                    {
                        panelToShrink.Width = panelMinWidth;
                        panelToShrink.Invalidate();

                        panel21.Invoke((MethodInvoker)delegate
                        {
                            panel21.Location = new Point(panel18.Location.X + panel18.Width + 8, panel21.Location.Y);
                        });
                        panel22.Invoke((MethodInvoker)delegate
                        {
                            panel22.Location = new Point(panel21.Location.X + panel21.Width + 8, panel22.Location.Y);
                        });
                        panel23.Invoke((MethodInvoker)delegate
                        {
                            panel23.Location = new Point(panel22.Location.X + panel22.Width + 8, panel23.Location.Y);
                        });
                        panel24.Invoke((MethodInvoker)delegate
                        {
                            panel24.Location = new Point(panel23.Location.X + panel23.Width + 8, panel24.Location.Y);
                        });
                    });

                    ShrinkPanelTimer.Stop();
                }
                else // shrink further
                {
                    panelToShrink.Width = currentWidthShrinkingPanel;
                    panelToShrink.Invalidate();

                    if (!ExpandPanelTimerHoriz.Enabled) // if the expand timer is running, it will handle these relocates instead
                    {
                        //shift the other panels along
                        panel21.Invoke((MethodInvoker)delegate
                        {
                            panel21.Location = new Point(panel18.Location.X + panel18.Width + 8, panel21.Location.Y);
                        });
                        panel22.Invoke((MethodInvoker)delegate
                        {
                            panel22.Location = new Point(panel21.Location.X + panel21.Width + 8, panel22.Location.Y);
                        });
                        panel23.Invoke((MethodInvoker)delegate
                        {
                            panel23.Location = new Point(panel22.Location.X + panel22.Width + 8, panel23.Location.Y);
                        });
                        panel24.Invoke((MethodInvoker)delegate
                        {
                            panel24.Location = new Point(panel23.Location.X + panel23.Width + 8, panel24.Location.Y);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "ShrinkPanelTimer_Tick");
            }
        }

        #endregion

        #region buttons to expand/shrink panels

        private void BtnExpandGridlinesPanel_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnExpandGridlinesPanel.Text == "▶")
                {
                    ShrinkOpenPanels();
                    StartExpandingPanelHoriz(panel18);
                    currentWidthExpandingPanel = panel18.Width;
                    btnExpandGridlinesPanel.Invoke((MethodInvoker)delegate
                    {
                        btnExpandGridlinesPanel.Text = "◀";
                    });
                }
                else
                {
                    StartShrinkingPanel(panel18);
                    currentWidthShrinkingPanel = panel18.Width;
                    btnExpandGridlinesPanel.Invoke((MethodInvoker)delegate
                    {
                        btnExpandGridlinesPanel.Text = "▶";
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnExpandGridlinesPanel_Click");
            }
        }

        private void BtnExpandDatelinesPanel_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnExpandDatelinesPanel.Text == "▶")
                {
                    ShrinkOpenPanels();
                    StartExpandingPanelHoriz(panel21);
                    currentWidthExpandingPanel = panel21.Width;
                    btnExpandDatelinesPanel.Invoke((MethodInvoker)delegate
                    {
                        btnExpandDatelinesPanel.Text = "◀";
                    });
                }
                else
                {
                    StartShrinkingPanel(panel21);
                    currentWidthShrinkingPanel = panel21.Width;
                    btnExpandDatelinesPanel.Invoke((MethodInvoker)delegate
                    {
                        btnExpandDatelinesPanel.Text = "▶";
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnExpandDatelinesPanel_Click");
            }
        }

        private void BtnExpandTransactionsPanel_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnExpandTransactionsPanel.Text == "▶")
                {
                    ShrinkOpenPanels();
                    StartExpandingPanelHoriz(panel22);
                    currentWidthExpandingPanel = panel22.Width;
                    btnExpandTransactionsPanel.Invoke((MethodInvoker)delegate
                    {
                        btnExpandTransactionsPanel.Text = "◀";
                    });
                }
                else
                {
                    StartShrinkingPanel(panel22);
                    currentWidthShrinkingPanel = panel22.Width;
                    btnExpandTransactionsPanel.Invoke((MethodInvoker)delegate
                    {
                        btnExpandTransactionsPanel.Text = "▶";
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnExpandTransactionsPanel_Click");
            }
        }

        private void BtnExpandTrackingPanel_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnExpandTrackingPanel.Text == "▶")
                {
                    ShrinkOpenPanels();
                    StartExpandingPanelHoriz(panel23);
                    currentWidthExpandingPanel = panel23.Width;
                    btnExpandTrackingPanel.Invoke((MethodInvoker)delegate
                    {
                        btnExpandTrackingPanel.Text = "◀";
                    });
                }
                else
                {
                    StartShrinkingPanel(panel23);
                    currentWidthShrinkingPanel = panel23.Width;
                    btnExpandTrackingPanel.Invoke((MethodInvoker)delegate
                    {
                        btnExpandTrackingPanel.Text = "▶";
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnExpandTrackingPanel_Click");
            }
        }

        #endregion

        #endregion

        #region hide/show chart elements

        private void BtnShowPriceGridlines_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnShowPriceGridlines.Text == "✔️")
                {
                    btnShowPriceGridlines.Invoke((MethodInvoker)delegate
                    {
                        btnShowPriceGridlines.Text = "✖️";
                    });
                    showPriceGridLines = false;
                    InterruptAndStartNewRobotSpeak("Price grid lines hidden.");
                    DrawPriceChart();
                }
                else
                {
                    btnShowPriceGridlines.Invoke((MethodInvoker)delegate
                    {
                        btnShowPriceGridlines.Text = "✔️";
                    });
                    showPriceGridLines = true;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Price grid lines displayed.");
                    DrawPriceChart();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnShowPriceGridlines_Click");
            }
        }

        private void BtnShowDateGridlines_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnShowDateGridlines.Text == "✔️")
                {
                    btnShowDateGridlines.Invoke((MethodInvoker)delegate
                    {
                        btnShowDateGridlines.Text = "✖️";
                    });
                    showDateGridLines = false;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Date grid lines hidden.");
                    DrawPriceChart();
                }
                else
                {
                    btnShowDateGridlines.Invoke((MethodInvoker)delegate
                    {
                        btnShowDateGridlines.Text = "✔️";
                    });
                    showDateGridLines = true;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Date grid lines displayed.");
                    DrawPriceChart();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnShowDateGridlines_Click");
            }
        }

        private void BtnShowCostBasis_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnShowCostBasis.Text == "✔️")
                {
                    btnShowCostBasis.Invoke((MethodInvoker)delegate
                    {
                        btnShowCostBasis.Text = "✖️";
                    });
                    showCostBasis = false;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Cost basis hidden.");
                    DrawPriceChart();
                }
                else
                {
                    btnShowCostBasis.Invoke((MethodInvoker)delegate
                    {
                        btnShowCostBasis.Text = "✔️";
                    });
                    showCostBasis = true;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Cost basis displayed.");
                    DrawPriceChart();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnShowCostBasis_Click");
            }
        }

        private void BtnShowBuyDates_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnShowBuyDates.Text == "✔️")
                {
                    btnShowBuyDates.Invoke((MethodInvoker)delegate
                    {
                        btnShowBuyDates.Text = "✖️";
                    });
                    showBuyDates = false;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Buy dates hidden.");
                    DrawPriceChart();
                }
                else
                {
                    btnShowBuyDates.Invoke((MethodInvoker)delegate
                    {
                        btnShowBuyDates.Text = "✔️";
                    });
                    showBuyDates = true;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Buy dates displayed.");
                    DrawPriceChart();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnShowBuyDates_Click");
            }
        }

        private void BtnShowSellDates_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnShowSellDates.Text == "✔️")
                {
                    btnShowSellDates.Invoke((MethodInvoker)delegate
                    {
                        btnShowSellDates.Text = "✖️";
                    });
                    showSellDates = false;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Sell dates hidden.");
                    DrawPriceChart();
                }
                else
                {
                    btnShowSellDates.Invoke((MethodInvoker)delegate
                    {
                        btnShowSellDates.Text = "✔️";
                    });
                    showSellDates = true;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Sell dates displayed.");
                    DrawPriceChart();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnShowSellDates_Click");
            }
        }

        private void BtnShowBuyBubbles_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnShowBuyBubbles.Text == "✔️")
                {
                    btnShowBuyBubbles.Invoke((MethodInvoker)delegate
                    {
                        btnShowBuyBubbles.Text = "✖️";
                    });
                    showBuyBubbles = false;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Buy transactions hidden.");
                    DrawPriceChart();
                }
                else
                {
                    btnShowBuyBubbles.Invoke((MethodInvoker)delegate
                    {
                        btnShowBuyBubbles.Text = "✔️";
                    });
                    showBuyBubbles = true;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Buy transactions displayed.");
                    DrawPriceChart();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnShowBuyBubbles_Click");
            }
        }

        private void BtnShowSellBubbles_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnShowSellBubbles.Text == "✔️")
                {
                    btnShowSellBubbles.Invoke((MethodInvoker)delegate
                    {
                        btnShowSellBubbles.Text = "✖️";
                    });
                    showSellBubbles = false;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Sell transactions hidden.");
                    DrawPriceChart();
                }
                else
                {
                    btnShowSellBubbles.Invoke((MethodInvoker)delegate
                    {
                        btnShowSellBubbles.Text = "✔️";
                    });
                    showSellBubbles = true;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Sell transactions displayed.");
                    DrawPriceChart();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnShowSellBubbles_Click");
            }
        }

        private void BtnCursorTrackPrice_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnCursorTrackPrice.Text == "✖️")
                {
                    DisableCursorTracking();
                    btnCursorTrackPrice.Invoke((MethodInvoker)delegate
                    {
                        btnCursorTrackPrice.Text = "✔️";
                    });
                    cursorTrackPrice = true;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Mouse cursor now tracking the price.");
                    DrawPriceChart();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnCursorTrackPrice_Click");
            }
        }

        private void BtnCursorTrackBuyTX_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnCursorTrackBuyTX.Text == "✖️")
                {
                    DisableCursorTracking();
                    btnCursorTrackBuyTX.Invoke((MethodInvoker)delegate
                    {
                        btnCursorTrackBuyTX.Text = "✔️";
                    });
                    cursorTrackBuyTX = true;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Mouse cursor now tracking transactions where you bought or received bitcoin.");
                    DrawPriceChart();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnCursorTrackBuyTX_Click");
            }
        }

        private void BtnCursorTrackSellTX_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnCursorTrackSellTX.Text == "✖️")
                {
                    DisableCursorTracking();
                    btnCursorTrackSellTX.Invoke((MethodInvoker)delegate
                    {
                        btnCursorTrackSellTX.Text = "✔️";
                    });
                    cursorTrackSellTX = true;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Mouse cursor now tracking transactions where you sold or spent bitcoin.");
                    DrawPriceChart();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnCursorTrackSellTX_Click");
            }
        }

        private void BtnCursorTrackNothing_Click(object sender, EventArgs e)
        {
            try
            {
                DisableCursorTracking();
                if (btnCursorTrackNothing.Text == "✖️")
                {
                    btnCursorTrackNothing.Invoke((MethodInvoker)delegate
                    {
                        btnCursorTrackNothing.Text = "✔️";
                    });
                    cursorTrackNothing = true;
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Mouse cursor no longer tracking any data.");
                    DrawPriceChart();
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnCursorTrackNothing_Click");
            }
        }

        private void BtnShowHideLabel_Click(object sender, EventArgs e)
        {
            try
            {
                if (btnShowHideLabel.Text == "▶")
                {
                    StartExpandingPanelHoriz(panelTransactionLabel);
                    currentWidthExpandingPanel = panelTransactionLabel.Width;
                    btnShowHideLabel.Invoke((MethodInvoker)delegate
                    {
                        btnShowHideLabel.Text = "◀";
                    });
                    lblShowHideLabel.Invoke((MethodInvoker)delegate
                    {
                        lblShowHideLabel.Text = "Hide label";
                    });
                }
                else
                {
                    StartShrinkingPanel(panelTransactionLabel);
                    currentWidthShrinkingPanel = panelTransactionLabel.Width;
                    btnShowHideLabel.Invoke((MethodInvoker)delegate
                    {
                        btnShowHideLabel.Text = "▶";
                    });
                    lblShowHideLabel.Invoke((MethodInvoker)delegate
                    {
                        lblShowHideLabel.Text = "Show label";
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnShowHideLabel_Click");
            }
        }

        private void DisableCursorTracking()
        {
            try
            {
                btnCursorTrackNothing.Invoke((MethodInvoker)delegate
                {
                    btnCursorTrackNothing.Text = "✖️";
                });
                btnCursorTrackSellTX.Invoke((MethodInvoker)delegate
                {
                    btnCursorTrackSellTX.Text = "✖️";
                });
                btnCursorTrackBuyTX.Invoke((MethodInvoker)delegate
                {
                    btnCursorTrackBuyTX.Text = "✖️";
                });
                btnCursorTrackPrice.Invoke((MethodInvoker)delegate
                {
                    btnCursorTrackPrice.Text = "✖️";
                });
                cursorTrackPrice = false;
                cursorTrackBuyTX = false;
                cursorTrackSellTX = false;
                cursorTrackNothing = false;
            }
            catch (Exception ex)
            {
                HandleException(ex, "DisableCursorTracking");
            }
        }

        #endregion

        #endregion

        #region summary

        private void BtnSummary_Click(object sender, EventArgs e)
        {
            try
            {
                if (listViewTransactions.Items.Count == 0)
                {
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("You need at least one transaction before I can provide a summary.");
                    return;
                }

                // prepare data
                lblSummaryTransactionCount.Invoke((MethodInvoker)delegate
                {
                    lblSummaryTransactionCount.Text = Convert.ToString(listViewTransactions.Items.Count - 1);
                });
                label85.Invoke((MethodInvoker)delegate
                {
                    label85.Location = new Point(lblSummaryTransactionCount.Location.X + lblSummaryTransactionCount.Width, label85.Location.Y);
                });
                lblSummaryTXCountRecdBTC.Invoke((MethodInvoker)delegate
                {
                    lblSummaryTXCountRecdBTC.Text = Convert.ToString(totalTXCountReceiveBTC);
                });
                label74.Invoke((MethodInvoker)delegate
                {
                    label74.Location = new Point(lblSummaryTXCountRecdBTC.Location.X + lblSummaryTXCountRecdBTC.Width, label74.Location.Y);
                });
                lblSummaryTXCountSpentBTC.Invoke((MethodInvoker)delegate
                {
                    lblSummaryTXCountSpentBTC.Text = Convert.ToString(totalTXCountSpendBTC);
                });
                label75.Invoke((MethodInvoker)delegate
                {
                    label75.Location = new Point(lblSummaryTXCountSpentBTC.Location.X + lblSummaryTXCountSpentBTC.Width, label75.Location.Y);
                });
                lblSummaryBTCHeld.Invoke((MethodInvoker)delegate
                {
                    lblSummaryBTCHeld.Text = lblTotalBTCAmount.Text;
                    lblSummaryBTCHeld.Location = new Point(label65.Location.X + label65.Width, lblSummaryBTCHeld.Location.Y);
                });
                label71.Invoke((MethodInvoker)delegate
                {
                    label71.Location = new Point(lblSummaryBTCHeld.Location.X + lblSummaryBTCHeld.Width, label71.Location.Y);
                });
                lblSummaryCostBasis.Invoke((MethodInvoker)delegate
                {
                    lblSummaryCostBasis.Text = lblFinalCostBasis.Text;
                });
                label92.Invoke((MethodInvoker)delegate
                {
                    label92.Location = new Point(lblSummaryCostBasis.Location.X + lblSummaryCostBasis.Width - 3, label92.Location.Y);
                });

                lblSummaryValueOfBTC.Invoke((MethodInvoker)delegate
                {
                    double temp = Convert.ToDouble(lblTotalBTCAmount.Text[1..]) * Convert.ToDouble(priceInSelectedCurrency);
                    temp = Math.Round(temp, 2);
                    lblSummaryValueOfBTC.Text = selectedCurrencySymbol + Convert.ToString(temp);
                });

                label66.Invoke((MethodInvoker)delegate
                {
                    label66.Location = new Point(lblSummaryValueOfBTC.Location.X + lblSummaryValueOfBTC.Width, label66.Location.Y);
                });
                lblSummaryNetFiatAmount.Invoke((MethodInvoker)delegate
                {
                    lblSummaryNetFiatAmount.Text = lblTotalFiatAmount.Text;
                    lblSummaryNetFiatAmount.Location = new Point(label66.Location.X + label66.Width, lblSummaryNetFiatAmount.Location.Y);
                });
                label86.Invoke((MethodInvoker)delegate
                {
                    label86.Location = new Point(lblSummaryNetFiatAmount.Location.X + lblSummaryNetFiatAmount.Width - 4, label86.Location.Y);
                });
                string temp = Convert.ToString(Convert.ToDecimal(lblTotalBTCAmount.Text[1..]) / 21000000 * 100);
                lblSummaryPercentOfAllBitcoinOwned.Invoke((MethodInvoker)delegate
                {
                    lblSummaryPercentOfAllBitcoinOwned.Text = temp[..Math.Min(10, temp.Length)] + "%";
                    lblSummaryPercentOfAllBitcoinOwned.Location = new Point(label85.Location.X + label85.Width, lblSummaryPercentOfAllBitcoinOwned.Location.Y);
                });
                label73.Invoke((MethodInvoker)delegate
                {
                    label73.Location = new Point(lblSummaryPercentOfAllBitcoinOwned.Location.X + lblSummaryPercentOfAllBitcoinOwned.Width, label73.Location.Y);
                });

                double changeInValue;
                if (Convert.ToDouble(lblTotalFiatAmount.Text[1..]) != 0)
                {
                    changeInValue = (Convert.ToDouble(lblSummaryValueOfBTC.Text[1..]) - Convert.ToDouble(lblTotalFiatAmount.Text[1..])) / Math.Abs(Convert.ToDouble(lblTotalFiatAmount.Text[1..])) * 100;
                }
                else
                {
                    changeInValue = (Convert.ToDouble(lblSummaryValueOfBTC.Text[1..]) - Convert.ToDouble(lblTotalFiatAmount.Text[1..])) / 1 * 100;
                }
                changeInValue = Math.Round(changeInValue, 2);
                if (changeInValue > 0)
                {
                    lblSummaryPercentageChangeInValue.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryPercentageChangeInValue.Text = "▲" + Convert.ToString(changeInValue) + "%";
                        lblSummaryPercentageChangeInValue.ForeColor = Color.DarkSeaGreen;
                    });
                }
                if (changeInValue < 0)
                {
                    lblSummaryPercentageChangeInValue.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryPercentageChangeInValue.Text = "▼" + Convert.ToString(changeInValue) + "%";
                        lblSummaryPercentageChangeInValue.ForeColor = Color.RosyBrown;
                    });
                }
                if (changeInValue == 0)
                {
                    lblSummaryPercentageChangeInValue.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryPercentageChangeInValue.Text = Convert.ToString(changeInValue) + "%";
                        lblSummaryPercentageChangeInValue.ForeColor = Color.DimGray;
                    });
                }

                lblSummaryCostToValue.Invoke((MethodInvoker)delegate
                {
                    lblSummaryCostToValue.Text = "from " + lblSummaryNetFiatAmount.Text + " to " + lblSummaryValueOfBTC.Text;
                });
                #region 'buy' transactions
                if (totalTXCountReceiveBTC > 0)
                {
                    lblSummaryTotalFiatSpentOnBuyTransactions.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryTotalFiatSpentOnBuyTransactions.Text = selectedCurrencySymbol + Convert.ToString(Math.Abs(Math.Round(totalFiatSpentOnBuyTransactions, 2)));
                    });
                    lblSummaryTotalBTCRecdFromBuyTransactions.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryTotalBTCRecdFromBuyTransactions.Text = Convert.ToString(Math.Round(totalBTCReceivedOnBuyTransactions, 8));
                    });
                    lblSummaryAvgFiatAmtSpentPerBuyTransaction.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryAvgFiatAmtSpentPerBuyTransaction.Text = selectedCurrencySymbol + Convert.ToString(Math.Abs(Math.Round(totalFiatSpentOnBuyTransactions / totalTXCountReceiveBTC, 2)));
                    });
                    lblSummaryAvgBTCRecdPerBuyTransaction.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryAvgBTCRecdPerBuyTransaction.Text = Convert.ToString(Math.Round(totalBTCReceivedOnBuyTransactions / totalTXCountReceiveBTC, 8));
                    });

                    lblSummaryHighestPricePaid.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryHighestPricePaid.Text = selectedCurrencySymbol + Convert.ToString(highestPricePaid);
                    });
                    if (lowestPricePaid == 999999999)
                    {
                        lblSummaryLowestPricePaid.Invoke((MethodInvoker)delegate
                        {
                            lblSummaryLowestPricePaid.Text = "-";
                        });
                    }
                    else
                    {
                        lblSummaryLowestPricePaid.Invoke((MethodInvoker)delegate
                        {
                            lblSummaryLowestPricePaid.Text = selectedCurrencySymbol + Convert.ToString(lowestPricePaid);
                        });
                    }

                    lblSummaryMostFiatSpentInOneTX.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryMostFiatSpentInOneTX.Text = selectedCurrencySymbol + Convert.ToString(mostFiatSpentInOneTransaction);
                    });
                    lblSummaryMostBTCReceivedInOneTX.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryMostBTCReceivedInOneTX.Text = Convert.ToString(mostBTCReceivedInOneTransaction);
                    });
                }
                #endregion
                #region 'sell' transactions
                if (totalTXCountSpendBTC > 0)
                {
                    lblSummaryTotalFiatReceivedOnSellTransactions.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryTotalFiatReceivedOnSellTransactions.Text = selectedCurrencySymbol + Convert.ToString(Math.Abs(totalFiatReceivedOnSellTransactions));
                    });
                    lblSummaryTotalBTCSpentFromSellTransactions.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryTotalBTCSpentFromSellTransactions.Text = Convert.ToString(Math.Round(totalBTCSpentOnSellTransactions, 8));
                    });
                    lblSummaryAvgFiatAmtReceivedPerSellTransaction.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryAvgFiatAmtReceivedPerSellTransaction.Text = selectedCurrencySymbol + Convert.ToString(Math.Round(totalFiatReceivedOnSellTransactions / totalTXCountSpendBTC, 2));
                    });
                    lblSummaryAvgBTCSpentPerSellTransaction.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryAvgBTCSpentPerSellTransaction.Text = Convert.ToString(Math.Round(totalBTCSpentOnSellTransactions / totalTXCountSpendBTC, 8));
                    });

                    lblSummaryHighestPriceSold.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryHighestPriceSold.Text = selectedCurrencySymbol + Convert.ToString(highestPriceSold);
                    });
                    if (lowestPriceSold == 999999999)
                    {
                        lblSummaryLowestPriceSold.Invoke((MethodInvoker)delegate
                        {
                            lblSummaryLowestPriceSold.Text = "-";
                        });
                    }
                    else
                    {
                        lblSummaryLowestPriceSold.Invoke((MethodInvoker)delegate
                        {
                            lblSummaryLowestPriceSold.Text = selectedCurrencySymbol + Convert.ToString(lowestPriceSold);
                        });
                    }

                    lblSummaryMostFiatReceivedInOneTX.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryMostFiatReceivedInOneTX.Text = selectedCurrencySymbol + Convert.ToString(mostFiatReceivedInOneTransaction);
                    });
                    lblSummaryMostBTCSpentInOneTX.Invoke((MethodInvoker)delegate
                    {
                        lblSummaryMostBTCSpentInOneTX.Text = Convert.ToString(mostBTCSpentInOneTransaction);
                    });
                }
                #endregion
                #region network age since first transaction
                string bitcoinAge = "";
                string firstTransactionText = "";
                if (yearOfFirstTransaction != 0 && monthOfFirstTransaction != 0 && dayOfFirstTransaction != 0)
                {
                    firstTransactionText = "Your first transaction was " + CalculateDateDifference(yearOfFirstTransaction, monthOfFirstTransaction, dayOfFirstTransaction) + ", on the " + FormatDate(yearOfFirstTransaction, monthOfFirstTransaction, dayOfFirstTransaction) + ". ";
                    bitcoinAge = "Bitcoin was " + CalculateDateDifferenceGenesis(yearOfFirstTransaction, monthOfFirstTransaction, dayOfFirstTransaction) + " old at that time.";
                }
                if (yearOfFirstTransaction != 0 && monthOfFirstTransaction != 0 && dayOfFirstTransaction == 0)
                {
                    dayOfFirstTransaction = 15;
                    firstTransactionText = "Your first transaction was approx. " + CalculateDateDifference(yearOfFirstTransaction, monthOfFirstTransaction, dayOfFirstTransaction) + ", on the " + FormatDate(yearOfFirstTransaction, monthOfFirstTransaction, dayOfFirstTransaction) + ". This is an estimated date because your earliest transaction lacks a complete date. ";
                    bitcoinAge = "Bitcoin was " + CalculateDateDifferenceGenesis(yearOfFirstTransaction, monthOfFirstTransaction, dayOfFirstTransaction) + " old at that time.";
                }
                if (yearOfFirstTransaction != 0 && monthOfFirstTransaction == 0 && dayOfFirstTransaction == 0)
                {
                    dayOfFirstTransaction = 15;
                    monthOfFirstTransaction = 6;
                    firstTransactionText = "Your first transaction was approx. " + CalculateDateDifference(yearOfFirstTransaction, monthOfFirstTransaction, dayOfFirstTransaction) + ", on the " + FormatDate(yearOfFirstTransaction, monthOfFirstTransaction, dayOfFirstTransaction) + ". This is an estimated date because your earliest transaction lacks a complete date. ";
                    bitcoinAge = "Bitcoin was " + CalculateDateDifferenceGenesis(yearOfFirstTransaction, monthOfFirstTransaction, dayOfFirstTransaction) + " old at that time.";
                }
                lblFirstTXDate.Invoke((MethodInvoker)delegate
                {
                    lblFirstTXDate.Text = firstTransactionText + bitcoinAge;
                });
                #endregion
                panel19.Invoke((MethodInvoker)delegate
                {
                    panel19.Location = new Point(panel19.Location.X, lblFirstTXDate.Location.Y + lblFirstTXDate.Height);
                });

                label62.Invoke((MethodInvoker)delegate
                {
                    label62.Text = "Avg " + selectedCurrencyName + " spent per transaction";
                });
                label67.Invoke((MethodInvoker)delegate
                {
                    label67.Text = "Total " + selectedCurrencyName + " spent";
                });
                label81.Invoke((MethodInvoker)delegate
                {
                    label81.Text = "Most " + selectedCurrencyName + " spent in one TX";
                });
                label63.Invoke((MethodInvoker)delegate
                {
                    label63.Text = "Avg " + selectedCurrencyName + " received per transaction";
                });
                label70.Invoke((MethodInvoker)delegate
                {
                    label70.Text = "Total " + selectedCurrencyName + " received";
                });
                label88.Invoke((MethodInvoker)delegate
                {
                    label88.Text = "Most " + selectedCurrencyName + " received in one TX";
                });

                DisableEverythingForPopupPanel();
                panelSpeechBorder1.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder1.Location = new Point(panelSummaryContainer.Location.X - 1, panelSummaryContainer.Location.Y - 1);
                    panelSpeechBorder1.Height = panelSummaryContainer.Height - ((int)(21 * UIScale));
                    panelSpeechBorder1.Width = panelSummaryContainer.Width + 2;
                    panelSpeechBorder1.BringToFront();
                });
                panelSpeechBorder2.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder2.Location = new Point(panelSummaryContainer.Location.X - 1, panelSummaryContainer.Location.Y - 1);
                    panelSpeechBorder2.Height = panelSummaryContainer.Height + 2;
                    panelSpeechBorder2.Width = (int)(50 * UIScale);
                    panelSpeechBorder2.BringToFront();
                });
                panelSpeechBorder3.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder3.Location = new Point(panelSummaryContainer.Location.X - 1, (int)(591 * UIScale));
                    panelSpeechBorder3.Height = (int)(21 * UIScale);
                    panelSpeechBorder3.Width = panelSummaryContainer.Width + 2;
                    panelSpeechBorder3.BringToFront();
                });
                panelSummaryContainer.Invoke((MethodInvoker)delegate
                {
                    panelSummaryContainer.Visible = true;
                    panelSummaryContainer.BringToFront();
                });
                ShowSpeechBubbleHelpTextOutline();
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = false;
                    panelHideSpeechTriangle.BringToFront();
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnSummary_Click");
            }
        }

        private void BtnCloseSummary_Click(object sender, EventArgs e)
        {
            try
            {
                EnableEverythingAfterPopupPanel();
                HideSpeechBubbleHelpTextOutline();
                panelSummaryContainer.Invoke((MethodInvoker)delegate
                {
                    panelSummaryContainer.Visible = false;
                });
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = true;
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnCloseSummary_Click");
            }
        }

        private void BtnCloseSummaryButton_Click(object sender, EventArgs e)
        {
            try
            {
                BtnCloseSummary_Click(sender, e);
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnCloseSummaryButton_Click");
            }
        }

        #endregion

        #region currency selection

        private void BtnCurrency_Click(object sender, EventArgs e)
        {
            try
            {
                if (panelCurrency.Height > 0)
                { //shrink the panel
                    currentHeightShrinkingPanel = panelCurrency.Height;
                    StartShrinkingPanelVert(panelCurrency);

                }
                else
                { //expand the panel
                    currentHeightExpandingPanel = panelCurrency.Height;
                    StartExpandingPanelVert(panelCurrency);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnCurrency_Click");
            }
        }

        private async void BtnUSD_Click(object sender, EventArgs e)
        {
            try
            {
                BtnClearInput_Click(sender, e);
                EnableCurrencyButtons();
                btnUSD.Enabled = false;
                currentHeightShrinkingPanel = panelCurrency.Height;
                StartShrinkingPanelVert(panelCurrency);
                selectedCurrencyName = "USD";
                selectedCurrencySymbol = "$";
                HistoricPrices.Clear();
                await GetHistoricPricesAsyncWrapper();
                await SetupTransactionsList();
                SetCurrencyNamesAndSymbolsAndSave();
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnUSD_Click");
            }
        }

        private async void BtnEUR_Click(object sender, EventArgs e)
        {
            try
            {
                BtnClearInput_Click(sender, e);
                EnableCurrencyButtons();
                btnEUR.Enabled = false;
                currentHeightShrinkingPanel = panelCurrency.Height;
                StartShrinkingPanelVert(panelCurrency);
                selectedCurrencyName = "EUR";
                selectedCurrencySymbol = "€";
                HistoricPrices.Clear();
                await GetHistoricPricesAsyncWrapper();
                await SetupTransactionsList();
                SetCurrencyNamesAndSymbolsAndSave();
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnEUR_Click");
            }
        }

        private async void BtnGBP_Click(object sender, EventArgs e)
        {
            try
            {
                BtnClearInput_Click(sender, e);
                EnableCurrencyButtons();
                btnGBP.Enabled = false;
                currentHeightShrinkingPanel = panelCurrency.Height;
                StartShrinkingPanelVert(panelCurrency);
                selectedCurrencyName = "GBP";
                selectedCurrencySymbol = "£";
                HistoricPrices.Clear();
                await GetHistoricPricesAsyncWrapper();
                await SetupTransactionsList();
                SetCurrencyNamesAndSymbolsAndSave();
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnGBP_Click");
            }
        }

        private async void BtnXAU_Click(object sender, EventArgs e)
        {
            try
            {
                BtnClearInput_Click(sender, e);
                EnableCurrencyButtons();
                btnXAU.Enabled = false;
                currentHeightShrinkingPanel = panelCurrency.Height;
                StartShrinkingPanelVert(panelCurrency);
                selectedCurrencyName = "XAU";
                selectedCurrencySymbol = "Ꜷ";
                HistoricPrices.Clear();
                await GetHistoricPricesAsyncWrapper();
                await SetupTransactionsList();
                SetCurrencyNamesAndSymbolsAndSave();
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnXAU_Click");
            }
        }

        private void SetCurrencyNamesAndSymbolsAndSave()
        {
            try
            {
                lblCurrentPrice.Invoke((MethodInvoker)delegate
                {
                    lblCurrentPrice.Text = selectedCurrencySymbol + Convert.ToDecimal(priceInSelectedCurrency).ToString("0.00");
                    //lblCurrentPrice.Location = new Point(pictureBoxRefreshPrice.Location.X - lblCurrentPrice.Width, lblCurrentPrice.Location.Y);
                    lblCurrentPrice.Location = new Point(pictureBoxBTCLogo.Location.X + pictureBoxBTCLogo.Width + 4, lblCurrentPrice.Location.Y);
                });
                pictureBoxRefreshPrice.Invoke((MethodInvoker)delegate
                {
                    pictureBoxRefreshPrice.Location = new Point(lblCurrentPrice.Location.X + lblCurrentPrice.Width, pictureBoxRefreshPrice.Location.Y);
                });
                panelHeaderPrice.Invoke((MethodInvoker)delegate
                {
                    panelHeaderPrice.Width = pictureBoxBTCLogo.Width + lblCurrentPrice.Width + pictureBoxRefreshPrice.Width + 10;
                    panelHeaderPrice.Location = new Point(btnAbout.Location.X - panelHeaderPrice.Width - 4, panelHeaderPrice.Location.Y);
                });

                btnSummary.Invoke((MethodInvoker)delegate
                {
                    btnSummary.Location = new Point(panelHeaderPrice.Location.X - btnSummary.Width - 6, btnSummary.Location.Y);
                });
                btnCurrency.Invoke((MethodInvoker)delegate
                {
                    btnCurrency.Text = selectedCurrencySymbol + " " + selectedCurrencyName;
                });
                label31.Invoke((MethodInvoker)delegate
                {
                    label31.Text = selectedCurrencyName + " total";
                });
                label26.Invoke((MethodInvoker)delegate
                {
                    label26.Text = selectedCurrencyName;
                });
                label11.Invoke((MethodInvoker)delegate
                {
                    label11.Text = selectedCurrencyName + "*";
                });
                label9.Invoke((MethodInvoker)delegate
                {
                    label9.Text = "Price (" + selectedCurrencyName + ")*";
                });
                label23.Invoke((MethodInvoker)delegate
                {
                    label23.Text = "Price (" + selectedCurrencyName + ")";
                });
                label1.Invoke((MethodInvoker)delegate
                {
                    label1.Text = "Price paid (" + selectedCurrencyName + ")";
                });
                label52.Invoke((MethodInvoker)delegate
                {
                    label52.Text = selectedCurrencySymbol;
                });
                label54.Invoke((MethodInvoker)delegate
                {
                    label54.Text = selectedCurrencySymbol;
                });
                if (btnBoughtBitcoin.Text == "✔️")
                {
                    lblFiatAmountSpentRecd.Invoke((MethodInvoker)delegate
                    {
                        lblFiatAmountSpentRecd.Text = selectedCurrencyName + " spent";
                    });
                }
                else
                {
                    lblFiatAmountSpentRecd.Invoke((MethodInvoker)delegate
                    {
                        lblFiatAmountSpentRecd.Text = selectedCurrencyName + " received";
                    });
                }
                if (currencyInFile != selectedCurrencyName)
                {
                    SaveSettingsToSettingsFile();
                }
                if (!initialCurrencySetup)
                {
                    panelWelcome.Visible = false;
                    panelRobotSpeakOuter.Visible = true;
                    InterruptAndStartNewRobotSpeak("Retrieved the current price denominated in your default currency (" + selectedCurrencyName + ").");
                }
                initialCurrencySetup = false;
            }
            catch (Exception ex)
            {
                HandleException(ex, "SetCurrencyNamesAndSymbolsAndSave");
            }
        }

        private void EnableCurrencyButtons()
        {
            try
            {
                btnUSD.Enabled = true;
                btnEUR.Enabled = true;
                btnGBP.Enabled = true;
                btnXAU.Enabled = true;
            }
            catch (Exception ex)
            {
                HandleException(ex, "EnableCurrencyButtons");
            }
        }

        #endregion

        #region converter

        private void BtnConvert_Click(object sender, EventArgs e)
        {
            try
            {
                BtnCloseSettings_Click(sender, e);
                DisableEverythingForPopupPanel();
                panelSpeechBorder1.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder1.Location = new Point(panelConvertContainer.Location.X - 1, panelConvertContainer.Location.Y - 1);
                    panelSpeechBorder1.Height = panelConvertContainer.Height - (int)(21 * UIScale);
                    panelSpeechBorder1.Width = panelConvertContainer.Width + 2;
                    panelSpeechBorder1.BringToFront();
                });
                panelSpeechBorder2.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder2.Location = new Point(panelConvertContainer.Location.X - 1, panelConvertContainer.Location.Y - 1);
                    panelSpeechBorder2.Height = panelConvertContainer.Height + 2;
                    panelSpeechBorder2.Width = (int)(50 * UIScale);
                    panelSpeechBorder2.BringToFront();
                });
                panelSpeechBorder3.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder3.Location = new Point(panelConvertContainer.Location.X - 1, (int)(591 * UIScale));
                    panelSpeechBorder3.Height = (int)(21 * UIScale);
                    panelSpeechBorder3.Width = panelConvertContainer.Width + 2;
                    panelSpeechBorder3.BringToFront();
                });
                panelConvertContainer.Invoke((MethodInvoker)delegate
                {
                    panelConvertContainer.Visible = true;
                    panelConvertContainer.BringToFront();
                });
                ShowSpeechBubbleHelpTextOutline();
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = false;
                    panelHideSpeechTriangle.BringToFront();
                });

                #region USD list
                if (string.IsNullOrEmpty(CurrentPriceUSD) || !double.TryParse(CurrentPriceUSD, out _))
                {
                    CurrentPriceUSD = "0";
                }
                labelPCUSD1.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD1.Text = (Convert.ToDecimal(CurrentPriceUSD) / 100000000).ToString("0.00");
                });
                labelPCUSD2.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD2.Text = (Convert.ToDecimal(CurrentPriceUSD) / 10000000).ToString("0.00");
                });
                labelPCUSD3.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD3.Text = (Convert.ToDecimal(CurrentPriceUSD) / 1000000).ToString("0.00");
                });
                labelPCUSD4.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD4.Text = (Convert.ToDecimal(CurrentPriceUSD) / 100000).ToString("0.00");
                });
                labelPCUSD5.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD5.Text = (Convert.ToDecimal(CurrentPriceUSD) / 10000).ToString("0.00");
                });
                labelPCUSD6.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD6.Text = (Convert.ToDecimal(CurrentPriceUSD) / 1000).ToString("0.00");
                });
                labelPCUSD7.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD7.Text = (Convert.ToDecimal(CurrentPriceUSD) / 100).ToString("0.00");
                });
                labelPCUSD8.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD8.Text = (Convert.ToDecimal(CurrentPriceUSD) / 10).ToString("0.00");
                });
                labelPCUSD9.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD9.Text = (Convert.ToDecimal(CurrentPriceUSD)).ToString("0.00");
                });
                labelPCUSD10.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD10.Text = (Convert.ToDecimal(CurrentPriceUSD) * 10).ToString("0.00");
                });
                labelPCUSD11.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD11.Text = (Convert.ToDecimal(CurrentPriceUSD) * 100).ToString("0.00");
                });
                labelPCUSD12.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD12.Text = (Convert.ToDecimal(CurrentPriceUSD) * 1000).ToString("0.00");
                });
                labelPCUSD13.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD13.Text = (Convert.ToDecimal(CurrentPriceUSD) * 10000).ToString("0.00");
                });
                labelPCUSD14.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD14.Text = (Convert.ToDecimal(CurrentPriceUSD) * 100000).ToString("0.00");
                });
                labelPCUSD15.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD15.Text = (Convert.ToDecimal(CurrentPriceUSD) * 1000000).ToString("0.00");
                });
                labelPCUSD16.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD16.Text = (Convert.ToDecimal(CurrentPriceUSD) * 10000000).ToString("0.00");
                });
                labelPCUSD17.Invoke((MethodInvoker)delegate
                {
                    labelPCUSD17.Text = (Convert.ToDecimal(CurrentPriceUSD) * 21000000).ToString("0.00");
                });
                #endregion
                #region EUR list
                if (string.IsNullOrEmpty(CurrentPriceEUR) || !double.TryParse(CurrentPriceEUR, out _))
                {
                    CurrentPriceEUR = "0";
                }
                labelPCEUR1.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR1.Text = (Convert.ToDecimal(CurrentPriceEUR) / 100000000).ToString("0.00");
                });
                labelPCEUR2.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR2.Text = (Convert.ToDecimal(CurrentPriceEUR) / 10000000).ToString("0.00");
                });
                labelPCEUR3.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR3.Text = (Convert.ToDecimal(CurrentPriceEUR) / 1000000).ToString("0.00");
                });
                labelPCEUR4.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR4.Text = (Convert.ToDecimal(CurrentPriceEUR) / 100000).ToString("0.00");
                });
                labelPCEUR5.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR5.Text = (Convert.ToDecimal(CurrentPriceEUR) / 10000).ToString("0.00");
                });
                labelPCEUR6.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR6.Text = (Convert.ToDecimal(CurrentPriceEUR) / 1000).ToString("0.00");
                });
                labelPCEUR7.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR7.Text = (Convert.ToDecimal(CurrentPriceEUR) / 100).ToString("0.00");
                });
                labelPCEUR8.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR8.Text = (Convert.ToDecimal(CurrentPriceEUR) / 10).ToString("0.00");
                });
                labelPCEUR9.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR9.Text = (Convert.ToDecimal(CurrentPriceEUR)).ToString("0.00");
                });
                labelPCEUR10.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR10.Text = (Convert.ToDecimal(CurrentPriceEUR) * 10).ToString("0.00");
                });
                labelPCEUR11.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR11.Text = (Convert.ToDecimal(CurrentPriceEUR) * 100).ToString("0.00");
                });
                labelPCEUR12.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR12.Text = (Convert.ToDecimal(CurrentPriceEUR) * 1000).ToString("0.00");
                });
                labelPCEUR13.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR13.Text = (Convert.ToDecimal(CurrentPriceEUR) * 10000).ToString("0.00");
                });
                labelPCEUR14.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR14.Text = (Convert.ToDecimal(CurrentPriceEUR) * 100000).ToString("0.00");
                });
                labelPCEUR15.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR15.Text = (Convert.ToDecimal(CurrentPriceEUR) * 1000000).ToString("0.00");
                });
                labelPCEUR16.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR16.Text = (Convert.ToDecimal(CurrentPriceEUR) * 10000000).ToString("0.00");
                });
                labelPCEUR17.Invoke((MethodInvoker)delegate
                {
                    labelPCEUR17.Text = (Convert.ToDecimal(CurrentPriceEUR) * 21000000).ToString("0.00");
                });
                #endregion
                #region GBP list
                if (string.IsNullOrEmpty(CurrentPriceGBP) || !double.TryParse(CurrentPriceGBP, out _))
                {
                    CurrentPriceGBP = "0";
                }
                labelPCGBP1.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP1.Text = (Convert.ToDecimal(CurrentPriceGBP) / 100000000).ToString("0.00");
                });
                labelPCGBP2.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP2.Text = (Convert.ToDecimal(CurrentPriceGBP) / 10000000).ToString("0.00");
                });
                labelPCGBP3.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP3.Text = (Convert.ToDecimal(CurrentPriceGBP) / 1000000).ToString("0.00");
                });
                labelPCGBP4.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP4.Text = (Convert.ToDecimal(CurrentPriceGBP) / 100000).ToString("0.00");
                });
                labelPCGBP5.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP5.Text = (Convert.ToDecimal(CurrentPriceGBP) / 10000).ToString("0.00");
                });
                labelPCGBP6.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP6.Text = (Convert.ToDecimal(CurrentPriceGBP) / 1000).ToString("0.00");
                });
                labelPCGBP7.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP7.Text = (Convert.ToDecimal(CurrentPriceGBP) / 100).ToString("0.00");
                });
                labelPCGBP8.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP8.Text = (Convert.ToDecimal(CurrentPriceGBP) / 10).ToString("0.00");
                });
                labelPCGBP9.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP9.Text = (Convert.ToDecimal(CurrentPriceGBP)).ToString("0.00");
                });
                labelPCGBP10.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP10.Text = (Convert.ToDecimal(CurrentPriceGBP) * 10).ToString("0.00");
                });
                labelPCGBP11.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP11.Text = (Convert.ToDecimal(CurrentPriceGBP) * 100).ToString("0.00");
                });
                labelPCGBP12.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP12.Text = (Convert.ToDecimal(CurrentPriceGBP) * 1000).ToString("0.00");
                });
                labelPCGBP13.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP13.Text = (Convert.ToDecimal(CurrentPriceGBP) * 10000).ToString("0.00");
                });
                labelPCGBP14.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP14.Text = (Convert.ToDecimal(CurrentPriceGBP) * 100000).ToString("0.00");
                });
                labelPCGBP15.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP15.Text = (Convert.ToDecimal(CurrentPriceGBP) * 1000000).ToString("0.00");
                });
                labelPCGBP16.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP16.Text = (Convert.ToDecimal(CurrentPriceGBP) * 10000000).ToString("0.00");
                });
                labelPCGBP17.Invoke((MethodInvoker)delegate
                {
                    labelPCGBP17.Text = (Convert.ToDecimal(CurrentPriceGBP) * 21000000).ToString("0.00");
                });
                #endregion
                #region XAU list
                if (string.IsNullOrEmpty(CurrentPriceXAU) || !double.TryParse(CurrentPriceXAU, out _))
                {
                    CurrentPriceXAU = "0";
                }
                labelPCXAU1.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU1.Text = (Convert.ToDecimal(CurrentPriceXAU) / 100000000).ToString("0.00");
                });
                labelPCXAU2.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU2.Text = (Convert.ToDecimal(CurrentPriceXAU) / 10000000).ToString("0.00");
                });
                labelPCXAU3.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU3.Text = (Convert.ToDecimal(CurrentPriceXAU) / 1000000).ToString("0.00");
                });
                labelPCXAU4.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU4.Text = (Convert.ToDecimal(CurrentPriceXAU) / 100000).ToString("0.00");
                });
                labelPCXAU5.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU5.Text = (Convert.ToDecimal(CurrentPriceXAU) / 10000).ToString("0.00");
                });
                labelPCXAU6.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU6.Text = (Convert.ToDecimal(CurrentPriceXAU) / 1000).ToString("0.00");
                });
                labelPCXAU7.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU7.Text = (Convert.ToDecimal(CurrentPriceXAU) / 100).ToString("0.00");
                });
                labelPCXAU8.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU8.Text = (Convert.ToDecimal(CurrentPriceXAU) / 10).ToString("0.00");
                });
                labelPCXAU9.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU9.Text = (Convert.ToDecimal(CurrentPriceXAU)).ToString("0.00");
                });
                labelPCXAU10.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU10.Text = (Convert.ToDecimal(CurrentPriceXAU) * 10).ToString("0.00");
                });
                labelPCXAU11.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU11.Text = (Convert.ToDecimal(CurrentPriceXAU) * 100).ToString("0.00");
                });
                labelPCXAU12.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU12.Text = (Convert.ToDecimal(CurrentPriceXAU) * 1000).ToString("0.00");
                });
                labelPCXAU13.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU13.Text = (Convert.ToDecimal(CurrentPriceXAU) * 10000).ToString("0.00");
                });
                labelPCXAU14.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU14.Text = (Convert.ToDecimal(CurrentPriceXAU) * 100000).ToString("0.00");
                });
                labelPCXAU15.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU15.Text = (Convert.ToDecimal(CurrentPriceXAU) * 1000000).ToString("0.00");
                });
                labelPCXAU16.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU16.Text = (Convert.ToDecimal(CurrentPriceXAU) * 10000000).ToString("0.00");
                });
                labelPCXAU17.Invoke((MethodInvoker)delegate
                {
                    labelPCXAU17.Text = (Convert.ToDecimal(CurrentPriceXAU) * 21000000).ToString("0.00");
                });
                #endregion
                #region calculate fields derived from user input
                SetCalculatedFiatAmounts();
                SetCalculatedUSDAmount();
                SetCalculatedEURAmount();
                SetCalculatedGBPAmount();
                SetCalculatedXAUAmount();
                #endregion
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnConvert_Click");
            }
        }

        private void SetCalculatedFiatAmounts()
        {
            try
            {
                labelPCUSDcustom.Invoke((MethodInvoker)delegate
                {
                    labelPCUSDcustom.Text = (Convert.ToDecimal(textBoxConvertBTCtoFiat.Text) * Convert.ToDecimal(labelPCUSD9.Text)).ToString("0.00");
                });
                labelPCEURcustom.Invoke((MethodInvoker)delegate
                {
                    labelPCEURcustom.Text = (Convert.ToDecimal(textBoxConvertBTCtoFiat.Text) * Convert.ToDecimal(labelPCEUR9.Text)).ToString("0.00");
                });
                labelPCGBPcustom.Invoke((MethodInvoker)delegate
                {
                    labelPCGBPcustom.Text = (Convert.ToDecimal(textBoxConvertBTCtoFiat.Text) * Convert.ToDecimal(labelPCGBP9.Text)).ToString("0.00");
                });
                labelPCXAUcustom.Invoke((MethodInvoker)delegate
                {
                    labelPCXAUcustom.Text = (Convert.ToDecimal(textBoxConvertBTCtoFiat.Text) * Convert.ToDecimal(labelPCXAU9.Text)).ToString("0.00");
                });
            }
            catch (WebException ex)
            {
                HandleException(ex, "SetCalculatedFiatAmounts");
            }
        }

        private void FiatAmountTextBoxes_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (textBoxConvertUSDtoBTC.Text != "")
                {
                    SetCalculatedUSDAmount();
                }
                if (textBoxConvertEURtoBTC.Text != "")
                {
                    SetCalculatedEURAmount();
                }
                if (textBoxConvertGBPtoBTC.Text != "")
                {
                    SetCalculatedGBPAmount();
                }
                if (textBoxConvertXAUtoBTC.Text != "")
                {
                    SetCalculatedXAUAmount();
                }
            }
            catch (WebException ex)
            {
                HandleException(ex, "FiatAmountTextBoxes_TextChanged");
            }
        }

        private void SetCalculatedUSDAmount()
        {
            try
            {
                if (decimal.TryParse(labelPCUSD9.Text, out decimal pcusd9text) && decimal.TryParse(textBoxConvertUSDtoBTC.Text, out decimal usdtobtctext))
                {
                    if (labelPCUSD9.Text != "USD" && pcusd9text > 0)
                    {
                        lblCalculatedUSDFromBTCAmount.Invoke((MethodInvoker)delegate
                        {
                            lblCalculatedUSDFromBTCAmount.Text = (usdtobtctext / pcusd9text).ToString("0.00000000");
                        });
                        label267.Invoke((MethodInvoker)delegate
                        {
                            label267.Text = "$" + textBoxConvertUSDtoBTC.Text + " USD (US dollar) =";
                        });
                        lblCalculatedUSDFromBTCAmount.Invoke((MethodInvoker)delegate
                        {
                            lblCalculatedUSDFromBTCAmount.Location = new Point(label267.Location.X + label267.Width - 4, lblCalculatedUSDFromBTCAmount.Location.Y);
                        });
                        label273.Invoke((MethodInvoker)delegate
                        {
                            label273.Location = new Point(lblCalculatedUSDFromBTCAmount.Location.X + lblCalculatedUSDFromBTCAmount.Width, label273.Location.Y);
                        });
                    }
                }
            }
            catch (WebException ex)
            {
                HandleException(ex, "SetCalculatedUSDAmount");
            }
        }

        private void SetCalculatedEURAmount()
        {
            try
            {
                if (decimal.TryParse(labelPCEUR9.Text, out decimal pceur9text) && decimal.TryParse(textBoxConvertEURtoBTC.Text, out decimal eurtobtctext))
                {
                    if (labelPCEUR9.Text != "EUR" && pceur9text > 0)
                    {
                        lblCalculatedEURFromBTCAmount.Invoke((MethodInvoker)delegate
                        {
                            lblCalculatedEURFromBTCAmount.Text = (eurtobtctext / pceur9text).ToString("0.00000000");
                        });
                        label270.Invoke((MethodInvoker)delegate
                        {
                            label270.Text = "€" + textBoxConvertEURtoBTC.Text + " EUR (European euro) =";
                        });
                        lblCalculatedEURFromBTCAmount.Invoke((MethodInvoker)delegate
                        {
                            lblCalculatedEURFromBTCAmount.Location = new Point(label270.Location.X + label270.Width - 4, lblCalculatedEURFromBTCAmount.Location.Y);
                        });
                        label274.Invoke((MethodInvoker)delegate
                        {
                            label274.Location = new Point(lblCalculatedEURFromBTCAmount.Location.X + lblCalculatedEURFromBTCAmount.Width, label274.Location.Y);
                        });
                    }
                }
            }
            catch (WebException ex)
            {
                HandleException(ex, "SetCalculatedEURAmount");
            }
        }

        private void SetCalculatedGBPAmount()
        {
            try
            {
                if (decimal.TryParse(labelPCGBP9.Text, out decimal pcgbp9text) && decimal.TryParse(textBoxConvertGBPtoBTC.Text, out decimal gbptobtctext))
                {
                    if (labelPCGBP9.Text != "GBP" && pcgbp9text > 0)
                    {
                        lblCalculatedGBPFromBTCAmount.Invoke((MethodInvoker)delegate
                        {
                            lblCalculatedGBPFromBTCAmount.Text = (gbptobtctext / pcgbp9text).ToString("0.00000000");
                        });
                        label269.Invoke((MethodInvoker)delegate
                        {
                            label269.Text = "£" + textBoxConvertGBPtoBTC.Text + " GBP (British pound sterling) =";
                        });
                        lblCalculatedGBPFromBTCAmount.Invoke((MethodInvoker)delegate
                        {
                            lblCalculatedGBPFromBTCAmount.Location = new Point(label269.Location.X + label269.Width - 4, lblCalculatedGBPFromBTCAmount.Location.Y);
                        });
                        label276.Invoke((MethodInvoker)delegate
                        {
                            label276.Location = new Point(lblCalculatedGBPFromBTCAmount.Location.X + lblCalculatedGBPFromBTCAmount.Width, label276.Location.Y);
                        });
                    }
                }
            }
            catch (WebException ex)
            {
                HandleException(ex, "SetCalculatedGBPAmount");
            }
        }

        private void SetCalculatedXAUAmount()
        {
            try
            {
                if (decimal.TryParse(labelPCXAU9.Text, out decimal pcxau9text) && decimal.TryParse(textBoxConvertXAUtoBTC.Text, out decimal xautobtctext))
                {
                    if (labelPCXAU9.Text != "XAU" && pcxau9text > 0)
                    {
                        lblCalculatedXAUFromBTCAmount.Invoke((MethodInvoker)delegate
                        {
                            lblCalculatedXAUFromBTCAmount.Text = (xautobtctext / pcxau9text).ToString("0.00000000");
                        });
                        label268.Invoke((MethodInvoker)delegate
                        {
                            label268.Text = "Ꜷ" + textBoxConvertXAUtoBTC.Text + " XAU (ounce of gold) =";
                        });
                        lblCalculatedXAUFromBTCAmount.Invoke((MethodInvoker)delegate
                        {
                            lblCalculatedXAUFromBTCAmount.Location = new Point(label268.Location.X + label268.Width - 4, lblCalculatedXAUFromBTCAmount.Location.Y);
                        });
                        label275.Invoke((MethodInvoker)delegate
                        {
                            label275.Location = new Point(lblCalculatedXAUFromBTCAmount.Location.X + lblCalculatedXAUFromBTCAmount.Width, label275.Location.Y);
                        });
                    }
                }
            }
            catch (WebException ex)
            {
                HandleException(ex, "SetCalculatedXAUAmount");
            }
        }

        private void BtnCloseConverter_Click(object sender, EventArgs e)
        {
            try
            {
                EnableEverythingAfterPopupPanel();
                HideSpeechBubbleHelpTextOutline();
                panelConvertContainer.Invoke((MethodInvoker)delegate
                {
                    panelConvertContainer.Visible = false;
                });
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = true;
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnCloseConverter_Click");
            }
        }

        #region validate user inputs

        private void CurrencyTextBoxes_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (sender is System.Windows.Forms.TextBox textBox)
                {
                    string text = textBox.Text;

                    // Allow digits, backspace, and decimal point
                    if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
                    {
                        e.Handled = true;
                        return;
                    }

                    // Only allow one decimal point
                    if (e.KeyChar == '.' && text.Contains('.'))
                    {
                        e.Handled = true;
                        return;
                    }

                    // Get the current caret position
                    int caretPos = textBox.SelectionStart;

                    // Check if the new character is being inserted after the decimal point
                    if (text.Contains('.') && caretPos > text.IndexOf('.'))
                    {
                        // Allow two digits after the decimal point
                        int decimalPlaces = text.Length - text.IndexOf('.') - 1;
                        if (decimalPlaces >= 2)
                        {
                            e.Handled = true;
                            return;
                        }
                    }

                    // Combine the current text with the newly typed character
                    string newText = text[..caretPos] + e.KeyChar + text[caretPos..];

                    // Remove any commas in the text
                    string strippedText = newText.Replace(",", "");

                    // max = 100 trillion
                    if (!string.IsNullOrEmpty(strippedText) && decimal.TryParse(strippedText, out decimal value))
                    {
                        if (value > 100000000000000)
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                HandleException(ex, "CurrencyTextBoxes_KeyPress - validating currency input");
            }
        }

        private void TextBoxConvertBTCtoFiat_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (sender is System.Windows.Forms.TextBox textBox)
                {
                    string text = textBox.Text;

                    // Allow digits, backspace, and decimal point
                    if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
                    {
                        e.Handled = true;
                        return;
                    }

                    // Only allow one decimal point
                    if (e.KeyChar == '.' && text.Contains('.'))
                    {
                        e.Handled = true;
                        return;
                    }

                    // Get the current caret position
                    int caretPos = textBox.SelectionStart;

                    // Check if the new character is being inserted after the decimal point
                    if (text.Contains('.') && caretPos > text.IndexOf('.'))
                    {
                        // Allow 8 digits after the decimal point
                        int decimalPlaces = text.Length - text.IndexOf('.') - 1;
                        if (decimalPlaces >= 8)
                        {
                            e.Handled = true;
                            return;
                        }
                    }

                    // Combine the current text with the newly typed character
                    string newText = text[..caretPos] + e.KeyChar + text[caretPos..];

                    // Remove any commas in the text
                    string strippedText = newText.Replace(",", "");

                    // max = 21 million
                    if (!string.IsNullOrEmpty(strippedText) && decimal.TryParse(strippedText, out decimal value))
                    {
                        if (value > 21000000)
                        {
                            e.Handled = true;
                            return;
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                HandleException(ex, "textBoxConvertBTCtoFiat_KeyPress - validating BTC input");
            }
        }

        private void TextBoxConvertBTCtoFiat_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (textBoxConvertBTCtoFiat.Text != "")
                {
                    SetCalculatedFiatAmounts();
                }
            }
            catch (WebException ex)
            {
                HandleException(ex, "TextBoxConvertBTCtoFiat_TextChanged");
            }
        }

        #endregion

        #region set textboxes to 0 if left empty by user

        private void TextBoxConvertUSDtoBTC_Leave(object sender, EventArgs e)
        {
            try
            {
                if (textBoxConvertUSDtoBTC.Text == "")
                {
                    textBoxConvertUSDtoBTC.Invoke((MethodInvoker)delegate
                    {
                        textBoxConvertUSDtoBTC.Text = "1.00";
                    });
                }
            }
            catch (WebException ex)
            {
                HandleException(ex, "TextBoxConvertUSDtoBTC_Leave");
            }
        }

        private void TextBoxConvertEURtoBTC_Leave(object sender, EventArgs e)
        {
            try
            {
                if (textBoxConvertEURtoBTC.Text == "")
                {
                    textBoxConvertEURtoBTC.Invoke((MethodInvoker)delegate
                    {
                        textBoxConvertEURtoBTC.Text = "1.00";
                    });
                }
            }
            catch (WebException ex)
            {
                HandleException(ex, "TextBoxConvertEURtoBTC_Leave");
            }
        }

        private void TextBoxConvertGBPtoBTC_Leave(object sender, EventArgs e)
        {
            try
            {
                if (textBoxConvertGBPtoBTC.Text == "")
                {
                    textBoxConvertGBPtoBTC.Invoke((MethodInvoker)delegate
                    {
                        textBoxConvertGBPtoBTC.Text = "1.00";
                    });
                }
            }
            catch (WebException ex)
            {
                HandleException(ex, "TextBoxConvertGBPtoBTC_Leave");
            }
        }

        private void TextBoxConvertXAUtoBTC_Leave(object sender, EventArgs e)
        {
            try
            {
                if (textBoxConvertXAUtoBTC.Text == "")
                {
                    textBoxConvertXAUtoBTC.Invoke((MethodInvoker)delegate
                    {
                        textBoxConvertXAUtoBTC.Text = "1.00";
                    });
                }
            }
            catch (WebException ex)
            {
                HandleException(ex, "TextBoxConvertXAUtoBTC_Leave");
            }
        }

        private void TextBoxConvertBTCtoFiat_Leave(object sender, EventArgs e)
        {
            try
            {
                if (textBoxConvertBTCtoFiat.Text == "")
                {
                    textBoxConvertBTCtoFiat.Invoke((MethodInvoker)delegate
                    {
                        textBoxConvertBTCtoFiat.Text = "1.00000000";
                    });
                }
            }
            catch (WebException ex)
            {
                HandleException(ex, "TextBoxConvertBTCtoFiat_Leave");
            }
        }

        #endregion

        #endregion

        #region settings

        private void PictureBoxSettings_Click(object sender, EventArgs e)
        {
            try
            {
                HideAnyRobotMessageThatsPresent();
                DisableEverythingForPopupPanel();
                panelSpeechBorder1.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder1.Location = new Point(panelSettingsContainer.Location.X - 1, panelSettingsContainer.Location.Y - 1);
                    panelSpeechBorder1.Height = panelSettingsContainer.Height - (int)(21 * UIScale);
                    panelSpeechBorder1.Width = panelSettingsContainer.Width + 2;
                    panelSpeechBorder1.BringToFront();
                });
                panelSpeechBorder2.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder2.Location = new Point(panelSettingsContainer.Location.X - 1, panelSettingsContainer.Location.Y - 1);
                    panelSpeechBorder2.Height = panelSettingsContainer.Height + 2;
                    panelSpeechBorder2.Width = (int)(50 * UIScale);
                    panelSpeechBorder2.BringToFront();
                });
                panelSpeechBorder3.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder3.Location = new Point(panelSettingsContainer.Location.X - 1, (int)(591 * UIScale));
                    panelSpeechBorder3.Height = (int)(21 * UIScale);
                    panelSpeechBorder3.Width = panelSettingsContainer.Width + 2;
                    panelSpeechBorder3.BringToFront();
                });
                panelSettingsContainer.Invoke((MethodInvoker)delegate
                {
                    panelSettingsContainer.Visible = true;
                    panelSettingsContainer.BringToFront();
                });
                ShowSpeechBubbleHelpTextOutline();
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = false;
                    panelHideSpeechTriangle.BringToFront();
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "PictureBoxSettings_Click");
            }
        }

        private void BtnCloseSettings_Click(object sender, EventArgs e)
        {
            try
            {
                EnableEverythingAfterPopupPanel();
                HideSpeechBubbleHelpTextOutline();
                panelSettingsContainer.Invoke((MethodInvoker)delegate
                {
                    panelSettingsContainer.Visible = false;
                });
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = true;
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnCloseSettings_Click");
            }
        }

        #region UIScale selection
        private void btnBiggerScale_Click(object sender, EventArgs e)
        {
            try
            {
                if (lblScaleAmount.Text == "100%")
                {
                    lblScaleAmount.Invoke((MethodInvoker)delegate
                    {
                        lblScaleAmount.Text = "125%";
                    });
                    UIScaleToBeSavedToSettings = 1.25;

                    // enable the shrink button
                    btnSmallerScale.Invoke((MethodInvoker)delegate
                    {
                        btnSmallerScale.ForeColor = Color.White;
                        btnSmallerScale.BackColor = Color.FromArgb(255, 192, 128);
                        btnSmallerScale.Enabled = true;
                    });
                    if (UIScaleInFile != Convert.ToString(UIScaleToBeSavedToSettings))
                    {
                        SaveUIScaleToSettingsFile();
                    }
                }
                else
                {
                    if (lblScaleAmount.Text == "125%")
                    {
                        lblScaleAmount.Invoke((MethodInvoker)delegate
                        {
                            lblScaleAmount.Text = "150%";
                        });
                        UIScaleToBeSavedToSettings = 1.5;
                        if (UIScaleInFile != Convert.ToString(UIScaleToBeSavedToSettings))
                        {
                            SaveUIScaleToSettingsFile();
                        }
                    }
                    else
                    {
                        if (lblScaleAmount.Text == "150%")
                        {
                            lblScaleAmount.Invoke((MethodInvoker)delegate
                            {
                                lblScaleAmount.Text = "175%";
                            });
                            UIScaleToBeSavedToSettings = 1.75;
                            if (UIScaleInFile != Convert.ToString(UIScaleToBeSavedToSettings))
                            {
                                SaveUIScaleToSettingsFile();
                            }
                        }
                        else
                        {
                            if (lblScaleAmount.Text == "175%")
                            {
                                lblScaleAmount.Invoke((MethodInvoker)delegate
                                {
                                    lblScaleAmount.Text = "200%";
                                });
                                UIScaleToBeSavedToSettings = 2;

                                // disable the enlarge button
                                btnBiggerScale.Invoke((MethodInvoker)delegate
                                {
                                    btnBiggerScale.ForeColor = Color.Gray;
                                    btnBiggerScale.BackColor = Color.FromArgb(243, 243, 243);
                                    btnBiggerScale.Enabled = false;
                                });

                                if (UIScaleInFile != Convert.ToString(UIScaleToBeSavedToSettings))
                                {
                                    SaveUIScaleToSettingsFile();
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "btnBiggerScale_Click");
            }
        }

        private void btnSmallerScale_Click(object sender, EventArgs e)
        {
            try
            {
                if (lblScaleAmount.Text == "200%")
                {
                    lblScaleAmount.Invoke((MethodInvoker)delegate
                    {
                        lblScaleAmount.Text = "175%";
                    });
                    UIScaleToBeSavedToSettings = 1.75;

                    // enable the enlarge button
                    btnBiggerScale.Invoke((MethodInvoker)delegate
                    {
                        btnBiggerScale.ForeColor = Color.White;
                        btnBiggerScale.BackColor = Color.FromArgb(255, 192, 128);
                        btnBiggerScale.Enabled = true;
                    });

                    if (UIScaleInFile != Convert.ToString(UIScaleToBeSavedToSettings))
                    {
                        SaveUIScaleToSettingsFile();
                    }
                }
                else
                {
                    if (lblScaleAmount.Text == "175%")
                    {
                        lblScaleAmount.Invoke((MethodInvoker)delegate
                        {
                            lblScaleAmount.Text = "150%";
                        });
                        UIScaleToBeSavedToSettings = 1.5;
                        if (UIScaleInFile != Convert.ToString(UIScaleToBeSavedToSettings))
                        {
                            SaveUIScaleToSettingsFile();
                        }
                    }
                    else
                    {
                        if (lblScaleAmount.Text == "150%")
                        {
                            lblScaleAmount.Invoke((MethodInvoker)delegate
                            {
                                lblScaleAmount.Text = "125%";
                            });
                            UIScaleToBeSavedToSettings = 1.25;
                            if (UIScaleInFile != Convert.ToString(UIScaleToBeSavedToSettings))
                            {
                                SaveUIScaleToSettingsFile();
                            }
                        }
                        else
                        {
                            if (lblScaleAmount.Text == "125%")
                            {
                                lblScaleAmount.Invoke((MethodInvoker)delegate
                                {
                                    lblScaleAmount.Text = "100%";
                                });
                                UIScaleToBeSavedToSettings = 1;

                                // disable the shrink button
                                btnSmallerScale.Invoke((MethodInvoker)delegate
                                {
                                    btnSmallerScale.ForeColor = Color.Gray;
                                    btnSmallerScale.BackColor = Color.FromArgb(234, 234, 234);
                                    btnSmallerScale.Enabled = false;
                                });

                                if (UIScaleInFile != Convert.ToString(UIScaleToBeSavedToSettings))
                                {
                                    SaveUIScaleToSettingsFile();
                                }
                            }
                            else
                            {
                                return;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "btnBiggerScale_Click");
            }
        }
        #endregion

        #region hover effect on settings picturebox (mimic button)

        private void PictureBoxSettings_MouseEnter(object sender, EventArgs e)
        {
            try
            {
                if (pictureBoxSettings.Image != Resources.SettingsDisabled)
                {
                    pictureBoxSettings.Invoke((MethodInvoker)delegate
                    {
                        pictureBoxSettings.Image = Resources.SettingsHover;
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "PictureBoxSettings_MouseEnter");
            }
        }

        private void PictureBoxSettings_MouseLeave(object sender, EventArgs e)
        {
            try
            {
                if (pictureBoxSettings.Image != Resources.SettingsDisabled)
                {
                    pictureBoxSettings.Invoke((MethodInvoker)delegate
                    {
                        pictureBoxSettings.Image = Resources.Settings;
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "PictureBoxSettings_MouseLeave");
            }
        }

        #endregion

        #endregion

        #region exit, minimise, move, about

        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void BtnAbout_Click(object sender, EventArgs e)
        {
            try
            {
                DisableEverythingForPopupPanel();
                // display about screen:
                Form frm = new About
                {
                    Owner = this, // Set the parent window as the owner of the modal window
                    StartPosition = FormStartPosition.CenterParent, // Set the start position to center of parent
                    CurrentVersion = CurrentVersion
                };
                frm.ShowDialog(this);
                EnableEverythingAfterPopupPanel();
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnAbout_Click");
            }
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            BtnAbout_Click(sender, e);
        }

        private void PictureBox2_Click(object sender, EventArgs e)
        {
            BtnAbout_Click(sender, e);
        }

        #region move window

        private void BtnMoveWindow_MouseDown(object sender, MouseEventArgs e) // move the form when the move control is used
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void BtnMoveWindow_MouseUp(object sender, MouseEventArgs e) // reset colour of the move form control
        {
            var args = e as MouseEventArgs;
            if (args.Button == MouseButtons.Right)
            {
                return;
            }
            btnMoveWindow.BackColor = Color.White;
        }

        private void BtnMoveWindow_Click(object sender, EventArgs e)
        {
            if (e is MouseEventArgs args)
            {
                if (args.Button != MouseButtons.Left)
                {
                    return;
                }
            }
        }

        #endregion

        #endregion

        #region documentation

        private void BtnCloseHelpText_Click(object sender, EventArgs e)
        {
            try
            {
                EnableEverythingAfterPopupPanel();
                HideSpeechBubbleHelpTextOutline();
                panelHelpTextContainer.Invoke((MethodInvoker)delegate
                {
                    panelHelpTextContainer.Visible = false;
                });
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = true;
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnCloseHelpText_Click");
            }
        }

        private void BtnHelpAddTransaction_Click(object sender, EventArgs e)
        {
            try
            {
                HideAnyRobotMessageThatsPresent();
                panelHelpTextContainer.Invoke((MethodInvoker)delegate
                {
                    panelHelpTextContainer.Location = new Point(panelHelpTextContainer.Location.X, (int)(290 * UIScale));
                    panelHelpTextContainer.Height = (int)(321 * UIScale);
                });
                panelHelp.Invoke((MethodInvoker)delegate
                {
                    panelHelp.Height = (int)(293 * UIScale);
                });
                lblHelpText.Invoke((MethodInvoker)delegate
                {
                    lblHelpText.Height = (int)(277 * UIScale);
                });
                DisableEverythingForPopupPanel();
                PlaceSpeechBubbleHelpTextOutline();
                label42.Invoke((MethodInvoker)delegate
                {
                    label42.Text = "Transaction input (help)";
                });
                lblHelpText.Invoke((MethodInvoker)delegate
                {
                    lblHelpText.Text = "Input all your transactions here. The more accurate you can be the better, but Cubit will do its best to fill in the gaps for you if you don't have all the information needed." + Environment.NewLine + Environment.NewLine + "Make sure to select your fiat currency at the top of the screen before entering transactions. Start by selecting 'Received Bitcoin' if you bought, earned, or were gifted an amount of Bitcoin, or 'Spent Bitcoin' if you sold, paid or gifted an amount of Bitcoin. Note that Cubit won't allow you to spend bitcoin that you don't already have, so your first transaction should be a 'receive' transaction." + Environment.NewLine + Environment.NewLine + "Fill in as much of the date of the transaction as possible. If you know the year and month but not the day then the median bitoin price for that month will be used as an estimate. If you only know the year then the median price for that year will be used. If you know the exact date then the estimate will be an average price for that date. In periods of higher volatility using estimates will increase the margin of error later on, so it's always best to be as complete as you can be." + Environment.NewLine + Environment.NewLine + "Once you input the amount of fiat money or the amount of bitcoin that was transacted, estimates will also be provided for the amount of bitcoin or fiat you will have received or spent. This is based purely on the exchange rate and won't take account of things such as exchange fees, non-KYC premium, etc. It's best to provide all the correct figures if you can." + Environment.NewLine + Environment.NewLine + "The 'Label' field can be used to record a small note about the transaction if you want to, and a colour code can also be assigned to the transaction.";
                });
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = false;
                });
                panelHelpTextContainer.Invoke((MethodInvoker)delegate
                {
                    panelHelpTextContainer.Visible = true;
                    panelHelpTextContainer.BringToFront();
                });
                ShowSpeechBubbleHelpTextOutline();
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnHelpAddTransaction_Click");
            }
        }

        private void BtnHelpTransactionList_Click(object sender, EventArgs e)
        {
            try
            {
                HideAnyRobotMessageThatsPresent();
                panelHelpTextContainer.Invoke((MethodInvoker)delegate
                {
                    panelHelpTextContainer.Location = new Point(panelHelpTextContainer.Location.X, (int)(110 * UIScale));
                    panelHelpTextContainer.Height = (int)(501 * UIScale);
                });
                panelHelp.Invoke((MethodInvoker)delegate
                {
                    panelHelp.Height = (int)(473 * UIScale);
                });
                lblHelpText.Invoke((MethodInvoker)delegate
                {
                    lblHelpText.Height = (int)(457 * UIScale);
                });
                DisableEverythingForPopupPanel();
                PlaceSpeechBubbleHelpTextOutline();
                label42.Invoke((MethodInvoker)delegate
                {
                    label42.Text = "Transaction list (help)";
                });
                lblHelpText.Invoke((MethodInvoker)delegate
                {
                    lblHelpText.Text = "The list displays all of your transactions in chronological or reverse-chronological order." + Environment.NewLine + Environment.NewLine + "R/S - Displays 'Recv' (receive) for a transaction where you received bitcoin, or 'Spnd' (spend) for a transaction where you spent bitcoin." + Environment.NewLine + Environment.NewLine + "YYYY/MM/DD - The date of the transaction. If a partial date was provided a '-' will display in the missing fields." + Environment.NewLine + Environment.NewLine + "Price - the price paid/received for the transaction (price of 1 bitcoin)." + Environment.NewLine + Environment.NewLine + "Est. - The type of estimate used to determine the price: DA - daily average, MM - monthly median, AM - annual median, N - not estimated, an accurate price was provided." + Environment.NewLine + Environment.NewLine + "Range - If an estimate is being used, this is the potential margin of error. This margin is governed by the amount of price volitility that occurred in the undefined period of time. The more accurate you can be with the date input the lower the margin of error will be." + Environment.NewLine + Environment.NewLine + "USD/EUR/GBP/XAU - the amount of fiat currency involved in the transaction. If an estimate was used a 'Y' will show under the 'Est.' column." + Environment.NewLine + Environment.NewLine + "BTC - the amount of bitcoin involved in the transaction. If an estimate was used a 'Y' will show under the 'Est.' column." + Environment.NewLine + Environment.NewLine + "USD/EUR/GBP/XAU total - rolling fiat total." + Environment.NewLine + Environment.NewLine + "BTC total - rolling BTC total." + Environment.NewLine + Environment.NewLine + "Cost basis - the rolling cost basis (average purchase price) of your bitcoin holdings." + Environment.NewLine + Environment.NewLine + "CC - shows the colour code, if one was applied." + Environment.NewLine + Environment.NewLine + "🏷️ - denotes whether a label was added to the transaction. If there is a label it can be viewed in full using the button below the table." + Environment.NewLine + Environment.NewLine + "The ▲ button can be used to increase the display area of the transaction list.";
                });
                panelHelpTextContainer.Invoke((MethodInvoker)delegate
                {
                    panelHelpTextContainer.Visible = true;
                    panelHelpTextContainer.BringToFront();
                });
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = false;
                });
                ShowSpeechBubbleHelpTextOutline();
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnHelpTransactionList_Click");
            }
        }

        private void BtnHelpChart_Click(object sender, EventArgs e)
        {
            try
            {
                HideAnyRobotMessageThatsPresent();
                panelHelpTextContainer.Invoke((MethodInvoker)delegate
                {
                    panelHelpTextContainer.Location = new Point(panelHelpTextContainer.Location.X, (int)(200 * UIScale));
                    panelHelpTextContainer.Height = (int)(411 * UIScale);
                });
                panelHelp.Invoke((MethodInvoker)delegate
                {
                    panelHelp.Height = (int)(383 * UIScale);
                });
                lblHelpText.Invoke((MethodInvoker)delegate
                {
                    lblHelpText.Height = (int)(367 * UIScale);
                });
                DisableEverythingForPopupPanel();
                PlaceSpeechBubbleHelpTextOutline();
                label42.Invoke((MethodInvoker)delegate
                {
                    label42.Text = "Chart (help)";
                });
                lblHelpText.Invoke((MethodInvoker)delegate
                {
                    lblHelpText.Text = "The orange plotted line on the chart represents the price of 1 bitcoin since its inception to the present day, with the date along the x axis and the selected fiat currency value on the y axis." + Environment.NewLine + Environment.NewLine + "The purple solid line represents your historic cost basis (average purchase price) of all your bitcoin taking all of your past transactions in to account. The purple horizontal dashed line represents the current cost basis. When the value of 1 bitcoin is above this line your bitcoin is worth more in fiat terms than it cost you. The cost basis line can be disabled in the options above the chart." + Environment.NewLine + Environment.NewLine + "The vertical green lines show the dates of transactions where you bought or received bitcoin and the red vertical lines show transactions where you sold or spent bitcoin. The transaction lines can be disabled in the options above the chart." + Environment.NewLine + Environment.NewLine + "The red and green circles are positioned to show the date of a transaction and the value of bitcoin at the time of the transaction. The radius of the circle is determined by the significance in size of that transaction (in fiat terms) compared to all other transactions of that type. The biggest circles are your biggest transactions. Green circles represent transactions where you've bought or received bitcoin and red circles represent transactions where you've sold or spent bitcoin. The transaction circles can be disabled in the options above the chart." + Environment.NewLine + Environment.NewLine + "The upper-left of the chart shows the values of the closest plotted point to the mouse cursor. You can select which data is tracked in the options above the chart." + Environment.NewLine + Environment.NewLine + "Price and date gridlines can be individually disabled in the options above the chart." + Environment.NewLine + Environment.NewLine + "The chart can be viewed with either a linear scale or a logarithmic scale at any time with the buttons above the chart." + Environment.NewLine + Environment.NewLine + "The ▼ button can be used to increase the display area of the chart.";
                });
                panelHelpTextContainer.Invoke((MethodInvoker)delegate
                {
                    panelHelpTextContainer.Visible = true;
                    panelHelpTextContainer.BringToFront();
                });
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = false;
                });
                ShowSpeechBubbleHelpTextOutline();
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnHelpChart_Click");
            }
        }

        private void PlaceSpeechBubbleHelpTextOutline()
        {
            try
            {
                panelSpeechBorder1.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder1.Location = new Point(panelHelpTextContainer.Location.X - 1, panelHelpTextContainer.Location.Y - 1);
                    panelSpeechBorder1.Height = panelHelpTextContainer.Height - (int)(21 * UIScale);
                    panelSpeechBorder1.Width = panelHelpTextContainer.Width + 2;
                    panelSpeechBorder1.BringToFront();
                });
                panelSpeechBorder2.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder2.Location = new Point(panelHelpTextContainer.Location.X - 1, panelHelpTextContainer.Location.Y - 1);
                    panelSpeechBorder2.Height = panelHelpTextContainer.Height + 2;
                    panelSpeechBorder2.Width = (int)(50 * UIScale);
                    panelSpeechBorder2.BringToFront();
                });
                panelSpeechBorder3.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder3.Location = new Point(panelHelpTextContainer.Location.X - 1, (int)(591 * UIScale));
                    panelSpeechBorder3.Height = (int)(21 * UIScale);
                    panelSpeechBorder3.Width = panelHelpTextContainer.Width + 2;
                    panelSpeechBorder3.BringToFront();
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "PlaceSpeechBubbleHelpTextOutline");
            }
        }

        #endregion

        #region common code

        #region disable/enable everything for panel 'popups'

        private void DisableEverythingForPopupPanel()
        {
            try
            {
                panelAddTransactionContainer.Enabled = false;
                panelChartOuter.Enabled = false;
                panelTXListOuter.Enabled = false;
                panelTXListFooter.Enabled = false;
                panelTopControls.Enabled = false;
                pictureBoxRobot.Enabled = false;
                btnHelpTransactionList.Enabled = false;
                panel9.Enabled = false;
                pictureBoxSettingsDisabled.Invoke((MethodInvoker)delegate
                {
                    pictureBoxSettingsDisabled.Visible = true;
                    pictureBoxSettingsDisabled.BringToFront();
                });
                pictureBoxRefreshPrice.Invoke((MethodInvoker)delegate
                {
                    pictureBoxRefreshPrice.Image = Resources.RefreshDisabled;
                });
                panelSpeechBorderRobot1.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorderRobot1.Visible = false;
                });
                panelSpeechBorderRobot2.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorderRobot2.Visible = false;
                });
                panelSpeechBorderRobot3.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorderRobot3.Visible = false;
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "DisableEverythingForPopupPanel");
            }
        }

        private void EnableEverythingAfterPopupPanel()
        {
            try
            {
                panelAddTransactionContainer.Enabled = true;
                panelChartOuter.Enabled = true;
                panelTXListOuter.Enabled = true;
                panelTXListFooter.Enabled = true;
                panelTopControls.Enabled = true;
                pictureBoxRobot.Enabled = true;
                btnHelpTransactionList.Enabled = true;
                panel9.Enabled = true;
                pictureBoxRefreshPrice.Invoke((MethodInvoker)delegate
                {
                    pictureBoxRefreshPrice.Image = Resources.Refresh;
                });
                pictureBoxSettingsDisabled.Invoke((MethodInvoker)delegate
                {
                    pictureBoxSettingsDisabled.Visible = false;
                    pictureBoxSettingsDisabled.SendToBack();
                });
                pictureBoxSettings.Invoke((MethodInvoker)delegate
                {
                    pictureBoxSettings.Image = Resources.Settings;
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "EnableEverythingAfterPopupPanel");
            }
        }

        #endregion

        #region hide and show speech bubble outline
        private void ShowSpeechBubbleHelpTextOutline()
        {
            try
            {
                panelSpeechBorder1.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder1.Visible = true;
                });
                panelSpeechBorder2.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder2.Visible = true;
                });
                panelSpeechBorder3.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder3.Visible = true;
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "ShowSpeechBubbleHelpTextOutline");
            }
        }

        private void HideSpeechBubbleHelpTextOutline()
        {
            try
            {
                panelSpeechBorder1.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder1.Visible = false;
                });
                panelSpeechBorder2.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder2.Visible = false;
                });
                panelSpeechBorder3.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorder3.Visible = false;
                });
            }
            catch (Exception ex)
            {
                HandleException(ex, "HideSpeechBubbleHelpTextOutline");
            }
        }

        private void HideAnyRobotMessageThatsPresent()
        {
            robotIgnoreChanges = true;
            InterruptAndStartNewRobotSpeak("");
            robotIgnoreChanges = false;

            panelWelcome.Visible = false;
            panelRobotSpeakOuter.Visible = false;
            panelSpeechBubble.Visible = false;
        }
        #endregion

        #region date stuff

        public static string CalculateDateDifference(int inputYear, int inputMonth, int inputDay)
        {
            DateTime currentDate = DateTime.Now;
            DateTime inputDate = new(inputYear, inputMonth, inputDay);

            // Calculate the difference between the current date and the input date
            TimeSpan difference = currentDate - inputDate;

            // Calculate years, months, and days
            int years = difference.Days / 365;
            int months = (difference.Days % 365) / 30;
            int days = difference.Days % 30;

            // Build the result string
            string result = $"{years} years, {months} months, and {days} days ago";

            return result;
        }

        public static string CalculateDateDifferenceGenesis(int inputYear, int inputMonth, int inputDay)
        {
            DateTime GenesisDate = new(2009, 1, 9);
            DateTime inputDate = new(inputYear, inputMonth, inputDay);

            // Calculate the difference between the current date and the input date
            TimeSpan difference = inputDate - GenesisDate;

            // Calculate years, months, and days
            int years = difference.Days / 365;
            int months = (difference.Days % 365) / 30;
            int days = difference.Days % 30;

            // Build the result string
            string result = $"{years} years, {months} months, and {days} days";

            return result;
        }

        public static string FormatDate(int inputYear, int inputMonth, int inputDay)
        {
            DateTime inputDate = new(inputYear, inputMonth, inputDay);

            string formattedDate = $"{inputDate.Day}{GetDaySuffix(inputDate.Day)} {inputDate:MMMM yyyy}";

            return formattedDate;
        }

        public static string GetDaySuffix(int day)
        {
            if (day >= 11 && day <= 13)
            {
                return "th";
            }

            return (day % 10) switch
            {
                1 => "st",
                2 => "nd",
                3 => "rd",
                _ => "th",
            };
        }

        #endregion

        #region validate decimal field input 

        private void Numeric2DecimalsTextBoxValidation_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                // Allow digits, decimal point, and control keys
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                {
                    e.Handled = true;
                }

                // Allow only one decimal point
                if (e.KeyChar == '.' && ((System.Windows.Forms.TextBox)sender).Text.Contains('.'))
                {
                    e.Handled = true;
                }

                // Allow only two digits after the decimal point
                if (((System.Windows.Forms.TextBox)sender).Text.Contains('.'))
                {
                    string[] parts = ((System.Windows.Forms.TextBox)sender).Text.Split('.');
                    if (parts.Length > 1 && parts[1].Length >= 2 && !char.IsControl(e.KeyChar))
                    {
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "Numeric2DecimalsTextBoxValidation_KeyPress");
            }
        }

        private void Numeric8DecimalsTextBoxValidation_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                // Allow digits, decimal point, and control keys
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
                {
                    e.Handled = true;
                }

                // Allow only one decimal point
                if (e.KeyChar == '.' && ((System.Windows.Forms.TextBox)sender).Text.Contains('.'))
                {
                    e.Handled = true;
                }

                // Allow up to 8 digits after the decimal point
                if (((System.Windows.Forms.TextBox)sender).Text.Contains('.'))
                {
                    string[] parts = ((System.Windows.Forms.TextBox)sender).Text.Split('.');
                    if (parts.Length > 1 && parts[1].Length >= 8 && !char.IsControl(e.KeyChar))
                    {
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "Numeric8DecimalsTextBoxValidation_KeyPress");
            }
        }

        #endregion

        #region determine median and range

        private (decimal, decimal, decimal) GetMedianRangeAndPercentageDifference(decimal[] numbers)
        {
            decimal median = Math.Round(numbers.OrderBy(n => n).ElementAt(numbers.Length / 2), 2);
            decimal range = Math.Round((numbers.Max() - numbers.Min()) / 2, 2);
            decimal percentageDifference = (median != 0) ? Math.Round(((100 / median) * range), 2) : 0;
            selectedRangePercentage = percentageDifference;
            selectedMedianPrice = median;
            return (median, range, percentageDifference);
        }

        #endregion

        #region robot speech and animation

        private void InterruptAndStartNewRobotSpeak(string newRobotText)
        {
            try
            {
                if (!robotIgnoreChanges)
                {
                    panelSpeechBorderRobot1.Invoke((MethodInvoker)delegate
                    {
                        panelSpeechBorderRobot1.Visible = false;
                    });
                    panelSpeechBorderRobot2.Invoke((MethodInvoker)delegate
                    {
                        panelSpeechBorderRobot2.Visible = false;
                    });
                    panelSpeechBorderRobot3.Invoke((MethodInvoker)delegate
                    {
                        panelSpeechBorderRobot3.Visible = false;
                    });
                    if (panelConvertContainer.Visible || panelDeleteTXContainer.Visible || panelHelpTextContainer.Visible || panelResetContainer.Visible || panelSummaryContainer.Visible || panelSettingsContainer.Visible)
                    {
                        panelHideSpeechTriangle.Visible = false;
                    }
                    else
                    {
                        panelHideSpeechTriangle.Visible = true;
                    }

                    // Cancel the previous task if it's running
                    robotSpeakCancellationTokenSource?.Cancel();

                    // Create a new cancellation token source
                    robotSpeakCancellationTokenSource = new CancellationTokenSource();

                    // Start the new RobotSpeakAsync task
                    _ = RobotSpeakAsync(newRobotText, robotSpeakCancellationTokenSource.Token);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "InterruptAndStartNewRobotSpeak");
            }
        }

        private async Task RobotSpeakAsync(string robotText, CancellationToken cancellationToken)
        {
            if (isRobotSpeaking)
            {
                return; // Ignore new calls if the method is already running
            }
            try
            {
                panelSpeechBubble.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBubble.Height = 0;
                    panelSpeechBubble.Location = new Point(panelSpeechBubble.Location.X, (int)(66 * UIScale));
                    panelSpeechBubble.Visible = true;
                    panelSpeechBubble.BringToFront();
                });
                SpeechBubblecurrentHeight = 0;

                lblRobotSpeak.Invoke((MethodInvoker)delegate
                {
                    lblRobotSpeak.Text = "";
                });

                StartExpandingRobot();
                await WaitForExpandRobotTimerToStop();
                panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                {
                    panelHideSpeechTriangle.Visible = false;
                });
                expandRobotTimerRunning = false;

                int charIndex = 0;
                while (charIndex < robotText.Length)
                {
                    // Check if the task should be cancelled
                    cancellationToken.ThrowIfCancellationRequested();

                    Random random = new();
                    int randomValue = random.Next(10);
                    if (randomValue >= 0 && randomValue <= 6)
                    {
                        pictureBoxRobot.Invoke((MethodInvoker)delegate
                        {
                            pictureBoxRobot.Image = Resources.BitcoinCBCRobot;
                        });
                    }
                    if (randomValue == 7)
                    {
                        pictureBoxRobot.Invoke((MethodInvoker)delegate
                        {
                            pictureBoxRobot.Image = Resources.BitcoinCBCRobotActive;
                        });
                    }
                    if (randomValue == 8)
                    {
                        pictureBoxRobot.Invoke((MethodInvoker)delegate
                        {
                            pictureBoxRobot.Image = Resources.BitcoinCBCRobotActive2;
                        });
                    }
                    if (randomValue == 9)
                    {
                        pictureBoxRobot.Invoke((MethodInvoker)delegate
                        {
                            pictureBoxRobot.Image = Resources.BitcoinCBCRobotActive3;
                        });
                    }
                    lblRobotSpeak.Invoke((MethodInvoker)delegate
                    {
                        lblRobotSpeak.Text += robotText[charIndex];
                    });
                    charIndex++;
                    await Task.Delay(30, cancellationToken); // Delay between characters
                }
                await Task.Delay(3000, cancellationToken);
                if (panelHelpTextContainer.Visible == false && panelSummaryContainer.Visible == false)
                {
                    panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
                    {
                        panelHideSpeechTriangle.Visible = true;
                    });
                }
                StartShrinkingRobot();
                pictureBoxRobot.Invoke((MethodInvoker)delegate
                {
                    pictureBoxRobot.Image = Resources.BitcoinCBCRobot;
                });
            }
            finally
            {
                isRobotSpeaking = false;
            }
        }

        private void BlinkRobotTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Random random = new();
                int randomValue = random.Next(20);
                if (randomValue == 1)
                {
                    pictureBoxRobot.Invoke((MethodInvoker)delegate
                    {
                        pictureBoxRobot.Image = Resources.BitcoinCBCRobotBlink;
                    });
                }
                else
                {
                    pictureBoxRobot.Invoke((MethodInvoker)delegate
                    {
                        pictureBoxRobot.Image = Resources.BitcoinCBCRobot;
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "BlinkRobotTimer_Tick");
            }
        }

        private void PictureBoxRobot_Click(object sender, EventArgs e)
        {
            try
            {
                panelSpeechBorderRobot1.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorderRobot1.Visible = false;
                });
                panelSpeechBorderRobot2.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorderRobot2.Visible = false;
                });
                panelSpeechBorderRobot3.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorderRobot3.Visible = false;
                });
                panelWelcome.Visible = true;
                panelRobotSpeakOuter.Visible = false;
                // Create a Random object to select a random message
                Random random = new();
                int randomIndex = random.Next(0, welcomeMessages.Count);

                labelWelcomeText.Invoke((MethodInvoker)delegate
                {
                    // Set the label text to the randomly selected message
                    labelWelcomeText.Text = welcomeMessages[randomIndex];
                });
                pictureBox3.Invoke((MethodInvoker)delegate
                {
                    pictureBox3.Image = welcomeImages[randomIndex];
                });
                InterruptAndStartNewRobotSpeak("None of this text is visible on screen but it's long enough to make a small delay");
            }
            catch (Exception ex)
            {
                HandleException(ex, "PictureBoxRobot_Click");
            }
        }

        private void PictureBoxRobot_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                if (panelSummary.Visible || panelHelpTextContainer.Visible || panelSettingsContainer.Visible || panelConvertContainer.Visible || panelDeleteTXContainer.Visible || panelResetContainer.Visible)
                {
                    Point lineStart = new((int)(83 * UIScale), (int)(71 * UIScale));
                    Point lineEnd = new((int)(95 * UIScale), (int)(71 * UIScale));

                    using (Pen pen = new(Color.FromArgb(255, 128, 0), 1))
                    {
                        e.Graphics.DrawLine(pen, lineStart, lineEnd);
                    }

                    lineStart = new Point((int)(83 * UIScale), (int)(61 * UIScale));
                    lineEnd = new Point((int)(95 * UIScale), (int)(71 * UIScale));

                    using (Pen pen = new(Color.FromArgb(255, 128, 0), 1))
                    {
                        e.Graphics.DrawLine(pen, lineStart, lineEnd);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "PictureBoxRobot_Paint");
            }

        }

        #region expand and shrink robot speech

        private void ShrinkRobotTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                SpeechBubblecurrentHeight -= 4;

                if (SpeechBubblecurrentHeight <= 0) // shrinking is complete
                {
                    ShrinkRobotTimer.Stop();
                    panelWelcome.Visible = false;
                    panelSpeechBubble.Visible = false;
                    if (panelConvertContainer.Visible || panelDeleteTXContainer.Visible || panelHelpTextContainer.Visible || panelResetContainer.Visible || panelSummaryContainer.Visible || panelSettingsContainer.Visible)
                    {
                        panelHideSpeechTriangle.Visible = false;
                    }
                    else
                    {
                        panelHideSpeechTriangle.Visible = true;
                    }
                }
                else // shrink further
                {
                    pictureBoxRobot.Invalidate();
                    panelSpeechBubble.Invoke((MethodInvoker)delegate
                    {
                        panelSpeechBubble.Height = SpeechBubblecurrentHeight;
                        panelSpeechBubble.Location = new Point(panelSpeechBubble.Location.X, panelSpeechBubble.Location.Y + 2);
                        panelSpeechBubble.Invalidate();
                        if (panelConvertContainer.Visible || panelDeleteTXContainer.Visible || panelHelpTextContainer.Visible || panelResetContainer.Visible || panelSummaryContainer.Visible || panelSettingsContainer.Visible)
                        {
                            panelHideSpeechTriangle.Visible = false;
                        }
                        else
                        {
                            panelHideSpeechTriangle.Visible = true;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "ShrinkRobotTimer_Tick");
            }
        }

        private void StartShrinkingRobot()
        {
            try
            {
                SpeechBubblecurrentHeight = (int)(93 * UIScale);
                panelSpeechBorderRobot1.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorderRobot1.Visible = false;
                });
                panelSpeechBorderRobot2.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorderRobot2.Visible = false;
                });
                panelSpeechBorderRobot3.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBorderRobot3.Visible = false;
                });
                ShrinkRobotTimer.Start();
            }
            catch (Exception ex)
            {
                HandleException(ex, "StartShrinkingRobot");
            }
        }

        private void ExpandRobotTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                SpeechBubblecurrentHeight += 4;
                if (SpeechBubblecurrentHeight >= (int)(97 * UIScale)) // expanding is complete
                {
                    ExpandRobotTimer.Stop();
                    expandRobotTimerRunning = false;
                    /*
                    panelSpeechBorderRobot1.Invoke((MethodInvoker)delegate
                    {
                        panelSpeechBorderRobot1.Location = new Point(panelSpeechBubble.Location.X - 1, panelSpeechBubble.Location.Y - 1);
                        panelSpeechBorderRobot1.Height = panelSpeechBubble.Height - (int)(13 * UIScale);
                        panelSpeechBorderRobot1.Width = panelSpeechBubble.Width + 2;
                        panelSpeechBorderRobot1.Visible = true;
                    });
                    panelSpeechBorderRobot2.Invoke((MethodInvoker)delegate
                    {
                        panelSpeechBorderRobot2.Location = new Point(panelSpeechBubble.Location.X - 1, panelSpeechBubble.Location.Y - 1);
                        panelSpeechBorderRobot2.Height = panelSpeechBubble.Height + 2;
                        panelSpeechBorderRobot2.Width = (int)(50 * UIScale);
                        panelSpeechBorderRobot2.Visible = true;
                    });
                    panelSpeechBorderRobot3.Invoke((MethodInvoker)delegate
                    {
                        panelSpeechBorderRobot3.Location = new Point(panelSpeechBubble.Location.X - 1, (int)(99 * UIScale));
                        panelSpeechBorderRobot3.Height = (int)(17 * UIScale);
                        panelSpeechBorderRobot3.Width = panelSpeechBubble.Width + 2;
                        panelSpeechBorderRobot3.Visible = true;
                    });
                    */
                }

                else // expand further
                {

                    panelSpeechBubble.Invoke((MethodInvoker)delegate
                    {
                        panelSpeechBubble.Height = SpeechBubblecurrentHeight;
                        panelSpeechBubble.Invalidate();
                        panelSpeechBubble.Location = new Point(panelSpeechBubble.Location.X, panelSpeechBubble.Location.Y - 2);
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "ExpandRobotTimer_Tick");
            }
        }

        private void StartExpandingRobot()
        {
            try
            {
                panelSpeechBubble.Height = 0;
                ExpandRobotTimer.Start();
                expandRobotTimerRunning = true;
            }
            catch (Exception ex)
            {
                HandleException(ex, "StartExpandingRobot");
            }
        }

        private async Task WaitForExpandRobotTimerToStop()
        {
            try
            {
                while (expandRobotTimerRunning)
                {
                    await Task.Delay(100);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "WaitForExpandRobotTimerToStop");
            }
        }

        #endregion

        #endregion

        #region rounded form & panels

        private void BitcoinCBC_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                #region rounded border around rounded form
                // Paint the border with a 1-pixel width
                using var pen = new Pen(Color.FromArgb(255, 192, 128), 6);
                var rect = ClientRectangle;
                rect.Inflate(-1, -1);
                e.Graphics.DrawPath(pen, GetRoundedRect(rect, 25));
                #endregion
                int orangeStripeHeight = ((int)(759 * UIScale)) - 5;
                panel40.Invoke((MethodInvoker)delegate
                {
                    panel40.Location = new Point(panel40.Location.X, orangeStripeHeight);
                });
                panel40.BringToFront();
            }
            catch (Exception ex)
            {
                HandleException(ex, "Form_Paint");
            }
        }

        private void Panel_Paint(object? sender, PaintEventArgs e)
        {
            try
            {
                if (sender == null)
                {
                    return;
                }
                Panel panel = (Panel)sender;

                // Create a GraphicsPath object with rounded corners
                System.Drawing.Drawing2D.GraphicsPath path = new();
                int cornerRadius = (int)(18 * UIScale);
                path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
                path.AddArc(panel.Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
                path.AddArc(panel.Width - cornerRadius, panel.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
                path.AddArc(0, panel.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
                path.CloseFigure();

                // Set the panel's region to the rounded path
                panel.Region = new Region(path);
            }
            catch (Exception ex)
            {
                HandleException(ex, "Panel_Paint");
            }
        }

        private static GraphicsPath GetRoundedRect(Rectangle rectangle, int radius)
        {
            GraphicsPath path = new();
            path.AddArc(rectangle.X, rectangle.Y, radius, radius, 180, 90);
            path.AddArc(rectangle.Width - radius, rectangle.Y, radius, radius, 270, 90);
            path.AddArc(rectangle.Width - radius, rectangle.Height - radius, radius, radius, 0, 90);
            path.AddArc(rectangle.X, rectangle.Height - radius, radius, radius, 90, 90);
            path.CloseFigure();
            return path;
        }

        #endregion

        #region vertically expanding panels animation

        private void StartExpandingPanelVert(Panel panel)
        {
            try
            {
                panelToExpandVert = panel;
                ExpandPanelTimerVert.Start();
            }
            catch (Exception ex)
            {
                HandleException(ex, "StartExpandingPanelVert");
            }
        }

        private void StartShrinkingPanelVert(Panel panel)
        {
            try
            {
                panelToShrinkVert = panel;
                ShrinkPanelTimerVert.Start();
            }
            catch (Exception ex)
            {
                HandleException(ex, "StartShrinkingPanelVert");
            }
        }

        private void ExpandPanelTimerVert_Tick(object sender, EventArgs e)
        {
            try
            {
                if (panelToExpandVert == panelCurrency)
                {
                    currentHeightExpandingPanel += 8;
                    panelMaxHeight = ((int)(128 * UIScale));
                }
                if (panelToExpandVert == panelColors)
                {
                    currentHeightExpandingPanel += 8;
                    panelMaxHeight = ((int)(129 * UIScale));
                }
                if (panelToExpandVert == panelChartOuter)
                {
                    currentHeightExpandingPanel += 10;
                    panelMaxHeight = ((int)(692 * UIScale));

                }
                if (currentHeightExpandingPanel >= panelMaxHeight) // expanding is complete
                {
                    panelToExpandVert.Invoke((MethodInvoker)delegate
                    {
                        panelToExpandVert.Height = panelMaxHeight;
                        panelToExpandVert.Invalidate();
                    });
                    ExpandPanelTimerVert.Stop();
                    if (chartChangingSize == true)
                    {
                        chartChangingSize = false;
                        btnExpandShrinkChart.Enabled = true;
                        btnExpandShrinkList.Enabled = true;
                    }
                    if (listChangingSize == true)
                    {
                        listChangingSize = false;
                        btnExpandShrinkList.Enabled = true;
                        btnExpandShrinkChart.Enabled = true;
                    }
                }
                else // expand further
                {
                    panelToExpandVert.Invoke((MethodInvoker)delegate
                    {
                        panelToExpandVert.Height = currentHeightExpandingPanel;
                        panelToExpandVert.Invalidate();
                    });

                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "ExpandPanelTimerVert_Tick");
            }
        }

        private void ShrinkPanelTimerVert_Tick(object sender, EventArgs e)
        {
            try
            {
                if (panelToShrinkVert == panelCurrency || panelToShrinkVert == panelColors)
                {
                    currentHeightShrinkingPanel -= 8;
                    panelMinHeight = 0;
                }

                if (panelToShrinkVert == panelChartOuter)
                {
                    currentHeightShrinkingPanel -= 800;
                    panelMinHeight = ((int)(435 * UIScale));
                }
                if (currentHeightShrinkingPanel <= panelMinHeight) // shrinking is complete
                {
                    panelToShrinkVert.Invoke((MethodInvoker)delegate
                    {
                        panelToShrinkVert.Height = panelMinHeight;
                    });
                    ShrinkPanelTimerVert.Stop();
                    if (chartChangingSize == true)
                    {
                        chartChangingSize = false;
                        panelChartContainer.Invoke((MethodInvoker)delegate
                        {
                            panelChartContainer.Height = ((int)(433 * UIScale));
                        });
                        btnExpandShrinkChart.Invoke((MethodInvoker)delegate
                        {
                            btnExpandShrinkChart.Enabled = true;
                        });
                        btnExpandShrinkList.Invoke((MethodInvoker)delegate
                        {
                            btnExpandShrinkList.Enabled = true;
                        });
                    }
                    if (listChangingSize == true)
                    {
                        listChangingSize = false;
                        btnExpandShrinkChart.Invoke((MethodInvoker)delegate
                        {
                            btnExpandShrinkChart.Enabled = true;
                        });
                        btnExpandShrinkList.Invoke((MethodInvoker)delegate
                        {
                            btnExpandShrinkList.Enabled = true;
                        });
                    }
                }

                else // shrink further
                {
                    panelToShrinkVert.Invoke((MethodInvoker)delegate
                    {
                        panelToShrinkVert.Height = currentHeightShrinkingPanel;
                        panelToShrinkVert.Invalidate();
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "ShrinkPanelTimerVert_Tick");
            }
        }

        #endregion

        #region vertically moving panels animation

        private void StartMovingPanelUp(Panel panel)
        {
            try
            {
                panelToMoveUp = panel;
                MovePanelUpTimer.Start();
            }
            catch (Exception ex)
            {
                HandleException(ex, "StartMovingPanelUp");
            }
        }

        private void StartMovingPanelDown(Panel panel)
        {
            try
            {
                panelToMoveDown = panel;
                MovePanelDownTimer.Start();
            }
            catch (Exception ex)
            {
                HandleException(ex, "StartMovingPanelDown");
            }
        }

        private void MovePanelUpTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (panelToMoveUp == panelTXListOuter)
                {
                    currentLocationMovingPanel -= (int)(800 * UIScale);
                    panelTopLocation = ((int)(55 * UIScale));
                }
                if (currentLocationMovingPanel <= panelTopLocation) // moving up is complete
                {
                    panelTransactionsContainer.Invoke((MethodInvoker)delegate
                    {
                        panelTransactionsContainer.AutoScroll = true;
                    });
                    panelToMoveUp.Invoke((MethodInvoker)delegate
                    {
                        panelToMoveUp.Location = new Point(panelToMoveUp.Location.X, panelTopLocation);
                    });
                    MovePanelUpTimer.Stop();
                    if (listChangingSize == true)
                    {
                        listChangingSize = false;
                        btnExpandShrinkChart.Enabled = true;
                        btnExpandShrinkList.Enabled = true;
                    }
                    panelScrollbarContainer.Invoke((MethodInvoker)delegate
                    {
                        panelScrollbarContainer.Location = new Point(panelScrollbarContainer.Location.X, panelTXListOuter.Location.Y + 1);
                        panelScrollbarContainer.Height = panelTransactionsContainer.Height + (int)(29 * UIScale);
                        panelScrollbarContainer.Invalidate();
                    });
                    vScrollBar1.Invoke((MethodInvoker)delegate
                    {
                        vScrollBar1.Height = panelScrollbarContainer.Height - (int)(27 * UIScale);
                    });
                }
                else // keep moving up
                {
                    panelToMoveUp.Invoke((MethodInvoker)delegate
                    {
                        panelToMoveUp.Location = new Point(panelToMoveUp.Location.X, currentLocationMovingPanel);
                        panelToMoveUp.Invalidate();
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "MovePanelUpTimer_Tick");
            }
        }

        private void MovePanelDownTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                currentLocationMovingPanel += 800;
                if (panelToMoveDown == panelTXListOuter)
                {
                    panelBottomLocation = ((int)(510 * UIScale));
                }
                if (currentLocationMovingPanel >= panelBottomLocation) // moving down is complete
                {
                    panelTransactionsContainer.Invoke((MethodInvoker)delegate
                    {
                        panelTransactionsContainer.AutoScroll = true;
                    });
                    panelToMoveDown.Invoke((MethodInvoker)delegate
                    {
                        panelToMoveDown.Location = new Point(panelToMoveDown.Location.X, panelBottomLocation);
                    });
                    MovePanelDownTimer.Stop();
                    panelTXListOuter.Height = ((int)(235 * UIScale));
                    panelTransactionsContainer.Height = ((int)(177 * UIScale));
                    listViewTransactions.Height = ((int)(177 * UIScale));
                    panelScrollbarContainer.Height = ((int)(206 * UIScale));
                    if (listChangingSize == true)
                    {
                        listChangingSize = false;
                        btnExpandShrinkChart.Enabled = true;
                        btnExpandShrinkList.Enabled = true;
                    }
                    panelScrollbarContainer.Invoke((MethodInvoker)delegate
                    {
                        panelScrollbarContainer.Location = new Point(panelScrollbarContainer.Location.X, panelTXListOuter.Location.Y + 1);
                        panelScrollbarContainer.Height = panelTransactionsContainer.Height + (int)(29 * UIScale);
                        panelScrollbarContainer.Invalidate();
                    });
                    vScrollBar1.Invoke((MethodInvoker)delegate
                    {
                        vScrollBar1.Height = panelScrollbarContainer.Height - (int)(27 * UIScale);
                    });
                }
                else // keep moving down
                {
                    panelToMoveDown.Invoke((MethodInvoker)delegate
                    {
                        panelToMoveDown.Location = new Point(panelToMoveDown.Location.X, currentLocationMovingPanel);
                        panelToMoveDown.Invalidate();
                    });
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "MovePanelDownTimer_Tick");
            }
        }

        #endregion

        #region error handler

        private void HandleException(Exception ex, string methodName)
        {
            string errorMessage;
            if (ex is WebException)
            {
                errorMessage = $"Web exception in {methodName}: {ex.Message}";
            }
            else if (ex is HttpRequestException)
            {
                errorMessage = $"HTTP Request error in {methodName}: {ex.Message}";
            }
            else if (ex is JsonException)
            {
                errorMessage = $"JSON parsing error in {methodName}: {ex.Message}";
            }
            else
            {
                errorMessage = $"Error in {methodName}: {ex.Message}";
            }

            const int MaxErrorMessageLength = 100;

            if (errorMessage.Length > MaxErrorMessageLength)
            {
                errorMessage = errorMessage[..MaxErrorMessageLength] + "...";
            }
            panelWelcome.Visible = false;
            panelRobotSpeakOuter.Visible = true;
            InterruptAndStartNewRobotSpeak(errorMessage);
        }

        #endregion

        #region UI Scaling
        private void storeOriginalDimensions(Control parentControl)
        {
            try
            {
                foreach (Control control in parentControl.Controls)
                {
                    control.Tag = new Size(control.Width, control.Height);

                    // Store the original font size of each control
                    if (control.Font != null)
                    {
                        control.Tag = new Tuple<Size, Font>((Size)control.Tag, control.Font);
                    }

                    // Recursively handle controls within panels
                    if (control.HasChildren)
                    {
                        storeOriginalDimensions(control);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, "storeOriginalDimensions");
            }
        }

        private void ScaleAllElements_Click(object sender, EventArgs e)
        {
            btnScaleApply(this);
        }

        private void btnScaleApply(Control parentControl)
        {
            try
            {
                // Resize each control within the provided parentControl
                foreach (Control control in parentControl.Controls)
                {
                    Size originalSize = ((Tuple<Size, Font>)control.Tag).Item1;

                    // apply a scaled radius to buttons to keep them fully rounded
                    if (control.GetType() == typeof(RJButton))
                    {
                        RJButton rjButton = (RJButton)control;
                        // Set the borderRadius to a different value for each RJButton
                        rjButton.BorderRadius = (int)(11 * UIScale);
                    }

                    if (control.Name != formsPlot1.Name && control.Name != pictureBoxRobot.Name)
                    {
                        control.Width = (int)(originalSize.Width * UIScale);
                        control.Left = (int)(control.Left * UIScale);
                        control.Top = (int)(control.Top * UIScale);
                        control.Height = (int)(originalSize.Height * UIScale);
                    }
                    if (control.Name == pictureBoxRobot.Name)
                    {
                        control.Width = (int)(originalSize.Width * UIScale);
                        control.Height = (int)(originalSize.Height * UIScale);
                        control.Left = panel16.Width - pictureBoxRobot.Width;
                        control.Top = panel16.Height - pictureBoxRobot.Height;
                    }

                    // Resize font size
                    if (control.Font != null)
                    {
                        Font originalControlFont = ((Tuple<Size, Font>)control.Tag).Item2;
                        float scaledFontSize = (float)(originalControlFont.Size * UIScale);
                        control.Font = new Font(originalControlFont.FontFamily, scaledFontSize - 1, originalControlFont.Style);
                    }

                    // Recursively handle controls within panels
                    if (control.HasChildren)
                    {
                        btnScaleApply(control);
                    }
                }

                // Trigger layout update for child controls
                parentControl.PerformLayout();
                parentControl.Invalidate();
            }
            catch (Exception ex)
            {
                HandleException(ex, "btnScaleApply");
            }
        }

        #endregion

        #endregion

        #region classes

        public class HistoricPriceDataService
        {
            public static async Task<string> GetHistoricPriceDataAsync()
            {
                int retryCount = 3;
                while (retryCount > 0)
                {
                    using var client = new HttpClient();
                    try
                    {
                        client.BaseAddress = new Uri("https://api.blockchain.info/");
                        string blockChainInfoPeriod = "all";
                        var response = await client.GetAsync($"charts/market-price?timespan=" + blockChainInfoPeriod + "&format=json");
                        if (response.IsSuccessStatusCode)
                        {
                            return await response.Content.ReadAsStringAsync();
                        }
                        retryCount--;
                        await Task.Delay(3000);
                    }
                    catch (HttpRequestException)
                    {
                        retryCount--;
                        await Task.Delay(3000);
                    }
                }
                return string.Empty;
            }
        }

        public class PriceCoordsAndFormattedDateList
        {
            public string? X { get; set; } //unix date
            public decimal Y { get; set; } // USD price
            public string? FormattedDate { get; set; } // formatted yyyymmdd
        }

        public class PriceCoordinatesList
        {
            public string? X { get; set; }
            public decimal Y { get; set; }
        }

        // transactions file
        public class Transaction
        {
            public DateTime DateAdded { get; set; } // auto populated
            public string? TransactionType { get; set; } // Buy/Sell
            public string? Year { get; set; }
            public string? Month { get; set; }
            public string? Day { get; set; }
            public string? Price { get; set; }
            public string? EstimateType { get; set; } // N - no estimate, AM - Annual median, MM - Monthly median, DA - Daily average
            public string? EstimateRange { get; set; } // percentage
            public string? FiatAmount { get; set; }
            public string? FiatAmountEstimateFlag { get; set; }
            public string? BTCAmount { get; set; }
            public string? BTCAmountEstimateFlag { get; set; }
            public string? Label { get; set; }
            public string? LabelColor { get; set; }
        }

        // settings file
        public class Settings
        {
            public DateTime DateAdded { get; set; }
            public string? Type { get; set; }
            public string? Data { get; set; }
        }

        #endregion
    }
}
