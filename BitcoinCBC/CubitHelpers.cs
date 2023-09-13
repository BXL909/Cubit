using Cubit;
using Cubit.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ScottPlot;
using ScottPlot.Plottable;
using System.Data;
using System.Drawing.Drawing2D;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Control = System.Windows.Forms.Control;
using ListViewItem = System.Windows.Forms.ListViewItem;
using Panel = System.Windows.Forms.Panel;
/*  
╔═╗╦ ╦╔╗ ╦╔╦╗
║  ║ ║╠╩╗║ ║ 
╚═╝╚═╝╚═╝╩ ╩
*/

internal static class CubitHelpers
{


    private static async Task<List<Settings>> ReadSettingsFromJsonFileAsync()
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
        
        
        private static async Task<List<Settings>> ReadSettingsFromJsonFileAsync()
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
}