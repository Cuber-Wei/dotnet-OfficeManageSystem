using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OfficeMgtAdmin.Web.Models
{
    [Table("item")]
    public class Item
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Required]
        [StringLength(32)]
        [Column("code")]
        public required string Code { get; set; }

        [Required]
        [StringLength(256)]
        [Column("itemName")]
        public required string ItemName { get; set; }

        [Required]
        [Column("itemType")]
        public int ItemType { get; set; }

        [StringLength(64)]
        [Column("origin")]
        public string? Origin { get; set; }

        [StringLength(64)]
        [Column("itemSize")]
        public string? ItemSize { get; set; }

        [StringLength(64)]
        [Column("itemVersion")]
        public string? ItemVersion { get; set; }

        [StringLength(1024)]
        [Column("itemPic")]
        public string? ItemPic { get; set; }

        [Required]
        [Column("itemNum")]
        public int ItemNum { get; set; }

        [Column("createTime")]
        public DateTime CreateTime { get; set; }

        [Column("updateTime")]
        public DateTime UpdateTime { get; set; }

        [Required]
        [Column("isDelete")]
        public bool IsDelete { get; set; }
    }
} 