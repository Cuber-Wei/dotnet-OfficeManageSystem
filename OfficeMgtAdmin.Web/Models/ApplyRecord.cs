using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OfficeMgtAdmin.Web.Models
{
    [Table("apply_record")]
    public class ApplyRecord
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("userId")]
        public long UserId { get; set; }

        [Required]
        [Column("itemId")]
        public long ItemId { get; set; }

        [Required]
        [Column("applyNum")]
        public int ApplyNum { get; set; }

        [Required]
        [Column("applyStatus")]
        public int ApplyStatus { get; set; }

        [Column("applyDate")]
        public DateTime ApplyDate { get; set; }

        [Column("createTime")]
        public DateTime CreateTime { get; set; }

        [Column("updateTime")]
        public DateTime UpdateTime { get; set; }

        [Column("isDelete")]
        public bool IsDelete { get; set; }

        [ForeignKey("ItemId")]
        public virtual Item? Item { get; set; }
    }
} 