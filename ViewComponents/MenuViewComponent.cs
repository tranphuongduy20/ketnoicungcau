using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ketnoicungcau.business.ServiceWeb.Interface;
using ketnoicungcau.business.ModelWeb;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ketnoicungcau.business.Helpers;
using ketnoicungcau.business.Framework;

namespace ketnoicungcau.vn.ViewComponents
{
    public class MenuViewComponent : ViewComponent
    {
        private readonly IOptions<AppSettings> _settings;
        private readonly ICategoryServiceWeb _categorySvc;
        private readonly IUtilitiesServiceWeb _utilSvc;
        private readonly IWorkContext _workContext;

        public MenuViewComponent(
            IOptions<AppSettings> settings,
            IUtilitiesServiceWeb utilSvc,
            ICategoryServiceWeb categorySvc,
            IWorkContext workContext)
        {
            _settings = settings;
            _categorySvc = categorySvc;
            _utilSvc = utilSvc;
            _workContext = workContext;
        }
        public async Task<IViewComponentResult> InvokeAsync(ReqMenu request)
        {
            string viewName = $"{(request.ViewName)}{(request.IsMobile ? ".M" : "")}";
            var rootMenus = new List<Menu>();
            if(request.IsNavigation)
                rootMenus = await _utilSvc.GetAllMenu(true, false);
            else rootMenus = await _utilSvc.GetAllMenu(false, _workContext.IsMobile);

            if (request.IsMobile)
            {
                foreach (var item in rootMenus)
                    if (item.ChildMenus?.Count > 0)
                    {
                        item.Url = "javascript:void(0)";
                        item.Class = string.Empty;
                    }
                    else
                    {
                        item.Url = $"/{item.Url}";
                        item.Class = "no-child";
                    }
            }
            return View(viewName, rootMenus);
        }

        public class ReqMenu
        {
            public string ViewName = "Index";
            public bool IsMobile;
            public bool IsNavigation { get; set; }
        }
    }
}
