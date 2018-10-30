using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MediumSDK.Net.Domain
{
    public class MediumClient
    {
        public MediumClient(string clientId, string clientSecret, string state)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            State = state;
        }

        /// <summary>
        /// Medium user token
        /// </summary>
        public Token Token { get; private set; }


        /// <summary>
        /// Medium user
        /// </summary>
        public MediumUser User { get; private set; }


        /// <summary>
        /// Your client secret
        /// </summary>
        public string ClientSecret { get; private set; }


        /// <summary>
        /// Your client ID  
        /// </summary>
        public string ClientId { get; private set; }


        /// <summary>   
        /// State
        /// </summary>
        public string State { get; private set; }

        public async Task<Token> AuthenticateUser()
        {
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
            if (User == null)
            {
                var tokenRequest = (HttpWebRequest)WebRequest.Create(MediumRoutes.UserProfile);
                tokenRequest.Method = "GET";
                tokenRequest.ContentType = "application/x-www-form-urlencoded";
                tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                tokenRequest.Headers["Authorization"] = "Bearer " + Token;

                var user = tokenRequest.GetResponseJson<MediumUser>();

                return Task.FromResult(user);
            }
            else
            {
                return Task.FromResult(User);
            }
        }


        private async Task<string> GetAuthCode()
        {
            var http = new HttpListener();
            http.Prefixes.Add(MediumRoutes.RedirectUrl);
            http.Start();

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

            var context = await http.GetContextAsync();
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
            http.Close();

            return await Task.FromResult(code);
        }
    }
}
