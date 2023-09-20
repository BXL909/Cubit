/*  
╔═╗╦ ╦╔╗ ╦╔╦╗
║  ║ ║╠╩╗║ ║ 
╚═╝╚═╝╚═╝╩ ╩
pictures in robot speech
currency wording on summary screen
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
using static ScottPlot.Plottable.PopulationPlot;
using System.Windows.Forms;
using System.Runtime.Intrinsics.X86;
#endregion

namespace Cubit
{
    public partial class Cubit : Form
    {
        readonly string CurrentVersion = "0.9";

        #region variable declaration
        List<PriceCoordsAndFormattedDateList> HistoricPrices = new();
        readonly string[] months = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        readonly string[] monthsNumeric = { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12" };
        int selectedYear = 0;
        string selectedMonth = "";
        int selectedMonthNumeric = 0;
        string selectedDay = "";
        int selectedDayNumeric = 0;
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
        private bool expandRobotTimerRunning = false; // Robot - text is expanding
        bool robotIgnoreChanges = false; // Robot - suppress animation
        string priceEstimateType = "N"; // will hold the estimate type
        private Panel panelToExpandVert = new(); // panel animation vertical
        private Panel panelToShrinkVert = new(); // panel animation vertical
        private int panelMaxHeight = 0; // panel animation vertical
        private int panelMinHeight = 0; // panel animation vertical
        private int currentHeightExpandingPanel = 0; // panel animation vertical
        private int currentHeightShrinkingPanel = 0; // panel animation vertical
        bool initialCurrencySetup = true; // used to avoid robot message when currency is initially set
        bool currencyAlreadySavedInFile = false; // is ANY currency saved in the settings file
        string selectedCurrencyName = "USD"; // initial value. Can be changed and saved
        string currencyInFile = "USD"; // the currency save in the settings file
        string selectedCurrencySymbol = "$"; // initial value.
        decimal priceInSelectedCurrency = 0; // price of 1 btc in selected currency
        decimal exchangeRate = 1; // used to recalculate prices to selected currency
        private readonly List<string> welcomeMessages = new()
        {
            "🎵Never gonna give you up, Never gonna let you down🎵",
            "Who thought Cubit was a good name for a robot?",
            "Greetings, human.",
            "I'm here to assist you!",
            "1 BTC = 1 BTC",
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
            "Beep boop beep! I am a robot.",
            "I lost all my bitcoin in a spaceship accident.",
            "I'm reading a book on gravity. I can't put it down.",
        }; // silly robot text when clicked
        List<double> listBuyBTCTransactionDate = new();
        List<double> listBuyBTCTransactionFiatAmount = new();
        List<double> listBuyBTCTransactionPrice = new();
        List<double> listSellBTCTransactionDate = new();
        List<double> listSellBTCTransactionFiatAmount = new();
        List<double> listSellBTCTransactionPrice = new();
        #region chart variables
        bool safeToTrackPriceOnChart = false;
        private int LastHighlightedIndex = -1; // used by charts for mousemove events to highlight plots closest to pointer
        private ScottPlot.Plottable.ScatterPlot? scatter; // chart data gets plotted onto this
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

            #region rounded panels
            panel1.Paint += Panel_Paint;
            panel2.Paint += Panel_Paint;
            panel3.Paint += Panel_Paint;
            panel4.Paint += Panel_Paint;
            panel5.Paint += Panel_Paint;
            panel6.Paint += Panel_Paint;
            panel7.Paint += Panel_Paint;
            panel8.Paint += Panel_Paint;
            panelHelpTextContainer.Paint += Panel_Paint;
            panelAddTransaction.Paint += Panel_Paint;
            panel13.Paint += Panel_Paint;
            panel14.Paint += Panel_Paint;
            panelChartContainer.Paint += Panel_Paint;
            panelHelp.Paint += Panel_Paint;
            panel18.Paint += Panel_Paint;
            panel21.Paint += Panel_Paint;
            panel22.Paint += Panel_Paint;
            panel23.Paint += Panel_Paint;
            panel24.Paint += Panel_Paint;
            panel25.Paint += Panel_Paint;
            panel27.Paint += Panel_Paint;
            panel28.Paint += Panel_Paint;
            panel29.Paint += Panel_Paint;
            panel30.Paint += Panel_Paint;
            panel32.Paint += Panel_Paint;
            panelSummaryChangeInValue.Paint += Panel_Paint;
            panelSummaryBTCHeld.Paint += Panel_Paint;
            panelSummaryBuyTransactions.Paint += Panel_Paint;
            panelSummarySellTransactions.Paint += Panel_Paint;
            panelSummary.Paint += Panel_Paint;
            panelSummaryContainer.Paint += Panel_Paint;
            panelTransactionLabel.Paint += Panel_Paint;
            panelColors.Paint += Panel_Paint;
            panelColorMenu.Paint += Panel_Paint;
            panelRobotSpeakOuter.Paint += Panel_Paint;
            panelWelcome.Paint += Panel_Paint;
            panelCurrencyMenu.Paint += Panel_Paint;
            panelCurrency.Paint += Panel_Paint;
            panelTXListFooter.Paint += Panel_Paint;
            panelAddTransactionContainer.Paint += Panel_Paint;
            panelScrollbarContainer.Paint += Panel_Paint;
            panelSpeechBubble.Paint += Panel_Paint;
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
        private async void BitcoinCBC_Load(object sender, EventArgs e)
        {
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

            #region get historic price list
            await GetHistoricPricesAsyncWrapper();
            #endregion

            #region start any required timers
            blinkTimer.Start(); // used only to make robot blink
            #endregion

            #region rounded panels
            panel1.Invalidate();
            panel2.Invalidate();
            panel3.Invalidate();
            panel4.Invalidate();
            panel5.Invalidate();
            panel6.Invalidate();
            panel7.Invalidate();
            panel8.Invalidate();
            panelHelpTextContainer.Invalidate();
            panelAddTransaction.Invalidate();
            panel13.Invalidate();
            panel14.Invalidate();
            panelChartContainer.Invalidate();
            panelHelp.Invalidate();
            panel18.Invalidate();
            panel21.Invalidate();
            panel22.Invalidate();
            panel23.Invalidate();
            panel24.Invalidate();
            panel25.Invalidate();
            panel27.Invalidate();
            panel28.Invalidate();
            panel29.Invalidate();
            panel30.Invalidate();
            panel32.Invalidate();
            panelSummaryChangeInValue.Invalidate();
            panelSummaryBTCHeld.Invalidate();
            panelSummaryBuyTransactions.Invalidate();
            panelSummarySellTransactions.Invalidate();
            panelSummary.Invalidate();
            panelSummaryContainer.Invalidate();
            panelTransactionLabel.Invalidate();
            panelColorMenu.Invalidate();
            panelColors.Invalidate();
            panelRobotSpeakOuter.Invalidate();
            panelWelcome.Invalidate();
            panelCurrency.Invalidate();
            panelCurrencyMenu.Invalidate();
            panelAddTransactionContainer.Invalidate();
            panelScrollbarContainer.Invalidate();
            panelTXListFooter.Invalidate();
            panelSpeechBubble.Invalidate();
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
        #endregion

        #region settings (currency) file operations

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

            // Write the JSON string to the settings.json file
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
                    InterruptAndStartNewRobotSpeak("OK. I'm ready to accept a transaction where you sold/spent bitcoin.");
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
                    InterruptAndStartNewRobotSpeak("OK. I'm ready to accept a transaction where you bought/received bitcoin.");
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
        #endregion

        #region transaction amounts input
        private void CopyPriceEstimateToInputIfNecessary(string estimate)
        {
            if (btnUsePriceEstimateFlag.Text == "✔️")
            {
                textBoxPriceInput.Invoke((MethodInvoker)delegate
                {
                    textBoxPriceInput.Text = Convert.ToString(estimate);
                });
            }
        }

        private void CopyFiatEstimateToInputIfNecessary(string estimate)
        {
            if (btnUseFiatEstimateFlag.Text == "✔️")
            {
                textBoxFiatInput.Invoke((MethodInvoker)delegate
                {
                    textBoxFiatInput.Text = Convert.ToString(estimate);
                });
            }
        }

        private void CopyBTCEstimateToInputIfNecessary(string estimate)
        {
            if (btnUseBTCEstimateFlag.Text == "✔️")
            {
                textBoxBTCInput.Invoke((MethodInvoker)delegate
                {
                    textBoxBTCInput.Text = Convert.ToString(estimate);
                });
            }
        }

        private void TextBoxPriceInput_TextChanged(object sender, EventArgs e)
        {
            lblAddDataPrice.Invoke((MethodInvoker)delegate
            {
                lblAddDataPrice.Text = textBoxPriceInput.Text;
            });
        }

        private void TextBoxFiatInput_TextChanged(object sender, EventArgs e)
        {
            lblAddDataFiat.Invoke((MethodInvoker)delegate
            {
                lblAddDataFiat.Text = textBoxFiatInput.Text;
            });
            if (lblAddDataFiat.Text != "")
            {
                decimal estimatedBTC = Math.Round((Convert.ToDecimal(lblAddDataFiat.Text) / Convert.ToDecimal(lblAddDataPrice.Text)), 8);
                lblEstimatedBTC.Invoke((MethodInvoker)delegate
                {
                    lblEstimatedBTC.Text = "(" + Convert.ToString((decimal)estimatedBTC) + ")";
                });
                CopyBTCEstimateToInputIfNecessary(lblEstimatedBTC.Text.Trim('(', ')'));
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

        private void TextBoxBTCInput_TextChanged(object sender, EventArgs e)
        {
            lblAddDataBTC.Invoke((MethodInvoker)delegate
            {
                lblAddDataBTC.Text = textBoxBTCInput.Text;
            });
            if (lblAddDataBTC.Text != "")
            {
                decimal estimatedFiat = Math.Round((Convert.ToDecimal(lblAddDataPrice.Text) * Convert.ToDecimal(lblAddDataBTC.Text)), 2);
                lblEstimatedFiat.Invoke((MethodInvoker)delegate
                {
                    lblEstimatedFiat.Text = "(" + Convert.ToString((decimal)estimatedFiat) + ")";
                });
                CopyFiatEstimateToInputIfNecessary(lblEstimatedFiat.Text.Trim('(', ')'));
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

        private void BtnUsePriceEstimateFlag_Click(object sender, EventArgs e)
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
        }

        private void BtnUseFiatEstimateFlag_Click_1(object sender, EventArgs e)
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

        private void BtnUseBTCEstimateFlag_Click(object sender, EventArgs e)
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

        private void BtnRed1_Click(object sender, EventArgs e)
        {
            btnLabelColor.Invoke((MethodInvoker)delegate
            {
                btnLabelColor.BackColor = btnRed1.BackColor;
            });
            selectedColor = "1";
            BtnLabelColor_Click(sender, e);
        }

        private void BtnGreen2_Click(object sender, EventArgs e)
        {
            btnLabelColor.Invoke((MethodInvoker)delegate
            {
                btnLabelColor.BackColor = btnGreen2.BackColor;
            });
            selectedColor = "2";
            BtnLabelColor_Click(sender, e);
        }

        private void BtnBlue3_Click(object sender, EventArgs e)
        {
            btnLabelColor.Invoke((MethodInvoker)delegate
            {
                btnLabelColor.BackColor = btnBlue3.BackColor;
            });
            selectedColor = "3";
            BtnLabelColor_Click(sender, e);
        }

        private void BtnPink4_Click(object sender, EventArgs e)
        {
            btnLabelColor.Invoke((MethodInvoker)delegate
            {
                btnLabelColor.BackColor = btnPink4.BackColor;
            });
            selectedColor = "4";
            BtnLabelColor_Click(sender, e);
        }

        private void BtnBrown5_Click(object sender, EventArgs e)
        {
            btnLabelColor.Invoke((MethodInvoker)delegate
            {
                btnLabelColor.BackColor = btnBrown5.BackColor;
            });
            selectedColor = "5";
            BtnLabelColor_Click(sender, e);
        }

        private void BtnPurple6_Click(object sender, EventArgs e)
        {
            btnLabelColor.Invoke((MethodInvoker)delegate
            {
                btnLabelColor.BackColor = btnPurple6.BackColor;
            });
            selectedColor = "6";
            BtnLabelColor_Click(sender, e);
        }

        private void BtnYellow7_Click(object sender, EventArgs e)
        {
            btnLabelColor.Invoke((MethodInvoker)delegate
            {
                btnLabelColor.BackColor = btnYellow7.BackColor;
            });
            selectedColor = "7";
            BtnLabelColor_Click(sender, e);
        }

        private void BtnOrange8_Click(object sender, EventArgs e)
        {
            btnLabelColor.Invoke((MethodInvoker)delegate
            {
                btnLabelColor.BackColor = btnOrange8.BackColor;
            });
            selectedColor = "8";
            BtnLabelColor_Click(sender, e);
        }
        #endregion

        #region clear input fields
        private void BtnClearInput_Click(object sender, EventArgs e)
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
            InterruptAndStartNewRobotSpeak("Ready for a new transaction!");
        }
        #endregion

        #region check conditions to enable 'add' button

        private void CheckConditionsToEnableAddButton_TextChanged(object sender, EventArgs e)
        {
            if (lblAddDataYear.Text != "" && lblAddDataYear.Text != "-"
            && lblAddDataPrice.Text != "" && lblAddDataPrice.Text != "-"
            && lblAddDataFiat.Text != "" && lblAddDataFiat.Text != "-"
            && lblAddDataBTC.Text != "" && lblAddDataBTC.Text != "-")
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

        #endregion

        #region delete transaction         

        private void BtnDeleteTransaction_Click(object sender, EventArgs e)
        {
            btnDeleteTransaction.Invoke((MethodInvoker)delegate
            {
                btnDeleteTransaction.Visible = false;
            });
            btnConfirmDelete.Invoke((MethodInvoker)delegate
            {
                btnConfirmDelete.Visible = true;
            });
            btnCancelDelete.Invoke((MethodInvoker)delegate
            {
                btnCancelDelete.Visible = true;
            });

            string robotConfirmation = "";

            if (listViewTransactions.SelectedItems.Count > 0)
            {
                ListViewItem selectedItem = listViewTransactions.SelectedItems[0]; // Get selected row

                string transactionNumber = selectedItem.SubItems[0].Text; // 1st column
                string dateAdded = selectedItem.SubItems[17].Text; // 18th column
                TXDataToDelete = selectedItem.SubItems[18].Text;
                string btcamount = selectedItem.SubItems[9].Text;
                string txdate = selectedItem.SubItems[1].Text + "/" + selectedItem.SubItems[2].Text + "/" + selectedItem.SubItems[3].Text;
                robotConfirmation = "Are you sure you want to delete this transaction for " + btcamount + " bitcoin on " + txdate + "?";
            }
            panelWelcome.Visible = false;
            panelRobotSpeakOuter.Visible = true;
            InterruptAndStartNewRobotSpeak(robotConfirmation);
        }

        private async void BtnConfirmDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DeleteTransactionFromJsonFile(TXDataToDelete);
                await SetupTransactionsList();
                DrawPriceChart();
                panelWelcome.Visible = false;
                panelRobotSpeakOuter.Visible = true;
                InterruptAndStartNewRobotSpeak("Transaction deleted.");
                TXDataToDelete = "";
                BtnCancelDelete_Click(sender, e); // revert buttons back to original state
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnConfirmDelete_Click");
            }
        }

        private void BtnCancelDelete_Click(object sender, EventArgs e)
        {
            btnDeleteTransaction.Invoke((MethodInvoker)delegate
            {
                btnDeleteTransaction.Visible = true;
            });
            btnConfirmDelete.Invoke((MethodInvoker)delegate
            {
                btnConfirmDelete.Visible = false;
            });
            btnCancelDelete.Invoke((MethodInvoker)delegate
            {
                btnCancelDelete.Visible = false;
            });
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
                    lblCurrentPrice.Text = Convert.ToDecimal(priceInSelectedCurrency).ToString("0.00");
                    lblCurrentPrice.Location = new Point(btnPriceRefresh.Location.X - lblCurrentPrice.Width, lblCurrentPrice.Location.Y);
                });
                pictureBoxBTCLogo.Invoke((MethodInvoker)delegate
                {
                    pictureBoxBTCLogo.Location = new Point(lblCurrentPrice.Location.X - pictureBoxBTCLogo.Width, pictureBoxBTCLogo.Location.Y);
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
                selectedYear = 2008 + (int)comboBoxYearInput.SelectedIndex + 1;

                List<PriceCoordsAndFormattedDateList> PricesForSelectedYear = HistoricPrices
                    .Where(pricelist => pricelist.FormattedDate?.Length >= 4 && Convert.ToInt16(pricelist.FormattedDate[..4]) == selectedYear)
                    .ToList();

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
                InterruptAndStartNewRobotSpeak("The median price of 1 bitcoin through " + selectedYear + " was $" + Convert.ToString(median) + ", with a range of +/-" + Convert.ToString(range));
            }
            else
            {
                if (comboBoxMonthInput.SelectedIndex > 0 && comboBoxDayInput.SelectedIndex < 1) // year and month have been set
                {
                    selectedYear = 2008 + (int)comboBoxYearInput.SelectedIndex + 1;
                    selectedMonthNumeric = Convert.ToInt16(monthsNumeric[comboBoxMonthInput.SelectedIndex - 1]);
                    selectedMonth = months[comboBoxMonthInput.SelectedIndex - 1];

                    List<PriceCoordsAndFormattedDateList> PricesForSelectedYearMonth = HistoricPrices
                    .Where(pricelist => Convert.ToInt16(pricelist.FormattedDate?[..4]) == selectedYear)
                    .Where(pricelist => Convert.ToInt16(pricelist.FormattedDate?.Substring(4, 2)) == selectedMonthNumeric)
                    .ToList();

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
                    InterruptAndStartNewRobotSpeak("The median price of 1 bitcoin through " + selectedMonth + " " + selectedYear + " was $" + Convert.ToString(median) + ", with a range of +/-" + Convert.ToString(range));
                }
                else
                {
                    if (comboBoxMonthInput.SelectedIndex > 0 && comboBoxDayInput.SelectedIndex > 0) // year, month and day have been set
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
                                InterruptAndStartNewRobotSpeak("The average price of 1 bitcoin for " + selectedDay + " " + selectedMonth + ", " + selectedYear + " was $" + estimatedPrice);
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
                                InterruptAndStartNewRobotSpeak("The average price of 1 bitcoin for " + selectedDay + " " + selectedMonth + ", " + selectedYear + " was $0.0");
                            }
                        }
                        else
                        {
                            panelWelcome.Visible = false;
                            panelRobotSpeakOuter.Visible = true;
                            InterruptAndStartNewRobotSpeak($"Failed to fetch data. Status code: {response.StatusCode}");
                        }
                    }
                }
            }
        }

        #endregion

        #region refresh current price

        private async void BtnPriceRefresh_Click(object sender, EventArgs e)
        {
            HistoricPrices.Clear();
            await GetHistoricPricesAsyncWrapper();
            await SetupTransactionsList();
            InitializeChart();
            DrawPriceChart();
            panelWelcome.Visible = false;
            panelRobotSpeakOuter.Visible = true;
            InterruptAndStartNewRobotSpeak("Retrieved up to date price and adjusted my calculations.");
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
            DateTime today = DateTime.Now;
            string transactionType = "Sell";
            if (btnBoughtBitcoin.Text == "✔️")
            {
                transactionType = "Buy";
            }
            var newTransaction = new Transaction { DateAdded = today, TransactionType = transactionType, Year = lblAddDataYear.Text, Month = lblAddDataMonth.Text, Day = lblAddDataDay.Text, Price = lblAddDataPrice.Text, EstimateType = lblAddDataPriceEstimateType.Text, EstimateRange = lblAddDataRange.Text, FiatAmount = lblAddDataFiat.Text, FiatAmountEstimateFlag = lblAddDataFiatEstimateFlag.Text, BTCAmount = lblAddDataBTC.Text, BTCAmountEstimateFlag = lblAddDataBTCEstimateFlag.Text, Label = lblAddDataLabel.Text, LabelColor = selectedColor };
            // Read the existing transactions from the JSON file
            var transactions = ReadTransactionsFromJsonFile();

            // Add the new transaction to the list
            transactions.Add(newTransaction);

            // Write the updated list of transactions back to the JSON file
            WriteTransactionsToJsonFile(transactions);
            BtnClearInput_Click(sender, e);
            await SetupTransactionsList();
            isRobotSpeaking = false;
            panelWelcome.Visible = false;
            panelRobotSpeakOuter.Visible = true;
            InterruptAndStartNewRobotSpeak("Transaction added to list and saved.");
            DrawPriceChart();
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

        #endregion

        #endregion

        #region transactions listview

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

                bool transactionFound = false;
                var transactions = ReadTransactionsFromJsonFile();
                if (transactions.Count > 0)
                {
                    transactionFound = true;

                    Transaction firstTransaction = transactions.FirstOrDefault();
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

                    panelScrollbarContainer.Invoke((MethodInvoker)delegate
                    {
                        panelScrollbarContainer.Height = 25;
                    });
                    return;
                }

                panelScrollbarContainer.Invoke((MethodInvoker)delegate
                {
                    panelScrollbarContainer.Height = 206;
                });
                lblTotalBTCAmount.Invoke((MethodInvoker)delegate
                {
                    lblTotalBTCAmount.Visible = true;
                });
                lblTotalFiatAmount.Invoke((MethodInvoker)delegate
                {
                    lblTotalFiatAmount.Visible = true;
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
                        listViewTransactions.Columns.Add("TX num", 38);
                    });
                }
                if (listViewTransactions.Columns.Count == 1)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("YYYY", 38);
                    });
                }

                if (listViewTransactions.Columns.Count == 2)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("MM", 27);
                    });
                }

                if (listViewTransactions.Columns.Count == 3)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("DD", 27);
                    });
                }
                if (listViewTransactions.Columns.Count == 4)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Price", 85);
                    });
                }
                if (listViewTransactions.Columns.Count == 5)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Est", 30);
                    });
                }
                if (listViewTransactions.Columns.Count == 6)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Range", 50);
                    });
                }
                if (listViewTransactions.Columns.Count == 7)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Fiat amt.", 85);
                    });
                }
                if (listViewTransactions.Columns.Count == 8)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Est", 30);
                    });
                }
                if (listViewTransactions.Columns.Count == 9)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("BTC amt.", 95);
                    });
                }
                if (listViewTransactions.Columns.Count == 10)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Est.", 30);
                    });
                }
                if (listViewTransactions.Columns.Count == 11)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("▲▼", 0);
                    });
                }
                if (listViewTransactions.Columns.Count == 12)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("P/L", 85);
                    });
                }
                if (listViewTransactions.Columns.Count == 13)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("P/L %", 75);
                    });
                }
                if (listViewTransactions.Columns.Count == 14)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Cost basis", 95);
                    });
                }
                if (listViewTransactions.Columns.Count == 15)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("LabelColor", 20);
                    });
                }
                if (listViewTransactions.Columns.Count == 16)
                {
                    listViewTransactions.Invoke((MethodInvoker)delegate
                    {
                        listViewTransactions.Columns.Add("Label icon", 20);
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
                decimal rollingBTCAmount = 0;
                decimal rollingOriginalFiatAmount = 0;
                decimal rollingCostBasis = 0;
                decimal rollingCurrentValue = 0;

                foreach (var transaction in transactions)
                {
                    ListViewItem item = new(Convert.ToString(listViewTransactions.Items.Count + 1)); // create new row
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

                    if (transaction.TransactionType == "Buy")
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

                    string priceText = System.Text.RegularExpressions.Regex.IsMatch(lblCurrentPrice.Text, @"^[^0-9]")
                        ? lblCurrentPrice.Text[1..]
                        : lblCurrentPrice.Text;
                    currentValue = Math.Round(Convert.ToDecimal(transaction.BTCAmount) * Convert.ToDecimal(priceText), 2);
                    if (currentValue > Math.Round(Math.Abs(Convert.ToDecimal(transaction.FiatAmount)), 2)) // profit
                    {
                        item.SubItems.Add("▲");
                    }
                    else // loss
                    {
                        item.SubItems.Add("▼");
                    }
                    item.SubItems.Add(Convert.ToString(currentValue));
                    rollingCurrentValue += currentValue;
                    decimal ChangeInValuePercentage = 0;
                    if (currentValue > Math.Round(Math.Abs(Convert.ToDecimal(transaction.FiatAmount)), 2)) // profit
                    {
                        decimal temp = 100 / (Math.Abs(Convert.ToDecimal(transaction.FiatAmount)));

                        ChangeInValuePercentage = Math.Round(((currentValue - Math.Abs(Convert.ToDecimal(transaction.FiatAmount))) / Math.Abs(Convert.ToDecimal(transaction.FiatAmount))) * 100, 2);
                    }
                    else // loss
                    {
                        decimal temp = 100 / (Math.Abs(Convert.ToDecimal(transaction.FiatAmount)));
                        ChangeInValuePercentage = Math.Round(((currentValue - Math.Abs(Convert.ToDecimal(transaction.FiatAmount))) / Math.Abs(Convert.ToDecimal(transaction.FiatAmount))) * 100, 2);
                    }
                    item.SubItems.Add(Convert.ToString(ChangeInValuePercentage));

                    rollingBTCAmount = Math.Round(rollingBTCAmount + Convert.ToDecimal(transaction.BTCAmount), 8);
                    rollingOriginalFiatAmount = Math.Round(rollingOriginalFiatAmount + Convert.ToDecimal(transaction.FiatAmount), 2);
                    if (rollingBTCAmount > 0)
                    {
                        rollingCostBasis = Math.Abs(Math.Round(rollingOriginalFiatAmount / rollingBTCAmount, 2));
                    }
                    else
                    {
                        rollingCostBasis = 0;
                    }
                    item.SubItems.Add(Convert.ToString(rollingCostBasis));
                    lblTotalBTCAmount.Invoke((MethodInvoker)delegate
                    {
                        lblTotalBTCAmount.Text = "₿" + Convert.ToString(rollingBTCAmount);
                    });
                    if (rollingOriginalFiatAmount >= 0)
                    {
                        lblTotalFiatAmount.Invoke((MethodInvoker)delegate
                        {
                            lblTotalFiatAmount.ForeColor = Color.OliveDrab;
                        });
                    }
                    else
                    {
                        lblTotalFiatAmount.Invoke((MethodInvoker)delegate
                        {
                            lblTotalFiatAmount.ForeColor = Color.IndianRed;
                        });
                    }
                    lblTotalFiatAmount.Invoke((MethodInvoker)delegate
                    {
                        lblTotalFiatAmount.Text = label54.Text + Convert.ToString(Math.Abs(rollingOriginalFiatAmount));
                    });
                    lblFinalCostBasis.Invoke((MethodInvoker)delegate
                    {
                        lblFinalCostBasis.Text = selectedCurrencySymbol + Convert.ToString(rollingCostBasis);
                    });
                    lblTotalCurrentValue.Invoke((MethodInvoker)delegate
                    {
                        lblTotalCurrentValue.Text = label54.Text + Convert.ToString(rollingCurrentValue);
                    });
                    if (rollingCurrentValue >= rollingOriginalFiatAmount)
                    {
                        lblTotalCurrentValue.Invoke((MethodInvoker)delegate
                        {
                            lblTotalCurrentValue.ForeColor = Color.OliveDrab;
                        });
                    }
                    else
                    {
                        lblTotalCurrentValue.Invoke((MethodInvoker)delegate
                        {
                            lblTotalCurrentValue.ForeColor = Color.IndianRed;
                        });
                    }
                    lblFinalChangeinValuePercentage.Invoke((MethodInvoker)delegate
                    {
                        lblFinalChangeinValuePercentage.Text = Convert.ToString(Math.Round(((rollingCurrentValue - Math.Abs(rollingOriginalFiatAmount)) / Math.Abs(rollingOriginalFiatAmount)) * 100, 2)) + "%";
                    });
                    if (Math.Round(((rollingCurrentValue - Math.Abs(rollingOriginalFiatAmount)) / Math.Abs(rollingOriginalFiatAmount)) * 100, 2) >= 100)
                    {
                        lblFinalChangeinValuePercentage.Invoke((MethodInvoker)delegate
                        {
                            lblFinalChangeinValuePercentage.ForeColor = Color.OliveDrab;
                            lblFinalChangeinValuePercentage.Text = "▲ " + lblFinalChangeinValuePercentage.Text;
                        });
                    }
                    else
                    {
                        lblFinalChangeinValuePercentage.Invoke((MethodInvoker)delegate
                        {
                            lblFinalChangeinValuePercentage.ForeColor = Color.IndianRed;
                            lblFinalChangeinValuePercentage.Text = "▼ " + lblFinalChangeinValuePercentage.Text;
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

                vScrollBar1.Width = 23;
                vScrollBar1.Maximum = listViewTransactions.Items.Count - 1;

                // now reverse the order so most recent are first (did it this way round to calculate the rolling balances and cost basis first)
                ListView listView = listViewTransactions;
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
                if (textWidth > columnWidth)
                {
                    // Truncate the text
                    var maxText = text[..(text.Length * columnWidth / textWidth - 3)] + "...";

                    var bounds = new Rectangle(e.SubItem.Bounds.Left, e.SubItem.Bounds.Top, columnWidth, e.SubItem.Bounds.Height);
                    // Clear the background

                    #region alternating row colours

                    if (e.Item!.Selected)
                    {
                        //e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 231, 217)), bounds);
                        e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(150, 150, 150)), bounds);
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

                    #endregion

                    if (e.ColumnIndex == 5) // Price estimate type
                    {
                        if (!string.IsNullOrEmpty(maxText) && maxText == "DA")
                        {
                            using Brush backgroundBrush = new SolidBrush(Color.FromArgb(255, 230, 230));
                            e.Graphics.FillRectangle(backgroundBrush, e.SubItem.Bounds);
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(maxText) && maxText == "MM")
                            {
                                using Brush backgroundBrush = new SolidBrush(Color.FromArgb(255, 205, 205));
                                e.Graphics.FillRectangle(backgroundBrush, e.SubItem.Bounds);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(maxText) && maxText == "AM")
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
                    if (e.ColumnIndex == 7) // Fiat amount
                    {
                        if (!string.IsNullOrEmpty(maxText) && maxText[0] == '-')

                        {
                            e.SubItem.ForeColor = Color.IndianRed;
                        }
                        else
                        {
                            e.SubItem.ForeColor = Color.OliveDrab;
                        }
                    }
                    if (e.ColumnIndex == 8) // Fiat amount estimate flag
                    {
                        if (!string.IsNullOrEmpty(maxText) && maxText[0] == 'Y')
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
                    if (e.ColumnIndex == 9) // Bitcoin amount
                    {
                        if (!string.IsNullOrEmpty(maxText) && maxText[0] == '-')

                        {
                            e.SubItem.ForeColor = Color.IndianRed;
                        }
                        else
                        {
                            e.SubItem.ForeColor = Color.OliveDrab;
                        }
                    }
                    if (e.ColumnIndex == 10) // Bitcoin amount estimate flag
                    {
                        if (!string.IsNullOrEmpty(maxText) && maxText[0] == 'Y')
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
                    if (e.ColumnIndex == 11) // ▲▼
                    {
                        if (!string.IsNullOrEmpty(maxText) && maxText[0] == '▼')

                        {
                            e.SubItem.ForeColor = Color.IndianRed;
                        }
                        else
                        {
                            e.SubItem.ForeColor = Color.OliveDrab;
                        }
                    }
                    if (e.ColumnIndex == 12) // current value
                    {
                        string fiatSpent = System.Text.RegularExpressions.Regex.IsMatch(e.Item.SubItems[e.ColumnIndex - 5].Text, @"^[^0-9]")
                            ? e.Item.SubItems[e.ColumnIndex - 5].Text[1..]
                            : e.Item.SubItems[e.ColumnIndex - 5].Text;
                        decimal fiatSpentDecimal = Convert.ToDecimal(fiatSpent);

                        if (!string.IsNullOrEmpty(text) && Convert.ToDecimal(text) < fiatSpentDecimal)
                        {
                            e.SubItem.ForeColor = Color.IndianRed;
                        }
                        else
                        {
                            e.SubItem.ForeColor = Color.OliveDrab;
                        }
                    }
                    if (e.ColumnIndex == 13) // change %
                    {
                        if (!string.IsNullOrEmpty(maxText) && maxText[0] == '-')

                        {
                            e.SubItem.ForeColor = Color.IndianRed;
                        }
                        else
                        {
                            e.SubItem.ForeColor = Color.OliveDrab;
                        }
                    }
                    if (e.ColumnIndex == 14) // cost basis
                    {
                        string priceText = System.Text.RegularExpressions.Regex.IsMatch(lblCurrentPrice.Text, @"^[^0-9]")
                        ? lblCurrentPrice.Text[1..]
                        : lblCurrentPrice.Text;
                        if (Convert.ToDecimal(maxText) > Convert.ToDecimal(priceText))
                        {
                            e.SubItem.ForeColor = Color.IndianRed;
                        }
                        else
                        {
                            e.SubItem.ForeColor = Color.OliveDrab;
                        }
                    }
                    if (e.ColumnIndex == 15) // label
                    {
                        if (!string.IsNullOrEmpty(maxText))
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
                    TextRenderer.DrawText(e.Graphics, maxText, font, bounds, e.Item!.ForeColor, TextFormatFlags.EndEllipsis | TextFormatFlags.Left);
                }
                else if (textWidth < columnWidth)
                {
                    // Clear the background
                    var bounds = new Rectangle(e.SubItem.Bounds.Left, e.SubItem.Bounds.Top, columnWidth, e.SubItem.Bounds.Height);


                    if (e.Item!.Selected)
                    {
                        //e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 231, 217)), bounds);
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
                    if (e.ColumnIndex == 5) // price estimate type
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
                    if (e.ColumnIndex == 7) // Fiat amount
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
                    if (e.ColumnIndex == 8) // Fiat amount estimate flag
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
                    if (e.ColumnIndex == 9) // Bitcoin amount
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
                    if (e.ColumnIndex == 10) // Bitcoin amount estimate flag
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
                    if (e.ColumnIndex == 11) // ▲▼
                    {
                        if (!string.IsNullOrEmpty(text) && text[0] == '▼')
                        {
                            e.SubItem.ForeColor = Color.IndianRed;
                        }
                        else
                        {
                            e.SubItem.ForeColor = Color.OliveDrab;
                        }
                    }
                    if (e.ColumnIndex == 12) // current value
                    {
                        string fiatSpent = System.Text.RegularExpressions.Regex.IsMatch(e.Item.SubItems[e.ColumnIndex - 5].Text, @"^[^0-9]")
                        ? e.Item.SubItems[e.ColumnIndex - 5].Text[1..]
                        : e.Item.SubItems[e.ColumnIndex - 5].Text;
                        decimal fiatSpentDecimal = Convert.ToDecimal(fiatSpent);

                        if (!string.IsNullOrEmpty(text) && Convert.ToDecimal(text) < fiatSpentDecimal)
                        {
                            e.SubItem.ForeColor = Color.IndianRed;
                        }
                        else
                        {
                            e.SubItem.ForeColor = Color.OliveDrab;
                        }
                    }
                    if (e.ColumnIndex == 13) // change %
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
                    if (e.ColumnIndex == 14) // cost basis
                    {
                        string priceText = System.Text.RegularExpressions.Regex.IsMatch(lblCurrentPrice.Text, @"^[^0-9]")
                        ? lblCurrentPrice.Text[1..]
                        : lblCurrentPrice.Text;
                        // string priceText = lblCurrentPrice.Text[1..]; // remove currency character £$
                        if (Convert.ToDecimal(text) > Convert.ToDecimal(priceText))

                        {
                            e.SubItem.ForeColor = Color.IndianRed;
                        }
                        else
                        {
                            e.SubItem.ForeColor = Color.OliveDrab;
                        }
                    }
                    if (e.ColumnIndex == 15) // label
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
            if (e.NewValue >= 0 && e.NewValue < listViewTransactions.Items.Count)
            {
                listViewTransactions.Invoke((MethodInvoker)delegate
                {
                    listViewTransactions.TopItem = listViewTransactions.Items[e.NewValue];
                });
            }
        }

        private void ListViewTransactions_SelectedIndexChanged(object sender, EventArgs e)
        {
            vScrollBar1.Value = listViewTransactions.Items.IndexOf(listViewTransactions.TopItem);
        }

        #endregion

        #region toggle reverse listview order

        private void BtnListReverse_Click(object sender, EventArgs e)
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

        #endregion

        #endregion

        #region chart - price linear and log

        private void InitializeChart()
        {
            formsPlot1.Plot.Margins(x: .1, y: .1);
            formsPlot1.Plot.Style(ScottPlot.Style.Black);
            formsPlot1.RightClicked -= formsPlot1.DefaultRightClickEvent; // disable default right-click event
            formsPlot1.Configuration.DoubleClickBenchmark = false;
            formsPlot1.Plot.Palette = ScottPlot.Palette.Amber;
            formsPlot1.Plot.YAxis.AxisLabel.IsVisible = false;

            formsPlot1.Plot.Style(
                figureBackground: Color.Transparent,
                dataBackground: Color.White,
                titleLabel: Color.Black,
                axisLabel: Color.Gray);

            Color newGridlineColor = Color.FromArgb(235, 235, 235);
            formsPlot1.Plot.Style(grid: newGridlineColor);
            formsPlot1.Refresh();
        }

        private void DrawPriceChart()
        {
            if (btnPriceChartScaleLinear.Enabled == false) //Linear chart selected
            {
                DrawPriceChartLinear();
            }
            else //Log chart selected
            {
                DrawPriceChartLog();
            }
        }

        private void BtnPriceChartScaleLinear_Click(object sender, EventArgs e)
        {
            btnPriceChartScaleLinear.Enabled = false;
            btnPriceChartScaleLog.Enabled = true;
            DrawPriceChart();
        }

        private void BtnPriceChartScaleLog_Click(object sender, EventArgs e)
        {
            btnPriceChartScaleLinear.Enabled = true;
            btnPriceChartScaleLog.Enabled = false;
            DrawPriceChart();
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

                #region cost basis horizontal line

                if (showCostBasis)
                {
                    if (double.TryParse(lblFinalCostBasis.Text, out double costBasis))
                    {
                        var hline = formsPlot1.Plot.AddHorizontalLine(y: costBasis, color: Color.OliveDrab, width: 1, style: LineStyle.Dash);
                        hline.PositionLabel = false;
                        hline.DragEnabled = false;

                        var CBline = formsPlot1.Plot.AddText(Convert.ToString(costBasis), x: xValues.Min(), y: costBasis, color: Color.OliveDrab);
                        CBline.Font.Color = Color.White; ;
                        CBline.BackgroundColor = Color.OliveDrab;
                        CBline.BackgroundFill = true;
                        CBline.BorderSize = 0;
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

                #region cost basis horizontal line

                if (showCostBasis)
                {
                    if (double.TryParse(lblFinalCostBasis.Text, out double costBasis))
                    {
                        var hline = formsPlot1.Plot.AddHorizontalLine(y: Math.Log10(costBasis), color: Color.OliveDrab, width: 1, style: LineStyle.Dash, label: "H");
                        hline.PositionLabel = false;
                        hline.DragEnabled = false;

                        var CBline = formsPlot1.Plot.AddText(Convert.ToString(costBasis), x: xValuesFiltered.Min(), y: Math.Log10(costBasis), color: Color.OliveDrab);
                        CBline.Font.Color = Color.White; ;
                        CBline.BackgroundColor = Color.OliveDrab;
                        CBline.BackgroundFill = true;
                        CBline.BorderSize = 0;
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

                // prevent navigating beyond the data
                formsPlot1.Plot.YAxis.SetBoundary(minY, maxY);
                formsPlot1.Plot.XAxis.SetBoundary(xValues.Min(), xValues.Max());

                // Add a red circle we can move around later as a highlighted point indicator
                HighlightedPoint = formsPlot1.Plot.AddPoint(0, 0);
                HighlightedPoint.Color = Color.Red;
                HighlightedPoint.MarkerSize = 10;
                HighlightedPoint.MarkerShape = ScottPlot.MarkerShape.openCircle;
                HighlightedPoint.IsVisible = false;
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
                                actualAnnotation.Font.Color = Color.Gray;
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
                                var actualAnnotation = formsPlot1.Plot.AddAnnotation($"Price:{pointY} | Date:{formattedPointX}", Alignment.UpperLeft);
                                actualAnnotation.Font.Name = "Consolas";
                                actualAnnotation.Shadow = false;
                                actualAnnotation.BorderWidth = 0;
                                actualAnnotation.BorderColor = Color.White;
                                actualAnnotation.MarginX = 2;
                                actualAnnotation.MarginY = 2;
                                actualAnnotation.Font.Color = Color.Gray;
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

        #region expand and shrink chart panels

        #region expand

        private void StartExpandingPanelHoriz(Panel panel)
        {
            panelToExpand = panel;
            ExpandPanelTimerHoriz.Start();
        }

        private void ExpandPanelTimer_Tick(object sender, EventArgs e)
        {
            currentWidthExpandingPanel += 4;
            if (panelToExpand == panel18)
            {
                panelMaxWidth = 212;
            }
            if (panelToExpand == panel21)
            {
                panelMaxWidth = 292;
            }
            if (panelToExpand == panel22)
            {
                panelMaxWidth = 264;
            }
            if (panelToExpand == panel23)
            {
                panelMaxWidth = 443;
            }
            if (panelToExpand == panelTransactionLabel)
            {
                panelMaxWidth = 650;
                currentWidthExpandingPanel += 8; // twice as fast for this panel
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

        #endregion

        #region shrink

        private void StartShrinkingPanel(Panel panel)
        {
            panelToShrink = panel;
            ShrinkPanelTimer.Start();
        }

        private void ShrinkOpenPanels() // used whenever any panel is expanded to close all others 
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

        private void ShrinkPanelTimer_Tick(object sender, EventArgs e)
        {
            currentWidthShrinkingPanel -= 4;
            if (panelToShrink == panel18)
            {
                panelMinWidth = 92;
            }
            if (panelToShrink == panel21)
            {
                panelMinWidth = 84;
            }
            if (panelToShrink == panel22)
            {
                panelMinWidth = 64;
            }
            if (panelToShrink == panel23)
            {
                panelMinWidth = 121;
            }
            if (panelToShrink == panelTransactionLabel)
            {
                panelMinWidth = 105;
                currentWidthShrinkingPanel -= 8; // twice as fast for this panel
            }

            if (currentWidthShrinkingPanel <= panelMinWidth) // expanding is complete
            {
                panelToShrink.Invoke((MethodInvoker)delegate
                {
                    panelToShrink.Width = panelMinWidth;
                    panelToShrink.Invalidate();
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
                        panel21.Location = new Point(panel18.Location.X + panel18.Width + 10, panel21.Location.Y);
                    });
                    panel22.Invoke((MethodInvoker)delegate
                    {
                        panel22.Location = new Point(panel21.Location.X + panel21.Width + 10, panel22.Location.Y);
                    });
                    panel23.Invoke((MethodInvoker)delegate
                    {
                        panel23.Location = new Point(panel22.Location.X + panel22.Width + 10, panel23.Location.Y);
                    });
                    panel24.Invoke((MethodInvoker)delegate
                    {
                        panel24.Location = new Point(panel23.Location.X + panel23.Width + 10, panel24.Location.Y);
                    });
                }
            }
        }

        #endregion

        #region buttons to expand/shrink panels

        private void BtnExpandGridlinesPanel_Click(object sender, EventArgs e)
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

        private void BtnExpandDatelinesPanel_Click(object sender, EventArgs e)
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

        private void BtnExpandTransactionsPanel_Click(object sender, EventArgs e)
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

        private void BtnExpandTrackingPanel_Click(object sender, EventArgs e)
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

        #endregion

        #endregion

        #region hide/show chart elements

        private void BtnShowPriceGridlines_Click(object sender, EventArgs e)
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

        private void BtnShowDateGridlines_Click(object sender, EventArgs e)
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

        private void BtnShowCostBasis_Click(object sender, EventArgs e)
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

        private void BtnShowBuyDates_Click(object sender, EventArgs e)
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

        private void BtnShowSellDates_Click(object sender, EventArgs e)
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

        private void BtnShowBuyBubbles_Click(object sender, EventArgs e)
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

        private void BtnShowSellBubbles_Click(object sender, EventArgs e)
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

        private void BtnCursorTrackPrice_Click(object sender, EventArgs e)
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

        private void BtnCursorTrackBuyTX_Click(object sender, EventArgs e)
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

        private void BtnCursorTrackSellTX_Click(object sender, EventArgs e)
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

        private void BtnCursorTrackNothing_Click(object sender, EventArgs e)
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

        private void DisableCursorTracking()
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

        #endregion

        #endregion

        #region currency selection

        private void BtnCurrency_Click(object sender, EventArgs e)
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

        private async void BtnUSD_Click(object sender, EventArgs e)
        {
            EnableCurrencyButtons();
            btnUSD.Enabled = false;
            currentHeightShrinkingPanel = panelCurrency.Height;
            StartShrinkingPanelVert(panelCurrency);
            selectedCurrencyName = "USD";
            selectedCurrencySymbol = "$";
            HistoricPrices.Clear();
            await GetHistoricPricesAsyncWrapper();
            await SetupTransactionsList();
            InitializeChart();
            DrawPriceChart();
            lblCurrentPrice.Invoke((MethodInvoker)delegate
            {
                lblCurrentPrice.Text = "$" + lblCurrentPrice.Text;
                lblCurrentPrice.Location = new Point(btnPriceRefresh.Location.X - lblCurrentPrice.Width, lblCurrentPrice.Location.Y);
            });
            pictureBoxBTCLogo.Invoke((MethodInvoker)delegate
            {
                pictureBoxBTCLogo.Location = new Point(lblCurrentPrice.Location.X - pictureBoxBTCLogo.Width, pictureBoxBTCLogo.Location.Y);
            });
            btnCurrency.Invoke((MethodInvoker)delegate
            {
                btnCurrency.Text = "$ USD";
            });
            label26.Invoke((MethodInvoker)delegate
            {
                label26.Text = "USD";
            });
            label11.Invoke((MethodInvoker)delegate
            {
                label11.Text = "USD*";
            });
            label9.Invoke((MethodInvoker)delegate
            {
                label9.Text = "Price (USD)*";
            });
            label23.Invoke((MethodInvoker)delegate
            {
                label23.Text = "Price (USD)";
            });
            label1.Invoke((MethodInvoker)delegate
            {
                label1.Text = "Price (USD)";
            });
            label52.Invoke((MethodInvoker)delegate
            {
                label52.Text = "$";
            });
            label54.Invoke((MethodInvoker)delegate
            {
                label54.Text = "$";
            });
            if (btnBoughtBitcoin.Text == "✔️")
            {
                lblFiatAmountSpentRecd.Invoke((MethodInvoker)delegate
                {
                    lblFiatAmountSpentRecd.Text = "USD spent";
                });
            }
            else
            {
                lblFiatAmountSpentRecd.Invoke((MethodInvoker)delegate
                {
                    lblFiatAmountSpentRecd.Text = "USD received";
                });
            }
            if (currencyInFile != "USD")
            {
                SaveSettingsToSettingsFile();
            }
            if (!initialCurrencySetup)
            {
                panelWelcome.Visible = false;
                panelRobotSpeakOuter.Visible = true;
                InterruptAndStartNewRobotSpeak("Currency set to USD and saved as your default currency.");
            }
            initialCurrencySetup = false;
        }

        private async void BtnEUR_Click(object sender, EventArgs e)
        {
            EnableCurrencyButtons();
            btnEUR.Enabled = false;
            currentHeightShrinkingPanel = panelCurrency.Height;
            StartShrinkingPanelVert(panelCurrency);
            selectedCurrencyName = "EUR";
            selectedCurrencySymbol = "€";
            HistoricPrices.Clear();
            await GetHistoricPricesAsyncWrapper();
            await SetupTransactionsList();
            InitializeChart();
            DrawPriceChart();
            lblCurrentPrice.Invoke((MethodInvoker)delegate
            {
                lblCurrentPrice.Text = "€" + lblCurrentPrice.Text;
                lblCurrentPrice.Location = new Point(btnPriceRefresh.Location.X - lblCurrentPrice.Width, lblCurrentPrice.Location.Y);
            });
            pictureBoxBTCLogo.Invoke((MethodInvoker)delegate
            {
                pictureBoxBTCLogo.Location = new Point(lblCurrentPrice.Location.X - pictureBoxBTCLogo.Width, pictureBoxBTCLogo.Location.Y);
            });
            btnCurrency.Invoke((MethodInvoker)delegate
            {
                btnCurrency.Text = "€ EUR";
            });
            label26.Invoke((MethodInvoker)delegate
            {
                label26.Text = "EUR";
            });
            label11.Invoke((MethodInvoker)delegate
            {
                label11.Text = "EUR*";
            });
            label9.Invoke((MethodInvoker)delegate
            {
                label9.Text = "Price (EUR)*";
            });
            label23.Invoke((MethodInvoker)delegate
            {
                label23.Text = "Price (EUR)";
            });
            label1.Invoke((MethodInvoker)delegate
            {
                label1.Text = "Price (EUR)";
            });
            label52.Invoke((MethodInvoker)delegate
            {
                label52.Text = "€";
            });
            label54.Invoke((MethodInvoker)delegate
            {
                label54.Text = "€";
            });
            if (btnBoughtBitcoin.Text == "✔️")
            {
                lblFiatAmountSpentRecd.Invoke((MethodInvoker)delegate
                {
                    lblFiatAmountSpentRecd.Text = "EUR spent";
                });
            }
            else
            {
                lblFiatAmountSpentRecd.Invoke((MethodInvoker)delegate
                {
                    lblFiatAmountSpentRecd.Text = "EUR received";
                });
            }
            if (currencyInFile != "EUR")
            {
                SaveSettingsToSettingsFile();
            }
            if (!initialCurrencySetup)
            {
                panelWelcome.Visible = false;
                panelRobotSpeakOuter.Visible = true;
                InterruptAndStartNewRobotSpeak("Currency set to EUR and saved as your default currency.");
            }
            initialCurrencySetup = false;
        }

        private async void BtnGBP_Click(object sender, EventArgs e)
        {
            EnableCurrencyButtons();
            btnGBP.Enabled = false;
            currentHeightShrinkingPanel = panelCurrency.Height;
            StartShrinkingPanelVert(panelCurrency);
            selectedCurrencyName = "GBP";
            selectedCurrencySymbol = "£";
            HistoricPrices.Clear();
            await GetHistoricPricesAsyncWrapper();
            await SetupTransactionsList();
            InitializeChart();
            DrawPriceChart();
            lblCurrentPrice.Invoke((MethodInvoker)delegate
            {
                lblCurrentPrice.Text = "£" + lblCurrentPrice.Text;
                lblCurrentPrice.Location = new Point(btnPriceRefresh.Location.X - lblCurrentPrice.Width, lblCurrentPrice.Location.Y);
            });
            pictureBoxBTCLogo.Invoke((MethodInvoker)delegate
            {
                pictureBoxBTCLogo.Location = new Point(lblCurrentPrice.Location.X - pictureBoxBTCLogo.Width, pictureBoxBTCLogo.Location.Y);
            });
            btnCurrency.Invoke((MethodInvoker)delegate
            {
                btnCurrency.Text = "£ GBP";
            });
            label26.Invoke((MethodInvoker)delegate
            {
                label26.Text = "GBP";
            });
            label11.Invoke((MethodInvoker)delegate
            {
                label11.Text = "GBP*";
            });
            label9.Invoke((MethodInvoker)delegate
            {
                label9.Text = "Price (GBP)*";
            });
            label23.Invoke((MethodInvoker)delegate
            {
                label23.Text = "Price (GBP)";
            });
            label1.Invoke((MethodInvoker)delegate
            {
                label1.Text = "Price (GBP)";
            });
            label52.Invoke((MethodInvoker)delegate
            {
                label52.Text = "£";
            });
            label54.Invoke((MethodInvoker)delegate
            {
                label54.Text = "£";
            });
            if (btnBoughtBitcoin.Text == "✔️")
            {
                lblFiatAmountSpentRecd.Invoke((MethodInvoker)delegate
                {
                    lblFiatAmountSpentRecd.Text = "GBP spent";
                });
            }
            else
            {
                lblFiatAmountSpentRecd.Invoke((MethodInvoker)delegate
                {
                    lblFiatAmountSpentRecd.Text = "GBP received";
                });
            }
            if (currencyInFile != "GBP")
            {
                SaveSettingsToSettingsFile();
            }
            if (!initialCurrencySetup)
            {
                panelWelcome.Visible = false;
                panelRobotSpeakOuter.Visible = true;
                InterruptAndStartNewRobotSpeak("Currency set to GBP and saved as your default currency.");
            }
            initialCurrencySetup = false;
        }

        private async void BtnXAU_Click(object sender, EventArgs e)
        {
            EnableCurrencyButtons();
            btnXAU.Enabled = false;
            currentHeightShrinkingPanel = panelCurrency.Height;
            StartShrinkingPanelVert(panelCurrency);
            selectedCurrencyName = "XAU";
            selectedCurrencySymbol = "Ꜷ";
            HistoricPrices.Clear();
            await GetHistoricPricesAsyncWrapper();
            await SetupTransactionsList();
            InitializeChart();
            DrawPriceChart();
            lblCurrentPrice.Invoke((MethodInvoker)delegate
            {
                lblCurrentPrice.Text = "Ꜷ" + lblCurrentPrice.Text;
                lblCurrentPrice.Location = new Point(btnPriceRefresh.Location.X - lblCurrentPrice.Width, lblCurrentPrice.Location.Y);
            });
            pictureBoxBTCLogo.Invoke((MethodInvoker)delegate
            {
                pictureBoxBTCLogo.Location = new Point(lblCurrentPrice.Location.X - pictureBoxBTCLogo.Width, pictureBoxBTCLogo.Location.Y);
            });
            btnCurrency.Invoke((MethodInvoker)delegate
            {
                btnCurrency.Text = "Ꜷ XAU";
            });
            label26.Invoke((MethodInvoker)delegate
            {
                label26.Text = "XAU";
            });
            label11.Invoke((MethodInvoker)delegate
            {
                label11.Text = "XAU*";
            });
            label9.Invoke((MethodInvoker)delegate
            {
                label9.Text = "Price (XAU)*";
            });
            label23.Invoke((MethodInvoker)delegate
            {
                label23.Text = "Price (XAU)";
            });
            label1.Invoke((MethodInvoker)delegate
            {
                label1.Text = "Price (GBP)";
            });
            label52.Invoke((MethodInvoker)delegate
            {
                label52.Text = "Ꜷ";
            });
            label54.Invoke((MethodInvoker)delegate
            {
                label54.Text = "Ꜷ";
            });
            if (btnBoughtBitcoin.Text == "✔️")
            {
                lblFiatAmountSpentRecd.Invoke((MethodInvoker)delegate
                {
                    lblFiatAmountSpentRecd.Text = "XAU spent";
                });
            }
            else
            {
                lblFiatAmountSpentRecd.Invoke((MethodInvoker)delegate
                {
                    lblFiatAmountSpentRecd.Text = "XAU received";
                });
            }
            if (currencyInFile != "XAU")
            {
                SaveSettingsToSettingsFile();
            }
            if (!initialCurrencySetup)
            {
                panelWelcome.Visible = false;
                panelRobotSpeakOuter.Visible = true;
                InterruptAndStartNewRobotSpeak("Currency set to XAU and saved as your default currency.");
            }
            initialCurrencySetup = false;
        }

        private void EnableCurrencyButtons()
        {
            btnUSD.Enabled = true;
            btnEUR.Enabled = true;
            btnGBP.Enabled = true;
            btnXAU.Enabled = true;
        }

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
                // display about screen:
                Form frm = new About
                {
                    Owner = this, // Set the parent window as the owner of the modal window
                    StartPosition = FormStartPosition.CenterParent, // Set the start position to center of parent
                    CurrentVersion = CurrentVersion
                };
                frm.ShowDialog(this);
            }
            catch (Exception ex)
            {
                HandleException(ex, "BtnAbout_Click");
            }
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
            panelAddTransactionContainer.Enabled = true;
            panel14.Enabled = true;
            panel13.Enabled = true;
            panelTopControls.Enabled = true;
            panelHelpTextContainer.Invoke((MethodInvoker)delegate
            {
                panelHelpTextContainer.Visible = false;
            });
            panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
            {
                panelHideSpeechTriangle.Visible = true;
            });
        }

        private void BtnHelpAddTransaction_Click(object sender, EventArgs e)
        {
            panelAddTransactionContainer.Enabled = false;
            panel14.Enabled = false;
            panel13.Enabled = false;
            panelTopControls.Enabled = false;
            lblHelpText.Invoke((MethodInvoker)delegate
            {
                lblHelpText.Text = "Input all your transactions here. The more accurate you can be the better, but Cubit will do its best to fill in the gaps for you if you don't have all the information needed." + Environment.NewLine + "Start by selecting 'Received Bitcoin' if you bought, earned, was gifted, etc an amount of Bitcoin, or 'Spent Bitcoin' if you sold, paid or gave an amount of Bitcoin." + Environment.NewLine + "Fill in as much of the date of the transaction as possible. If you know the year and month but not the day then the median bitoin price for that month will be used as an estimate. If you only know the year then the median price for that year will be used. If you know the exact date then the estimate will be an average price for that date. In periods of higher volatility using estimates will increase the margin of error later on, so it's always best to be as complete as you can be." + Environment.NewLine + "Once you input the amount of fiat money or the amount of bitcoin that was transacted, estimates will also be provided for the amount of bitcoin or fiat you will have received or spent. This is based purely on the exchange rate and won't take account of things such as exchange fees, non-KYC premium, etc, so once more it's best to provide all the correct figures if you can." + Environment.NewLine + "The 'Label' field can be used to record a small note about the transaction if you want to.";
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
        }

        private void BtnHelpTransactionList_Click(object sender, EventArgs e)
        {
            panelAddTransactionContainer.Enabled = false;
            panel14.Enabled = false;
            panel13.Enabled = false;
            panelTopControls.Enabled = false;
            lblHelpText.Invoke((MethodInvoker)delegate
            {
                lblHelpText.Text = "YYYY MM DD - The date of the transaction. If a partial date was provided a '-' will display in the missing fields." + Environment.NewLine + "Price - the value of 1 bitcoin at the time of the transaction." + Environment.NewLine + "Est. - The type of estimate used to determine the price: DA - daily average, MM - monthly median, AM - annual median, N - not estimated, accurate price was inputted." + Environment.NewLine + "Range - If an estimate is being used, this is the potential margin of error. The more accurate you can be with the date input the lower the margin of error will be." + Environment.NewLine + "Fiat - the amount of fiat currency involved in the transaction. A 'Y' will show under 'Est.' if an estimate was used." + Environment.NewLine + "BTC - the amount of bitcoin involved in the transaction. A 'Y' will show under 'Est.' if an estimate was used." + Environment.NewLine + "Change - the difference between the value of the bitcoin at the time of the transaction and its value today." + Environment.NewLine + "Change % - the difference between the value of the bitcoin at the time of the transaction its value today." + Environment.NewLine + "Cost basis - the rolling cost basis of your bitcoin holdings.";
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
        }

        private void BtnHelpChart_Click(object sender, EventArgs e)
        {
            panelAddTransactionContainer.Enabled = false;
            panel14.Enabled = false;
            panel13.Enabled = false;
            panelTopControls.Enabled = false;
            lblHelpText.Invoke((MethodInvoker)delegate
            {
                lblHelpText.Text = "The orange plotted line on the chart represents the price of 1 bitcoin since its inception to the present day, with the date along the x axis and the fiat value on the y axis." + Environment.NewLine + Environment.NewLine + "The horizontal green dashed line represents the cost basis of all your bitcoin taking all of your past transactions in to account. When the value of 1 bitcoin is above this line your bitcoin is worth more in fiat terms than it cost you. The cost basis line can be disabled in the options above the chart." + Environment.NewLine + Environment.NewLine + "The vertical green lines show the dates of transactions where you bought or received bitcoin and the red vertical lines show transactions where you sold or spent bitcoin. The transaction lines can be disabled in the options above the chart." + Environment.NewLine + Environment.NewLine + "The green circles are positioned to show the date of a transaction and the value of bitcoin at the time of the transaction. The radius of the circle is determined by the significance in size of that transaction (in fiat terms) compared to all other transactions of that type. The biggest circles are your biggest transactions. Green circles represent transactions where you've bought or received bitcoin and red circles represent transactions where you've sold or spent bitcoin. The transaction circles can be disabled in the options above the chart." + Environment.NewLine + Environment.NewLine + "The upper-left of the chart shows the values of the closest plotted point to the mouse cursor. You can select which data is tracked in the options above the chart." + Environment.NewLine + Environment.NewLine + "Price and date gridlines can be individually disabled in the options above the chart." + Environment.NewLine + Environment.NewLine + "The chart can be viewed with either a linear scale or a logarithmic scale at any time with the buttons above the chart.";
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
        }

        #endregion

        #region common code

        #region validate decimal field input 

        private void Numeric2DecimalsTextBoxValidation_KeyPress(object sender, KeyPressEventArgs e)
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

        private void Numeric8DecimalsTextBoxValidation_KeyPress(object sender, KeyPressEventArgs e)
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
            if (!robotIgnoreChanges)
            {
                panelHideSpeechTriangle.Visible = true;
                // Cancel the previous task if it's running
                robotSpeakCancellationTokenSource?.Cancel();

                // Create a new cancellation token source
                robotSpeakCancellationTokenSource = new CancellationTokenSource();

                // Start the new RobotSpeakAsync task
                _ = RobotSpeakAsync(newRobotText, robotSpeakCancellationTokenSource.Token);
            }
        }

        private async Task RobotSpeakAsync(string robotText, CancellationToken cancellationToken)
        {
            if (isRobotSpeaking)
            {
                return; // Ignore new calls if the method is already running
            }

            // isRobotSpeaking = true;

            try
            {
                panelSpeechBubble.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBubble.Height = 0;
                    panelSpeechBubble.Location = new Point(panelSpeechBubble.Location.X, 66);
                    panelSpeechBubble.Visible = true;
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

        private void PictureBoxRobot_Click(object sender, EventArgs e)
        {
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
            InterruptAndStartNewRobotSpeak("None of this text is visible on screen but it's long enough to make a small delay");
        }

        #region expand and shrink robot speech

        private void ShrinkRobotTimer_Tick(object sender, EventArgs e)
        {
            SpeechBubblecurrentHeight -= 4;

            if (SpeechBubblecurrentHeight <= 0) // shrinking is complete
            {
                ShrinkRobotTimer.Stop();
                panelSpeechBubble.Visible = false;
                panelWelcome.Visible = false;
            }
            else // shrink further
            {
                pictureBoxRobot.Invalidate();
                panelSpeechBubble.Invoke((MethodInvoker)delegate
                {
                    panelSpeechBubble.Height = SpeechBubblecurrentHeight;
                    panelSpeechBubble.Location = new Point(panelSpeechBubble.Location.X, panelSpeechBubble.Location.Y + 2);
                    panelSpeechBubble.Invalidate();
                });
            }
        }

        private void StartShrinkingRobot()
        {
            SpeechBubblecurrentHeight = 93;
            ShrinkRobotTimer.Start();
        }

        private void ExpandRobotTimer_Tick(object sender, EventArgs e)
        {
            SpeechBubblecurrentHeight += 4;

            if (SpeechBubblecurrentHeight >= 100) // expanding is complete
            {
                ExpandRobotTimer.Stop();
                expandRobotTimerRunning = false;
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

        private void StartExpandingRobot()
        {
            panelSpeechBubble.Height = 0;
            ExpandRobotTimer.Start();
            expandRobotTimerRunning = true;
        }

        private async Task WaitForExpandRobotTimerToStop()
        {
            while (expandRobotTimerRunning)
            {
                await Task.Delay(100);
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
            }
            catch (Exception ex)
            {
                HandleException(ex, "Form_Paint");
            }
        }

        private void Panel_Paint(object? sender, PaintEventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            Panel panel = (Panel)sender;

            // Create a GraphicsPath object with rounded corners
            System.Drawing.Drawing2D.GraphicsPath path = new();
            int cornerRadius = 9;
            cornerRadius *= 2;
            path.AddArc(0, 0, cornerRadius, cornerRadius, 180, 90);
            path.AddArc(panel.Width - cornerRadius, 0, cornerRadius, cornerRadius, 270, 90);
            path.AddArc(panel.Width - cornerRadius, panel.Height - cornerRadius, cornerRadius, cornerRadius, 0, 90);
            path.AddArc(0, panel.Height - cornerRadius, cornerRadius, cornerRadius, 90, 90);
            path.CloseFigure();

            // Set the panel's region to the rounded path
            panel.Region = new Region(path);
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
            panelToExpandVert = panel;
            ExpandPanelTimerVert.Start();
        }

        private void StartShrinkingPanelVert(Panel panel)
        {
            panelToShrinkVert = panel;
            ShrinkPanelTimerVert.Start();
        }

        private void ExpandCurrencyTimer_Tick(object sender, EventArgs e)
        {
            currentHeightExpandingPanel += 4;
            if (panelToExpandVert == panelCurrency)
            {
                panelMaxHeight = 129;
            }
            if (panelToExpandVert == panelColors)
            {
                panelMaxHeight = 129;
            }
            if (currentHeightExpandingPanel >= panelMaxHeight) // expanding is complete
            {
                panelToExpandVert.Invoke((MethodInvoker)delegate
                {
                    panelToExpandVert.Height = panelMaxHeight;
                });
                ExpandPanelTimerVert.Stop();
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

        private void ShrinkCurrencyTimer_Tick(object sender, EventArgs e)
        {
            currentHeightShrinkingPanel -= 4;
            if (panelToShrinkVert == panelCurrency || panelToShrink == panelColors)
            {
                panelMinHeight = 0;
            }

            if (currentHeightShrinkingPanel <= panelMinHeight) // shrinking is complete
            {
                panelToShrinkVert.Invoke((MethodInvoker)delegate
                {
                    panelToShrinkVert.Height = panelMinHeight;
                });
                ShrinkPanelTimerVert.Stop();
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

        private void BtnShowHideLabel_Click(object sender, EventArgs e)
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

        private void BtnSummary_Click(object sender, EventArgs e)
        {
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

            lblSummaryValueOfBTC.Invoke((MethodInvoker)delegate
            {
                lblSummaryValueOfBTC.Text = lblTotalCurrentValue.Text;
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
                label86.Location = new Point(lblSummaryNetFiatAmount.Location.X + lblSummaryNetFiatAmount.Width, label86.Location.Y);
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
            lblSummaryPercentageChangeInValue.Invoke((MethodInvoker)delegate
            {
                lblSummaryPercentageChangeInValue.Text = lblFinalChangeinValuePercentage.Text;
            });
            if (lblSummaryPercentageChangeInValue.Text[..1] == "▲")
            {
                lblSummaryPercentageChangeInValue.Invoke((MethodInvoker)delegate
                {
                    lblSummaryPercentageChangeInValue.ForeColor = Color.DarkSeaGreen;
                });
            }
            else
            {
                lblSummaryPercentageChangeInValue.Invoke((MethodInvoker)delegate
                {
                    lblSummaryPercentageChangeInValue.ForeColor = Color.RosyBrown;
                });
            }
            lblSummaryCostToValue.Invoke((MethodInvoker)delegate
            {
                lblSummaryCostToValue.Text = "from " + lblSummaryNetFiatAmount.Text + " to " + lblSummaryValueOfBTC.Text;
            });
            #region 'buy' transactions
            lblSummaryTotalFiatSpentOnBuyTransactions.Invoke((MethodInvoker)delegate
            {
                lblSummaryTotalFiatSpentOnBuyTransactions.Text = selectedCurrencySymbol + Convert.ToString(Math.Abs(totalFiatSpentOnBuyTransactions));
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
            #endregion
            #region 'sell' transactions
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
            #endregion
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
            panel19.Invoke((MethodInvoker)delegate
            {
                panel19.Location = new Point(panel19.Location.X, lblFirstTXDate.Location.Y + lblFirstTXDate.Height);
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
            lblSummaryMostFiatSpentInOneTX.Invoke((MethodInvoker)delegate
            {
                lblSummaryMostFiatSpentInOneTX.Text = selectedCurrencySymbol + Convert.ToString(mostFiatSpentInOneTransaction);
            });
            lblSummaryMostBTCReceivedInOneTX.Invoke((MethodInvoker)delegate
            {
                lblSummaryMostBTCReceivedInOneTX.Text = Convert.ToString(mostBTCReceivedInOneTransaction);
            });
            lblSummaryMostFiatReceivedInOneTX.Invoke((MethodInvoker)delegate
            {
                lblSummaryMostFiatReceivedInOneTX.Text = selectedCurrencySymbol + Convert.ToString(mostFiatReceivedInOneTransaction);
            });
            lblSummaryMostBTCSpentInOneTX.Invoke((MethodInvoker)delegate
            {
                lblSummaryMostBTCSpentInOneTX.Text = Convert.ToString(mostBTCSpentInOneTransaction);
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

            panelAddTransactionContainer.Enabled = false;
            panel14.Enabled = false;
            panel13.Enabled = false;
            panelTopControls.Enabled = false;
            panel9.Enabled = false;
            panelSummaryContainer.Invoke((MethodInvoker)delegate
            {
                panelSummaryContainer.Visible = true;
                panelSummaryContainer.BringToFront();
            });
            panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
            {
                panelHideSpeechTriangle.Visible = false;
                panelHideSpeechTriangle.BringToFront();
            });
        }

        private void BtnCloseSummary_Click(object sender, EventArgs e)
        {
            panelAddTransactionContainer.Enabled = true;
            panel14.Enabled = true;
            panel13.Enabled = true;
            panelTopControls.Enabled = true;
            panel9.Enabled = true;
            panelSummaryContainer.Invoke((MethodInvoker)delegate
            {
                panelSummaryContainer.Visible = false;
            });
            panelHideSpeechTriangle.Invoke((MethodInvoker)delegate
            {
                panelHideSpeechTriangle.Visible = true;
            });
        }

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
    }
}
