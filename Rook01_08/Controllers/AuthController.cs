using Dapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Rook01_08.Data.Dapper;
using Rook01_08.Data.EF;
using Rook01_08.Filters.Authentication;
using Rook01_08.Models.Auth;
using Rook01_08.Models.Auth.Constans;
using Rook01_08.Models.Auth.DTOs;
using Rook01_08.Models.Auth.Tokens;
using Rook01_08.Models.Auth.Tokens.DTOs;
using Rook01_08.Services.EMail.Views;
using Rook01_08.Services.SecurityKey;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Rook01_08.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDBContext _context;
        private readonly DapperDBContext _dapper;
        private readonly IConfiguration _configuration;
        //private readonly TokenValidationParameters tokenValidationParameters;


        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager
            , ApplicationDBContext context, DapperDBContext dapper, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _dapper = dapper;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please provide all required fields"); //todo: Sending back the model errors
            }

            var mailUser = await _userManager.FindByEmailAsync(model.Email);
            var nameUser = await _userManager.FindByNameAsync(model.UserName);

            if (mailUser != null || nameUser != null)
            {
                var errorMess = (mailUser != null) ? ((nameUser != null)
                        ? $"Both User {model.UserName} and E-mail {model.Email} already exist!"
                        : $"E-mail {model.Email} already exists!")
                    : $"User {model.UserName} already exists!";

                return BadRequest(errorMess);
            }

            ApplicationUser newUser = new ApplicationUser(model);

            var result = await _userManager.CreateAsync(newUser, model.Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("Signup",
                    String.Join(", ", result.Errors.Select(e => e.Description)));

                return BadRequest("User could not be created");
            }

            newUser = await _userManager.FindByEmailAsync(model.Email);
            if (newUser is null)
            {
                return BadRequest("Confirmation process failed");
            }

            var roleResult = await _userManager.AddToRoleAsync(newUser, UserRoles.CelebEditor);//todo: only for test

            var confToken = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
            var confLink = Url.ActionLink("ConfirmEmail", "Auth"
                , new { userId = newUser.Id, token = confToken });//todo: to the Page
            await AuthEMailView.SendConfirmationAsync(newUser.Email, newUser.UserName, confLink);

            return Created(nameof(Register), $"User {model.UserName} created");
        }

        [HttpGet("Confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            //var user1 = await _context.Users
            //    .FirstOrDefaultAsync<ApplicationUser>(u => u.UserKey == uid);
            //var user = await _userManager.FindByIdAsync(user1.Id.ToString());
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return Ok("You have confirmed it.");
                }
                var t = 34;
            }

            return new NotFoundResult();
        }

        private async Task<RefreshToken> GenerateRefreshTokenAsync(string userKey, int userId)
        {
            var refreshToken = new RefreshToken()
            {
                UserKey = userKey,
                UserId = userId,
                SecKey = ChKey.NewChKey(64, 8),
                LongToken = ChKey.NewChKey(64, 7) + "<" + ChKey.NewChKey(64, 8),
                DateExpire = DateTime.UtcNow.AddMinutes(79),
                IsRevoked = false
            };

            string sqlComm = "EXEC Auth.SP_RefreshT_Ups @userKey=@userKeyP, @userId=@userIdP"
                + ",@secKey=@secKeyP, @longToken=@longTokenP, @dateExpire=@dateExpireP";
            DynamicParameters dparams = new();
            dparams.Add("@userKeyP", refreshToken.UserKey, DbType.String);
            dparams.Add("@userIdP", refreshToken.UserId, DbType.Int32);
            dparams.Add("@secKeyP", refreshToken.SecKey, DbType.String);
            dparams.Add("@longTokenP", refreshToken.LongToken, DbType.String);
            dparams.Add("@dateExpireP", refreshToken.DateExpire, DbType.DateTime);

            await _dapper.ExecuteWithParametersAsync(sqlComm, dparams);//todo: handle the result
            //dapper.ExecuteWithParameters("Auth.SP_RefreshT_Ups", dparams);

            //await context.RefreshToken.AddAsync(refreshToken);
            //await context.SaveChangesAsync();//todo: with storedprocedure

            return refreshToken;
        }

        private async Task<AuthResult> GenerateJwtTokenAsync(RefreshToken refreshToken, bool isNewRefreshToken = false)
        {
            var authClaims = new List<Claim>()
            {
                new Claim(JwtRegisteredClaimNames.NameId , refreshToken.UserKey),
                new Claim(JwtRegisteredClaimNames.Nonce, refreshToken.SecKey)
             };

            //Add user roles
            string sqlComm = "EXEC Auth.SP_RolesGetByUserId @userId = @userIdP";
            DynamicParameters dparams = new();
            dparams.Add("@userIdP", refreshToken.UserId, DbType.Int32);

            var userRoles = await _dapper.LoadDataWithParametersAsync<string>(sqlComm, dparams);
            //var userRoles = await userManager.GetRolesAsync(user);
            foreach(var role in userRoles)
            {
                authClaims.Add(new Claim("role", role));
            }

            var SigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var token = new JwtSecurityToken(
                issuer : _configuration["JWT:Issuer"],
                audience : _configuration["JWT:Audience"],
                claims: authClaims,
                expires: DateTime.UtcNow.AddSeconds(457),
                signingCredentials: new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256)
                );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            return new AuthResult()
            {
                Token = jwtToken,
                RefreshToken = isNewRefreshToken ? refreshToken.LongToken : "",
                DateExpire = token.ValidTo
            };
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please provide all required fields");
            }

            //todo: e-mail or username
            var user = await _userManager.FindByEmailAsync(model.UserName);

            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                return Ok(await GenerateJwtTokenAsync(await GenerateRefreshTokenAsync(user.UserKey, user.Id), true));
            }

            return Unauthorized();
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [CheckTokenFilterFactory]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenRequest model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Please provide all the required fields");
            }

            var refreshToken = HttpContext.Items["RefreshToken"] as RefreshToken;

            if (refreshToken != null && model.RefreshToken == refreshToken.LongToken)
            {

                if (refreshToken.DateExpire < DateTime.UtcNow.AddMinutes(24))
                {
                    return Ok(await GenerateJwtTokenAsync(
                        await GenerateRefreshTokenAsync(refreshToken.UserKey, refreshToken.UserId), true));
                }
                return Ok(await GenerateJwtTokenAsync(refreshToken));
            }

            return Unauthorized();
        }

        private DateTime UnixTimeStampToDateInUTC(long unixTimeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTimeStamp);
        }

    }
}
