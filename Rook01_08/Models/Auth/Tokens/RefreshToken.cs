using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Rook01_08.Models.Auth.Tokens
{
    public class RefreshToken
    {
        [Key]
        [StringLength(8)]
        public string UserKey { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(8)]
        public string SecKey { get; set; } = string.Empty;

        [Required]
        [StringLength(16)]
        public string LongToken { get; set; } = string.Empty;

        [Required]
        public DateTime DateExpire { get; set; }

        public bool IsRevoked { get; set; } = false;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; }

    }
}
