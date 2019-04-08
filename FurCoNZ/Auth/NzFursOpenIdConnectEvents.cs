using System;
using System.Collections.Generic;
using System.Linq;
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
            var user = await userService.GetUserAsync(context.User.Value<string>("sub"), context.HttpContext.RequestAborted);

            // User already exists,m nothing to do?
            if (user != null)
                return;

            // Create a new user, fill it in with defaults. 
            await userService.CreateUserAsync(new User
            {
                Id = context.User.Value<string>("sub"),
                Name = context.User.Value<string>("name"),
            });
            
            return;
        }
    }
}
