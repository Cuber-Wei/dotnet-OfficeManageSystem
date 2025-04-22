namespace OfficeMgtAdmin
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var user = UserIdEntry.Text?.Trim();
            var pass = PasswordEntry.Text;

            if (string.IsNullOrWhiteSpace(user))
            {
                await DisplayAlert("提示", "用户名不能为空", "确定");
                return;
            }
            if (string.IsNullOrWhiteSpace(pass))
            {
                await DisplayAlert("提示", "密码不能为空", "确定");
                return;
            }
            if (user == "admin" && pass == "0000")
            {
                await DisplayAlert("提示", "登录成功", "确定");
                Application.Current.MainPage = new NavigationPage(new Views.AdminMainPage());
            }
            else
            {
                await DisplayAlert("提示", "用户名或密码不正确", "确定");
            }
        }
    }
}
