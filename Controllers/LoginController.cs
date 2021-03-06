using ketnoicungcau.business.Caching;
using ketnoicungcau.business.ServiceWeb.Interface;
using ketnoicungcau.vn.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ketnoicungcau.business.Framework;
using ketnoicungcau.vn.Models;
using ketnoicungcau.business;
using System.Net;
using ketnoicungcau.business.Helpers;
using Microsoft.AspNetCore.Http;
using ketnoicungcau.business.Service.Interface;
using ketnoicungcau.business.Helpers.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using static ketnoicungcau.vn.Models.ApiModel;
using System;
using ketnoicungcau.business.Model;
using System.IO;

namespace ketnoicungcau.vn.Controllers
{
    public class LoginController : BaseController {

        #region Constructors
        private readonly ILogger<FAQController> _logger;
        private readonly ICache _cache;
        private readonly IUserHelpers _userHelpers;
        private readonly ICompanyService _companySvc;
        private readonly IUtilitiesServiceWeb _utilSvcWeb;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IWorkContext _workContext;

        public LoginController(ILogger<FAQController> logger,
            ICache cache,
            IHttpContextAccessor httpContextAccessor, 
            IUserHelpers userHelpers,
            ICompanyService companySvc,
            IUtilitiesServiceWeb utilSvcWeb,
            IWebHostEnvironment hostingEnvironment,
            IWorkContext workContext,
            IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _logger = logger;
            _cache = cache;
            _userHelpers = userHelpers;
            _companySvc = companySvc;
            _utilSvcWeb = utilSvcWeb;
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _workContext = workContext;
        }
        #endregion

        [Route("dang-nhap")]
        public async Task<IActionResult> Login()
        {
            // L???y th??ng tin user
            var company = await _userHelpers.GetCookiesUserWeb();
            if (company != null && company.Companyid > 0) return Redirect("/");
            string viewName = $"Views/Login/Login{(_workContext.IsMobile ? ".M" : "")}.cshtml";
            return View(viewName);
        }

        [Route("dang-nhap-v2")]
        public async Task<IActionResult> LoginV2()
        {
            // L???y th??ng tin user
            var company = await _userHelpers.GetCookiesUserWeb();
            if (company != null && company.Companyid > 0) return Redirect("/");
            string viewName = $"Views/Login/LoginV2{(_workContext.IsMobile ? ".M" : "")}.cshtml";
            return View(viewName);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitLogin(RequestLogin request)
        { 
            // Validate
            var res = request.Validate();
            if (res != null)
                return ReturnData(res);

            //ki???m tra n???u login sai 5 l???n li??n t???c th?? kh??a acc
            var ssLogin = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionLogin>(ConfigConstants.SS_WRONGPASSWORD_WEB);
            //G???i h??m check login
            var company = await _companySvc.GetCompanyByTaxId(request.Taxid);
            if (company == null || company.Companyid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "M?? s??? thu??? ho???c m???t kh???u ch??a ????ng, vui l??ng th??? l???i" });
            if (ssLogin != null && ssLogin.CountFailed >= 5)
            {
                //Kh??a acc
                var updateCompanyLock = await _companySvc.UpdateActiveCompany(company.Companyid, 0, "Admin");
                _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_WRONGPASSWORD_WEB, "");
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "T??i kho???n c???a b???n ???? b??? kh??a do nh???p sai nh???t kh???u 5 l???n, vui l??ng li??n h??? qu???n tr??? vi??n ????? ???????c h??? tr???" });
            }
            if (company.Isactived == 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "T??i kho???n c???a b???n ???? b??? kh??a, vui l??ng li??n h??? qu???n tr??? vi??n ????? ???????c h??? tr???" });

            if (WebHelpers.EncryptMD5(request.Password + company.Taxid) != company.Passwordin)
            {
                //t??ng ?????m l???i l??n 1 m???i l???n nh???p sai
                if (ssLogin == null || ssLogin.Companyid <= 0)
                {
                    ssLogin = new SessionLogin()
                    {
                        Companyid = company.Companyid,
                        Taxid = company.Taxid,
                        CountFailed = 1,
                        Time = DateTime.Now
                    };
                }
                else
                {
                    ssLogin.CountFailed += 1;
                }
                _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_WRONGPASSWORD_WEB, ssLogin);
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "M?? s??? thu??? ho???c m???t kh???u ch??a ????ng, vui l??ng th??? l???i" });
            }

            // Validate captcha
            var isCheckCaptcha = ValidateHelper.ValidateRecaptcha(_httpContextAccessor.HttpContext.Request);
            if (isCheckCaptcha == false) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui l??ng x??c th???c captcha" });

            //N???u login th??nh c??ng - l??u session v?? cookie ????ng nh???p, x??a session sai m???t kh???u
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_LOGIN_WEB, company.Companyid);
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_WRONGPASSWORD_WEB, "");
            CookiesHelpers.SetCookie(_httpContextAccessor.HttpContext.Response, WebHelpers.EncryptMD5(ConfigConstants.COOKIE_LOGIN_WEB), WebHelpers.EncryptData(Newtonsoft.Json.JsonConvert.SerializeObject(company.Companyid), true), 7);

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = "????ng nh???p th??nh c??ng"});
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitLoginV2(RequestLogin request)
        {
            // Validate
            var res = request.Validate();
            if (res != null)
                return ReturnData(res);

            //ki???m tra n???u login sai 5 l???n li??n t???c th?? kh??a acc
            var ssLogin = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionLogin>(ConfigConstants.SS_WRONGPASSWORD_WEB);
            //G???i h??m check login
            var company = await _companySvc.GetCompanyByTaxId(request.Taxid);
            if (company == null || company.Companyid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "M?? s??? thu??? ho???c m???t kh???u ch??a ????ng, vui l??ng th??? l???i" });
            if (ssLogin != null && ssLogin.CountFailed >= 5)
            {
                //Kh??a acc
                var updateCompanyLock = await _companySvc.UpdateActiveCompany(company.Companyid, 0, "Admin");
                _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_WRONGPASSWORD_WEB, "");
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "T??i kho???n c???a b???n ???? b??? kh??a do nh???p sai m???t kh???u 5 l???n, vui l??ng li??n h??? qu???n tr??? vi??n ????? ???????c h??? tr???" });
            }
            if (company.Isactived == 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "T??i kho???n c???a b???n ???? b??? kh??a, vui l??ng li??n h??? qu???n tr??? vi??n ????? ???????c h??? tr???" });
            if (WebHelpers.EncryptMD5(request.Password + company.Taxid) != company.Passwordin)
            {
                //t??ng ?????m l???i l??n 1 m???i l???n nh???p sai
                if (ssLogin == null || ssLogin.Companyid <= 0)
                {
                    ssLogin = new SessionLogin()
                    {
                        Companyid = company.Companyid,
                        Taxid = company.Taxid,
                        CountFailed = 1,
                        Time = DateTime.Now
                    };
                }
                else
                {
                    ssLogin.CountFailed += 1;
                }
                _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_WRONGPASSWORD_WEB, ssLogin);
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "M?? s??? thu??? ho???c m???t kh???u ch??a ????ng, vui l??ng th??? l???i" });
            }

            //N???u login th??nh c??ng - l??u session v?? cookie l???i
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_LOGIN_WEB, company.Companyid);
            CookiesHelpers.SetCookie(_httpContextAccessor.HttpContext.Response, WebHelpers.EncryptMD5(ConfigConstants.COOKIE_LOGIN_WEB), WebHelpers.EncryptData(Newtonsoft.Json.JsonConvert.SerializeObject(company.Companyid), true), 7);

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = "????ng nh???p th??nh c??ng" });
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string Phonenumber)
        {
            if (string.IsNullOrEmpty(Phonenumber) || UtilitiesExtensions.IsPhoneNumber(Phonenumber) == false) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui l??ng nh???p ????ng ?????nh d???ng s??? ??i???n tho???i" });

            // Ki???m tra session OTP
            var ssLogin = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionForgotPasswordOTP>(ConfigConstants.SS_FORGOTPASSWORD_OTP_WEB);
            if (ssLogin != null && ssLogin.Phonenumber == Phonenumber && ssLogin.IsTimeOutOTP() == false)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "B???n v???a y??u c???u g???i OTP, vui l??ng th??? l???i sau gi??y l??t" });

            // L???y th??ng tin user
            var company = await _companySvc.GetCompanyByPhone(Phonenumber);
            if (company == null || company.Companyid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "S??? ??i???n tho???i ch??a ????ng k??, vui l??ng ki???m tra l???i" });
            if (company != null && company.Isactived == 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "T??i kho???n c???a b???n ???? b??? kh??a do nh???p sai nh???t kh???u 5 l???n, vui l??ng li??n h??? qu???n tr??? vi??n ????? ???????c h??? tr???" });

            // Validate captcha
            var isCheckCaptcha = ValidateHelper.ValidateRecaptcha(_httpContextAccessor.HttpContext.Request);
            if (isCheckCaptcha == false) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui l??ng x??c th???c captcha" });

            // T???o OTP
            var otp = "2703";//WebHelpers.GenerateRandomCode();

            // G???i OTP
            // Doing...

            // L??u session
            var ssOTP = new SessionForgotPasswordOTP()
            {
                Time = DateTime.Now,
                Companyid = company.Companyid,
                Taxid = company.Taxid,
                Phonenumber = company.Phonenumber,
                OTP = otp,
                isResend = false
            };
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_FORGOTPASSWORD_OTP_WEB, ssOTP);

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = "???? g???i OTP"} );
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ResendOTPForgotPassword(string Phonenumber)
        {
            if (string.IsNullOrEmpty(Phonenumber)) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui l??ng nh???p s??? ??i???n tho???i" });

            // Ki???m tra session OTP
            var ssLogin = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionForgotPasswordOTP>(ConfigConstants.SS_FORGOTPASSWORD_OTP_WEB);
            if (ssLogin == null) ReturnData(new BaseResponseResult() { code = -1, errormessage = "Kh??ng t??m th???y y??u c???u, vui l??ng th??? l???i" });
            if (ssLogin.Phonenumber == Phonenumber && ssLogin.IsTimeOutOTP() == false)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "B???n v???a y??u c???u g???i OTP ????ng k?? t??i kho???n, vui l??ng th??? l???i sau gi??y l??t" });
            if (ssLogin.isResend)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "B???n y??u c???u g???i l???i OTP qu?? nhi???u, vui l??ng th??? l???i sau" });
            // T???o OTP
            var otp = "2703";//WebHelpers.GenerateRandomCode();

            // G???i OTP
            // Doing...

            // L??u session
            ssLogin.OTP = otp;
            ssLogin.Time = DateTime.Now;
            ssLogin.isResend = true;
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_FORGOTPASSWORD_OTP_WEB, ssLogin);

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = "???? g???i l???i m??" });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitForgotPassword(RequestForgotPassword request)
        {
            // Validate
            var res = request.Validate();
            if (res != null)
                return ReturnData(res);

            // Ki???m tra session OTP
            var ssLogin = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionForgotPasswordOTP>(ConfigConstants.SS_FORGOTPASSWORD_OTP_WEB);
            if (ssLogin == null) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Y??u c???u l???y l???i m???t kh???u ???? h???t hi???u l???c, vui l??ng g???i l???i y??u c???u" });
            if (ssLogin.IsTimeOutOTP()) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "M?? OTP ???? h???t h???n. Vui l??ng l???y OTP kh??c" });
            if (ssLogin.CountFailed >= 5)
            {
                _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_FORGOTPASSWORD_OTP_WEB, "");
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "M?? OTP ???? h???t hi???u l???c do nh???p sai qu?? 5 l???n. Vui l??ng l???y OTP kh??c" });
            }
            if (request.OTP.Equals(ssLogin.OTP) == false)
            {
                ssLogin.CountFailed++;
                _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_FORGOTPASSWORD_OTP_WEB, ssLogin);
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "M?? x??c nh???n kh??ng ????ng, vui l??ng th??? l???i" });
            }

            // L???y th??ng tin doanh nghi???p
            var company = await _companySvc.GetCompanyByPhone(request.Phonenumber);
            if (company == null || company.Companyid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "T??i kho???n kh??ng t???n t???i." });
            if (company != null && company.Isactived == -1) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "T??i kho???n c???a b???n ???? b??? kh??a do nh???p sai nh???t kh???u 5 l???n, vui l??ng li??n h??? qu???n tr??? vi??n ????? ???????c h??? tr???" });

            // C???p nh???t l???i password
            var updatePassword = await _companySvc.UpdatePasswordCompany(company.Companyid, WebHelpers.EncryptMD5(request.Password + company.Taxid));
            if (updatePassword <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT });

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = "?????i m???t kh???u th??nh c??ng" });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitRenewUserByTax(RequestInsertRenewUser request)
        {
            // Utilities extensipn
            request.Companyname = UtilitiesExtensions.RemoveHarmfulCharacter(request.Companyname);
            request.Taxid = UtilitiesExtensions.RemoveHarmfulCharacter(request.Taxid);
            request.Note = UtilitiesExtensions.RemoveHarmfulCharacter(request.Note);

            // Validate
            var res = request.Validate();
            if (res != null)
                return ReturnData(res);

            var checkImage = CheckFormatImage(request.Licenseimage, checkExist: true);
            if (checkImage.code == -1) return ReturnData(new BaseResponseResult() { code = -1, errormessage = checkImage.errormessage });

            checkImage = CheckFormatImage(request.Confirmimage, checkExist: true);
            if (checkImage.code == -1) return ReturnData(new BaseResponseResult() { code = -1, errormessage = checkImage.errormessage });

            var companyByTax = await _companySvc.GetCompanyByTaxId(request.Taxid);
            if (companyByTax == null || companyByTax.Companyid <= 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "M?? s??? thu??? ch??a ???????c ????ng k??, vui l??ng nh???p m?? s??? thu??? c???a doanh nghi???p ho???c li??n h??? v???i t???ng ????i" });

            var companyByPhone = await _companySvc.GetCompanyByPhone(request.Phonenumber);
            if (companyByPhone != null && companyByPhone.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "S??? ??i???n tho???i ???? ???????c ????ng k??, h??y nh???p s??? ??i???n tho???i kh??c ho???c li??n h??? v???i t???ng ????i" });

            var companyByEmail = await _companySvc.GetCompanyByEmail(request.Email);
            if (companyByEmail != null && companyByEmail.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Email ???? ???????c ????ng k??, h??y nh???p email kh??c ho???c li??n h??? v???i t???ng ????i" });

            #region Move Image t??? folder t???m qua folder ch??nh

            string contentRootPath = _hostingEnvironment.ContentRootPath;

            // Move image t??? folder temp qua folder ch??nh
            var srcPathLogo = contentRootPath + "/wwwroot" + request.Confirmimage;
            var desPathLogo = srcPathLogo.Replace("tempcompanyconfirmimage", "companyconfirmimage");

            if (srcPathLogo != desPathLogo)
            {
                try
                {
                    var folderPath = $"/wwwroot/images/upload/companyconfirmimage/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/";

                    if (!Directory.Exists(contentRootPath + folderPath))
                        Directory.CreateDirectory(contentRootPath + folderPath);
                    System.IO.File.Move(srcPathLogo, desPathLogo);
                    if (System.IO.File.Exists(srcPathLogo))
                        System.IO.File.Delete(srcPathLogo);
                }
                catch(Exception e)
                {
                    _logger.LogError(e.Message);
                }
                
            }

            var srcPathGpkd = contentRootPath + "/wwwroot" + request.Licenseimage;
            var desPathGpkd = srcPathGpkd.Replace("tempcompanybusinesslicense", "companybusinesslicense");

            if (srcPathGpkd != desPathGpkd)
            {
                try
                {
                    var folderPath = $"/wwwroot/images/upload/companybusinesslicense/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/";

                    if (!Directory.Exists(contentRootPath + folderPath))
                        Directory.CreateDirectory(contentRootPath + folderPath);
                    System.IO.File.Move(srcPathGpkd, desPathGpkd);
                    if (System.IO.File.Exists(srcPathGpkd))
                        System.IO.File.Delete(srcPathGpkd);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }

               
            }

            #endregion

            var userRenew = new UserRenew()
            {
                Companyname = request.Companyname,
                Taxcode = request.Taxid,
                Phonenumber = request.Phonenumber,
                Email = request.Email,
                Note = request.Note,
                Licenseimage = request.Licenseimage.Replace("temp", ""),
                Confirmimage = request.Confirmimage.Replace("temp", ""),
                Status = 0,
                Isactived = 1,
                Activeddate = DateTime.Now,
                Activeduser = "Admin",
                Createduser = "User Web",
            };       

            // Insert
            var resCode = await _utilSvcWeb.InsertUserRenew(userRenew);
            if (resCode > 0) return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = "G???i y??u c???u th??nh c??ng" }); 
            
            return ReturnData(new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT });
        }

        [Route("logout")]
        public ActionResult Logout(bool requireLogin = false)
        {
            CookiesHelpers.RemoveCookies(_httpContextAccessor.HttpContext.Response, WebHelpers.EncryptMD5(ConfigConstants.COOKIE_LOGIN_WEB));
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_LOGIN_WEB, null);
            if (requireLogin) return Redirect("/dang-nhap");
            return Redirect("/");
        }

        [Route("dang-ky")]
        public async Task<IActionResult> Registry()
        {
            // L???y th??ng tin user
            var company = await _userHelpers.GetCookiesUserWeb();
            if (company != null && company.Companyid > 0) return Redirect("/");

            string viewName = $"Views/Login/Registry{(_workContext.IsMobile ? ".M" : "")}.cshtml";

            var vm = new vmRegistry()
            {
                ListCate = await _utilSvcWeb.GetAllCategory(),
                ListProvince = await _utilSvcWeb.GetAllProvince(),
            };

            return View(viewName, vm);
        }

        [HttpPost]
        public async Task<IActionResult> UploadLogo(IFormCollection collection)
        {
            if (collection == null || collection.Files == null || collection.Files.Count <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Can not find image." });

            var files = collection.Files;

            var companyPath = $"/wwwroot/images/upload/tempcompanylogo/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/";

            var image = await UploadImage(files[0], companyPath, ConfigConstants.UploadImageType.CompanyLogo);
            if (image != null && image.code == (int)HttpStatusCode.OK)
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = image.message });
            else
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = image.errormessage });
        }

        [HttpPost]
        public async Task<IActionResult> UploadBusinessLicense(IFormCollection collection)
        {
            if (collection == null || collection.Files == null || collection.Files.Count <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Can not find image." });

            var files = collection.Files;

            var companyPath = $"/wwwroot/images/upload/tempcompanybusinesslicense/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/";

            var image = await UploadImage(files[0], companyPath, ConfigConstants.UploadImageType.CompanyBusinessLicense);
            if (image != null && image.code == (int)HttpStatusCode.OK)
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = image.message });
            else return ReturnData(new BaseResponseResult() { code = -1, errormessage = image.errormessage });
        }

        [HttpPost]
        public async Task<IActionResult> UploadConfirmImage(IFormCollection collection)
        {
            if (collection == null || collection.Files == null || collection.Files.Count <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Can not find image." });

            var files = collection.Files;

            var companyPath = $"/wwwroot/images/upload/tempcompanyconfirmimage/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/";

            var image = await UploadImage(files[0], companyPath, ConfigConstants.UploadImageType.CompanyBusinessLicense);
            if (image != null && image.code == (int)HttpStatusCode.OK)
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = image.message });
            else return ReturnData(new BaseResponseResult() { code = -1, errormessage = image.errormessage });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitCreateCompanyNoOTP(RequestUpdateCompany request)
        {
            // UtilitiesExtensions
            request.Companyname = UtilitiesExtensions.RemoveHarmfulCharacter(request.Companyname);
            request.Taxid = UtilitiesExtensions.RemoveHarmfulCharacter(request.Taxid);
            request.Address = UtilitiesExtensions.RemoveHarmfulCharacter(request.Address);
            request.Saleproduct = UtilitiesExtensions.RemoveHarmfulCharacter(request.Saleproduct);
            request.Representname = UtilitiesExtensions.RemoveHarmfulCharacter(request.Representname);
            request.Representposition = UtilitiesExtensions.RemoveHarmfulCharacter(request.Representposition);
            request.Zaloid = UtilitiesExtensions.RemoveHarmfulCharacter(request.Zaloid);

            // Validate
            var res = request.Validate(1);
            if (res != null)
                return ReturnData(res);

            var checkImage = CheckFormatImage(request.Logosrc, checkExist: true);
            if (checkImage.code == -1) return ReturnData(new BaseResponseResult() { code = -1, errormessage = checkImage.errormessage });

            checkImage = CheckFormatImage(request.Gpkdsrc, checkExist: true);
            if (checkImage.code == -1) return ReturnData(new BaseResponseResult() { code = -1, errormessage = checkImage.errormessage });

            // Check s??t, check email, check m?? s??? thu??? ???? ????ng k?? t??i kho???n ch??a
            var companyByTax = await _companySvc.GetCompanyByTaxId(request.Taxid);
            if (companyByTax != null && companyByTax.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "M?? s??? thu??? ???? ???????c ????ng k??, h??y nh???p m?? s??? thu??? kh??c ho???c li??n h??? v???i t???ng ????i" });

            var companyByPhone = await _companySvc.GetCompanyByPhone(request.Phonenumber);
            if (companyByPhone != null && companyByPhone.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "S??? ??i???n tho???i ???? ???????c ????ng k??, h??y nh???p s??? ??i???n tho???i kh??c ho???c li??n h??? v???i t???ng ????i" });

            var companyByEmail = await _companySvc.GetCompanyByEmail(request.Email);
            if (companyByEmail != null && companyByEmail.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Email ???? ???????c ????ng k??, h??y nh???p email kh??c ho???c li??n h??? v???i t???ng ????i" });

            #region Move Image t??? folder t???m qua folder ch??nh

            string contentRootPath = _hostingEnvironment.ContentRootPath;

            // Move image t??? folder temp qua folder ch??nh
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
                    if (System.IO.File.Exists(srcPathGpkd))
                        System.IO.File.Delete(srcPathGpkd);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }

            }

            #endregion

            // Validate captcha
            var isCheckCaptcha = ValidateHelper.ValidateRecaptcha(_httpContextAccessor.HttpContext.Request);
            if (isCheckCaptcha == false) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui l??ng x??c th???c captcha" });

            var company = new Company
            {
                Companyname = request.Companyname,
                Taxid = request.Taxid,
                Address = request.Address,
                Phonenumber = request.Phonenumber,
                Logosrc = request.Logosrc.Replace("temp", ""),
                Gpkdsrc = request.Gpkdsrc.Replace("temp", ""),
                Email = request.Email,
                Zaloid = request.Zaloid,
                Fburl = request.Fburl,
                Weburl = request.Weburl,
                Saleproduct = request.Saleproduct,
                Listcategoryid = request.Listcategoryid,
                Provinceid = request.Provinceid,
                Representname = request.Representname,
                Representposition = request.Representposition,
                Description = request.Description,
                Isactived = 1
            };
            company.Createduser = "web registry";
            company.Passwordin = WebHelpers.EncryptMD5(request.Password + request.Taxid);

            // Insert company
            var resCode = await _companySvc.InsertCompany(company);

            if (resCode > 0)
            {
                // Render form ????ng k?? th??nh c??ng v?? m???i ????ng nh???p
                var html = await RenderViewToStringAsync("~/Views/Login/_Success.cshtml", company.Phonenumber);
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = html });
            }

            return ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Th??m m???i doanh nghi???p th???t b???i" });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitCreateCompany(RequestUpdateCompany request)
        {
            // UtilitiesExtensions
            request.Companyname = UtilitiesExtensions.RemoveHarmfulCharacter(request.Companyname);
            request.Taxid = UtilitiesExtensions.RemoveHarmfulCharacter(request.Taxid);
            request.Address = UtilitiesExtensions.RemoveHarmfulCharacter(request.Address);
            request.Saleproduct = UtilitiesExtensions.RemoveHarmfulCharacter(request.Saleproduct);
            request.Representname = UtilitiesExtensions.RemoveHarmfulCharacter(request.Representname);
            request.Representposition = UtilitiesExtensions.RemoveHarmfulCharacter(request.Representposition);
            request.Zaloid = UtilitiesExtensions.RemoveHarmfulCharacter(request.Zaloid);

            // Validate
            var res = request.Validate(1);
            if (res != null)
                return ReturnData(res);

            var checkImage = CheckFormatImage(request.Logosrc, checkExist: true);
            if (checkImage.code == -1) return ReturnData(new BaseResponseResult() { code = -1, errormessage = checkImage.errormessage });

            checkImage = CheckFormatImage(request.Gpkdsrc, checkExist: true);
            if (checkImage.code == -1) return ReturnData(new BaseResponseResult() { code = -1, errormessage = checkImage.errormessage });

            // Ki???m tra session OTP
            var ssRegistry = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionRegistryOTP>(ConfigConstants.SS_REGISTRY_OTP_WEB);
            if (ssRegistry != null && ssRegistry.Company.Phonenumber == request.Phonenumber && ssRegistry.IsTimeOutOTP() == false)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "B???n v???a y??u c???u g???i OTP ????ng k?? t??i kho???n, vui l??ng th??? l???i sau gi??y l??t" });

            // Check s??t, check email, check m?? s??? thu??? ???? ????ng k?? t??i kho???n ch??a
            var companyByTax = await _companySvc.GetCompanyByTaxId(request.Taxid);
            if (companyByTax != null && companyByTax.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "M?? s??? thu??? ???? ???????c ????ng k??, h??y nh???p m?? s??? thu??? kh??c ho???c li??n h??? v???i t???ng ????i" });

            var companyByPhone = await _companySvc.GetCompanyByPhone(request.Phonenumber);
            if (companyByPhone != null && companyByPhone.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "S??? ??i???n tho???i ???? ???????c ????ng k??, h??y nh???p s??? ??i???n tho???i kh??c ho???c li??n h??? v???i t???ng ????i" });

            var companyByEmail = await _companySvc.GetCompanyByEmail(request.Email);
            if (companyByEmail != null && companyByEmail.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Email ???? ???????c ????ng k??, h??y nh???p email kh??c ho???c li??n h??? v???i t???ng ????i" });


            #region Move Image t??? folder t???m qua folder ch??nh

            string contentRootPath = _hostingEnvironment.ContentRootPath;

            // Move image t??? folder temp qua folder ch??nh
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
                    if (System.IO.File.Exists(srcPathGpkd))
                        System.IO.File.Delete(srcPathGpkd);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
                
            }

            #endregion

            // Validate captcha
            var isCheckCaptcha = ValidateHelper.ValidateRecaptcha(_httpContextAccessor.HttpContext.Request);
            if (isCheckCaptcha == false) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui l??ng x??c th???c captcha" });

            var company = new Company
            {
                Companyname = request.Companyname,
                Taxid = request.Taxid,
                Address = request.Address,
                Phonenumber = request.Phonenumber,
                Logosrc = request.Logosrc.Replace("temp",""),
                Gpkdsrc = request.Gpkdsrc.Replace("temp", ""),
                Email = request.Email,
                Zaloid = request.Zaloid,
                Fburl = request.Fburl,
                Weburl = request.Weburl,
                Saleproduct = request.Saleproduct,
                Listcategoryid = request.Listcategoryid,
                Provinceid = request.Provinceid,
                Representname = request.Representname,
                Representposition = request.Representposition,
                Description = request.Description,
                Isactived = 1
            };
            company.Createduser = "web registry";
            company.Passwordin = WebHelpers.EncryptMD5(request.Password + request.Taxid);

            // T???o OTP
            var otp = "2703";//WebHelpers.GenerateRandomCode();

            // G???i OTP
            // Doing...

            // L??u session
            var ssOTP = new SessionRegistryOTP()
            {
                Time = DateTime.Now,
                Company = company,
                OTP = otp,
                isResend = false
            };
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_REGISTRY_OTP_WEB, ssOTP);

            // Render form x??c nh???n OTP
            var html = await RenderViewToStringAsync("~/Views/Login/_SubmitOTP.cshtml", company.Phonenumber);

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = html });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ResendOTPRegistry(string Phonenumber) 
        {
            if (string.IsNullOrEmpty(Phonenumber)) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui l??ng nh???p s??? ??i???n tho???i" });

            // Ki???m tra session OTP
            var ssRegistry = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionRegistryOTP>(ConfigConstants.SS_REGISTRY_OTP_WEB);
            if (ssRegistry == null) ReturnData(new BaseResponseResult() { code = -1, errormessage = "Kh??ng t??m th???y y??u c???u, vui l??ng th??? l???i" });
            if (ssRegistry.Company.Phonenumber == Phonenumber && ssRegistry.IsTimeOutOTP() == false)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "B???n v???a y??u c???u g???i OTP ????ng k?? t??i kho???n, vui l??ng th??? l???i sau gi??y l??t" });
            if (ssRegistry.isResend)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "B???n y??u c???u g???i l???i OTP qu?? nhi???u, vui l??ng th??? l???i sau" });

            // T???o OTP
            var otp = "2703";//WebHelpers.GenerateRandomCode();

            // G???i OTP
            // Doing...

            // L??u session
            ssRegistry.OTP = otp;
            ssRegistry.Time = DateTime.Now;
            ssRegistry.isResend = true;
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_REGISTRY_OTP_WEB, ssRegistry);

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = "???? g???i l???i m??" });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitOTPRegistry(string OTP)
        {
            // Validate
            if (string.IsNullOrEmpty(OTP)) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui l??ng nh???p m?? OTP" });

            // Ki???m tra session OTP
            var ssRegistry = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionRegistryOTP>(ConfigConstants.SS_REGISTRY_OTP_WEB);
            if (ssRegistry == null) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui l??ng l???y m?? OTP ????? l???y x??c th???c th??ng tin" });
            if (ssRegistry.IsTimeOutOTP()) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "M?? OTP ???? h???t h???n. Vui l??ng l???y OTP kh??c ????? ????ng nh???p." });
            if (ssRegistry.CountFailed >= 5)
            {
                _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_REGISTRY_OTP_WEB, "");
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "B???n ???? nh???p sai OTP 5 l???n, vui l??ng th??? l???i sau." });
            }
            if (OTP.Equals(ssRegistry.OTP) == false)
            {
                ssRegistry.CountFailed++;
                _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_REGISTRY_OTP_WEB, ssRegistry);
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "M?? OTP ch??a ????ng, vui l??ng nh???p l???i" });
            }

            // Insert company
            var resCode = await _companySvc.InsertCompany(ssRegistry.Company);

            if (resCode > 0)
            {
                // Render form ????ng k?? th??nh c??ng v?? m???i ????ng nh???p
                var html = await RenderViewToStringAsync("~/Views/Login/_Success.cshtml", ssRegistry.Company.Phonenumber);
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = html });
            }

            return ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Th??m m???i doanh nghi???p th???t b???i" });
        }
    }
}