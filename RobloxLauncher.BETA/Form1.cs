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

            dialog.StandardButtons = TaskDialogStandardButtons.None;

            backgroundWorker1.RunWorkerCompleted += (s, ev) =>
            {
                try
                {
                    dialog.Close(TaskDialogResult.Ok);
                }
                catch { }
            };

            dialog.InstructionText = "Launching Roblox...";
            dialog.Text = "Getting Authentication Url, Ticket, and Join Script.";
            dialog.Closing += dialog_Closing;
            dialog.Show();
        }

        void dialog_Closing(object sender, TaskDialogClosingEventArgs e)
        {
            Console.WriteLine(e.TaskDialogResult);
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
            TaskDialog dialog = (TaskDialog)e.Argument;
            WebClient client = new WebClient();
            launcher.ResetLaunchState();
            string read = client.DownloadString(new Uri("http://www.roblox.com/Game/PlaceLauncher.ashx?request=RequestGame&placeId=1818"));
            PlaceLauncherResp resp = Newtonsoft.Json.JsonConvert.DeserializeObject<PlaceLauncherResp>(read);
            launcher.Update();
            dialog.Text = "Setting Auth Ticket";
            launcher.AuthenticationTicket = resp.authenticationTicket;
            launcher.Update();
            dialog.Text = "Pre-Start Stage.";
            launcher.BringAppToFront();

            launcher.PreStartGame();

            launcher.Update();

            dialog.Text = "Starting ROBLOX with Auth and Join Script.";
            launcher.StartGame(resp.authenticationUrl, resp.joinScriptUrl);
           
            e.Result = true;
        }
    }
}
