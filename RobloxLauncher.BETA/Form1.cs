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
using System.IO;
using System.Drawing.Imaging;

namespace RobloxLauncher.BETA
{
    public partial class Form1 : Form
    {
        RobloxProxyLib.Launcher launcher;

        CookieAwareWebClient client = new CookieAwareWebClient();

        public Form1()
        {
            launcher = new RobloxProxyLib.Launcher();
            InitializeComponent();
            
        }

        // OMG, http://www.roblox.com/Game/PlaceLauncher.ashx?request=RequestGame&placeId=1818

        private void button1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("f");
            TaskDialog dialog = new TaskDialog();
            dialog.Opened += dialog_Opened;
            TaskDialogProgressBar bar = new TaskDialogProgressBar(0, 100, 2);
            bar.State = TaskDialogProgressBarState.Marquee;
            dialog.Controls.Add(bar);

            dialog.StandardButtons = TaskDialogStandardButtons.None;

            backgroundWorker1.RunWorkerCompleted += (s, ev) =>
            {
                if (ev.Result == (object)true)
                {
                    try
                    {
                        dialog.Close(TaskDialogResult.Ok);
                    }
                    catch { }
                }
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

        // Problem might happen when an update is requested, so I'll have to be careful.
        // Might also need to create a Wrapper to handle it, so I'll keep the Wrapper in the project for now.
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            TaskDialog dialog = (TaskDialog)e.Argument;
            try
            {

                Console.WriteLine("Launching ROBLOX as \"userid\": {0}", client.DownloadString("http://www.roblox.com/Game/GetCurrentUser.ashx"));

                if (string.IsNullOrWhiteSpace(textBox1.Text))
                {
                    dialog.Text = "The Place ID is empty.";
                    dialog.ProgressBar.State = TaskDialogProgressBarState.Error;
                    dialog.ProgressBar.Value = 5;
                    e.Result = false;
                    return;
                }

            

                client.DownloadString(new Uri("http://www.roblox.com/Game/KeepAlivePinger.ashx"));

                launcher.ResetLaunchState();
                string read = client.DownloadString(new Uri("http://www.roblox.com/Game/PlaceLauncher.ashx?request=RequestGame&placeId=" + textBox1.Text));
                PlaceLauncherResp resp = Newtonsoft.Json.JsonConvert.DeserializeObject<PlaceLauncherResp>(read);
                
                dialog.Text = "Setting Auth Ticket";
                launcher.AuthenticationTicket = resp.authenticationTicket;

                dialog.Text = "Pre-Start Stage.";
                launcher.BringAppToFront();

                launcher.PreStartGame();

                dialog.Text = "Starting ROBLOX with Auth and Join Script.";
                launcher.StartGame(resp.authenticationUrl, resp.joinScriptUrl);

                e.Result = true;
            }
            catch (Exception ex) { dialog.Text = "An error occured:" + ex.Message; e.Result = false; }
        }

        CookieAwareWebClient.UserInfo userInfo = new CookieAwareWebClient.UserInfo();

        TaskDialog ShowLoginError(string error)
        {
            TaskDialog dialog = new TaskDialog();
            dialog.Icon = TaskDialogStandardIcon.Error;
            dialog.InstructionText = "Error while logging inn.";
            dialog.Text = error;

            dialog.Show();
            return dialog;
        }

        string GetRespError(string respRaw)
        {
            switch (respRaw)
            {
                case "FailedLoginFloodcheck":
                    return "You are trying to log in too fast.";
                case "InvalidPassword":
                    return "The password provided is incorrect.";
                default:
                    return "Unknown Error";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            NetworkCredential cred;
            WindowsSecurity.GetCredentialsVistaAndUp("Roblox Launcher BETA", "Login to ROBLOX", out cred, this);
            if (cred != null)
            {
                CookieAwareWebClient.LoginResponse resp = client.Login(cred);
                if(resp.Status != "OK")
                {
                    TaskDialog dialog = new TaskDialog();
                    Console.WriteLine(resp.Raw);
                    dialog.InstructionText = "Error";
                    dialog.Icon = TaskDialogStandardIcon.Error;
                    dialog.Text = String.Format("An error occured while trying to log in: {0}({1})",GetRespError(resp.Status), resp.Status);
                    dialog.StandardButtons = TaskDialogStandardButtons.Retry | TaskDialogStandardButtons.Cancel;
                    if (dialog.Show() == TaskDialogResult.Retry)
                        button2_Click(sender, e);
                    return;
                }
                userInfo = resp.UserInfo;

                pictureBox1.ImageLocation = "http://www.roblox.com/Thumbs/Avatar.ashx?username=" + userInfo.UserName;

                label2.Text = String.Format("{0} ({1})", userInfo.UserName, userInfo.UserID);
            }
            
        }
    }
}
