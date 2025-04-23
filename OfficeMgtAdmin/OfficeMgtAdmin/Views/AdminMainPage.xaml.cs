using System;
using Microsoft.Maui.Controls;
using OfficeMgtAdmin.Data;
using Microsoft.Extensions.DependencyInjection;

namespace OfficeMgtAdmin.Views
{
    public partial class AdminMainPage : ContentPage
    {
        public AdminMainPage()
        {
            InitializeComponent();
        }

        private async void OnUserMaintenanceClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UserMaintenancePage());
        }

        private async void OnItemInfoClicked(object sender, EventArgs e)
        {
            var services = Application.Current?.Handler?.MauiContext?.Services;
            if (services == null)
            {
                await DisplayAlert("错误", "无法获取服务", "确定");
                return;
            }

            var context = services.GetRequiredService<ApplicationDbContext>();
            await Navigation.PushAsync(new ItemInfoPage(context));
        }

        private async void OnImportClicked(object sender, EventArgs e)
        {
            var services = Application.Current?.Handler?.MauiContext?.Services;
            if (services == null)
            {
                await DisplayAlert("错误", "无法获取服务", "确定");
                return;
            }

            var context = services.GetRequiredService<ApplicationDbContext>();
            await Navigation.PushAsync(new ItemImportPage(context));
        }

        private async void OnExportClicked(object sender, EventArgs e)
        {
            var services = Application.Current?.Handler?.MauiContext?.Services;
            if (services == null)
            {
                await DisplayAlert("错误", "无法获取服务", "确定");
                return;
            }

            var context = services.GetRequiredService<ApplicationDbContext>();
            await Navigation.PushAsync(new ItemExportPage(context));
        }

        private async void OnInventoryQueryClicked(object sender, EventArgs e)
        {
            var services = Application.Current?.Handler?.MauiContext?.Services;
            if (services == null)
            {
                await DisplayAlert("错误", "无法获取服务", "确定");
                return;
            }

            var context = services.GetRequiredService<ApplicationDbContext>();
            await Navigation.PushAsync(new InventoryQueryPage(context));
        }
    }
} 