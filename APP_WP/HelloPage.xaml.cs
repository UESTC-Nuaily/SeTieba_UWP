using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Http;

namespace APP_WP
{

    public class NavLink
    {
        public String Label { get; set; }
        public Type LinkType { get; set; }
        public override String ToString()
        {
            return Label;
        }
    }
    public sealed partial class HelloPage : Page
    {
        public User user;
        public List<NavLink> NavLinks = new List<NavLink>()  //建立汉堡菜单的主要几个页面
        {
            new NavLink() { Label = "Page1", LinkType = typeof(Page1) },
            new NavLink() { Label = "Page2", LinkType = typeof(Page2) },
            new NavLink() { Label = "Page3", LinkType = typeof(Page3) },
            new NavLink() { Label = "Page4", LinkType = typeof(Page4) }
        };
        public HelloPage() {
            this.InitializeComponent();
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
            SystemNavigationManager.GetForCurrentView().BackRequested += PageBackRequested;
        }
        private void NavLinkClick(object sender, ItemClickEventArgs e)   //当汉堡菜单中被选中，则跳转到选中页面
        {
            NavLink link = e.ClickedItem as NavLink;
            if (link != null && link.LinkType != null)
                contentFrame.Navigate(link.LinkType);
            splitView.IsPaneOpen = false;
        }
        private void SplitViewToggle_Click(object sender, RoutedEventArgs e) 
        {
            splitView.IsPaneOpen = !splitView.IsPaneOpen;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e) //接收登录时传输过来的用户信息
        {
            user = (User)e.Parameter;
            if (e.NavigationMode == NavigationMode.New)
            {
                contentFrame.Navigate(typeof(Page1),user);
            }
            base.OnNavigatedTo(e);
        }
        private void PageBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (contentFrame == null)
                return;
            if (contentFrame.CanGoBack)
            {
                e.Handled = true;
                contentFrame.GoBack();
            }
        }
    }
}
