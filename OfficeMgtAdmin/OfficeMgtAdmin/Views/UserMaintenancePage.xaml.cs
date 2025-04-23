using System.Collections.ObjectModel;
using System.Text.Json;
using OfficeMgtAdmin.Models;

namespace OfficeMgtAdmin.Views
{
    public partial class UserMaintenancePage : ContentPage
    {
        private readonly string _userJsonPath;
        private List<User> _users;

        public UserMaintenancePage()
        {
            InitializeComponent();
            _userJsonPath = Path.Combine("F:", "code_repository", "dotNetProjects", "OfficeMgtAdmin", "user.json");
            _users = new List<User>();
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                var userJson = File.ReadAllText(_userJsonPath);
                _users = JsonSerializer.Deserialize<List<User>>(userJson) ?? new List<User>();
                UsersCollection.ItemsSource = _users;
            }
            catch (Exception ex)
            {
                DisplayAlert("错误", $"加载用户数据失败: {ex.Message}", "确定");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UserIdEntry.Text) || string.IsNullOrWhiteSpace(UserNameEntry.Text))
            {
                await DisplayAlert("提示", "用户ID和姓名不能为空", "确定");
                return;
            }

            var user = new User
            {
                Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1,
                UserId = UserIdEntry.Text.Trim(),
                UserName = UserNameEntry.Text.Trim(),
                Password = PasswordEntry.Text?.Trim() ?? "123456",
                Gender = GenderPicker.SelectedItem?.ToString(),
                BirthDate = BirthDatePicker.Date,
                Phone = PhoneEntry.Text?.Trim()
            };

            _users.Add(user);
            SaveUsers();
            ClearEntries();
            LoadUsers();
        }

        private void SaveUsers()
        {
            try
            {
                var userJson = JsonSerializer.Serialize(_users, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_userJsonPath, userJson);
            }
            catch (Exception ex)
            {
                DisplayAlert("错误", $"保存用户数据失败: {ex.Message}", "确定");
            }
        }

        private void ClearEntries()
        {
            UserIdEntry.Text = string.Empty;
            UserNameEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;
            GenderPicker.SelectedIndex = -1;
            BirthDatePicker.Date = DateTime.Now;
            PhoneEntry.Text = string.Empty;
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var userId = (string)button.CommandParameter;

            var confirm = await DisplayAlert("确认", "确定要删除该用户吗？", "确定", "取消");
            if (confirm)
            {
                var user = _users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    _users.Remove(user);
                    SaveUsers();
                    LoadUsers();
                }
            }
        }
    }
} 