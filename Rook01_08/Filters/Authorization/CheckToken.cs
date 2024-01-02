using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Rook01_08.Data.Dapper;
using Rook01_08.Models.Auth.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Rook01_08.Filters.Authentication
{
    public class CheckTokenFilterFactory : Attribute, IFilterFactory
    {
        public bool IsReusable => false;
        //private bool allowAnonymous { get; }

        //public CheckTokenFilterFactory(bool AllowAnonymous = false)
        //{
        //    this.allowAnonymous = AllowAnonymous;
        //}

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetService<ILoggerFactory>();
            var dapper = serviceProvider.GetService<DapperDBContext>();
            return new CheckTokenFilter(logger, dapper);

        }
    }

    public class CheckTokenFilter : IAsyncAuthorizationFilter
    {
        private readonly ILoggerFactory? _logger;
        private readonly DapperDBContext _dapper;

        public CheckTokenFilter(ILoggerFactory? logger, DapperDBContext dapper)
        {
            this._logger = logger;
            this._dapper = dapper;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {

            if(context.HttpContext.Request.Headers.Any(h => h.Key.Equals("Authorization")))
            {
                // It has bearer token
                var jwtNid = JwtRegisteredClaimNames.NameId;
                var jwtNid2 = ClaimTypes.NameIdentifier;
                var jwtNonce = JwtRegisteredClaimNames.Nonce;
                //var jwtExp = JwtRegisteredClaimNames.Exp;
                var keyClaim = context.HttpContext.User.Claims
                    .FirstOrDefault(c => (c.Type == jwtNid2 || c.Type == jwtNid));
                var secClaim = context.HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == jwtNonce);

                if (keyClaim != null && secClaim != null)
                {
                    var userKey = keyClaim.Value;

                    string sqlComm = "EXEC Auth.SP_RefreshToken @userKey = @userKeyP";
                    DynamicParameters dparams = new();
                    dparams.Add("@userKeyP", userKey, DbType.String);
                    var refreshToken = await _dapper.LoadDataSingleWithParametersAsync<RefreshToken>(sqlComm, dparams);

                    if (secClaim.Value == refreshToken.SecKey)
                    {
                        context.HttpContext.Items["RefreshToken"] = refreshToken;
                    }
                    else
                    {
                        context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                    }
                }
                else
                {
                    context.Result = new StatusCodeResult(StatusCodes.Status401Unauthorized);
                }
            }
            else
            {
                // It has not got token
                context.HttpContext.Items["RefreshToken"] = null;

                //var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                //if (controllerActionDescriptor != null)
                //{
                //    var fAuth = controllerActionDescriptor.MethodInfo.GetCustomAttributes(inherit: true)
                //        .Any(a => a.GetType().Equals(typeof(AuthorizeAttribute)));

                //    var finedC = controllerActionDescriptor.ControllerTypeInfo
                //        ?.GetCustomAttributes(typeof(AllowAnonymousAttribute), true)?.Any() ?? false;
                //}

            }

            return;
        }
    }
}
