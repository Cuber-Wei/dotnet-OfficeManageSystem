using System.Text.Json;

namespace OfficeMgtAdmin
{
    public partial class MainPage : ContentPage
    {
        private readonly string _userJsonPath;

        public MainPage()
        {
            InitializeComponent();
            _userJsonPath = Path.Combine("F:", "code_repository", "dotNetProjects", "OfficeMgtAdmin", "user.json");
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserIdEntry.Text) || string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert("提示", "用户ID和密码不能为空", "确定");
                return;
            }

            try
            {
                var userJson = File.ReadAllText(_userJsonPath);
                var users = JsonSerializer.Deserialize<List<Models.User>>(userJson);

                var user = users?.FirstOrDefault(u => u.UserId == UserIdEntry.Text && u.Password == PasswordEntry.Text);
                if (user == null)
                {
                    await DisplayAlert("错误", "用户名或密码不正确", "确定");
                    return;
                }

                await Navigation.PushAsync(new Views.AdminMainPage());
                Navigation.RemovePage(this);
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"登录失败: {ex.Message}", "确定");
            }
        }
    }
}
