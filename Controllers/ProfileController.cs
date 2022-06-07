using ketnoicungcau.business;
using ketnoicungcau.business.Enums;
using ketnoicungcau.business.Framework;
using ketnoicungcau.business.Helpers;
using ketnoicungcau.business.Helpers.Interface;
using ketnoicungcau.business.Model;
using ketnoicungcau.business.ModelWeb;
using ketnoicungcau.business.Service.Interface;
using ketnoicungcau.business.ServiceWeb.Interface;
using ketnoicungcau.vn.Extensions;
using ketnoicungcau.vn.Models;
using ketnoicungcau.vn.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace ketnoicungcau.vn.Controllers
{
    [AuthorizeExt]
    public class ProfileController : BaseController
    {
        #region Constructors
        private readonly ILogger<ProfileController> _logger;
        private readonly IUserHelpers _userHelpers;
        private readonly ICompanyService _companySvc;
        private readonly IProductService _productSvc;
        private readonly IProductServiceWeb _productSvcWeb;
        private readonly ICategoryServiceWeb _categorySvcWeb;
        private readonly IUtilitiesServiceWeb _utilitiesSvcWeb;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private IWorkContext _workContext;

        public ProfileController(ILogger<ProfileController> logger,
            IWorkContext workContext,
            IUserHelpers userHelpers, 
            ICompanyService companySvc,
            IProductService productSvc,
            IProductServiceWeb productSvcWeb,
            ICategoryServiceWeb categorySvcWeb,
            IUtilitiesServiceWeb utilitiesSvcWeb,
            IWebHostEnvironment hostingEnvironment) : base(hostingEnvironment)
        {
            _logger = logger;
            _companySvc = companySvc;
            _productSvc = productSvc;
            _productSvcWeb = productSvcWeb;
            _categorySvcWeb = categorySvcWeb;
            _utilitiesSvcWeb = utilitiesSvcWeb;
            _userHelpers = userHelpers;
            _workContext = workContext;
            _hostingEnvironment = hostingEnvironment;
        }
        #endregion

        #region Profile

        [Route("profile")]
        public async Task<IActionResult> ChangeInfo()
        {
            string viewName = $"ChangeInfo{(_workContext.IsMobile ? ".M" : "")}";

            var currentUser = await _userHelpers.GetCookiesUserWeb();
            if (currentUser == null || currentUser.Companyid <= 0)
                return Redirect("/");

            var vm = new vmProfile()
            {
                Company = currentUser,
                CompanyCategoryId = currentUser.Listcategoryid.Split(','),
                CompanyProvince = await _companySvc.GetProvinceCompany(currentUser.Companyid, 0, 100),
                ListCate = await _utilitiesSvcWeb.GetAllCategory(),
            };

            return View(viewName, vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitChangeInfo(RequestUpdateCompany request)
        {

            // UtilitiesExtensions
            request.Companyname = UtilitiesExtensions.RemoveHarmfulCharacter(request.Companyname);
            request.Address = UtilitiesExtensions.RemoveHarmfulCharacter(request.Address);
            request.Zaloid = UtilitiesExtensions.RemoveHarmfulCharacter(request.Zaloid);
            request.Saleproduct = UtilitiesExtensions.RemoveHarmfulCharacter(request.Saleproduct);
            request.Representname = UtilitiesExtensions.RemoveHarmfulCharacter(request.Representname);
            request.Representposition = UtilitiesExtensions.RemoveHarmfulCharacter(request.Representposition);
            request.Listcategoryid = UtilitiesExtensions.RemoveHarmfulCharacter(request.Listcategoryid);

            // Validate
            var res = request.Validate();
            if (res != null)
                return ReturnData(res);
            var checkImage = CheckFormatImage(request.Logosrc, checkExist: true);
            if (checkImage.code == -1) return ReturnData(new BaseResponseResult() { code = -1, errormessage = checkImage.errormessage });

            checkImage = CheckFormatImage(request.Gpkdsrc, checkExist: true);
            if (checkImage.code == -1) return ReturnData(new BaseResponseResult() { code = -1, errormessage = checkImage.errormessage });

            // Lấy thông tin company
            var company = await _userHelpers.GetCookiesUserWeb();
            if (company == null || company.Companyid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT });

            #region Xóa Image

            string contentRootPath = _hostingEnvironment.ContentRootPath;

            // Xóa logo cũ
            if (company.Logosrc != request.Logosrc)
            {
                try
                {
                    string path = string.Empty;
                    if (company.Logosrc.StartsWith("/cms/"))
                        path = Path.Combine(contentRootPath.Replace("ketnoicungcau.vn", "cms.ketnoicungcau.vn/wwwroot") + company.Logosrc.Replace("/cms/", "/"));
                    else
                        path = contentRootPath + "/wwwroot" + company.Logosrc;
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
                catch(Exception e)
                {
                    _logger.LogError(e.Message);
                }
                
            }

            // Xóa GPKD cũ
            if (company.Gpkdsrc != request.Gpkdsrc)
            {
                try
                {
                    string path = string.Empty;
                    if (company.Gpkdsrc.StartsWith("/cms/"))
                        path = Path.Combine(contentRootPath.Replace("ketnoicungcau.vn", "cms.ketnoicungcau.vn/wwwroot") + company.Gpkdsrc.Replace("/cms/", "/"));
                    else
                        path = contentRootPath + "/wwwroot" + company.Gpkdsrc;
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
                
            }
            #endregion

            #region Move Image từ folder tạm qua folder chính

            // Move image từ folder temp qua folder chính
            var srcPathLogo = contentRootPath + "/wwwroot" + request.Logosrc;
            var desPathLogo = srcPathLogo.Replace("tempcompanylogo", "companylogo"); 

            if (srcPathLogo != desPathLogo)
            {
                try
                {
                    var folderPath = $"/wwwroot/images/upload/companylogo/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/";

                    if (!Directory.Exists(contentRootPath + folderPath))
                        Directory.CreateDirectory(contentRootPath + folderPath);
                    System.IO.File.Move(srcPathLogo, desPathLogo);
                    if (System.IO.File.Exists(srcPathLogo))
                        System.IO.File.Delete(srcPathLogo);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }

                
            }

            var srcPathGpkd = contentRootPath + "/wwwroot" + request.Gpkdsrc;
            var desPathGpkd = srcPathGpkd.Replace("tempcompanybusinesslicense", "companybusinesslicense");

            if (srcPathGpkd != desPathGpkd)
            {
                try
                {
                    var folderPath = $"/wwwroot/images/upload/companybusinesslicense/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/";

                    if (!Directory.Exists(contentRootPath + folderPath))
                        Directory.CreateDirectory(contentRootPath + folderPath);
                    System.IO.File.Move(srcPathGpkd, desPathGpkd);
                    if(System.IO.File.Exists(srcPathGpkd))
                        System.IO.File.Delete(srcPathGpkd);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }

                
            }
            #endregion

            company.Companyname = request.Companyname;
            company.Address = request.Address;
            company.Logosrc = request.Logosrc.Replace("temp", "");
            company.Gpkdsrc = request.Gpkdsrc.Replace("temp", "");
            company.Zaloid = request.Zaloid;
            company.Fburl = request.Fburl;
            company.Weburl = request.Weburl;
            company.Saleproduct = request.Saleproduct;  
            company.Representname = request.Representname;
            company.Representposition = request.Representposition;
            company.Listcategoryid = request.Listcategoryid;
            company.Description = request.Description;
            company.Updateduser = ConfigConstants.USER_UPDATE_WEB + company.Companyid;

            //Những province được chọn ở form sẽ được add vô theo formart: provinceid|0 (provinceid|isdeleted)
            var listNewProvince = new List<string>();
            if (!string.IsNullOrEmpty(request.Provinceid))
            {
                request.Provinceid = request.Provinceid.Replace(" ", "");
                listNewProvince = request.Provinceid.Contains(",") ? request.Provinceid.Split(",").ToList() : new List<string> { request.Provinceid };
                foreach (var province in listNewProvince)
                {
                    company.Provinceid += $"{province},";
                }
            }

            //Loại những province cũ được add ở trên và remove các provice đã bỏ chọn: provinceid|1 (provinceid|isdeleted)
            //if (!string.IsNullOrEmpty(request.Provinceoldid))
            //{
            //    request.Provinceoldid = request.Provinceoldid.Replace(" ", "");
            //    listOldProvince = request.Provinceoldid.Contains(",") ? request.Provinceoldid.Split(",").ToList() : new List<string> { request.Provinceoldid };
            //    foreach (var oldProvince in listOldProvince)
            //    {
            //        //ko remove những provinceid đã add ở trên
            //        if (listNewProvince.Contains(oldProvince))
            //            continue;
            //        company.Provinceid += $"{oldProvince}|1,";
            //    }
            //}

            if (!string.IsNullOrEmpty(company.Provinceid) && company.Provinceid.EndsWith(","))
                company.Provinceid = company.Provinceid.Substring(0, company.Provinceid.Length - 1);

            Console.WriteLine(company.Provinceid);

            var resCode = await _companySvc.UpdateCompany(company);

            if (resCode > 0)
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = $"Cập nhật doanh nghiệp thành công" });

            return ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Cập nhật doanh nghiệp thất bại" });
        }

        #endregion

        #region Change Password

        [Route("doi-mat-khau")]
        public async Task<IActionResult> ChangePassword()
        {
            string viewName = $"ChangePassword{(_workContext.IsMobile ? ".M" : "")}";

            var currentUser = await _userHelpers.GetCookiesUserWeb();
            if (currentUser == null || currentUser.Companyid <= 0)
                return Redirect("/");

            return View(viewName);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitChangePassword(RequestChangePassword request)
        {
            // Validate
            var res = request.Validate();
            if (res != null)
                return ReturnData(res);

            // Lấy thông tin company
            var company = await _userHelpers.GetCookiesUserWeb();
            if (company == null || company.Companyid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Tài khoản không tồn tại" });

            // Kiểm tra mật khẩu hiện tại có đúng không
            if (company.Passwordin != WebHelpers.EncryptMD5(request.OldPassword + company.Taxid))
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Mật khẩu cũ không đúng" });

            // Kiểm tra mật khẩu hiện tại và mật khẩu cũ nhập vô trùng nhau ko
            if (company.Passwordin == WebHelpers.EncryptMD5(request.NewPassword + company.Taxid))
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Mật khẩu mới không được trùng mật khẩu cũ" });

            // Cập nhật lại password
            var updatePassword = await _companySvc.UpdatePasswordCompany(company.Companyid, WebHelpers.EncryptMD5(request.NewPassword + company.Taxid));
            if (updatePassword <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT });

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = "Cập nhật mật khẩu thành công" });
        }

        #endregion

        #region Kết nối của tôi

        [Route("mywork")]
        public async Task<IActionResult> MyWork()
        {
            string viewName = $"MyWork{(_workContext.IsMobile ? ".M" : "")}";

            var currentUser = await _userHelpers.GetCookiesUserWeb();
            if (currentUser == null || currentUser.Companyid <= 0)
                return Redirect("/");

            var vm = new vmMywork()
            {
                Company = currentUser,
                TabSend = await InitInfo(DealsTypeCompany.Sent, currentUser.Companyid),
                TabReceive = await InitInfo(DealsTypeCompany.Receive, currentUser.Companyid),
            };
            return View(viewName, vm);
        }

        public async Task<IActionResult> GetListDeals(DealsQueryModel query)
        {
            var currentUser = await _userHelpers.GetCookiesUserWeb();
            if (currentUser == null || currentUser.Companyid <= 0)
                return Redirect("/");

            var data = new ListDealsResult();

            if (query != null)
            {
                query.Companyid = currentUser.Companyid;
                var res = await _utilitiesSvcWeb.GetListDealsByQuery(query);
                
                if (res != null && res.Any())
                {
                    data.Total = res.FirstOrDefault().Totalrecord;
                    data.Left = CountLeft(data.Total, query);
                    var model = new InfoTabDeals()
                    {
                        ListDeals = res,
                        Type = query.Type
                    };
                    data.Listdeals = await RenderViewToStringAsync("~/Views/Profile/Partial/Deals/_DealsList.cshtml", model);
                }
            }

            return Json(new { total = data.Total, left = data.Left, listdeals = data.Listdeals, textResponseApi = data.TextResponseApi });
        }

        public async Task<IActionResult> GetDetailDeals(int dealsid)
        {
            var currentUser = await _userHelpers.GetCookiesUserWeb();
            if (currentUser == null || currentUser.Companyid <= 0)
                return Redirect("/");

            var res = await _utilitiesSvcWeb.GetDeals(dealsid);
            if (res == null) 
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT });

            if (res.Buycompanyid != currentUser.Companyid && res.Salecompanyid != currentUser.Companyid) 
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT });

            var html = await RenderViewToStringAsync("~/Views/Profile/Partial/Deals/_DealsDetail.cshtml", res);
            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = html });
        }

        public async Task<IActionResult> SubmitUpdateDeals(int dealsid, int status)
        {
            var currentUser = await _userHelpers.GetCookiesUserWeb();
            if (currentUser == null || currentUser.Companyid <= 0)
                return Redirect("/");

            var deal = await _utilitiesSvcWeb.GetDeals(dealsid);
            if (deal == null)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT });

            if (deal.Buycompanyid != currentUser.Companyid && deal.Salecompanyid != currentUser.Companyid)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT });

            var res = await _companySvc.UpdateDealStatus(dealsid, (short)status);
            if (res <= 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT });
            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK });
        }

        #endregion

        #region Product

        [Route("productlist")]
        public async Task<IActionResult> ProductList(RequestProductList query)
        {
            string viewName = $"ProductListSell{(_workContext.IsMobile ? ".M" : "")}";
            if (query.Type == (int)ProductType.Buy)
                viewName = $"ProductListBuy{(_workContext.IsMobile ? ".M" : "")}";

            var currentUser = await _userHelpers.GetCookiesUserWeb();
            if (currentUser == null || currentUser.Companyid <= 0)
                return Redirect("/");

            var res = await _productSvcWeb
                .SearchProduct(-1, currentUser.Companyid, (short)query.Type, -1, "", query.PageIndex, query.PageSize, false);
            var total = res != null && res.Any() ? res.FirstOrDefault().Totalrecord : 0;
            var left = total - (query.PageIndex + 1) * query.PageSize;
            var vm = new vmProductList()
            {
                ProductList = res,
                Filter = query,
                Total = total,
                Left = left > 0 ? left : 0,
            };

            return View(viewName, vm);
        }

        public async Task<IActionResult> GetProductList(RequestProductList query)
        {
            query.Keyword = query.Keyword ?? "";
            var currentUser = await _userHelpers.GetCookiesUserWeb();

            var res = await _productSvcWeb
                .SearchProduct(-1, currentUser.Companyid, (short)query.Type, -1, query.Keyword, query.PageIndex, query.PageSize, false);
            var listProduct = await RenderViewToStringAsync("~/Views/Profile/Partial/ProductList/_ProductList.cshtml", new RequestProductListView()
            {
                ListProduct = res,
                IsAjax = true,
            });
            var total = res != null && res.Any() ? res.FirstOrDefault().Totalrecord : 0;
            var left = total == 0 ? 0 : total - (query.PageIndex + 1) * query.PageSize;

            return Json(new { total = total, left = left, listProduct = listProduct });
        }


        [Route("addproduct")]
        public async Task<IActionResult> AddProduct(int type, long id)
        {
            string viewName = $"AddProductSell{(_workContext.IsMobile ? ".M" : "")}";
            if (type == (int)ProductType.Buy)
                viewName = $"AddProductBuy{(_workContext.IsMobile ? ".M" : "")}";

            var vm = new vmAddProduct()
            {
                ListCate = await _utilitiesSvcWeb.GetAllCategory(),
                ListProvince = await _utilitiesSvcWeb.GetAllProvince(),
                ListStandard = await _utilitiesSvcWeb.SearchStandard(-1, 1, 0, 100),
                ListSpecialties = await _categorySvcWeb.GetAllSpecialties(),
                ListUnit = await _productSvc.SearchProductUnit("", 0, 1000, 2, 1),
                ListBaseUnit = await _productSvc.SearchProductUnit("", 0, 100, 1, 1),
                Type = type,
                Product = null,
            };

            if (id > 0)
            {
                var currentUser = await _userHelpers.GetCookiesUserWeb();
                if (currentUser == null || currentUser.Companyid <= 0)
                    return Redirect("/");

                var product = await _productSvc.GetProductById(id);
                if (product != null)
                {
                    if (product.Productid <= 0 || product.Status != type || product.Companyid != currentUser.Companyid)
                    {
                        product = null;
                    }
                    else
                    {
                        vm.Images = await _productSvc.GetImageByType(product.Productid, (int)ImageType.Product);
                        vm.PriceItems = await _productSvc.GetProductPriceByProductId(product.Productid);
                    }
                }

                vm.Product = product;
            }

            return View(viewName, vm);
        }

        public async Task<IActionResult> GetBoxPrice(int numId)
        {
            var res = await _productSvc.SearchProductUnit("", 0, 100, 2, 1);
            var boxPrice = await RenderViewToStringAsync("~/Views/Profile/Partial/Addproduct/_BoxPrice.cshtml", new RequestBoxPrice()
            {
                ListUnit = res ?? new List<ProductUnit>(),
                numId = numId,
            });

            return Json(new { boxPrice = boxPrice });
        }

        public async Task<IActionResult> ActiveProduct(long productId, bool isActive)
        {
            if (productId <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Không tìm thấy sản phẩm" });

            var currentUser = await _userHelpers.GetCookiesUserWeb();
            if (currentUser == null || currentUser.Companyid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = ErrorMessageDefault });
            var product = await _productSvc.GetProductById(productId);
            if (product == null || product.Productid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = ErrorMessageDefault });
            if (product.Companyid != currentUser.Companyid) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Không thể thay đổi trạng thái của sản phẩm này" });


            short shortActive = 0;
            if (isActive == true)
                shortActive = 1;
            product.Isactived = shortActive;
            product.Updateduser = "WEB - " + currentUser.Companyid.ToString();

            var update = await _productSvc.UpdateProduct(product);
            if (update > 0)
            {
                var strmessage = isActive == true ? "Kích hoạt" : "Bỏ kích hoạt";
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = $"{strmessage} sản phẩm thành công" });
            }

            return ReturnData(new BaseResponseResult() { code = -1, errormessage = ErrorMessageDefault });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProduct(RequestUpdateProduct request)
        {
            request.Productname =  UtilitiesExtensions.RemoveHarmfulCharacter(request.Productname);
            request.Description = request.Description ?? "";
            request.Shortdescription = UtilitiesExtensions.RemoveHarmfulCharacter(request.Shortdescription);
            request.Fullname = UtilitiesExtensions.RemoveHarmfulCharacter(request.Fullname);
            request.Position = UtilitiesExtensions.RemoveHarmfulCharacter(request.Position);
            request.Orderprocedure = UtilitiesExtensions.RemoveHarmfulCharacter(request.Orderprocedure);
            request.Begindate = UtilitiesExtensions.RemoveHarmfulCharacter(request.Begindate);

            // Validate
            var validRes = await request.Validate(_productSvc);
            if (validRes != null)
                return ReturnData(validRes);

            var currentUser = await _userHelpers.GetCookiesUserWeb();
            if (currentUser == null || currentUser.Companyid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT });

            var product = new Product();
            if (request.Productid > 0)
            {
                product = await _productSvc.GetProductById(request.Productid);
                if (product == null || product.Productid <= 0)
                    return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Không tìm thấy sản phẩm" });

                if(product.Companyid != currentUser.Companyid)
                    return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Không thể sửa sản phẩm này" });
            }

            //UtilitiesExtensions
            product.Productname = request.Productname;
            product.Description = request.Description;
            product.Shortdescription = request.Shortdescription;
            product.Fullname = request.Fullname;
            product.Companyid = currentUser.Companyid;
            product.Status = request.Producttype;
            product.Gender = request.Gender;
            product.Email = request.Email;
            product.Phonenumber = request.Phonenumber;
            product.Positions = request.Position;
            product.Url = request.Productname.ToURL();
            product.Title = "";
            product.Price = 0;
            product.Displayorder = 0;
            product.Orderprocedure = request.Orderprocedure;
            product.Urlyoutube = request.Urlyoutube;
            product.Begindate = !string.IsNullOrEmpty(request.Begindate) ? DateTime.ParseExact(request.Begindate, "dd/MM/yyyy", new CultureInfo("vi-VN")) : DateTime.MinValue;
            product.Isfrequency = request.Frequency;
            product.Isbuy = request.IsBuy;
            var isActive = request.Isshow ? 1 : 0;
            product.Ishideprice = (short)(request.Isshowprice ? 0 : 1);
            product.Quantity = request.Quantity;
            product.Provinceid = request.Provinceid;
            product.Unitid = request.Unitid;

            ////Insert & update
            var res = 0; var str = "";
            if (request.Productid > 0)
            {
                str = "Cập nhật";
                product.Updateduser = currentUser.Companyid.ToString();
                //nếu thay đổi isactive thì cập nhật 2 field đó
                if (product.Isactived != (short)isActive)
                {
                    product.Activeduser = "WEB - " + currentUser.Companyid.ToString();
                    product.Activeddate = DateTime.Now;
                }
                product.Isactived = (short)isActive;
                res = await _productSvc.UpdateProduct(product);
                if (res > 0)
                    product = await _productSvc.GetProductById(product.Productid);
            }
            else
            {
                str = "Thêm mới";
                product.Createddate = DateTime.Now;
                product.Createduser = "WEB - " + currentUser.Companyid.ToString();
                product.Isactived = (short)isActive;
                if (request.Isshow)
                {
                    product.Activeduser = "WEB - " + currentUser.Companyid.ToString();
                    product.Activeddate = DateTime.Now;
                }
                else
                {
                    product.Activeduser = "";
                    product.Activeddate = DateTime.MinValue;
                }
                res = await _productSvc.InsertProduct(product);
                if (res > 0)
                    product = await _productSvc.GetProductById(res);
            }

            if (res > 0 && product != null && product.Productid > 0)
            {
                var images = await _productSvc.GetImageByType(product.Productid, (int)ImageType.Product);
                //Xóa all quan hệ trước khi add lại

                var del = await _productSvc.DeleteAllImageByType(product.Productid, (int)ImageType.Product, "WEB - " + currentUser.Companyid.ToString());
                del = await _productSvc.DeleteMapProductCategory(product.Productid);
                del = await _productSvc.DeleteMapProductSpecialties(product.Productid);
                del = await _productSvc.DeleteMapProductStandard(product.Productid);
                del = await _productSvc.DeleteProductPriceByProductId(product.Productid, "WEB - " + currentUser.Companyid.ToString());

                string contentRootPath = _hostingEnvironment.ContentRootPath;

                foreach (var item in images)
                {
                    if (request.Productimage.Contains(item.Image) == false)
                    {
                        try
                        {
                            string path = string.Empty;
                            if (item.Image.StartsWith("/cms/"))
                            {
                                path = Path.Combine(contentRootPath.Replace("ketnoicungcau.vn", "cms.ketnoicungcau.vn/wwwroot") + item.Image.Replace("/cms/", "/"));
                            }
                            else
                            {
                                path = Path.Combine(contentRootPath + "/wwwroot" + item.Image);
                            }

                            if (System.IO.File.Exists(path))
                                System.IO.File.Delete(path);
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e.Message);
                        }
                        
                    }
                }

                //Add quan hệ sản phẩm và image
                var lstImage = !string.IsNullOrEmpty(request.Productimage) && request.Productimage.Contains("|") ? request.Productimage.Split("|").ToList() : new List<string>() { request.Productimage };
                int idxImg = 0;
                foreach (var image in lstImage)
                {
                    if (string.IsNullOrEmpty(image) || image.Trim() == "|")
                        continue;
                    var checkImage = CheckFormatImage(image, checkExist: true);
                    if (checkImage.code == -1)
                        continue;
                    if (idxImg > 30)
                        break;

                    
                    var srcPath = contentRootPath + "/wwwroot" + image;
                    var desPath = srcPath.Replace("tempproduct", "product"); /*$"/wwwroot/images/upload/product/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/";*/

                    if (srcPath != desPath)
                    {
                        try
                        {
                            var folderPath = $"/wwwroot/images/upload/product/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/";

                            if (!Directory.Exists(contentRootPath + folderPath))
                                Directory.CreateDirectory(contentRootPath + folderPath);
                            System.IO.File.Move(srcPath, desPath);

                            if (System.IO.File.Exists(srcPath))
                                System.IO.File.Delete(srcPath);
                        }
                        catch(Exception e)
                        {
                            _logger.LogError(e.Message);
                        }
                    }

                    var imageBO = new ProductImage()
                    {
                        Objectid = product.Productid,
                        Objecttype = (int)ImageType.Product,
                        Image = desPath.Replace(contentRootPath + "/wwwroot", string.Empty),
                        Displayorder = 0,
                        Isactived = 1,
                        Activeddate = DateTime.Now,
                        Activeduser = "WEB - " + currentUser.Companyid.ToString(),
                        Createduser = "WEB - " + currentUser.Companyid.ToString()
                    };
                    var addImage = await _productSvc.InsertImageByType(imageBO);
                    idxImg++;
                }

                //Add quan hệ sản phẩm và danh mục sản phẩm
                var lstCategoryId = !string.IsNullOrEmpty(request.Categoryid) && request.Categoryid.Contains(",") ? request.Categoryid.Split(",").ToList() : new List<string>() { request.Categoryid };
                foreach (var category in lstCategoryId)
                {
                    long categoryId = 0;
                    long.TryParse(category, out categoryId);
                    if (categoryId <= 0)
                        continue;
                    var mapCategory = await _productSvc.MapProductCategory(product.Productid, categoryId);
                }

                //Add quan hệ sản phẩm và đặc sản vùng miền
                var lstSpecialtiesId = !string.IsNullOrEmpty(request.Specialtiesid) && request.Specialtiesid.Contains(",") ? request.Specialtiesid.Split(",").ToList() : new List<string>() { request.Specialtiesid };
                foreach (var specialties in lstSpecialtiesId)
                {
                    long specialtiesId = 0;
                    long.TryParse(specialties, out specialtiesId);
                    if (specialtiesId <= 0)
                        continue;
                    var mapSpecialties = await _productSvc.MapProductSpecialties(product.Productid, specialtiesId);
                }

                //Add quan hệ sản phẩm và tiêu chuẩn sản phẩm
                var lstStandardId = !string.IsNullOrEmpty(request.Standardid) && request.Standardid.Contains(",") ? request.Standardid.Split(",").ToList() : new List<string>() { request.Standardid };
                foreach (var standard in lstStandardId)
                {
                    long standardId = 0;
                    long.TryParse(standard, out standardId);
                    if (standardId <= 0)
                        continue;
                    var mapStandard = await _productSvc.MapProductStandard(product.Productid, standardId);
                }

                //Add quan hệ sản phẩm và giá
                if(product.Ishideprice == 0)
                {
                    if (request.Productpriceitem != null && request.Productpriceitem.Any())
                    {
                        foreach (var item in request.Productpriceitem)
                        {
                            var addPrice = await _productSvc.InsertProductPrice(product.Productid, item.Fromquantily, item.Toquantily, item.FrequencyUnitId, item.PriceSell, item.Unitprice, "WEB - " + currentUser.Companyid.ToString());
                        }
                    }
                }

                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = $"{str} sản phẩm thành công" });
            }

            return ReturnData(new BaseResponseResult() { code = -1, errormessage = $"{str} sản phẩm thất bại" });
        }

        public async Task<IActionResult> DeleteProduct(long id)
        {
            if (id <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Không tìm thấy sản phẩm" });

            var currentUser = await _userHelpers.GetCookiesUserWeb();
            if (currentUser == null || currentUser.Companyid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Hệ thống đang cập nhật. Vui lòng thử lại sau ít phút" });
            var product = await _productSvc.GetProductById(id);
            if(product.Companyid != currentUser.Companyid)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Không thể xoá sản phẩm này" });
            if (product == null || product.Productid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Hệ thống đang cập nhật. Vui lòng thử lại sau ít phút" });
            var del = await _productSvc.DeleteProduct(id, "WEB - " + currentUser.Companyid.ToString());
            if (del > 0)
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = "Xóa sản phẩm thành công" });
            return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Hệ thống đang cập nhật. Vui lòng thử lại sau ít phút" });
        }

        [HttpPost]
        public async Task<IActionResult> UploadProductImg(IFormCollection collection)
        {

            if (collection == null || collection.Files == null || collection.Files.Count <= 0) 
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Can not find image." });

            var files = collection.Files;
            if(files.Count > 30)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Chỉ có thể tải lên tối đa 30 ảnh" });

            var productPath = $"/wwwroot/images/upload/tempproduct/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/";

            var imgPath = "";
            var listImgPath = new List<string>();
            var namePath = new List<dynamic>();
            foreach (var img in files)
            {
                var res = await UploadImage(img, productPath, ConfigConstants.UploadImageType.Product);

                if (res == null)
                    return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Hệ thống đang cập nhật. Vui lòng thử lại sau ít phút" });

                if (res.code == (int)HttpStatusCode.OK)
                {
                    listImgPath.Add(res.message);
                    var temp = new
                    {
                        name = img.FileName,
                        path = res.message
                    };
                    namePath.Add(temp);
                }
                else return ReturnData(new BaseResponseResult() { code = -1, errormessage = res.errormessage });


            }

            imgPath = string.Join("|", listImgPath);
            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = imgPath, data = namePath });

        }

        #endregion

        #region Method
        private int CountLeft(int Total, DealsQueryModel query)
        {
            return Total - query.PageSize * (query.PageIndex + 1);
        }

        private async Task<InfoTabDeals> InitInfo(DealsTypeCompany type, long companyid)
        {
            var query = new DealsQueryModel()
            {
                Companyid = companyid,
                Type = type,
                PageIndex = 0,
                PageSize = 7,
                Status = "0",
            };

            var listdeals = await _utilitiesSvcWeb.GetListDealsByQuery(query);

            var total = 0;
            if (listdeals.FirstOrDefault() != null)
                total = listdeals.FirstOrDefault().Totalrecord;

            return new InfoTabDeals()
            {
                Type = type,
                Total = total,
                Left = CountLeft(total, query),
                ListDeals = listdeals
            };
        }
        #endregion
    }
}
