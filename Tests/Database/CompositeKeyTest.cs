namespace Tests.Database
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    [Table("CompositeKeyTest")]
    public partial class CompositeKeyTest
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int FirstKey { get; set; }

        [Key]
        [Column(Order = 1)]
        [StringLength(50)]
        public string SecondKey { get; set; }

        [Required]
        public string Value { get; set; }

        [Column(TypeName = "date")]
        public DateTime LastModified { get; set; }

        [Required]
        [StringLength(255)]
        public string LastModifiedBy { get; set; }
    }
}
