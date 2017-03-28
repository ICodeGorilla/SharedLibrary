using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tests.Database.EFCore
{
    [Table("__MigrationHistory")]
    public partial class MigrationHistory
    {
        [MaxLength(150)]
        public string MigrationId { get; set; }
        [MaxLength(300)]
        public string ContextKey { get; set; }
        [Required]
        public byte[] Model { get; set; }
        [Required]
        [MaxLength(32)]
        public string ProductVersion { get; set; }
    }
}
