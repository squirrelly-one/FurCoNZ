using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;

using FurCoNZ.Web.Services;
using FurCoNZ.Web.Models;
using FurCoNZ.Web.Controllers;
using FurCoNZ.Web.Helpers;
using Microsoft.AspNetCore.Routing;

namespace FurCoNZ.Web.Auth
{
    public static class NzFursOpenIdConnectEvents
    {
        public static async Task OnUserInformationReceived(UserInformationReceivedContext context)
        {
            var userService = context.HttpContext.RequestServices.GetService<IUserService>();
            var linkGenerator = context.HttpContext.RequestServices.GetService<LinkGenerator>();
            var identity = (ClaimsIdentity)context.Principal.Identity;
            var user = await userService.GetUserFromIssuerAsync(context.Principal.FindFirst("iss").Value, context.Principal.FindFirst("sub").Value, context.HttpContext.RequestAborted);

            // Create a new user if they do not exist locally fill and in the defaults. 
            if (user == null)
            {
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

                // Give new uers a chance to update their details
                // 1. Save redirect URL to session
                context.HttpContext.Session.Set(AccountController.RedirectAfterDetailsSessionKey, context.Properties.RedirectUri);
                // 2. Show firtst time page asking for name, details etc,
                context.Properties.RedirectUri = linkGenerator.GetUriByAction(context.HttpContext, nameof(AccountController.Index), "Account");
                // 3. if skipped or accepted, redirect to initial redirect page (See AccountController.UpdateAccount)
            }

            identity.AddClaim(new Claim("user", user.Id.ToString()));
            identity.AddClaim(new Claim("admin", user.IsAdmin.ToString()));
        }
    }
}
