using ketnoicungcau.business.Caching;
using ketnoicungcau.business.ModelWeb;
using ketnoicungcau.business.ServiceWeb.Interface;
using ketnoicungcau.vn.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ketnoicungcau.business.Enums;
using ketnoicungcau.business.Framework;
using Microsoft.AspNetCore.Hosting;
using ketnoicungcau.business.Helpers;
using ketnoicungcau.business.Extensions;

namespace ketnoicungcau.vn.Controllers
{
    public class CompanyController : BaseController
    {
        private int PageSize = 20;
        private int PageSizeBuy = 4;
        private int PageSizeSell = 8;

        #region Constructors
        private readonly ILogger<CompanyController> _logger;
        private ICache _cache;
        private ICompanyServiceWeb _companySvc;
        private IUtilitiesServiceWeb _utilSvc;
        private IProductServiceWeb _productSvc;
        private IWorkContext _workContext;
        public CompanyController(ILogger<CompanyController> logger,
            ICache cache,
            ICompanyServiceWeb companySvc,
            IUtilitiesServiceWeb utilSvc,
            IProductServiceWeb productSvc,
            IWorkContext workContext,
            IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _logger = logger;
            _cache = cache;
            _companySvc = companySvc;
            _utilSvc = utilSvc;
            _productSvc = productSvc;
            _workContext = workContext;
        }
        #endregion

        [Route("doanh-nghiep")]
        public async Task<IActionResult> Index()
        {
            string viewName = $"{("Index")}{(_workContext.IsMobile ? ".M" : "")}";
            var vm = new vmCompany()
            {
                ProductCategories = GetListCategory(),
                CategoryId = 0,
                PageSize = PageSize
            };

            var companyDatas = await _companySvc.SearchCompany("", 1, null, null, "-1", 0, PageSize, "");
            vm.Companys = companyDatas;
            vm.TotalCompany = companyDatas?.Count > 0 ? companyDatas.FirstOrDefault().Totalrecord : 0;

            return View(viewName, vm);
        }

        /// <summary>
        /// Des: Chi tiết doanh nghiệp/ Filter doanh nghiệp theo ngành hàng
        /// Note: Cấu trúc url trang CHI TIẾT DOANH NGHIỆP và trang FILTER DOANH NGHIỆP THEO NGÀNH HÀNG giống nhau nên dùng chung Action
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        [Route("doanh-nghiep/{url}")]
        public async Task<IActionResult> Detail(string url)
        {
            int companyId = 0;
            string strId = url.Substring(url.LastIndexOf("-") + 1);
            int.TryParse(strId, out companyId);

            string viewName = $"{("Detail")}{(_workContext.IsMobile ? ".M" : "")}";
            var vm = new vmCompany()
            {
                PageSize = PageSize,
            };

            //var allProductCate = await _utilSvc.GetAllCategory();
            if (companyId == 0)
            {
                #region Danh sách doanh nghiệp filter

                viewName = $"{("Index")}{(_workContext.IsMobile ? ".M" : "")}";
                var categories = await _utilSvc.GetAllCategory();
                if (categories?.Count > 0)
                {
                    var cate = categories.Where(x => x.Url.ToLower() == url.Trim().ToLower()).FirstOrDefault();
                    if (cate != null)
                    {
                        vm.Companys = await _companySvc.SearchCompany("", 1, null, null, "-1", 0, PageSize, cate.Categoryid.ToString());
                        vm.CategoryId = cate.Categoryid;
                    }
                    else
                        return RedirectPermanent("/");
                }

                #endregion
            }
            else
            {
                #region Redirect

                var company = await _companySvc.GetCompany(companyId, 1);
                if (company.Companyid <= 0 || company == null)
                    return Redirect("/doanh-nghiep");
                if ($"{SEOHelper.GenSEOUrl(company.Companyname)}-{company.Companyid}" != url)
                    return Redirect(company.GenCompanyUrl());

                #endregion

                #region Chi tiết doanh nghiệp

                vm.CompanyId = companyId;
                vm.Company = company;
                vm.ListSell = await _productSvc.TopSearchProduct((int)ProductType.Sell, _workContext.IsMobile ? 4 : PageSizeSell, companyId: companyId);
                vm.PageIndexSell = 0;
                vm.ListBuy = await _productSvc.TopSearchProduct((int)ProductType.Buy, PageSizeBuy, companyId: companyId);
                vm.PageIndexBuy = 0;
                vm.PageSizeBuy = PageSizeBuy;
                vm.PageSizeSell = _workContext.IsMobile ? 4 : PageSizeSell;

                #endregion
            }

            vm.ProductCategories = GetListCategory();

            if (vm.Company != null && !string.IsNullOrEmpty(vm.Company.Description))
                vm.Company.Description = HtmlHelper.FixContentImgTags(vm.Company.Description);
            return View(viewName, vm);
        }

        public async Task<IActionResult> GetCompanyList(CompanyQueryParam query)
        {
            var data = new CompanyResult();
            if (query != null)
            {
                var result = await _companySvc.SearchCompany("", 1, null, null, "-1", query.PageIndex, query.PageSize, query.StrListCateId);
                if (result?.Count > 0)
                {
                    data.Total = result.FirstOrDefault().Totalrecord;
                    data.StringListCompanyHtml = await RenderViewToStringAsync("~/Views/Company/_ListCompany.cshtml", result);
                }
            }

            return Json(new { total = data.Total, listcompanys = data.StringListCompanyHtml });
        }

        public async Task<IActionResult> GetProductList(ProductQueryParam query)
        {
            var result = new ProductResult();
            if (query != null)
            {
                var data = new List<ProductWeb>();
                if (query.ProductType == (int)ProductType.Buy)
                    data = await _productSvc.TopSearchProduct((int)ProductType.Buy, PageSizeBuy, query.PageIndexBuy, query.CompanyId);
                else
                    data = await _productSvc.TopSearchProduct((int)ProductType.Sell, _workContext.IsMobile ? 4 : PageSizeSell, query.PageIndexSell, query.CompanyId);

                if (data?.Count > 0)
                {
                    result.Total = data.FirstOrDefault().Totalrecord;
                    result.StringListProductHtml = await RenderViewToStringAsync("~/Views/Common/Partial/_ListProduct.cshtml", data);
                }
            }

            return Json(new { total = result.Total, listproducts = result.StringListProductHtml });
        }

        /// <summary>
        /// Des: Hàm lọc danh sách ngành hàng theo khai báo doanh nghiệp
        /// </summary>
        /// <param name="allProductCates"></param>
        /// <param name="companyDatas"></param>
        /// <returns></returns>
        public List<business.Model.ProductCategory> GetListCategory()
        {
            var companyDatas = _companySvc.SearchCompany("", 1, null, null, "-1", 0, 100, "");
            var allProductCates = _utilSvc.GetAllCategory();
            var result = new List<business.Model.ProductCategory>();
            if (companyDatas.Result?.Count > 0)
            {
                List<string> lstCateId = null;
                if (allProductCates.Result?.Count > 0)
                {
                    foreach (var item in companyDatas.Result)
                    {
                        lstCateId = new List<string>();
                        if (!string.IsNullOrEmpty(item.Listcategoryid))
                        {
                            lstCateId = item.Listcategoryid.Split(',').ToList();
                            if (lstCateId?.Count > 0)
                            {
                                foreach (var c in lstCateId)
                                {
                                    var objCate = allProductCates.Result.Where(x => x.Categoryid.ToString() == c).FirstOrDefault();
                                    if (objCate != null && !result.Where(x => x.Categoryid == objCate.Categoryid).Any())
                                        result.Add(objCate);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
