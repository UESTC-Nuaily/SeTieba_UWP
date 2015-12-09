using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace APP_WP
{
    public class Newpost {
        public string title = null;
        public string content = null;
        public string pics = null;
        public string at_users = null;
        public int subject_id;
        public int ba_id = 1;
    }
    public class NewpostInfo {
        public int errno { get; set; }
        public string msg { get; set; }
        public int pid { get; set; }
    }
    public sealed partial class NewPost : Page
    {
        User user;
        private string server = "http://202.115.12.170:8000/api/post";
        private HttpClient httpClient;
        private CancellationTokenSource cts;
        public NewPost()
        {
            this.InitializeComponent();
            httpClient = new HttpClient();
            cts = new CancellationTokenSource();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e) //接收登录时传输过来的用户信息
        {
            user = (User)e.Parameter;
        }

        private void Newpost_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Newpost newpost = new Newpost();
            newpost.title = Title_Box.Text;
            newpost.content = Content_Box.Text;
            newpost.subject_id = user.id;
            newpost.ba_id = 1;
            HttpRequestAsync(async () =>
            {
                string resourceAddress = server;
                string responseBody;
                HttpStringContent httpcontent = new HttpStringContent("title="+ newpost.title+"&content="+ newpost.content+"&subject_id="+ newpost.subject_id
                    +"&ba_id="+ newpost.ba_id+"&pics="+ newpost.pics+"&at_users="+ newpost.at_users);
                httpcontent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
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
                var serializer = new DataContractJsonSerializer(typeof(NewpostInfo));   //将json字符串解析成class
                var mStream = new MemoryStream(Encoding.Unicode.GetBytes(responseBody));
                NewpostInfo info = (NewpostInfo)serializer.ReadObject(mStream);
                if (info.errno == 0)
                {
                    await new MessageDialog(info.msg).ShowAsync();  //登录成功
                    Frame.Navigate(typeof(Page1));
                    return;
                }
                await new MessageDialog(info.msg).ShowAsync();
            });
        }
    }
}
