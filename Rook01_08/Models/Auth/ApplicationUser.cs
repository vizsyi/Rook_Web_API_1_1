using Microsoft.AspNetCore.Identity;
using Rook01_08.Models.Auth.DTOs;
using Rook01_08.Services.SecurityKey;
using System.ComponentModel.DataAnnotations;

namespace Rook01_08.Models.Auth
{
    public class ApplicationUser : IdentityUser<int>
    {
        public ApplicationUser() : base()
        {
        }
        public ApplicationUser(RegisterDTO model) : base(model.UserName)
        {
            this.Email= model.Email;
            this.UserKey = ChKey.NewChKey(64, 8);
        }

        [Required]
        [StringLength(8)]
        public string UserKey { get; set; }

        [Required]
        [Range(0, 2)]
        public byte Sex { get; set; }

    }
}
