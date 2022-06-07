using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ketnoicungcau.business.Caching;
using ketnoicungcau.business.Framework;
using ketnoicungcau.vn.ViewModels;

namespace ketnoicungcau.vn.ViewComponents
{
    public class MasterPageViewComponent : ViewComponent
    {
        private readonly ILogger<MasterPageViewComponent> _logger;
        private readonly IWorkContext _workContext;
        private readonly ICache _cache;

        public MasterPageViewComponent(
            ILogger<MasterPageViewComponent> logger,
            IWorkContext workContext,
            ICache cache)
        {
            _logger = logger;
            _workContext = workContext;
            _cache = cache;
        }

        public async Task<IViewComponentResult> InvokeAsync(ReqMasterPage request)
        {
            bool isMobile = request.isMobile ? request.isMobile : _workContext.IsMobile;
            string _viewName = $"{("Index")}{(isMobile ? ".M" : "")}";
            if (string.IsNullOrEmpty(request.viewName) == false)
                _viewName = $"{request.viewName}{(isMobile ? ".M" : "")}";
            string keycache = _cache.CreateKey(_viewName, request.isHeader, request.isMobile);
            var model = _cache.Get<vmMasterPage>(keycache);
            if (model != null)
                return View(_viewName, model);
            var time = Constants.TimeCache.dateHour;
            if (request.isHeader)
            {
                _cache.Set(keycache, model, time);
            }
            return View(_viewName, model);
        }

        public class ReqMasterPage
        {
            public string viewName = "Index";
            public bool isHeader = false;
            public bool isMobile = false;
            public int ProvinceId = 0;
        }
    }
}
