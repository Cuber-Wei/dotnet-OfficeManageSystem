using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OfficeMgtAdmin.Models
{
    public class ApplyRecord
    {
        [Key]
        public long Id { get; set; }
        
        [Required]
        public long UserId { get; set; }
        
        [Required]
        public long ItemId { get; set; }
        
        [Required]
        public int ApplyNum { get; set; }
        
        [Required]
        public int ApplyStatus { get; set; } // 0-申请/1-确认/2-驳回
        
        public DateTime ApplyDate { get; set; }
        
        public DateTime CreateTime { get; set; }
        
        public DateTime UpdateTime { get; set; }
        
        [Required]
        public bool IsDelete { get; set; }
        
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }
    }
} 