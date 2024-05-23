using System.Diagnostics;
using System.Drawing.Text;
using System.Net;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace CementInstaller
{
    public partial class MainForm : Form
    {
        public const string BASE_DOWNLOAD_URL = "https://github.com/CementGB/cementresources/raw/main/";
        public const string REPO_API_LINK = "https://api.github.com/repos/CementGB/cementresources/git/trees/main";

        public const string MSG_DONE = "Done downloading Cement! When you want to install an update, " +
                    "just run this application again. You can now close me, and reopen Gang Beasts.";

        public const string MSG_ERROR = "Whoops! An error occurred while trying to download Cement. " +
                        "This could be because you are not connected to the internet.";

        public const string MSG_GANG_BEASTS_NOT_OPEN = "Please open Gang Beasts, then click the retry button. " +
            "The installer will automatically close it.";

        string downloadDots = ".";

        public MainForm()
        {
            InitializeComponent();
        }

        private Process GetGangBeastsProcess()
        {
            foreach (Process process in Process.GetProcesses())
            {
                if (process.ProcessName == "Gang Beasts")
                {
                    return process;
                }
            }

            return null;
        }

        private void SetFont()
        {
            //https://stackoverflow.com/questions/1297264/using-custom-fonts-on-a-label-on-winforms
            //Create your private font collection object.
            PrivateFontCollection pfc = new PrivateFontCollection();

            //Select your font from the resources.
            //My font here is "Digireu.ttf"
            int fontLength = Properties.Resources.Quantico_Regular.Length;

            // create a buffer to read in to
            byte[] fontdata = Properties.Resources.Quantico_Regular;

            // create an unsafe memory block for the font data
            System.IntPtr data = Marshal.AllocCoTaskMem(fontLength);

            // copy the bytes to the unsafe memory block
            Marshal.Copy(fontdata, 0, data, fontLength);

            // pass the font to the font collection
            pfc.AddMemoryFont(data, fontLength);

            infoText.Font = new Font(pfc.Families[0], infoText.Font.Size);
            infoText.ForeColor = Color.FromArgb(203, 246, 255);
            retryButton.Font = new Font(pfc.Families[0], retryButton.Font.Size);
            retryButton.ForeColor = Color.FromArgb(203, 246, 255);

        }

        private void SetValues()
        {
            BackColor = Color.FromArgb(146, 187, 228);
            retryButton.BackColor = Color.FromArgb(1, 66, 87);
            SetFont();
        }

        private void DownloadFile(string path, string link)
        {
            WebClient client = new WebClient();
            client.Proxy = null;
            client.DownloadFile(new Uri(link), path);
            client.Dispose();
        }

        private void ChangeText(string text)
        {
            infoText.Text = text;
        }

        private void DownloadFinished()
        {
            ChangeText(MSG_DONE);
            downloadTimer.Enabled = false;
        }

        private void DownloadTree(string url, string gangBeastsPath, string basePath = "")
        {
            try
            {
                // get tree data
                WebClient client = new WebClient();
                client.Proxy = null;
                client.Headers.Add("User-Agent", "request");
                string response = client.DownloadString(url);
                Console.WriteLine(response);
                client.Dispose();

                GitTreeResponse json = JsonSerializer.Deserialize<GitTreeResponse>(response);
                foreach (GitTreeElement element in json.tree)
                {
                    string path = Path.Combine(gangBeastsPath, basePath, element.path);
                    string relativePath = Path.Combine(basePath, element.path);
                    if (element.type == "tree")
                    {
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        DownloadTree(element.url, gangBeastsPath, relativePath);
                    }
                    else
                    {
                        DownloadFile(path, BASE_DOWNLOAD_URL + relativePath);
                    }
                }

                if (basePath == "")
                {
                    Invoke(new MethodInvoker(() => DownloadFinished()));
                }
            }
            catch
            {
                Invoke(new MethodInvoker(delegate () {
                    ChangeText(MSG_ERROR);
                    retryButton.Visible = true;
                }));
            }
        }

        private void TryInstallLoader()
        {
            Process GangBeard = GetGangBeastsProcess();
            if (GangBeard != null)
            {
                retryButton.Visible = false;
                string pathToGangBeard = Path.Combine(GangBeard.MainModule.FileName, "..");
                GangBeard.Kill();
                GangBeard.Dispose();
                Console.WriteLine($"Found gang beard at {pathToGangBeard}");
                infoText.Text = "Downloading";
                downloadTimer.Enabled = true;
                new Thread(() => DownloadTree(REPO_API_LINK, pathToGangBeard)).Start();
            }
            else
            {
                ChangeText(MSG_GANG_BEASTS_NOT_OPEN);
                retryButton.Visible = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetValues();
            TryInstallLoader();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TryInstallLoader();
        }

        private void downloadTimer_Tick(object sender, EventArgs e)
        {
            infoText.Text = "Downloading" + downloadDots;

            downloadDots += ".";
            if (downloadDots.Length > 3)
            {
                downloadDots = ".";
            }
        }
    }
}

[System.Serializable]
public class GitTreeResponse
{
    public string sha { get; set; }
    public string url { get; set; }
    public bool truncated { get; set; }
    public GitTreeElement[] tree { get; set; }
}

[System.Serializable]
public class GitTreeElement
{
    public string path { get; set; }
    public string mode { get; set; }
    public string type { get; set; }
    public string sha { get; set; }
    public string url { get; set; }
    public int size { get; set; }
}