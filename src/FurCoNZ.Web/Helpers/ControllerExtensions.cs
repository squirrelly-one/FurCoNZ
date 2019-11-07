using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FurCoNZ.Web.Services;

namespace FurCoNZ.Web.Helpers
{
    public static class ControllerExtensions
    {
        // https://stackoverflow.com/a/50024209
        // Usage: `viewHtml = await this.RenderViewAsync("Report", model);`
        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
        {
            var viewRenderService = controller.HttpContext.RequestServices.GetService(typeof(IViewRenderService)) as IViewRenderService;

            var result = await viewRenderService.RenderToStringAsync(controller, viewName, model, partial);
            return result.Output;
        }

        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, TModel model, bool partial = false)
        {
            var viewRenderService = controller.HttpContext.RequestServices.GetService(typeof(IViewRenderService)) as IViewRenderService;

            var result = await viewRenderService.RenderToStringAsync(controller, model, partial);
            return result.Output;
        }
    }
}
