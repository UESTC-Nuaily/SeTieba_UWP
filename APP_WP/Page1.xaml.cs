using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace APP_WP
{
    public class PostInfo
    {
        public int errno { get; set; }
        public string msg { get; set; }
        public int hotSum { get; set; }
        public List<Posts> posts { get; set; }   //因为获得的post信息是动态的，所以选择用list来接收
    }
    public class Posts
    {
        public int pid { get; set; }
        public int uid { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string pics { get; set; }
        public int subject_id { get; set; }
        public int ba_id { get; set; }
        public string at_users { get; set; }
        public string last_comment_at { get; set; }
        public int last_comment_id { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string deleted_at { get; set; }

    }
    public sealed partial class Page1 : Page
    {
        private string server = "http://202.115.12.170:8000/api/hotPosts";
        private HttpClient httpClient;
        private CancellationTokenSource cts;
        PostInfo hotpost;
        ObservableCollection<Posts> PostTitle = new ObservableCollection<Posts>();
        User user;
        public Page1()
        {
            this.InitializeComponent();
            hotpost_list.ItemsSource = PostTitle;
            httpClient = new HttpClient();
            cts = new CancellationTokenSource();
            HttpRequestAsync(async () =>
            {
                string resourceAddress = server;
                string responseBody;
                HttpResponseMessage response = await httpClient.GetAsync(new Uri(resourceAddress)).AsTask(cts.Token);
                responseBody = await response.Content.ReadAsStringAsync().AsTask(cts.Token);
                var serializer = new DataContractJsonSerializer(typeof(PostInfo));   //将json字符串解析成class
                var mStream = new MemoryStream(Encoding.Unicode.GetBytes(responseBody));
                hotpost = (PostInfo)serializer.ReadObject(mStream);
                init_list(hotpost);
                return responseBody;
            });
            
        }

        public void show_list(PostInfo hotpost)
        {
            int i;
            for (i = 0; i < hotpost.hotSum; i++)
                PostTitle.Add(new Posts { title = hotpost.posts[i].title });
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
                
        }
        public void init_list(PostInfo posts) {
            int i;
            for(i=0;i<hotpost.hotSum;i++)
            PostTitle.Add(new Posts { title=hotpost.posts[i].title});
        }
        private void hotpost_list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Frame.Navigate(typeof(Post), hotpost.posts[hotpost_list.SelectedIndex].pid); //将选中的post传入帖子页面
        }
        private void newpost_button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(NewPost), user);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e) //接收登录时传输过来的用户信息
        {
            user = (User)e.Parameter;
        }
    }
}
