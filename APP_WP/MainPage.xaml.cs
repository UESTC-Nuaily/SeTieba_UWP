using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;

namespace APP_WP
{
    [DataContract(Namespace = "APP_WP")]
    public class Info
    {
        [DataMember(Order = 0)]
        public int errno { get; set; }
        [DataMember(Order = 1)]
        public string msg { get; set; }
        [DataMember(Order = 2)]
        public User user { get; set;}

    }
   [DataContract(Namespace = "APP_WP")]
    public class User
    {
       [DataMember(Order = 0)]
        public int id { get; set; }
       [DataMember(Order = 1)]
        public string name { get; set; }
       [DataMember(Order = 2)]
        public string email { get; set; }
        [DataMember(Order = 3)]
        public string avatar { get; set; }
        [DataMember(Order = 4)]
        public string gender { get; set; }
        [DataMember(Order = 5)]
        public string school_id { get; set; }
        [DataMember(Order = 6)]
        public string tel { get; set; }
        [DataMember(Order = 7)]
        public string birth { get; set; }
        [DataMember(Order = 8)]
        public string created_at { get; set; }
        [DataMember(Order = 9)]
        public string updated_at { get; set; }
    }
    public sealed partial class MainPage : Page
    {
        private string server = "http://202.115.12.170:8000/api/auth";
        private HttpClient httpClient;
        private CancellationTokenSource cts;
        public MainPage()
        {
            this.InitializeComponent();
            httpClient = new HttpClient();
            cts = new CancellationTokenSource();
        }

        private void signup_Click(object sender, RoutedEventArgs e)   //跳转到注册页面
        {
            Frame.Navigate(typeof(Signup));
        }

        private void log_in_Click(object sender, RoutedEventArgs e)       //异步方法post数据实现登录
        {
            HttpRequestAsync(async () =>
            {
                string resourceAddress = server;
                string responseBody;
                HttpStringContent httpcontent = new HttpStringContent("username=" + ID_Box.Text + "&password=" + password_Box.Text);
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
                 var serializer = new DataContractJsonSerializer(typeof(Info));   //将json字符串解析成class
                 var mStream = new MemoryStream(Encoding.Unicode.GetBytes(responseBody));
                 Info info = (Info)serializer.ReadObject(mStream);
                 if (info.errno == 1) {
                     await new MessageDialog(info.msg).ShowAsync();  //登录失败
                     return ;
                 }
                 await new MessageDialog(info.msg).ShowAsync();//登录成功，跳转到第一页面
                 Frame.Navigate(typeof(HelloPage),info.user);
             });
        }
    }
}
