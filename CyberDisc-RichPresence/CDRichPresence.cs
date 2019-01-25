using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Automation;
using DiscordRPC;
using System.Collections.Generic;
using System.Linq;

namespace CyberDisc_RichPresence
{

    public class CDRichPresence : Form
    {
        private NotifyIcon trayIcon;
        private IContainer components;
        public DiscordRpcClient client;

        public CDRichPresence()
        {
            InitializeComponent();
            Load += OnLoad;
            ContextMenu menu = new ContextMenu();
            menu.MenuItems.Add(0, new MenuItem("Exit", new EventHandler(Exit_click)));
            trayIcon.ContextMenu = menu;
        }

        private void OnLoad(object sender, EventArgs e)
        {
            Run();
        }

        private async Task Run()
        {
            await Task.Delay(10);
            Hide();
            StartRichPresence();
            trayIcon.ShowBalloonTip(1000);
        }

        private void OnResize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void Exit_click(object sender, EventArgs e)
        {
            trayIcon.Dispose();
            Environment.Exit(0);
        }

        public void StartRichPresence()
        {
            client = new DiscordRpcClient("537710248664760322");
            client.Initialize();
            client.SetPresence(new RichPresence()
            {
                Details = "Idle",
                //State = "Idle",
                Assets = new Assets()
                {
                    LargeImageKey = "cd-logo",
                    LargeImageText = "Cyber Discovery",
                    SmallImageKey = "cd-logo"
                }
            });
            var timer = new System.Timers.Timer(150);
            timer.Elapsed += (sender, args) => { UpdateFromURL(); };
            timer.Start();
        }

        private void UpdateFromURL()
        {
            client.Invoke();
            string url = CurrentURL();
            if (url == null || url == "")
            {
                client.ClearPresence();
                return;
            }
            if (url.Contains("game"))
            {
                DoGameUpdate(url);
                return;
            }
            if (url.Contains("assess"))
            {
                DoAssesUpdate(url);
                return;
            }
            if (url.Contains("essentials"))
            {
                DoEssentialsUpdates(url);
                return;
            }
            if (url.Contains("elite"))
            {
                DoEliteUpdates(url);
                return;
            }
            SetRP("Idle", "", "cd");
        }

        private void DoEliteUpdates(string url)
        {
            SetRP("Working on elite", "Wait what this isnt possible", "game");
        }

        private void DoEssentialsUpdates(string url)
        {
            try
            {
                string[] args = url.Split('/');
                if (args.Where(s => s.Contains("section")).FirstOrDefault() != null)
                {
                    string cdmodule = args.ElementAt(Array.IndexOf(args, "module") + 1);
                    string cdsection = args.ElementAt(Array.IndexOf(args, "section") + 1);
                    if (cdsection.Contains('.')) cdsection = cdsection.Split('.')[0];
                    UpdateRP("", cdmodule, cdsection, "essentials");
                }
                else
                {
                    SetRP("Working on essentials", "Idle", "essentials");
                }
            }
            catch
            {
                SetRP("Working on essentials", "Idle", "essentials");
            }
        }

        private void DoAssesUpdate(string url)
        {
            try
            {
                string cdchal = url.Split('-')[1];
                UpdateRP("", "", cdchal, "assess");
            }
            catch
            {
                SetRP("Working on assess", "Idle", "assess");
                return;
            }
        }

        private void DoGameUpdate(string url)
        {
            try
            {
                string cdbase;
                string cdlevel;
                string cdchal;
                string[] args = url.Split('?')[1].Split('&');
                cdbase = args.Where(s => s.Contains("base")).FirstOrDefault().Split('=')[1];
                cdlevel = args.Where(s => s.Contains("level")).FirstOrDefault().Split('=')[1];
                cdchal = args.Where(s => s.Contains("challenge")).FirstOrDefault().Split('=')[1];
                UpdateRP(cdbase, cdlevel, cdchal, "game");
            }
            catch
            {
                SetRP("Working on game", "Idle", "game");
                return;
            }
        }

        private void UpdateRP(string cdbase, string cdlevel, string cdchal, string cdstage)
        {
            string leveltext = "";
            switch (cdbase)
            {
                case "1":
                    leveltext = "On HQ L" + cdlevel + "C" + cdchal;
                    break;
                case "2":
                    leveltext = "On Moon L" + cdlevel + "C" + cdchal;
                    break;
                case "3":
                    leveltext = "On Forensics L" + cdlevel + "C" + cdchal;
                    break;
            }
            if (cdstage == "assess") leveltext = "On challenge " + cdchal;
            if (cdstage == "essentials") leveltext = "On module M" + cdlevel + "S" + cdchal;
            SetRP("Working on " + cdstage, leveltext, cdstage);
        }

        private string CurrentURL()
        {
            List<string> urls = new List<string>();
            Process[] procsChrome = Process.GetProcessesByName("chrome");
            foreach (Process chrome in procsChrome)
            {
                if (chrome.MainWindowHandle == IntPtr.Zero) continue;

                AutomationElement element = AutomationElement.FromHandle(chrome.MainWindowHandle);
                if (element == null)
                    return null;
                Condition conditions = new AndCondition(
                    new PropertyCondition(AutomationElement.ProcessIdProperty, chrome.Id),
                    new PropertyCondition(AutomationElement.IsControlElementProperty, true),
                    new PropertyCondition(AutomationElement.IsContentElementProperty, true),
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));

                AutomationElement elementx = element.FindFirst(TreeScope.Descendants, conditions);
                if (elementx == null) continue;
                urls.Add(((ValuePattern)elementx.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string);
            }
            return urls.Where(s => s.Contains("joincyberdiscovery")).FirstOrDefault();
        }

        private void SetRP(string baseText, string challengeText, string icon)
        {
            client.SetPresence(new RichPresence()
            {
                Details = baseText,
                State = challengeText,
                Assets = new Assets()
                {
                    LargeImageKey = "cd-logo",
                    LargeImageText = "Cyber Discovery",
                    SmallImageKey = icon
                }
            });
        }

        private void InitializeComponent()
        {
            components = new Container();
            ComponentResourceManager resources = new ComponentResourceManager(typeof(CDRichPresence));
            trayIcon = new NotifyIcon(this.components);
            SuspendLayout();
            trayIcon.BalloonTipIcon = ToolTipIcon.Info;
            trayIcon.BalloonTipText = "Rich presence is running, right click to interact";
            trayIcon.BalloonTipTitle = "CyberDiscovery Rich Presence";
            trayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("trayIcon.Icon")));
            trayIcon.Text = "CyberDiscovery Rich Presence";
            trayIcon.Visible = true;
            ClientSize = new System.Drawing.Size(120, 0);
            Name = "CDRichPresence";
            ResumeLayout(false);

        }
    }
}
