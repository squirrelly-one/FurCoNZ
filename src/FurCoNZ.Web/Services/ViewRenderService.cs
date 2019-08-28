using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace FurCoNZ.Web.Services
{
    // Based off the following:
    // https://ppolyzos.com/2016/09/09/asp-net-core-render-view-to-string/
    // https://stackoverflow.com/a/50024209

    public class ViewRenderService : IViewRenderService
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public ViewRenderService(
            IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _razorViewEngine = razorViewEngine;
            _tempDataProvider = tempDataProvider;
            _serviceProvider = serviceProvider;
        }

        public async Task<string> RenderToStringAsync<TModel>(Controller controller, TModel model, bool partial = false)
        {
            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }
            return await RenderToStringAsync(controller, controller.ControllerContext.ActionDescriptor.ActionName, model, partial);
        }

        public async Task<string> RenderToStringAsync<TModel>(Controller controller, string viewName, TModel model, bool partial = false)
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

        public async Task<string> RenderToStringAsync<TModel>(string viewName, TModel model, bool partial = false, CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrWhiteSpace(viewName))
            {
                throw new ArgumentNullException(nameof(viewName));
            }

            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider, RequestAborted = cancellationToken };

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
            var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = model
            };
            var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);

            return await RenderToStringAsync(actionContext, viewName, viewData, tempData, partial);
        }

        private async Task<string> RenderToStringAsync(ActionContext actionContext, string viewName, ViewDataDictionary viewData, ITempDataDictionary tempData, bool partial = false)
        {
            var viewResult = _razorViewEngine.FindView(actionContext, viewName, !partial);

            if (viewResult.View == null)
            {
                throw new ArgumentNullException($"{viewName} does not match any available view");
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
                return stringWriter.ToString();
            }
        }
    }
}