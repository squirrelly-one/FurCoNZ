using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

using FurCoNZ.Services;
using FurCoNZ.Models;

namespace FurCoNZ.Auth
{
    public static class NzFursOpenIdConnectEvents
    {
        public static async Task OnUserInformationReceived(UserInformationReceivedContext context)
        {
            var userService = context.HttpContext.RequestServices.GetService<IUserService>();
            var identity = (ClaimsIdentity)context.Principal.Identity;
            var user = await userService.GetUserFromIssuerAsync(context.Principal.FindFirst("iss").Value, context.Principal.FindFirst("sub").Value, context.HttpContext.RequestAborted);

            // User already exists,m nothing to do?
            if (user != null)
            {
                identity.AddClaim(new Claim("user", user.Id.ToString()));
                identity.AddClaim(new Claim("admin", user.IsAdmin.ToString()));
                return;
            }

            // Create a new user, fill it in with defaults. 
            user = new User
            {
                Name = context.User.Value<string>("name"),
                Email = context.User.Value<string>("email"),
            };
            user.LinkedAccounts = new List<LinkedAccount>
            {
                new LinkedAccount
                {
                    Issuer = context.Principal.FindFirst("iss").Value,
                    Subject = context.Principal.FindFirst("sub").Value,
                    User = user,
                }
            };

            await userService.CreateUserAsync(user);

            identity.AddClaim(new Claim("user", user.Id.ToString()));
            //context.Result = HandleRequestResult.Success(new AuthenticationTicket())
        }
    }
}
