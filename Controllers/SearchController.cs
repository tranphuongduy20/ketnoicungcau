using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using ketnoicungcau.business.Caching;
using ketnoicungcau.business.Enums;
using ketnoicungcau.business.ModelWeb;
using ketnoicungcau.business.ServiceWeb.Interface;
using ketnoicungcau.vn.ViewModels;
using ketnoicungcau.business.Framework;
using Microsoft.AspNetCore.Hosting;
using ketnoicungcau.business;

namespace ketnoicungcau.vn.Controllers
{
    public class SearchController : BaseController
    {
        private readonly ISearchServiceWeb _searchSvc;
        private readonly IWorkContext _workContext;
        private readonly ICache _cache;


        #region Constructor

        public SearchController(ISearchServiceWeb searchSvc,
            IWorkContext workContext,
            ICache cache,
            IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _searchSvc = searchSvc;
            _workContext = workContext;
            _cache = cache;
        }

        #endregion

        #region Action

        public async Task<IActionResult> Index(SearchQuery query)
        {
            string viewName = $"Index{(_workContext.IsMobile ? ".M" : "")}";

            if(query == null || string.IsNullOrEmpty(query.KeyWord) || query.KeyWord.Length < 2)
                return View(viewName, new vmSearch());
            query.KeyWord = UtilitiesExtensions.RemoveHarmfulCharacter(query.KeyWord);
            query = HanleQueryParam(query);

            var keyCache = _cache.CreateKey(_workContext.IsMobile, business.Caching.Constants.CacheType.Product, query.SearchType, query.KeyWord.GetKeyWordName(false), query.PageIndex, query.PageSize);

            var vm = _cache.Get<vmSearch>(keyCache);

            if (vm != null)
                return View(viewName, vm);

            vm = await GenVmSearch(query);

            _cache.Set(keyCache, vm, Constants.TimeCache.dateDefaulForSearch);
            return View(viewName, vm);
        }

        // Action gọi ProductList
        public async Task<IActionResult> GetListProduct(SearchQuery query)
        {
            var Total = 0;
            var Left = 0;
            var listProduct = "";
            query.KeyWord = UtilitiesExtensions.RemoveHarmfulCharacter(query.KeyWord);
            query = HanleQueryParam(query);

            var keyCache = _cache.CreateKey(_workContext.IsMobile, business.Caching.Constants.CacheType.Product, query.SearchType, query.KeyWord.GetKeyWordName(false), query.PageIndex, query.PageSize);

            var resCache = _cache.Get<ListProductSearchResult>(keyCache);

            if (resCache != null)
            {
                listProduct = await RenderViewToStringAsync("~/Views/Common/Partial/_ListProduct.cshtml", resCache.ListProduct);
                Total = resCache.Total;
                Left = resCache.Left;
                return Json(new { total = Total, left = Left, listProduct = listProduct });
            }


            
            var res = await _searchSvc.GetListProduct(query);
            listProduct = await RenderViewToStringAsync("~/Views/Common/Partial/_ListProduct.cshtml", res.ListProduct);
            Total = res.Total;
            Left = res.Left;

            resCache = res;
            _cache.Set(keyCache, resCache, Constants.TimeCache.dateDefaulForSearch);
            return Json(new { total = Total, left = Left, listProduct = listProduct });
        }

        // Action gọi CompanyList
        public async Task<IActionResult> GetListCompany(SearchQuery query)
        {
            var Total = 0;
            var Left = 0;
            var listCompany = "";
            query.KeyWord = UtilitiesExtensions.RemoveHarmfulCharacter(query.KeyWord);
            query = HanleQueryParam(query);

            var keyCache = _cache.CreateKey(_workContext.IsMobile, business.Caching.Constants.CacheType.Product, query.SearchType, query.KeyWord.GetKeyWordName(false), query.PageIndex, query.PageSize);

            var resCache = _cache.Get<ListCompanySearchResult>(keyCache);

            if (resCache != null)
            {
                listCompany = await RenderViewToStringAsync("~/Views/Common/Partial/_ListCompany.cshtml", resCache.ListCompany);
                Total = resCache.Total;
                Left = resCache.Left;
                return Json(new { total = Total, left = Left, listCompany = listCompany });

            }

            
            var res = await _searchSvc.GetListCompany(query);
            listCompany = await RenderViewToStringAsync("~/Views/Common/Partial/_ListCompany.cshtml", res.ListCompany);
            Total = res.Total;
            Left = res.Left;

            resCache = res;
            _cache.Set(keyCache, resCache, Constants.TimeCache.dateDefaulForSearch);
            return Json(new { total = Total, left = Left, listCompany = listCompany });
        }

        #endregion

        #region Private method

        private async Task<vmSearch> GenVmSearch(SearchQuery query)
        {

            vmSearch vmSearch = new vmSearch()
            {
                Filter = query,
            };

            //dynamic res = null;
            if (query.SearchType == SearchType.Buy || query.SearchType == SearchType.Sell)
            {
                var res = await _searchSvc.GetListProduct(query);
                if ((res == null) || (res.ListProduct != null && res.ListProduct.Any() == false))
                {
                    if (query.SearchType == SearchType.Buy)
                        query.SearchType = SearchType.Sell;
                    else if (query.SearchType == SearchType.Sell)
                        query.SearchType = SearchType.Buy;

                    res = await _searchSvc.GetListProduct(query);

                    if ((res == null) || (res.ListProduct != null && res.ListProduct.Any() == false))
                    {
                        query.SearchType = SearchType.Company;
                        var resCompany = await _searchSvc.GetListCompany(query);
                        vmSearch.ListCompany = resCompany.ListCompany;
                        vmSearch.Total = resCompany.Total;
                        vmSearch.Left = resCompany.Left;
                    }
                    else
                    {
                        vmSearch.ListProduct = res.ListProduct;
                        vmSearch.Total = res.Total;
                        vmSearch.Left = res.Left;
                    }

                }
                else
                {
                    vmSearch.ListProduct = res.ListProduct;
                    vmSearch.Total = res.Total;
                    vmSearch.Left = res.Left;
                }
            }
            else if (query.SearchType == SearchType.Company)
            {
                var res = await _searchSvc.GetListCompany(query);
                vmSearch.ListCompany = res.ListCompany;
                vmSearch.Total = res.Total;
                vmSearch.Left = res.Left;
            }

            //if (query.SearchType == SearchType.Buy || query.SearchType == SearchType.Sell)
            //{
            //    res = await _searchSvc.GetListProduct(query);
            //    vmSearch.ListProduct = res.ListProduct;
            //}
            //else if (query.SearchType == SearchType.Company)
            //{
            //    res = await _searchSvc.GetListCompany(query);
            //    vmSearch.ListCompany = res.ListCompany;
            //}

            //if (res != null)
            //{
            //    vmSearch.Total = res.Total;
            //    vmSearch.Left = res.Left;
            //}
            return vmSearch;
        }

        private SearchQuery HanleQueryParam(SearchQuery query)
        {

            if (query != null)
            {
                query.PageSize = GetPageSize(query);
            }

            return query;
        }

        private int GetPageSize(SearchQuery query)
        {
            int pSize = 8;

            if (query != null)
            {
                if (_workContext.IsMobile)
                {
                    //if (query.SearchType == SearchType.Buy)
                    //{
                    //    pSize = (int)PageSize.BuyMobile;
                    //}
                    //else if (query.SearchType == SearchType.Sell)
                    //{
                    //    pSize = (int)PageSize.SellMobile;
                    //}
                    //else if (query.SearchType == SearchType.Company)
                    //{
                    //    pSize = (int)PageSize.CompanyMobile;
                    //}

                    pSize = 4;
                }
                else
                {
                    //if (query.SearchType == SearchType.Buy)
                    //{
                    //    pSize = (int)PageSize.Buy;
                    //}
                    //else if (query.SearchType == SearchType.Sell)
                    //{
                    //    pSize = (int)PageSize.Sell;
                    //}
                    //else
                    pSize = 8;
                    if (query.SearchType == SearchType.Company)
                    {
                        pSize = 10;
                    }
                    
                }
            }
            //Console.WriteLine(pSize);
            return pSize;
        }

        #endregion
    }
}
