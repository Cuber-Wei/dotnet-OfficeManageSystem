using OfficeMgtAdmin.Models;

namespace OfficeMgtAdmin.ViewModels
{
    public class ApplyRecordViewModel
    {
        private readonly ApplyRecord _applyRecord;
        private readonly User? _user;

        public ApplyRecordViewModel(ApplyRecord applyRecord, User? user)
        {
            _applyRecord = applyRecord;
            _user = user;
        }

        public long Id => _applyRecord.Id;
        public Item? Item => _applyRecord.Item;
        public int ApplyNum => _applyRecord.ApplyNum;
        public int ApplyStatus => _applyRecord.ApplyStatus;
        public DateTime ApplyDate => _applyRecord.ApplyDate;
        public string UserName => _user?.UserName ?? "未知";
    }
} 