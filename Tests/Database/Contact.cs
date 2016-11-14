namespace Tests.Database
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("Contact")]
    public partial class Contact
    {
        public int ContactID { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [Required]
        [StringLength(255)]
        public string EmailAddress { get; set; }

        [Column(TypeName = "date")]
        public DateTime LastModified { get; set; }

        [Required]
        [StringLength(255)]
        public string LastModifiedBy { get; set; }

        public int AccountID { get; set; }

        public virtual Account Account { get; set; }
    }
}
