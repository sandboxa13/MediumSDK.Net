using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DryIocAttributes;
using Newtonsoft.Json;

namespace MediumSDK.Net.Domain
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(IMediumClient))]
    public class MediumClient : IMediumClient   
    {
        private HttpListener _httpListener;

        public MediumClient()
        {
            ClientId = "ce250fa7c114";
            ClientSecret = "bb152d21f43b20de5174495f488cd71aede8efaa";
            State = "text";

            Token = new Token();
            User = new MediumUser();
        }

        /// <summary>
        /// Medium user token
        /// </summary>
        public Token Token { get; set; }


        /// <summary>
        /// Medium user
        /// </summary>
        public MediumUser User { get; set; }


        /// <summary>
        /// Your client secret
        /// </summary>
        public string ClientSecret { get; set; }


        /// <summary>
        /// Your client ID  
        /// </summary>
        public string ClientId { get; set; }


        /// <summary>   
        /// State
        /// </summary>
        public string State { get; set; }


        public async Task<Token> AuthenticateUser()
        {
            Task.Run(() => StartTimer());

            var code = await GetAuthCode();

            var tokenRequestBody =
                $"code={code}&client_id={ClientId}&client_secret={ClientSecret}&grant_type=authorization_code&redirect_uri={Uri.EscapeDataString(MediumRoutes.RedirectUrl)}";

            var tokenRequest = (HttpWebRequest)WebRequest.Create(MediumRoutes.Token);
            tokenRequest.Method = "POST";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            var byteVersion = Encoding.ASCII.GetBytes(tokenRequestBody);
            tokenRequest.ContentLength = byteVersion.Length;
            var stream = tokenRequest.GetRequestStream();
            await stream.WriteAsync(byteVersion, 0, byteVersion.Length);
            stream.Close();

            var tokenResponse = tokenRequest.GetResponse();

            using (var reader = new StreamReader(tokenResponse.GetResponseStream() ?? throw new NullReferenceException()))
            {
                var responseText = await reader.ReadToEndAsync();

                Token = JsonConvert.DeserializeObject<Token>(responseText);
            }

            return await Task.FromResult(Token);
        }

        public Task<MediumUser> GetUser()
        {
            if (Token.AccessToken == null) return Task.FromResult(User);

            var tokenRequest = (HttpWebRequest)WebRequest.Create(MediumRoutes.UserProfile);
            tokenRequest.Method = "GET";
            tokenRequest.ContentType = "application/x-www-form-urlencoded";
            tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            tokenRequest.Headers["Authorization"] = "Bearer " + Token.AccessToken;

            var user = tokenRequest.GetResponseJson<MediumUser>();

            return Task.FromResult(user);
        }

        private async Task<string> GetAuthCode()
        {
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(MediumRoutes.RedirectUrl);
            _httpListener.Start();
            var url =
                $"{MediumRoutes.Authorize}?client_id={ClientId}&scope=basicProfile,publishPost&state={State}&response_type=code&redirect_uri={MediumRoutes.RedirectUrl}";

            var proc = new Process
            {
                StartInfo =
                    {
                        UseShellExecute = true,
                        FileName = url
                    }
            };
            proc.Start();

            var context = await _httpListener.GetContextAsync();
            var response = context.Response;
            var responseString = "<html><head><meta http-equiv=\'refresh\' content=\'10;url=https://google.com\'></head><body>Please return to the app.</body></html>";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;

            await responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
            });

            var code = context.Request.QueryString.Get("code");
            _httpListener.Close();

            return code;
        }

        private void StartTimer()
        {
            var countDown = 60;

            while (true)
            {
                if (countDown == 0)
                {
                    _httpListener.Stop();
                    _httpListener.Close();
                    break;
                }

                countDown--;
                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
            }
        }
    }
}
