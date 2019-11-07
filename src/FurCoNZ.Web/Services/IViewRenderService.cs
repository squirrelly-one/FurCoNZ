using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FurCoNZ.Web.Services
{
    public interface IViewRenderService
    {
        Task<(string Output, ViewDataDictionary ViewData)> RenderToStringAsync<TModel>(Controller controller, TModel model, bool partial = false);
        Task<(string Output, ViewDataDictionary ViewData)> RenderToStringAsync<TModel>(Controller controller, string viewName, TModel model, bool partial = false);
        Task<(string Output, ViewDataDictionary ViewData)> RenderToStringAsync<TModel>(string viewName, TModel model, bool partial = false, CancellationToken cancellationToken = default);
    }
}
