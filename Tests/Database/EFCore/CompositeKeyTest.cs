using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tests.Database.EFCore
{
    public partial class CompositeKeyTest
    {
        public int FirstKey { get; set; }
        [MaxLength(50)]
        public string SecondKey { get; set; }
        [Required]
        [Column(TypeName = "varchar(max)")]
        public string Value { get; set; }
        [Column(TypeName = "date")]
        public DateTime LastModified { get; set; }
        [Required]
        [Column(TypeName = "varchar(255)")]
        public string LastModifiedBy { get; set; }
    }
}
