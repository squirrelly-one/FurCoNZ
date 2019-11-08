using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;

namespace FurCoNZ.Web
{
    /// <summary>
    /// I needed a null router to pass onto the ActionContext for Email templates
    /// Copied from https://github.com/aspnet/AspNetCore/blob/v2.2.7/src/Http/Routing/src/Internal/NullRouter.cs
    /// </summary>
    internal class NullRouter : IRouter
    {
        public static readonly NullRouter Instance = new NullRouter();

        private NullRouter()
        {
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return null;
        }

        public Task RouteAsync(RouteContext context)
        {
            return Task.CompletedTask;
        }
    }
}
