using System.ComponentModel.DataAnnotations;

namespace Rook01_08.Models.Celeb
{
    public class Celeb
    {
        [Key]
        [StringLength(6)]
        public string Id { get; set; } = "";

        [Required]
        [StringLength(30)]
        public string CelebName { get; set; } = "";

        [Required]
        public byte Sex { get; set; }

        public ushort? ProfessionId { get; set; }

        public Profession? Profession { get; set; }

        [Required]
        [StringLength(maximumLength:11, ErrorMessage = "Filename cannot be more than 11 character long")]
        public string PhotoFile { get; set; } = "";

        [StringLength(6)]
        public string? PairId { get; set; }

        public Celeb? Pair { get; set; }

        [Required]
        public bool Ready { get; set; }
    }
}
