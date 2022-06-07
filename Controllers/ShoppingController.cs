using ketnoicungcau.business.Caching;
using ketnoicungcau.business.ServiceWeb.Interface;
using ketnoicungcau.vn.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ketnoicungcau.business.Framework;
using ketnoicungcau.business.ModelWeb;
using System.Linq;
using ketnoicungcau.business;
using Microsoft.AspNetCore.Hosting;

namespace ketnoicungcau.vn.Controllers
{
    public class ShoppingController : BaseController {
        #region Constructors
        private readonly ILogger<ShoppingController> _logger;
        private readonly ICache _cache;
        private readonly IWorkContext _workContext;
        private readonly IProductServiceWeb _productSvc;
        private readonly IUtilitiesServiceWeb _utilitiesSvc;
        private readonly ICategoryServiceWeb _categorySvc;


        public ShoppingController(ILogger<ShoppingController> logger,
            ICache cache,
            IProductServiceWeb productSvc,
            IUtilitiesServiceWeb utilitiesSvc,
            ICategoryServiceWeb categorySvc,
            IWorkContext workContext,
            IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _logger = logger;
            _cache = cache;
            _workContext = workContext;
            _productSvc = productSvc;
            _utilitiesSvc = utilitiesSvc;
     
        }
        #endregion

        [Route("mua-sam-online")]
        public async Task<IActionResult> Index(string url)
        {
            string viewName =  $"Index{(_workContext.IsMobile ? ".M" : "")}";
            var keyCache = _cache.CreateKey(_workContext.IsMobile);
            var vm = _cache.Get<vmWebsite>(keyCache);
            if (vm != null)
                return View(viewName, vm);

            vm = new vmWebsite() 
            {
                categoryWebsites = _workContext.IsMobile ? await _utilitiesSvc.GetPromoteWebsite("", 0, 6, 1) : await _utilitiesSvc.GetPromoteWebsite("", 0 ,10, 1),
                //websites = await _utilitiesSvc.SearchPromoteWebsite("", 0, 1000),
            };
            _cache.Set(keyCache, vm);
            return View(viewName, vm);
        }

        [Route("mua-sam-online/{url}")]
        public async Task<IActionResult> ShoppingCate(string url)
        {   
            string viewName =  $"ShoppingCate{(_workContext.IsMobile ? ".M" : "")}";
            var keyCache = _cache.CreateKey(_workContext.IsMobile,url);
            var vm = _cache.Get<vmShoppingCate>(keyCache);
            if (vm != null)
                return View(viewName, vm);
            vm = await GenVmShoppingCate(url);
            _cache.Set(keyCache, vm);
            return View(viewName, vm);
        }


        public async Task<IActionResult> GetWebsiteList(SearchWebsiteQuery query)
        {
            var lsCate = query.StrListCateId;
            if (query.IsGetAll)
                lsCate = query.StrCate;
            
            var res = await _utilitiesSvc
                .SearchPromoteWebsiteByListCateId("", query.PageIndex, query.PageSize, lsCate, 1);
            var listWebsite = await RenderViewToStringAsync("~/Views/Shopping/Partial/_ListWebsite.cshtml", res);

            var total = res != null && res.Any() ? res[0].Totalrecord : 0;
            var left = total > 0 ? total - (query.PageIndex + 1) * query.PageSize : 0;
            return Json(new { listWebsite = listWebsite, left = left, total = total });
        }


        #region Private Method

        private async Task<vmShoppingCate> GenVmShoppingCate(string url)
        {
            var listCate = await _utilitiesSvc.GetAllCategory();
            var cateMatch = listCate.GetCategoryMatch(url);
            var idCate = cateMatch.Categoryid <= 0 ? -1 : cateMatch.Categoryid;
            var nameCate = listCate.FirstOrDefault(c => c.Categoryid == idCate)?.Categoryname;
            var vm = new vmShoppingCate();
            var pSize = _workContext.IsMobile ? 10 : 20;
            var resSvc = await _utilitiesSvc.GetPromoteWebsiteByCateId("", 0, pSize, (int)idCate, listCate, 1);
            vm.CategoryWebsite = new CategoryWebsite();
            vm.CategoryWebsite = resSvc.CategoryWebsite;
            vm.CategoryWebsite.cateName = nameCate;
            vm.CategoryWebsite.cateUrl = url;
            vm.CategoryWebsite.CateId = idCate;
            vm.Left = resSvc.Left;
            vm.Total = resSvc.Total;
            vm.StrCate = resSvc.StrCate;
            return vm;
        }
        #endregion


        public class SearchWebsiteQuery
        {
            public long CategoryId { get; set; }
            public string CategoryUrl { get; set; }
            public string StrListCateId { get; set; }
            public string StrCate { get; set; }
            public string IsFilterManyCate { get; set; }
            public int PageIndex { get; set; }
            public int PageSize { get; set; }
            public bool IsGetAll { get; set; }
        }
    }
}