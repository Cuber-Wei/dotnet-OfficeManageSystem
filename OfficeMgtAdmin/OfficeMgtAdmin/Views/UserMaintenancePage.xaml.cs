using System.Collections.ObjectModel;
using System.Text.Json;
using OfficeMgtAdmin.Models;

namespace OfficeMgtAdmin.Views
{
    public partial class UserMaintenancePage : ContentPage
    {
        private readonly string _userJsonPath;
        private List<User> _users;
        private User? _editingUser;
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _isAdminAuthenticated = false;

        public UserMaintenancePage()
        {
            InitializeComponent();
            _userJsonPath = Path.Combine("F:", "code_repository", "dotNetProjects", "OfficeMgtAdmin", "user.json");
            _users = new List<User>();
            BirthDatePicker.Date = DateTime.Now;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            // 初始化时隐藏主要功能界面，显示登录界面
            MainGrid.IsVisible = false;
            LoginGrid.IsVisible = true;

            // 确保存在管理员账户
            EnsureAdminUser();
        }

        private void EnsureAdminUser()
        {
            try
            {
                LoadUsers();
                if (!_users.Any(u => u.UserId == "admin"))
                {
                    var adminUser = new User
                    {
                        Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1,
                        UserId = "admin",
                        UserName = "admin",
                        Password = "0000", // 建议使用更强的密码并加密存储
                        BirthDate = DateTime.Now
                    };
                    _users.Add(adminUser);
                    SaveUsers();
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("错误", $"初始化管理员账户失败: {ex.Message}", "确定");
            }
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            var userId = LoginUserIdEntry.Text?.Trim();
            var password = LoginPasswordEntry.Text?.Trim();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                await DisplayAlert("提示", "请输入用户ID和密码", "确定");
                return;
            }

            var user = _users.FirstOrDefault(u => u.UserId == userId && u.Password == password);
            if (user != null && user.UserId == "admin")
            {
                _isAdminAuthenticated = true;
                MainGrid.IsVisible = true;
                LoginGrid.IsVisible = false;
                LoadUsers(); // 重新加载用户列表
            }
            else
            {
                await DisplayAlert("错误", "用户ID或密码错误，或没有管理员权限", "确定");
                LoginPasswordEntry.Text = string.Empty;
            }
        }

        private void LoadUsers()
        {
            try
            {
                if (File.Exists(_userJsonPath))
                {
                    var userJson = File.ReadAllText(_userJsonPath);
                    _users = JsonSerializer.Deserialize<List<User>>(userJson, _jsonOptions) ?? new List<User>();
                    if (_isAdminAuthenticated)
                    {
                        // 过滤掉管理员用户，只显示普通用户
                        var displayUsers = _users.Where(u => u.UserId != "admin").ToList();
                        UsersCollection.ItemsSource = displayUsers;
                    }
                }
            }
            catch (Exception ex)
            {
                DisplayAlert("错误", $"加载用户数据失败: {ex.Message}", "确定");
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (!_isAdminAuthenticated)
            {
                await DisplayAlert("错误", "没有管理员权限", "确定");
                return;
            }

            if (string.IsNullOrWhiteSpace(UserIdEntry.Text) || string.IsNullOrWhiteSpace(UserNameEntry.Text))
            {
                await DisplayAlert("提示", "用户ID和姓名不能为空", "确定");
                return;
            }

            // 防止创建新的管理员账户
            if (UserIdEntry.Text.Trim().ToLower() == "admin" && _editingUser?.UserId != "admin")
            {
                await DisplayAlert("提示", "不能创建管理员账户", "确定");
                return;
            }

            // 检查用户ID是否已存在（编辑模式下排除当前用户）
            if (_editingUser == null && _users.Any(u => u.UserId == UserIdEntry.Text))
            {
                await DisplayAlert("提示", "用户ID已存在", "确定");
                return;
            }

            try
            {
                if (_editingUser == null)
                {
                    // 创建新用户
                    var newUser = new User
                    {
                        Id = _users.Count > 0 ? _users.Max(u => u.Id) + 1 : 1,
                        UserId = UserIdEntry.Text.Trim(),
                        UserName = UserNameEntry.Text.Trim(),
                        Password = string.IsNullOrWhiteSpace(PasswordEntry.Text) ? "123456" : PasswordEntry.Text.Trim(),
                        Gender = GenderPicker.SelectedItem?.ToString(),
                        BirthDate = BirthDatePicker.Date,
                        Phone = PhoneEntry.Text?.Trim()
                    };
                    _users.Add(newUser);
                }
                else
                {
                    // 更新现有用户
                    _editingUser.UserId = UserIdEntry.Text.Trim();
                    _editingUser.UserName = UserNameEntry.Text.Trim();
                    if (!string.IsNullOrWhiteSpace(PasswordEntry.Text))
                    {
                        _editingUser.Password = PasswordEntry.Text.Trim();
                    }
                    _editingUser.Gender = GenderPicker.SelectedItem?.ToString();
                    _editingUser.BirthDate = BirthDatePicker.Date;
                    _editingUser.Phone = PhoneEntry.Text?.Trim();
                }

                SaveUsers();
                ClearEntries();
                LoadUsers();

                await DisplayAlert("提示", _editingUser == null ? "添加成功" : "修改成功", "确定");
                _editingUser = null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"保存失败: {ex.Message}", "确定");
            }
        }

        private void SaveUsers()
        {
            try
            {
                var userJson = JsonSerializer.Serialize(_users, _jsonOptions);
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
            _editingUser = null;
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (!_isAdminAuthenticated)
            {
                await DisplayAlert("错误", "没有管理员权限", "确定");
                return;
            }

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

        private async void OnViewDetailsClicked(object sender, EventArgs e)
        {
            if (!_isAdminAuthenticated)
            {
                await DisplayAlert("错误", "没有管理员权限", "确定");
                return;
            }

            var button = (Button)sender;
            var userId = (string)button.CommandParameter;
            var user = _users.FirstOrDefault(u => u.UserId == userId);

            if (user != null)
            {
                var message = $"用户ID: {user.UserId}\n" +
                            $"姓名: {user.UserName}\n" +
                            $"性别: {user.Gender ?? "未设置"}\n" +
                            $"出生日期: {user.BirthDate:yyyy-MM-dd}\n" +
                            $"联系电话: {user.Phone ?? "未设置"}";

                await DisplayAlert($"用户详情 - {user.UserName}", message, "确定");
            }
        }

        private async void OnEditClicked(object sender, EventArgs e)
        {
            if (!_isAdminAuthenticated)
            {
                await DisplayAlert("错误", "没有管理员权限", "确定");
                return;
            }

            var button = (Button)sender;
            var userId = (string)button.CommandParameter;
            var user = _users.FirstOrDefault(u => u.UserId == userId);

            if (user != null)
            {
                _editingUser = user;
                UserIdEntry.Text = user.UserId;
                UserNameEntry.Text = user.UserName;
                PasswordEntry.Text = string.Empty; // 不显示密码
                GenderPicker.SelectedItem = user.Gender;
                BirthDatePicker.Date = user.BirthDate;
                PhoneEntry.Text = user.Phone;

                await ScrollToTop();
            }
        }

        private async Task ScrollToTop()
        {
            // 如果页面在 ScrollView 中，可以滚动到顶部
            if (this.Parent is ScrollView scrollView)
            {
                await scrollView.ScrollToAsync(0, 0, true);
            }
        }

        private void OnLogoutClicked(object sender, EventArgs e)
        {
            _isAdminAuthenticated = false;
            MainGrid.IsVisible = false;
            LoginGrid.IsVisible = true;
            LoginPasswordEntry.Text = string.Empty;
            ClearEntries();
        }
    }
} 