using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace FurCoNZ.Web.Services
{
    // Based off the following:
    // https://ppolyzos.com/2016/09/09/asp-net-core-render-view-to-string/
    // https://stackoverflow.com/a/50024209
    // https://stackoverflow.com/a/53619710

    public class ViewRenderService : IViewRenderService
    {
        private readonly HttpContext _httpContext;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly HttpRequestFeature _defaultRequestFeature;
        private readonly CultureInfo _cultureInfo;

        public ViewRenderService(
            IHttpContextAccessor httpContextAccessor,
            IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            _httpContext = httpContextAccessor.HttpContext;
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
            _configuration = configuration;

            var emailBaseUrl = _configuration.GetSection("Email:BaseUrl").Get<string>();
            var url = new Uri(emailBaseUrl);

            _cultureInfo = CultureInfo.GetCultureInfo("en-NZ");

            _defaultRequestFeature = new HttpRequestFeature
            {
                Headers =
                {
                    {HeaderNames.Host, url.Authority },
                    {HeaderNames.AcceptLanguage, "en-NZ" },
                },
                Scheme = url.Scheme,
                QueryString = url.Query,
                Path = url.AbsolutePath,
            };
        }

        public async Task<(string Output, ViewDataDictionary ViewData)> RenderToStringAsync<TModel>(Controller controller, TModel model, bool partial = false)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }
            return await RenderToStringAsync(controller, controller.ControllerContext.ActionDescriptor.ActionName, model, partial);
        }

        public async Task<(string Output, ViewDataDictionary ViewData)> RenderToStringAsync<TModel>(Controller controller, string viewName, TModel model, bool partial = false)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }
            if (String.IsNullOrWhiteSpace(viewName))
            {
                return await RenderToStringAsync(controller, model, partial);
            }

            controller.ViewData.Model = model;

            return await RenderToStringAsync(controller.ControllerContext, viewName, controller.ViewData, controller.TempData, partial);
        }

        public async Task<(string Output, ViewDataDictionary ViewData)> RenderToStringAsync<TModel>(string viewName, TModel model, bool partial = false, CancellationToken cancellationToken = default)
        {
            using (var serviceScope = _serviceProvider.CreateScope())
            {
                if (String.IsNullOrWhiteSpace(viewName))
                {
                    throw new ArgumentNullException(nameof(viewName));
                }

                ActionContext actionContext;
                if (_httpContext != null)
                {
                    actionContext = new ActionContext(_httpContext, _httpContext.GetRouteData(), new ActionDescriptor());
                }
                else
                {
                    CultureInfo.DefaultThreadCurrentCulture = _cultureInfo;
                    CultureInfo.DefaultThreadCurrentUICulture = _cultureInfo;

                    var features = new FeatureCollection();
                    features.Set<IHttpRequestFeature>(_defaultRequestFeature);

                    var defaultHttpContext = new DefaultHttpContext (features){ RequestServices = serviceScope.ServiceProvider, RequestAborted = cancellationToken };
                    actionContext = new ActionContext(defaultHttpContext, new RouteData { Routers = { NullRouter.Instance } }, new ActionDescriptor());
                }

                var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                };
                var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

                return await RenderToStringAsync(actionContext, viewName, viewData, tempData, partial);
            }
        }

        private async Task<(string Output, ViewDataDictionary ViewData)> RenderToStringAsync(ActionContext actionContext, string viewName, ViewDataDictionary viewData, ITempDataDictionary tempData, bool partial = false)
        {
            var viewResult = _razorViewEngine.FindView(actionContext, viewName, !partial);

            if (viewResult.View == null)
            {
                var searchedLocations = String.Join("\n\t", viewResult.SearchedLocations);
                throw new ArgumentNullException(nameof(viewName), $"{viewName} does not match any available view. Searched locations:\n\t{searchedLocations}");
            }

            using (var stringWriter = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewData,
                    tempData,
                    stringWriter,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return (stringWriter.ToString(), viewData);
            }
        }
    }
}
