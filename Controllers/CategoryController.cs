using ketnoicungcau.business;
using ketnoicungcau.business.Caching;
using ketnoicungcau.business.Enums;
using ketnoicungcau.business.Framework;
using ketnoicungcau.business.ModelWeb;
using ketnoicungcau.business.ServiceWeb.Interface;
using ketnoicungcau.vn.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace ketnoicungcau.vn.Controllers
{
    public class CategoryController : BaseController
    {
        #region Constructors

        private readonly ILogger<CategoryController> _logger;
        private readonly ICache _cache;
        private readonly ICategoryServiceWeb _categorySvc;
        private readonly IUtilitiesServiceWeb _utilSvc;
        private readonly IWorkContext _workContext;
        private readonly IHttpContextAccessor _httpContext;
        public CategoryController(ILogger<CategoryController> logger,
            ICache cache,
            ICategoryServiceWeb categorySvc,
            IUtilitiesServiceWeb utilSvc,
            IWorkContext workContext,
            IWebHostEnvironment hostEnvironment,
            IHttpContextAccessor httpContext) : base(hostEnvironment)
        {
            _logger = logger;
            _cache = cache;
            _categorySvc = categorySvc;
            _utilSvc = utilSvc;
            _workContext = workContext;
            _httpContext = httpContext;
        }
        #endregion

        #region Action
        public async Task<IActionResult> Index(string ProductType = "", string CategoryUrl = "")
        {
            string viewName = $"Index{(_workContext.IsMobile ? ".M" : "")}";

            var keyCache = _cache.CreateKey(Constants.CacheType.Category, ProductType, CategoryUrl);
            var vm = _cache.Get<vmCategory>(keyCache);

            if (vm != null)
                return View(viewName, vm);

            // Check xem có đúng cate không
            var listCate = await _utilSvc.GetAllCategory();
            var cateMatch = listCate.GetCategoryMatch(CategoryUrl);
            var idCate = cateMatch.Categoryid <= 0 ? -1 : cateMatch.Categoryid;
            if (cateMatch.Categoryid == 0 && CategoryUrl != "")
                return Redirect("/" + ProductType);

            #region Redirect
            // check redirect
            var path = string.Format("{0}", _httpContext.HttpContext.Request.Path);

            var redirectUrl = GetRedirectUrl(path);
            if (!string.IsNullOrEmpty(redirectUrl))
                return RedirectPermanent(redirectUrl);

            // Nếu có mỗi CateUrl thì Redirect về chào bán
            if (ProductType == "")
                return Redirect("/" + CategoryUrl + "-chao-ban");
            #endregion

            var isSell = ProductType == "chao-ban" ? true : false;

            vm = new vmCategory()
            {
                isSell = isSell,
                CategoryId = cateMatch.Categoryid,
                CategoryName = cateMatch.Categoryname,
                InfoBuy = await InitInfo(SearchType.Buy, idCate, isSell),
                InfoSell = await InitInfo(SearchType.Sell, idCate, isSell),
                ListCateBuy = await _utilSvc.GetAllCategory(2),
                ListCateSell = await _utilSvc.GetAllCategory(3),
                ListProvinceBuy = await _utilSvc.GetAllProvince(2),
                ListProvinceSell = await _utilSvc.GetAllProvince(3),
                ListStandardBuy = await _utilSvc.GetAllStandard(2),
                ListStandardSell = await _utilSvc.GetAllStandard(3),
                ListSpecialistBuy = await _categorySvc.GetAllSpecialties(2),
                ListSpecialistSell  = await _categorySvc.GetAllSpecialties(3),
            };

            _cache.Set(keyCache, vm, Constants.TimeCache.dateHalfDefault);

            return View(viewName, vm);
        }

        public async Task<IActionResult> Sell(string CategoryUrl)
        {
            return await Index("chao-ban", CategoryUrl);
        }

        public async Task<IActionResult> Buy(string CategoryUrl)
        {
            return await Index("chao-mua", CategoryUrl);
        }

        public async Task<IActionResult> GetListProduct(SearchQuery query)
        {
            var keyCache = _cache.CreateKey(query.SearchType, query.StrListCategory, query.StrListProvince, query.StrListSpecialist, query.StrListStandard, query.PageSize, query.PageIndex, query.OrderType, query.IsGetAll);
            var data = _cache.Get<ListProductResult>(keyCache);
            var timeCache = Constants.TimeCache.dateDefault;
            if (data == null)
            {
                data = new ListProductResult();
                if (query != null)
                {
                    query = HandleQueryParam(query);
                    var res = await _categorySvc.SearchProductByFilter(query);
                    if (res != null && res.Any())
                    {
                        data.Total = res.FirstOrDefault().Totalrecord;
                        data.Left = CountLeft(data.Total, query);
                        data.Listproducts = await RenderViewToStringAsync("~/Views/Common/Partial/_ListProduct.cshtml", res);
                    }
                    else
                    {
                        timeCache = Constants.TimeCache.dateExcetion;
                    }
                }

                _cache.Set(keyCache, data, timeCache);
            }

            return Json(new { total = data.Total, left = data.Left, listproducts = data.Listproducts, textResponseApi = data.TextResponseApi });
        }
        #endregion

        #region Method

        private static string GetRedirectUrl(string cateUrl)
        {
            if (string.IsNullOrEmpty(cateUrl)) return "";
            string redirectUrl = "";
            if (cateUrl != cateUrl.ToLower())
            {
                redirectUrl = cateUrl.ToLower();
                return redirectUrl;
            }
            return redirectUrl;
        }
        private async Task<InfoTab> InitInfo(SearchType type, long categoryid, bool isSell)
        {
            var listProduct = new List<ProductWeb>();
            var query = new SearchQuery()
            {
                OrderType = 1,
                SearchType = type,
                PageIndex = 0,
                PageSize = GenPagesize(type),
                StrListCategory = "-1"
            };
        
            if ((type == SearchType.Sell && isSell) ||
                type == SearchType.Buy && isSell == false)
            {
                query.StrListCategory = categoryid.ToString();
                listProduct = await _categorySvc.SearchProductByFilter(query);
            }  

            var total = 0;
            if (listProduct.FirstOrDefault() != null)
                total = listProduct.FirstOrDefault().Totalrecord;

            return new InfoTab()
            {
                Total = total,
                Left = CountLeft(total, query),
                ListProduct = listProduct,
            };
        }

        private SearchQuery HandleQueryParam(SearchQuery query)
        {
            if (query != null)
            {
                query.PageSize = GenPagesize(query.SearchType);
            }

            return query;
        }

        private int GenPagesize(SearchType type)
        {
            if (_workContext.IsMobile)
                return type == SearchType.Sell ? (int)PageSize.SellMobile : (int)PageSize.BuyMobile;
            else
                return type == SearchType.Sell ? (int)PageSize.Sell : (int)PageSize.Buy;
        }

        private int CountLeft(int Total, SearchQuery query)
        {
            return Total - query.PageSize * (query.PageIndex + 1);
        }

        #endregion
    }
}
