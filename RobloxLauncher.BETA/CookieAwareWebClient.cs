using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Drawing;

namespace RobloxLauncher.BETA
{

    public class CookieAwareWebClient : WebClient
    {

        public class RobloxLoginRequest
        {
            public string username;
            public string password;
            public bool isCaptchaOn = false;
            public string challenge = "";
            public string captchaResponse = "";
        }


        public class UserInfo
        {
            public int UserID { get; set; }
            public string UserName { get; set; }
            public int RobuxBalance { get; set; }
            public int TicketsBalance { get; set; }
            public string ThumbnailUrl { get; set; }
            public bool IsAnyBuildersClubMember { get; set; }

            
        }

        public class LoginResponse
        {
            public string Status { get; set; }
            public UserInfo UserInfo { get; set; }
            public object PunishmentInfo { get; set; }
            [JsonIgnore]
            public string Raw { get; set; }
        }

        public void GetCaptchaChallengeImage()
        {

        }

        public LoginResponse Login(NetworkCredential cred)
        {
            RobloxLoginRequest loginData = new RobloxLoginRequest();
            loginData.username = cred.UserName;
            loginData.password = cred.Password;
            CookieContainer container;

            var request = (HttpWebRequest)WebRequest.Create("https://www.roblox.com/MobileAPI/Login");

            request.Method = "POST";
            request.ContentType = "application/json";
            var buffer = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(loginData));
            request.ContentLength = buffer.Length;
            var requestStream = request.GetRequestStream();
            requestStream.Write(buffer, 0, buffer.Length);
            requestStream.Close();

            container = request.CookieContainer = new CookieContainer();

            var response = request.GetResponse();
            LoginResponse resp = new LoginResponse();
            string raw = "";
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                raw = reader.ReadToEnd();
                resp = JsonConvert.DeserializeObject<LoginResponse>(raw);
                resp.Raw = raw;
            }
            response.Close();
            CookieContainer = container;

            return resp;
        }

        public CookieAwareWebClient(CookieContainer container)
        {
            CookieContainer = container;
        }

        public CookieAwareWebClient()
            : this(new CookieContainer())
        { }

        public CookieContainer CookieContainer { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            request.CookieContainer = CookieContainer;
            return request;
        }
    }
}
