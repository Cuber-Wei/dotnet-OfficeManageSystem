using Microsoft.EntityFrameworkCore;
using OfficeMgtAdmin.Data;
using OfficeMgtAdmin.Models;
using System.Collections.ObjectModel;

namespace OfficeMgtAdmin.Views
{
    public partial class ItemImportPage : ContentPage
    {
        private readonly ApplicationDbContext _context;
        private ObservableCollection<ImportRecord> _importRecords;

        public ItemImportPage(ApplicationDbContext context)
        {
            InitializeComponent();
            _context = context;
            _importRecords = new ObservableCollection<ImportRecord>();
            ImportRecordsCollection.ItemsSource = _importRecords;
            LoadItems();
            LoadImportRecords();
        }

        private async void LoadItems()
        {
            var items = await _context.Items.Where(i => !i.IsDelete).ToListAsync();
            ItemPicker.ItemsSource = items;
        }

        private async void LoadImportRecords()
        {
            var records = await _context.ImportRecords
                .Include(r => r.Item)
                .Where(r => !r.IsDelete)
                .OrderByDescending(r => r.ImportDate)
                .ToListAsync();

            _importRecords.Clear();
            foreach (var record in records)
            {
                _importRecords.Add(record);
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (ItemPicker.SelectedItem == null)
            {
                await DisplayAlert("提示", "请选择物品", "确定");
                return;
            }

            if (!int.TryParse(ImportNumEntry.Text, out int importNum) || importNum <= 0)
            {
                await DisplayAlert("提示", "请输入有效的购买数量", "确定");
                return;
            }

            if (!decimal.TryParse(SinglePriceEntry.Text, out decimal singlePrice) || singlePrice <= 0)
            {
                await DisplayAlert("提示", "请输入有效的单价", "确定");
                return;
            }

            var selectedItem = (Item)ItemPicker.SelectedItem;
            var importRecord = new ImportRecord
            {
                ItemId = selectedItem.Id,
                ImportNum = importNum,
                SinglePrice = singlePrice,
                ImportDate = ImportDatePicker.Date,
                CreateTime = DateTime.Now,
                UpdateTime = DateTime.Now,
                IsDelete = false
            };

            _context.ImportRecords.Add(importRecord);
            
            // Update item quantity
            selectedItem.ItemNum += importNum;
            selectedItem.UpdateTime = DateTime.Now;
            
            await _context.SaveChangesAsync();
            await DisplayAlert("提示", "保存成功", "确定");
            LoadImportRecords();
            ClearEntries();
        }

        private void ClearEntries()
        {
            ItemPicker.SelectedItem = null;
            ImportDatePicker.Date = DateTime.Now;
            ImportNumEntry.Text = string.Empty;
            SinglePriceEntry.Text = string.Empty;
        }
    }
} 