using ketnoicungcau.business.Caching;
using ketnoicungcau.business.Enums;
using ketnoicungcau.business.Framework;
using ketnoicungcau.business.Helpers;
using ketnoicungcau.business.Helpers.Interface;
using ketnoicungcau.business.infrastructure;
using ketnoicungcau.business.ModelWeb;
using ketnoicungcau.business.ServiceWeb.Interface;
using ketnoicungcau.vn.Models;
using ketnoicungcau.vn.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ketnoicungcau.vn.Controllers
{
    public class HomeController : BaseController
    {

        #region Constructors
        private readonly ILogger<HomeController> _logger;
        private readonly ICache _cache;
        private readonly IOptions<AppSettings> _settings;
        private readonly IProductServiceWeb _productSvc;
        private readonly ISearchServiceWeb _searchSvc;
        private readonly ICompanyServiceWeb _companySvc;
        private readonly INewsServiceWeb _newsSvc;
        private readonly IWorkContext _workContext;
        private readonly IUserHelpers _userHelpers;
        private readonly ICategoryServiceWeb _categorySvc;

        public HomeController(
            ICache cache,
            ILogger<HomeController> logger,
            IOptions<AppSettings> settings,
            IProductServiceWeb productSvc,
            ISearchServiceWeb searchSvc,
            ICompanyServiceWeb companySvc,
            INewsServiceWeb newsSvc,
            IUserHelpers userHelpers,
            ICategoryServiceWeb categorySvc,
            IWorkContext workContext,
            IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _cache = cache;
            _logger = logger;
            _settings = settings;
            _productSvc = productSvc;
            _searchSvc = searchSvc;
            _companySvc = companySvc;
            _newsSvc = newsSvc;
            _workContext = workContext;
            _userHelpers = userHelpers;
            _categorySvc = categorySvc;
        }
        #endregion

        public async Task<IActionResult> Index()
        {
            string viewName = $"{("Index")}{(_workContext.IsMobile ? ".M" : "")}";

            var keyCache = _cache.CreateKey(_workContext.IsMobile);
            var vm = _cache.Get<vmHome>(keyCache);

            var productRelated = new List<ProductWeb>();
            var companyCate = await _userHelpers.GetCookiesUserWeb();
            // đã đăng nhập
            if (companyCate != null && companyCate.Companyid > 0)
            {
                // cache đã có sp liên quan
                if (vm != null && vm.ListRelated != null && vm.ListRelated.Any())
                    return View(viewName, vm);
                else // cache chưa có sp liên quan
                {
                    var query = new SearchQuery()
                    {
                        StrListCategory = companyCate.Listcategoryid,
                        PageSize = 8
                    };
                    productRelated = await _categorySvc.SearchProductByFilter(query);
                    if (vm != null) vm.ListRelated = productRelated;
                }
                
            }
            // chưa đăng nhập và đã lưu cache => return cache
            else if (vm != null) return View(viewName, vm);

            vm = new vmHome()
            {
                ListSell = await _productSvc.TopSearchProductHome(3, 8, 2),
                ListBuy = await _productSvc.TopSearchProductHome(3, 8, 1),
                ListRelated = productRelated,
                ListNew = _workContext.IsMobile ? await _newsSvc.GetNewsHome(8) : await _newsSvc.GetNewsHome(12),
                ListNewTop1 = await _newsSvc.GetNewsHome(100),
                ListCompany = _workContext.IsMobile ? await _companySvc.TopSearchCompany(1, 8) : await _companySvc.TopSearchCompany(1, 10),
                ListImage = await _productSvc.GetImageByType(20, 2)
            };

            #region cat chuoi video
            foreach (var item in vm.ListNew)
            {
                if (item.Video.Contains("https://www.youtube.com/watch?v="))
                {
                    var str = "https://img.youtube.com/vi/";
                    item.Video = item.Video.Replace("https://www.youtube.com/watch?v=", string.Empty);
                    if (item.Video.Contains("&ab_channel="))
                    {
                        var idx = item.Video.IndexOf("&");
                        item.Video = item.Video.Substring(0, idx);
                    }
                    item.Video = str + item.Video + "/maxresdefault.jpg";
                }
            }
            #endregion

            foreach (var item in vm.ListNewTop1)
            {
                item.ListImage = vm.ListImage.Where(i => i.Objectid == item.Newsid).ToList();
            }
            if (vm.ListNew?.Count > 0)
                vm.ListNew.RemoveAll(e => e.Categoryid == 24);
            _cache.Set(keyCache, vm);
            return View(viewName, vm);
        }
        public IActionResult Privacy()
        {
            var n = 0;
            var t = 1 / n;
            return Content("");
        }

        public IActionResult StatusCode500()
        {
            return StatusCode(500);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("/Error")]
        public IActionResult Error()
        {
            var _hostingEnvironment = EngineContext.Current.Resolve<IWebHostEnvironment>();
            if (_hostingEnvironment.IsProduction())
                return StatusCode(500);
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> GetProductList(ProductQueryParam query)
        {
            var result = new ProductResult();
            if (query != null)
            {
                var data = new List<ProductWeb>();
                if (query.ProductType == (int)ProductType.Buy)
                    data = await _productSvc.TopSearchProductHome(3, 4, 1);
                else
                    data = await _productSvc.TopSearchProductHome(3, 4, 2);

                if (data?.Count > 0)
                {
                    result.Total = data.FirstOrDefault().Totalrecord;
                    result.StringListProductHtml = await RenderViewToStringAsync("~/Views/Common/Partial/_ListProduct.cshtml", data);
                }
            }
            return Json(new { total = result.Total, listproducts = result.StringListProductHtml });
        }
    }
}
