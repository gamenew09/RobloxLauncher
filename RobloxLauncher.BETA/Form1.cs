using RobloxLauncher.BETA.Wrapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.WindowsAPICodePack.Dialogs;
using System.Net;

namespace RobloxLauncher.BETA
{
    public partial class Form1 : Form
    {
        RobloxProxyLib.Launcher launcher;
        LauncherWrapper wrapper;

        public Form1()
        {
            launcher = new RobloxProxyLib.Launcher();
            wrapper = new LauncherWrapper();
            InitializeComponent();
            
        }

        // OMG, http://www.roblox.com/Game/PlaceLauncher.ashx?request=RequestGame&placeId=1818

        private void button1_Click(object sender, EventArgs e)
        {
            TaskDialog dialog = new TaskDialog();
            dialog.Opened += dialog_Opened;
            TaskDialogProgressBar bar = new TaskDialogProgressBar(0, 100, 2);
            bar.State = TaskDialogProgressBarState.Marquee;
            dialog.Controls.Add(bar);

            backgroundWorker1.RunWorkerCompleted += (s, ev) =>
            {
                dialog.Close();
            };

            dialog.Show();
        }

        void dialog_Opened(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync((TaskDialog)sender);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
        }


        public class PlaceLauncherResp
        {
            public string jobId { get; set; }
            public int status { get; set; }
            public string joinScriptUrl { get; set; }
            public string authenticationUrl { get; set; }
            public string authenticationTicket { get; set; }
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            WebClient client = new WebClient();
            string read = client.DownloadString(new Uri("http://www.roblox.com/Game/PlaceLauncher.ashx?request=RequestGame&placeId=1818"));
            PlaceLauncherResp resp = Newtonsoft.Json.JsonConvert.DeserializeObject<PlaceLauncherResp>(read);
            launcher.Update();
            launcher.AuthenticationTicket = resp.authenticationTicket;
            launcher.Update();
            launcher.BringAppToFront();

            launcher.PreStartGame();

            launcher.Update();

            launcher.StartGame(resp.authenticationUrl, resp.joinScriptUrl);
            e.Result = true;
        }
    }
}
