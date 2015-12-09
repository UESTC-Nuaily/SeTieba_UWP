using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    public class CInfo {
        public int errno { get; set; }
        public string msg { get; set; }
    }
    public class postInfo
    {
        public int errno { get; set; }
        public string msg { get; set; }
        public Posts post { get; set; }
    }
    public class floorInfo
    {
        public int errno { get; set; }
        public string msg { get; set; }
        public int floorSum { get; set; }
        public List<Floors> floors { get; set; }
    }
    public class Floors
    {
        public int fid { get; set; }
        public int pid { get; set; }
        public int uid { get; set; }
        public string content { get; set; }
        public string pics { get; set; }
        public string at_users { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string deleted_at { get; set; }
        public List<Comment> comments { get; set; }
        public int commentSum { get; set; }
    }
    public class Comment
    {
        public int cid { get; set; }
        public int from_id { get; set; }
        public int to_id { get; set; }
        public int to_pid { get; set; }
        public int to_fid { get; set; }
        public string content { get; set; }
        public string pics { get; set; }
        public string at_users { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string deleted_at { get; set; }
    }
    public sealed partial class Post : Page
    {
        int pid;
        postInfo postinfo;
        floorInfo floors;
        private string commentServer = "http://202.115.12.170:8000/api/floorDetail?pid=";
        private string postServer = "http://202.115.12.170:8000/api/post/";
        private string server = "http://202.115.12.170:8000/api/replyPost";
        private HttpClient httpClient;
        private CancellationTokenSource cts;
        ObservableCollection<Floors> CommentList = new ObservableCollection<Floors>();
        public Post()
        {
            this.InitializeComponent();
            httpClient = new HttpClient();
            cts = new CancellationTokenSource();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e) 
        {
            postInfo_list.ItemsSource = CommentList;
            pid = (int)e.Parameter;
            HttpRequestAsync(async () =>
            {
                string resourceAddress = postServer + pid;
                string responseBody;
                HttpResponseMessage response = await httpClient.GetAsync(new Uri(resourceAddress)).AsTask(cts.Token);
                responseBody = await response.Content.ReadAsStringAsync().AsTask(cts.Token);
                var serializer = new DataContractJsonSerializer(typeof(postInfo));   //将json字符串解析成class
                var mStream = new MemoryStream(Encoding.Unicode.GetBytes(responseBody));
                postinfo = (postInfo)serializer.ReadObject(mStream);
                init_post(postinfo.post);
                resourceAddress = commentServer + pid;
                response = await httpClient.GetAsync(new Uri(resourceAddress)).AsTask(cts.Token);
                responseBody = await response.Content.ReadAsStringAsync().AsTask(cts.Token);
                serializer = new DataContractJsonSerializer(typeof(floorInfo));   //将json字符串解析成class
                mStream = new MemoryStream(Encoding.Unicode.GetBytes(responseBody));
                floors = (floorInfo)serializer.ReadObject(mStream);
                init_comment(floors);
                return responseBody;
            });
            
        }
        public void init_post(Posts post) {
            title_Block.Text = "标题: " + post.title;
            content_Block.Text = "内容: " + post.content;
            subject_id_Block.Text = "发布者id: " + post.subject_id.ToString();
            last_comment_at_Block.Text = "最后评论时间: " + post.last_comment_at;
            created_at_Block.Text = "发布时间: " + post.created_at;
        }
        public void init_comment(floorInfo floors)
        {
            int i;
            for (i = 0; i < floors.floorSum; i++)
                CommentList.Add(new Floors { created_at=floors.floors[i].created_at, content = floors.floors[i].content });
        }
        private void Comment_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            HttpRequestAsync(async () =>
            {
                string resourceAddress = server;
                string responseBody;
                HttpStringContent httpcontent = new HttpStringContent("pid="+pid+"&content="+textBox.Text);
                httpcontent.Headers.ContentType = new Windows.Web.Http.Headers.HttpMediaTypeHeaderValue("application/x-www-form-urlencoded");
                HttpResponseMessage response = await httpClient.PostAsync(new Uri(resourceAddress), httpcontent).AsTask(cts.Token);
                responseBody = await response.Content.ReadAsStringAsync().AsTask(cts.Token);
                var serializer = new DataContractJsonSerializer(typeof(CInfo));   //将json字符串解析成class
                var mStream = new MemoryStream(Encoding.Unicode.GetBytes(responseBody));
                CInfo info = (CInfo)serializer.ReadObject(mStream);
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
            
        }
    }
}
