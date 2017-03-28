using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tests.Database.EFCore
{
    public partial class Contact
    {
        [Column("ContactID")]
        public int ContactId { get; set; }
        [Required]
        [Column(TypeName = "varchar(255)")]
        public string Name { get; set; }
        [Required]
        [Column(TypeName = "varchar(255)")]
        public string EmailAddress { get; set; }
        [Column(TypeName = "date")]
        public DateTime LastModified { get; set; }
        [Required]
        [Column(TypeName = "varchar(255)")]
        public string LastModifiedBy { get; set; }
        [Column("AccountID")]
        public int AccountId { get; set; }

        [ForeignKey("AccountId")]
        [InverseProperty("Contact")]
        public virtual Account Account { get; set; }
    }
}
