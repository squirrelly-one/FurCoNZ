using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FurCoNZ.Web.Services
{
    public interface IViewRenderService
    {
        Task<string> RenderToStringAsync<TModel>(Controller controller, TModel model, bool partial = false);
        Task<string> RenderToStringAsync<TModel>(Controller controller, string viewName, TModel model, bool partial = false);
        Task<string> RenderToStringAsync<TModel>(string viewName, TModel model, bool partial = false, CancellationToken cancellationToken = default);
    }
}
