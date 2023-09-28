using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Net;
using System.Runtime.InteropServices;

namespace Cubit
{
    public partial class About : Form
    {
        public string? CurrentVersion { get; set; }

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
        public About()
        {
            InitializeComponent();
            #region rounded form
            this.FormBorderStyle = FormBorderStyle.None;
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 25, 25));
            #endregion
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void About_Paint(object sender, PaintEventArgs e)
        {
            #region rounded border around rounded form
            // Paint the border with a 1-pixel width
            using var pen = new Pen(Color.FromArgb(255, 192, 128), 4);
            var rect = ClientRectangle;
            rect.Inflate(-1, -1);
            e.Graphics.DrawPath(pen, GetRoundedRect(rect, 25));
            #endregion
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

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = "https://bxl909.github.io/",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = "https://bxl909.github.io/",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void LinkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = "https://github.com/BXL909/Cubit",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = "https://cubit.btcdir.org/support",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void LinkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = "https://cubit.btcdir.org",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private async void About_Load(object sender, EventArgs e)
        {
            await CheckForUpdatesAsync();
        }

        /*
        private void CheckForUpdates()
        {
            try
            {
                CurrentVersion ??= "xx";
                lblCurrentVersion.Text = "Cubit v" + CurrentVersion.ToString();
                using WebClient client = new();
                string VersionURL = "https://cubit.btcdir.org/CubitVersion.txt";
                string LatestVersion = client.DownloadString(VersionURL);

                if (LatestVersion != CurrentVersion)
                {
                    lblLatestVersion.Invoke((MethodInvoker)delegate
                    {
                        lblLatestVersion.Text = "v" + LatestVersion + " is available";
                    });
                    linkLabelDownloadUpdate.Invoke((MethodInvoker)delegate
                    {
                        linkLabelDownloadUpdate.Text = "Download " + LatestVersion;
                        linkLabelDownloadUpdate.Visible = true;
                    });

                }
                else
                {
                    lblLatestVersion.Invoke((MethodInvoker)delegate
                    {
                        lblLatestVersion.Text = "(up to date)";
                    });
                    linkLabelDownloadUpdate.Visible = false;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }
        */
        private async Task CheckForUpdatesAsync()
        {
            try
            {
                CurrentVersion ??= "xx";
                lblCurrentVersion.Text = "Cubit v" + CurrentVersion.ToString();

                using HttpClient httpClient = new();
                string VersionURL = "https://cubit.btcdir.org/CubitVersion.txt";
                string LatestVersion = await httpClient.GetStringAsync(VersionURL);

                if (LatestVersion != CurrentVersion)
                {
                    lblLatestVersion.Invoke((MethodInvoker)delegate
                    {
                        lblLatestVersion.Text = "v" + LatestVersion + " is available";
                    });
                    linkLabelDownloadUpdate.Invoke((MethodInvoker)delegate
                    {
                        linkLabelDownloadUpdate.Text = "Download " + LatestVersion;
                        linkLabelDownloadUpdate.Visible = true;
                    });
                }
                else
                {
                    lblLatestVersion.Invoke((MethodInvoker)delegate
                    {
                        lblLatestVersion.Text = "(up to date)";
                    });
                    linkLabelDownloadUpdate.Visible = false;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void LinkLabelDownloadUpdate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = "https://cubit.btcdir.org",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private void HandleException(Exception ex)
        {
            string errorMessage;
            if (ex is WebException)
            {
                errorMessage = "Web exception occurred";
            }
            else if (ex is HttpRequestException)
            {
                errorMessage = "HTTP Request error";
            }
            else
            {
                errorMessage = "Error occurred";
            }

            lblErrorMessage.Invoke((MethodInvoker)delegate
            {
                lblErrorMessage.Text = errorMessage;
                lblErrorMessage.Visible = true;
            });
        }
    }
}
