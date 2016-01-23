using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RobloxLauncher.BETA.Wrapper
{
    public class LauncherWrapper
    {

        Assembly launcherAssem;
        Type Launcher;

        public LauncherWrapper()
        {
            launcherAssem = Assembly.LoadFrom(@"C:\Users\DevelopmentUser\Documents\visual studio 2013\Projects\RobloxLauncher.BETA\RobloxLauncher.BETA\obj\x86\Debug\Interop.RobloxProxyLib.dll");
            Launcher = launcherAssem.GetType("RobloxProxyLib.Launcher");

            foreach (MethodInfo t in Launcher.GetMethods())
            {
                Console.WriteLine(t);
            }
            //Launcher.GetMethod("HelloWorld").Invoke();
        }

        /*
        string AuthenticationTicket { set; }
        
        string InstallHost { get; }
        
        bool IsGameStarted { get; }
        
        bool IsUpToDate { get; }


        void BringAppToFront()
        {
            
        }
        
        void DeleteKeyValue(string key);
        
        string Get_Version();
        
        string GetKeyValue(string key);
        
        string Hello();
        
        void HelloWorld();
        
        void PreStartGame();
        
        void ResetLaunchState();
        
        void SetEditMode();
        
        void SetKeyValue(string key, string val);
       
        void SetSilentModeEnabled(bool silentModeEnbled);
        
        void SetStartInHiddenMode(bool hiddenModeEnabled);
        
        void StartGame(string authenticationUrl, string script);
        
        void UnhideApp();
        
        void Update();
        */
    }
}
