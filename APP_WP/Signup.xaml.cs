using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace APP_WP
{
    public sealed partial class Signup : Page
    {
        private string server = "http://202.115.12.170:8000/api/register";
        private HttpClient httpClient;
        private CancellationTokenSource cts;
        public Signup()
        {
            this.InitializeComponent();
            httpClient = new HttpClient();
            cts = new CancellationTokenSource();
        }

        private void sign_up_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            HttpRequestAsync(async () =>
            {
                string resourceAddress = server;
                string responseBody;
                HttpStringContent httpcontent = new HttpStringContent("email=" + email_Box.Text+"&username=" + ID_Box.Text + "&password=" + password_Box.Text);
                httpcontent.Headers.ContentType =new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue( "application/x-www-form-urlencoded");
                HttpResponseMessage response = await httpClient.PostAsync(new Uri(resourceAddress), httpcontent).AsTask(cts.Token);
                responseBody = await response.Content.ReadAsStringAsync().AsTask(cts.Token);
                return responseBody;
            });
        }
        private async void HttpRequestAsync(Func<Task<string>> httpRequestFuncAsync)
        {
            string responseBody;
            try
            {
                responseBody = await httpRequestFuncAsync();
                cts.Token.ThrowIfCancellationRequested();
            }
            catch (TaskCanceledException)
            {
                responseBody = "请求被取消";
            }
            catch (Exception ex)
            {
                responseBody = "异常消息" + ex.Message;
            }
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                Info info = ReadToObject(responseBody);
                if (info.errno == 0)
                {
                    await new MessageDialog(info.msg).ShowAsync();
                    Frame.Navigate(typeof(MainPage));
                    return;
                }
                await new MessageDialog(info.msg).ShowAsync();
            });
        }
        public static Info ReadToObject(string json)
        {
            Info deserializedUser = new Info();
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            DataContractJsonSerializer ser = new DataContractJsonSerializer(deserializedUser.GetType());
            deserializedUser = ser.ReadObject(ms) as Info;
            return deserializedUser;
        }
    }
}
