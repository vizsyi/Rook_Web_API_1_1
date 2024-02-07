using System.ComponentModel.DataAnnotations;

namespace Rook01_08.Models.Celeb
{
    public class Profession
    {
        [Key]
        public ushort Id { get; set; }

        [Required]
        [StringLength(maximumLength:40, ErrorMessage = "Profession cannot be more than 40 character long")]
        public string ProfessionName { get; set; } = "";

        public bool Sport { get; set; } = true;
    }
}
