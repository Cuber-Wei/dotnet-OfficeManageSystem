using OfficeMgtAdmin.Models;
using Microsoft.EntityFrameworkCore;

namespace OfficeMgtAdmin.ViewModels
{
    public class ApplyRecordViewModel
    {
        public long Id { get; }
        public long ItemId { get; }
        public long UserId { get; }
        public int ApplyNum { get; }
        public int ApplyStatus { get; }
        public DateTime ApplyDate { get; }
        public DateTime CreateTime { get; }
        public DateTime UpdateTime { get; }
        public bool IsDelete { get; }
        public Item? Item { get; }
        public string UserName { get; }
        public decimal SinglePrice { get; }
        public decimal TotalPrice { get; }

        public ApplyRecordViewModel(ApplyRecord record, User? user, decimal singlePrice)
        {
            Id = record.Id;
            ItemId = record.ItemId;
            UserId = record.UserId;
            ApplyNum = record.ApplyNum;
            ApplyStatus = record.ApplyStatus;
            ApplyDate = record.ApplyDate;
            CreateTime = record.CreateTime;
            UpdateTime = record.UpdateTime;
            IsDelete = record.IsDelete;
            Item = record.Item;
            UserName = user?.UserName ?? "未知用户";
            SinglePrice = singlePrice;
            TotalPrice = record.ApplyNum * singlePrice;
        }
    }
} 