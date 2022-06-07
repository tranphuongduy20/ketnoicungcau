using ketnoicungcau.business;
using ketnoicungcau.business.Caching;
using ketnoicungcau.business.Extensions;
using ketnoicungcau.business.Framework;
using ketnoicungcau.business.Helpers;
using ketnoicungcau.business.Helpers.Interface;
using ketnoicungcau.business.Model;
using ketnoicungcau.business.ModelWeb;
using ketnoicungcau.business.ServiceWeb.Interface;
using ketnoicungcau.vn.Models;
using ketnoicungcau.vn.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using ketnoicungcau.business.Enums;

namespace ketnoicungcau.vn.Controllers
{
    public class ProductController : BaseController
    {
        #region Constructors

        private readonly ILogger<ProductController> _logger;
        private readonly ICache _cache;
        private readonly IProductServiceWeb _productSvc;
        private readonly IUtilitiesServiceWeb _utilSvc;
        private readonly IUserHelpers _userHelpers;
        private readonly IWorkContext _workContext;
        public ProductController(ILogger<ProductController> logger,
            ICache cache,
            IProductServiceWeb productSvc,
            IUtilitiesServiceWeb utilSvc,
            IUserHelpers userHelpers,
            IWorkContext workContext,
            IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _logger = logger;
            _cache = cache;
            _productSvc = productSvc;
            _utilSvc = utilSvc;
            _userHelpers = userHelpers;
            _workContext = workContext;
        }
        #endregion

        #region Methods
        public async Task<IActionResult> Index(string CategoryUrl = "", string ProductUrl = "", int ProductID = 0)
        {
            string viewName = $"Index{(_workContext.IsMobile ? ".M" : "")}";

            #region Redirect

            ProductWeb product = await _productSvc.GetProductByID(ProductID);
            var resCheck = product.CheckProductIsValid();
            if (resCheck != "") return Redirect(resCheck);

            if (product.GenProductUrl() != "/" + CategoryUrl + "/" + ProductUrl + "-" + ProductID)
                return Redirect(product.GenProductUrl());

            #endregion

            // Lấy tình trạng deal hiện tại
            var deal = new Deals();
            var currentUser = await _userHelpers.GetCookiesUserWeb();
            if (currentUser != null && currentUser.Companyid > 0)
            {
                deal = await _productSvc.GetDeal(product.Company.Companyid, currentUser.Companyid, product.Productid);
            }

            var keyCache = _cache.CreateKey(Constants.CacheType.Product, CategoryUrl, ProductUrl, ProductID.GetProductName());
            var vm = _cache.Get<vmProductDetail>(keyCache);

            var wishlist = await _utilSvc.GetWishlist(product.Productid ?? ProductID, currentUser.Companyid);

            if (vm != null)
            {
                vm.UserCompany = currentUser;
                vm.Product = product;
                vm.Deal = deal;
                vm.TotalLike = await _utilSvc.GetTotalWishlistByProduct(product.Productid.ToString());
                vm.IsLike = wishlist != null && wishlist.Productid > 0 && wishlist.Companyid > 0 ? true : false;
                return View(viewName, vm);
            }

            vm = new vmProductDetail()
            {
                UserCompany = currentUser,
                Product = product,
                Deal = deal,
                ListPrices = await _utilSvc.SearchSellUnit((int)product.Productid),
                StringProductStandard = await _utilSvc.GetListStandardName(product.Liststandardid),
                StringSpecialties = await _utilSvc.GetListSpecialties(product.Listspecialtiesid),
                Provincename = await _productSvc.GetProvinceName(product.Provinceid),
                TotalLike = await _utilSvc.GetTotalWishlistByProduct(product.Productid.ToString()),
                IsLike = wishlist != null && wishlist.Productid > 0 && wishlist.Companyid > 0 ? true : false,
        };

            _cache.Set(keyCache, vm, Constants.TimeCache.dateHalfDefault);

            return View(viewName, vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddDeals(RequestAddDeals request)
        {
            var currentUser = await _userHelpers.GetCookiesUserWeb();
            if (currentUser == null || currentUser.Companyid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT });

            // UtilitiesExtensions
            request.Description = UtilitiesExtensions.RemoveHarmfulCharacter(request.Description);

            // Validate
            var res = request.Validate();
            if (res != null)
                return ReturnData(res);

            // Check đúng account không
            if (currentUser.Companyid != request.Buyer)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT });

            // Check tồn tại deals
            var deal = await _productSvc.GetDeal(request.Seller, request.Buyer, request.Product);
            if (deal != null && deal.Dealsid > 0 && deal.Dealstatus != (int)StatusDeals.Cancel && deal.Dealstatus != (int)StatusDeals.Deny)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Kết nối đã tồn tại" });

            var deals = new Deals()
            {
                Salecompanyid = request.Seller,
                Buycompanyid = request.Buyer,
                Productid = request.Product,
                Quantity = request.Quantity,
                Unitid = request.Unit,
                Saleprice = request.Value,
                Dealstatus = 1,
                Description = "",
                Isactived = 1,
                Createduser = ConfigConstants.USER_UPDATE_WEB + currentUser.Companyid
            };

            // Insert
            var resCode = await _productSvc.InsertDeals(deals);
            if (resCode > 0)
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = $"Thêm mới kết nối thành công" });

            return ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Thêm mới kết nối thất bại" });
        }

        [HttpPost]
        public async Task<IActionResult> AddWishlist(long productid)
        {
            if(productid <= 0)
                ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Không tìm thấy sản phẩm" });

            var product = await _productSvc.GetProductByID((int)productid);
            if(product == null )
                ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Không tìm thấy sản phẩm" });

            var currentUser = await _userHelpers.GetCookiesUserWeb();
            if (currentUser == null)
                ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Vui lòng đăng nhập" });

            var wishlist = await _utilSvc.GetWishlist(product.Productid ?? productid, currentUser.Companyid);
            if (wishlist != null && wishlist.Productid > 0 && wishlist.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = $"hêm mới sản phẩm yêu thích thất bại. Sản phẩm này đã được yêu thích" });

            var productWishlist = new ProductWishlist()
            {
                Productid = product.Productid ?? productid,
                Companyid = currentUser.Companyid
            };
            var res = await _utilSvc.InsertWishlist(productWishlist);
            if (res > 0)
            {
                var totalLike = await _utilSvc.GetTotalWishlistByProduct(productid.ToString());
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = totalLike, message = $"Thêm mới sản phẩm yêu thích thành công" });
            }

            return ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Thêm mới sản phẩm yêu thích thất bại" });
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlistByCompany(string listcompanyid)
        {
            var res = await _utilSvc.GetWishlistByCompany(listcompanyid);
            if (res != null)
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = $"Thêm mới sản phẩm yêu thích thành công" });

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = $"Thêm mới sản phẩm yêu thích thất bại" });
        }

        [HttpGet]
        public async Task<IActionResult> GetTotalWishlistByProduct(string listcompanyid)
        {
            var res = await _utilSvc.GetTotalWishlistByProduct(listcompanyid);
            if (res > 0)
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = $"Thêm mới sản phẩm yêu thích thành công" });

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = $"Thêm mới sản phẩm yêu thích thất bại" });
        }

        [HttpPost]
        public async Task<IActionResult> DelWishlist(long productid)
        {
            if (productid <= 0)
                ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Không tìm thấy sản phẩm" });

            var product = await _productSvc.GetProductByID((int)productid);
            if (product == null)
                ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Không tìm thấy sản phẩm" });

            var currentUser = await _userHelpers.GetCookiesUserWeb();
            if (currentUser == null)
                ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Vui lòng đăng nhập" });

            var wishlist = await _utilSvc.GetWishlist(product.Productid ?? productid, currentUser.Companyid);
            if(wishlist == null || wishlist.Productid <=0 || wishlist.Companyid <=0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Xoá sản phẩm yêu thích thất bại" });

            var res = await _utilSvc.DeleteWishlist(product.Productid ?? productid, currentUser.Companyid);
            if (res > 0)
            {
                var totalLike = await _utilSvc.GetTotalWishlistByProduct(productid.ToString());
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = totalLike, message = $"Xoá sản phẩm yêu thích thành công" });
            }

            return ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Xoá sản phẩm yêu thích thất bại" });
        }

        #endregion
    }
}
