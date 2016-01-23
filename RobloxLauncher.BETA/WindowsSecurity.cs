using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobloxLauncher.BETA
{
    public class InvalidOperatingSystemException : Exception
    {
        public InvalidOperatingSystemException(OperatingSystem systemInvalid)
            : base("System type of " + systemInvalid.VersionString + " is not usable.")
        {

        }

        public InvalidOperatingSystemException(string aboveOS)
            : base("The system is not above \"" + aboveOS + "\".")
        {

        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct CredentialUI_Info
    {
        public int cbSize;
        public IntPtr hwndParent;
        public string pszMessageText;
        public string pszCaptionText;
        public IntPtr hbmBanner;
    }

    internal static class WindowsSecurityNative
    {
        [DllImport("ole32.dll")]
        internal static extern void CoTaskMemFree(IntPtr ptr);

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        internal static extern int CredUIPromptForWindowsCredentials(ref CredentialUI_Info notUsedHere,
                                                                     int authError,
                                                                     ref uint authPackage,
                                                                     IntPtr InAuthBuffer,
                                                                     uint InAuthBufferSize,
                                                                     out IntPtr refOutAuthBuffer,
                                                                     out uint refOutAuthBufferSize,
                                                                     ref bool fSave,
                                                                     int flags);




        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        internal static extern bool CredUnPackAuthenticationBuffer(int dwFlags,
                                                                   IntPtr pAuthBuffer,
                                                                   uint cbAuthBuffer,
                                                                   StringBuilder pszUserName,
                                                                   ref int pcchMaxUserName,
                                                                   StringBuilder pszDomainName,
                                                                   ref int pcchMaxDomainame,
                                                                   StringBuilder pszPassword,
                                                                   ref int pcchMaxPassword);


    }

    public class WindowsSecurity
    {
        #region Native CredUI

        public enum CredUIPromptFlags : int
        {
            ALWAYS_SHOW_UI = 0x00080,
            COMPLETE_USERNAME = 0x00800,
            NO_NOT_PERSIST = 0x00002,
            EXCLUDE_CERTIFICATES = 0x00008,
            EXPECT_CONFIRMATION = 0x20000,
            GENERIC_CREDENTIALS = 0x40000,
            FLAGS_INCORRECT_PASSWORD = 0x00001,
            KEEP_USERNAME = 0x100000,
            PASSWORD_ONLY_OK = 0x00200,
            PERSIST = 0x01000,
            REQUEST_ADMINISTRATOR = 0x00004,
            REQUIRE_CERTIFICATE = 0x00010,
            REQUIRE_SMARTCARD = 0x00100,
            SERVER_CREDENTIAL = 0x04000,
            SHOW_SAVE_CHECK_BOX = 0x00040,
            USERNAME_TARGET_CREDENTIALS = 0x80000,
            VALIDATE_USERNAME = 0x00400
        }

        public enum CredUIReturnValues
        {
            ERROR_CANCELLED,
            ERROR_INVALID_FLAGS,
            ERROR_INVALID_PARAMETER,
            ERROR_NO_SUCH_LOGON_SESSION,
            ERROR_NONE
        }
        #endregion

        public static CredUIReturnValues ShowPromptForWindowsCredentials(CredentialUI_Info credui, CredUIPromptFlags flags, out NetworkCredential cred)
        {
            if (IsWinVistaOrHigher())
                throw new InvalidOperatingSystemException("Windows XP");

            credui.cbSize = Marshal.SizeOf(credui);
            uint authPackage = 0;
            IntPtr outCredBuffer = new IntPtr();
            uint outCredSize;
            bool save = false;

            int result = WindowsSecurityNative.CredUIPromptForWindowsCredentials(ref credui,
                                                           0,
                                                           ref authPackage,
                                                           IntPtr.Zero,
                                                           0,
                                                           out outCredBuffer,
                                                           out outCredSize,
                                                           ref save,
                                                           (int)flags);

            cred = null;

            if (result == 0)
            {
                var usernameBuf = new StringBuilder(100);
                var passwordBuf = new StringBuilder(100);
                var domainBuf = new StringBuilder(100);

                int maxUserName = 100;
                int maxDomain = 100;
                int maxPassword = 100;
                if (WindowsSecurityNative.CredUnPackAuthenticationBuffer(0, outCredBuffer, outCredSize, usernameBuf, ref maxUserName,
                                                   domainBuf, ref maxDomain, passwordBuf, ref maxPassword))
                {
                    //TODO: ms documentation says we should call this but i can't get it to work
                    //SecureZeroMem(outCredBuffer, outCredSize);

                    //clear the memory allocated by CredUIPromptForWindowsCredentials 
                    WindowsSecurityNative.CoTaskMemFree(outCredBuffer);
                    cred = new NetworkCredential()
                    {
                        UserName = usernameBuf.ToString(),
                        Password = passwordBuf.ToString(),
                        Domain = domainBuf.ToString()
                    };
                }
            }

            return (CredUIReturnValues)result;
        }

        static bool IsWinVistaOrHigher()
        {
            OperatingSystem OS = Environment.OSVersion;
            return (OS.Platform == PlatformID.Win32NT) && (OS.Version.Major >= 6);
        }

        public static void GetCredentialsVistaAndUp(string captionText, string messageText, out NetworkCredential networkCredential, IntPtr pointer)
        {
            CredentialUI_Info credui = new CredentialUI_Info();
            //credui.pszCaptionText = "Please enter the credentails for " + serverName;
            credui.pszCaptionText = captionText;
            credui.pszMessageText = messageText;
            credui.hwndParent = pointer;
            credui.cbSize = Marshal.SizeOf(credui);
            uint authPackage = 0;
            IntPtr outCredBuffer = new IntPtr();
            uint outCredSize;
            bool save = false;
            int result = WindowsSecurityNative.CredUIPromptForWindowsCredentials(ref credui,
                                                           0,
                                                           ref authPackage,
                                                           IntPtr.Zero,
                                                           0,
                                                           out outCredBuffer,
                                                           out outCredSize,
                                                           ref save,
                                                           1 /* Generic */);

            var usernameBuf = new StringBuilder(100);
            var passwordBuf = new StringBuilder(100);
            var domainBuf = new StringBuilder(100);

            int maxUserName = 100;
            int maxDomain = 100;
            int maxPassword = 100;
            if (result == 0)
            {
                if (WindowsSecurityNative.CredUnPackAuthenticationBuffer(0, outCredBuffer, outCredSize, usernameBuf, ref maxUserName,
                                                   domainBuf, ref maxDomain, passwordBuf, ref maxPassword))
                {
                    //TODO: ms documentation says we should call this but i can't get it to work
                    //SecureZeroMem(outCredBuffer, outCredSize);

                    //clear the memory allocated by CredUIPromptForWindowsCredentials 
                    WindowsSecurityNative.CoTaskMemFree(outCredBuffer);
                    networkCredential = new NetworkCredential()
                    {
                        UserName = usernameBuf.ToString(),
                        Password = passwordBuf.ToString(),
                        Domain = domainBuf.ToString()
                    };
                    return;
                }
            }

            networkCredential = null;
        }

        public static void GetCredentialsVistaAndUp(string captionText, string messageText, out NetworkCredential networkCredential, Form form = null)
        {
            IntPtr ptr = IntPtr.Zero;
            if(form != null)
                ptr = form.Handle;
            GetCredentialsVistaAndUp(captionText, messageText, out networkCredential, ptr);
        }

    }
}
