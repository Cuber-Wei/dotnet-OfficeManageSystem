using System;
using Microsoft.Maui.Controls;
using OfficeMgtAdmin.Data;

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
            await DisplayAlert("提示", "用户维护功能暂未实现", "确定");
        }

        private async void OnItemInfoClicked(object sender, EventArgs e)
        {
            var context = Application.Current.Handler.MauiContext.Services.GetService<ApplicationDbContext>();
            await Navigation.PushAsync(new ItemInfoPage(context));
        }

        private async void OnImportClicked(object sender, EventArgs e)
        {
            var context = Application.Current.Handler.MauiContext.Services.GetService<ApplicationDbContext>();
            await Navigation.PushAsync(new ItemImportPage(context));
        }

        private async void OnExportClicked(object sender, EventArgs e)
        {
            var context = Application.Current.Handler.MauiContext.Services.GetService<ApplicationDbContext>();
            await Navigation.PushAsync(new ItemExportPage(context));
        }

        private async void OnInventoryQueryClicked(object sender, EventArgs e)
        {
            var context = Application.Current.Handler.MauiContext.Services.GetService<ApplicationDbContext>();
            await Navigation.PushAsync(new InventoryQueryPage(context));
        }
    }
} 