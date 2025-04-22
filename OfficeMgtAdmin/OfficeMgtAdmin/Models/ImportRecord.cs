using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OfficeMgtAdmin.Models
{
    [Table("import_record")]
    public class ImportRecord
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [Column("itemId")]
        public long ItemId { get; set; }

        [Required]
        [Column("importNum")]
        public int ImportNum { get; set; }

        [Required]
        [Column("singlePrice", TypeName = "decimal(10,2)")]
        public decimal SinglePrice { get; set; }

        [Column("importDate")]
        public DateTime ImportDate { get; set; }

        [Column("createTime")]
        public DateTime CreateTime { get; set; }

        [Column("updateTime")]
        public DateTime UpdateTime { get; set; }

        [Column("isDelete")]
        public bool IsDelete { get; set; }

        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }
    }
} 