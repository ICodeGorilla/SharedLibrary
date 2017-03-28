using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tests.Database.EFCore
{
    public partial class Account
    {
        public Account()
        {
            Contact = new HashSet<Contact>();
        }

        [Column("AccountID")]
        public int AccountId { get; set; }
        [Required]
        [Column(TypeName = "varchar(255)")]
        public string CompanyName { get; set; }
        [Required]
        [Column(TypeName = "varchar(255)")]
        public string Address { get; set; }
        [Required]
        [Column(TypeName = "varchar(255)")]
        public string LastModifiedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime LastModified { get; set; }
        [Required]
        [Column(TypeName = "varchar(255)")]
        public string AccountReference { get; set; }

        [InverseProperty("Account")]
        public virtual ICollection<Contact> Contact { get; set; }
    }
}
