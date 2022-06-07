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
            // Lấy thông tin user
            var company = await _userHelpers.GetCookiesUserWeb();
            if (company != null && company.Companyid > 0) return Redirect("/");
            string viewName = $"Views/Login/Login{(_workContext.IsMobile ? ".M" : "")}.cshtml";
            return View(viewName);
        }

        [Route("dang-nhap-v2")]
        public async Task<IActionResult> LoginV2()
        {
            // Lấy thông tin user
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

            //kiểm tra nếu login sai 5 lần liên tục thì khóa acc
            var ssLogin = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionLogin>(ConfigConstants.SS_WRONGPASSWORD_WEB);
            //Gọi hàm check login
            var company = await _companySvc.GetCompanyByTaxId(request.Taxid);
            if (company == null || company.Companyid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Mã số thuế hoặc mật khẩu chưa đúng, vui lòng thử lại" });
            if (ssLogin != null && ssLogin.CountFailed >= 5)
            {
                //Khóa acc
                var updateCompanyLock = await _companySvc.UpdateActiveCompany(company.Companyid, 0, "Admin");
                _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_WRONGPASSWORD_WEB, "");
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Tài khoản của bạn đã bị khóa do nhập sai nhật khẩu 5 lần, vui lòng liên hệ quản trị viên để được hỗ trợ" });
            }
            if (company.Isactived == 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Tài khoản của bạn đã bị khóa, vui lòng liên hệ quản trị viên để được hỗ trợ" });

            if (WebHelpers.EncryptMD5(request.Password + company.Taxid) != company.Passwordin)
            {
                //tăng đếm lỗi lên 1 mỗi lần nhập sai
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
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Mã số thuế hoặc mật khẩu chưa đúng, vui lòng thử lại" });
            }

            // Validate captcha
            var isCheckCaptcha = ValidateHelper.ValidateRecaptcha(_httpContextAccessor.HttpContext.Request);
            if (isCheckCaptcha == false) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui lòng xác thực captcha" });

            //Nếu login thành công - lưu session và cookie đăng nhập, xóa session sai mật khẩu
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_LOGIN_WEB, company.Companyid);
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_WRONGPASSWORD_WEB, "");
            CookiesHelpers.SetCookie(_httpContextAccessor.HttpContext.Response, WebHelpers.EncryptMD5(ConfigConstants.COOKIE_LOGIN_WEB), WebHelpers.EncryptData(Newtonsoft.Json.JsonConvert.SerializeObject(company.Companyid), true), 7);

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = "Đăng nhập thành công"});
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

            //kiểm tra nếu login sai 5 lần liên tục thì khóa acc
            var ssLogin = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionLogin>(ConfigConstants.SS_WRONGPASSWORD_WEB);
            //Gọi hàm check login
            var company = await _companySvc.GetCompanyByTaxId(request.Taxid);
            if (company == null || company.Companyid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Mã số thuế hoặc mật khẩu chưa đúng, vui lòng thử lại" });
            if (ssLogin != null && ssLogin.CountFailed >= 5)
            {
                //Khóa acc
                var updateCompanyLock = await _companySvc.UpdateActiveCompany(company.Companyid, 0, "Admin");
                _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_WRONGPASSWORD_WEB, "");
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Tài khoản của bạn đã bị khóa do nhập sai mật khẩu 5 lần, vui lòng liên hệ quản trị viên để được hỗ trợ" });
            }
            if (company.Isactived == 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Tài khoản của bạn đã bị khóa, vui lòng liên hệ quản trị viên để được hỗ trợ" });
            if (WebHelpers.EncryptMD5(request.Password + company.Taxid) != company.Passwordin)
            {
                //tăng đếm lỗi lên 1 mỗi lần nhập sai
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
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Mã số thuế hoặc mật khẩu chưa đúng, vui lòng thử lại" });
            }

            //Nếu login thành công - lưu session và cookie lại
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_LOGIN_WEB, company.Companyid);
            CookiesHelpers.SetCookie(_httpContextAccessor.HttpContext.Response, WebHelpers.EncryptMD5(ConfigConstants.COOKIE_LOGIN_WEB), WebHelpers.EncryptData(Newtonsoft.Json.JsonConvert.SerializeObject(company.Companyid), true), 7);

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = "Đăng nhập thành công" });
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string Phonenumber)
        {
            if (string.IsNullOrEmpty(Phonenumber) || UtilitiesExtensions.IsPhoneNumber(Phonenumber) == false) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập đúng định dạng số điện thoại" });

            // Kiểm tra session OTP
            var ssLogin = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionForgotPasswordOTP>(ConfigConstants.SS_FORGOTPASSWORD_OTP_WEB);
            if (ssLogin != null && ssLogin.Phonenumber == Phonenumber && ssLogin.IsTimeOutOTP() == false)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Bạn vừa yêu cầu gửi OTP, vui lòng thử lại sau giây lát" });

            // Lấy thông tin user
            var company = await _companySvc.GetCompanyByPhone(Phonenumber);
            if (company == null || company.Companyid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Số điện thoại chưa đăng ký, vui lòng kiểm tra lại" });
            if (company != null && company.Isactived == 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Tài khoản của bạn đã bị khóa do nhập sai nhật khẩu 5 lần, vui lòng liên hệ quản trị viên để được hỗ trợ" });

            // Validate captcha
            var isCheckCaptcha = ValidateHelper.ValidateRecaptcha(_httpContextAccessor.HttpContext.Request);
            if (isCheckCaptcha == false) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui lòng xác thực captcha" });

            // Tạo OTP
            var otp = "2703";//WebHelpers.GenerateRandomCode();

            // Gửi OTP
            // Doing...

            // Lưu session
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

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = "Đã gửi OTP"} );
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ResendOTPForgotPassword(string Phonenumber)
        {
            if (string.IsNullOrEmpty(Phonenumber)) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập số điện thoại" });

            // Kiểm tra session OTP
            var ssLogin = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionForgotPasswordOTP>(ConfigConstants.SS_FORGOTPASSWORD_OTP_WEB);
            if (ssLogin == null) ReturnData(new BaseResponseResult() { code = -1, errormessage = "Không tìm thấy yêu cầu, vui lòng thử lại" });
            if (ssLogin.Phonenumber == Phonenumber && ssLogin.IsTimeOutOTP() == false)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Bạn vừa yêu cầu gửi OTP đăng ký tài khoản, vui lòng thử lại sau giây lát" });
            if (ssLogin.isResend)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Bạn yêu cầu gửi lại OTP quá nhiều, vui lòng thử lại sau" });
            // Tạo OTP
            var otp = "2703";//WebHelpers.GenerateRandomCode();

            // Gửi OTP
            // Doing...

            // Lưu session
            ssLogin.OTP = otp;
            ssLogin.Time = DateTime.Now;
            ssLogin.isResend = true;
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_FORGOTPASSWORD_OTP_WEB, ssLogin);

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = "Đã gửi lại mã" });
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

            // Kiểm tra session OTP
            var ssLogin = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionForgotPasswordOTP>(ConfigConstants.SS_FORGOTPASSWORD_OTP_WEB);
            if (ssLogin == null) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Yêu cầu lấy lại mật khẩu đã hết hiệu lực, vui lòng gửi lại yêu cầu" });
            if (ssLogin.IsTimeOutOTP()) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Mã OTP đã hết hạn. Vui lòng lấy OTP khác" });
            if (ssLogin.CountFailed >= 5)
            {
                _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_FORGOTPASSWORD_OTP_WEB, "");
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Mã OTP đã hết hiệu lực do nhập sai quá 5 lần. Vui lòng lấy OTP khác" });
            }
            if (request.OTP.Equals(ssLogin.OTP) == false)
            {
                ssLogin.CountFailed++;
                _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_FORGOTPASSWORD_OTP_WEB, ssLogin);
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Mã xác nhận không đúng, vui lòng thử lại" });
            }

            // Lấy thông tin doanh nghiệp
            var company = await _companySvc.GetCompanyByPhone(request.Phonenumber);
            if (company == null || company.Companyid <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Tài khoản không tồn tại." });
            if (company != null && company.Isactived == -1) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Tài khoản của bạn đã bị khóa do nhập sai nhật khẩu 5 lần, vui lòng liên hệ quản trị viên để được hỗ trợ" });

            // Cập nhật lại password
            var updatePassword = await _companySvc.UpdatePasswordCompany(company.Companyid, WebHelpers.EncryptMD5(request.Password + company.Taxid));
            if (updatePassword <= 0) return ReturnData(new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT });

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = "Đổi mật khẩu thành công" });
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
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Mã số thuế chưa được đăng ký, vui lòng nhập mã số thuế của doanh nghiệp hoặc liên hệ với tổng đài" });

            var companyByPhone = await _companySvc.GetCompanyByPhone(request.Phonenumber);
            if (companyByPhone != null && companyByPhone.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Số điện thoại đã được đăng ký, hãy nhập số điện thoại khác hoặc liên hệ với tổng đài" });

            var companyByEmail = await _companySvc.GetCompanyByEmail(request.Email);
            if (companyByEmail != null && companyByEmail.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Email đã được đăng ký, hãy nhập email khác hoặc liên hệ với tổng đài" });

            #region Move Image từ folder tạm qua folder chính

            string contentRootPath = _hostingEnvironment.ContentRootPath;

            // Move image từ folder temp qua folder chính
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
            if (resCode > 0) return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = "Gửi yêu cầu thành công" }); 
            
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
            // Lấy thông tin user
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

            // Check sđt, check email, check mã số thuế đã đăng ký tài khoản chưa
            var companyByTax = await _companySvc.GetCompanyByTaxId(request.Taxid);
            if (companyByTax != null && companyByTax.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Mã số thuế đã được đăng ký, hãy nhập mã số thuế khác hoặc liên hệ với tổng đài" });

            var companyByPhone = await _companySvc.GetCompanyByPhone(request.Phonenumber);
            if (companyByPhone != null && companyByPhone.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Số điện thoại đã được đăng ký, hãy nhập số điện thoại khác hoặc liên hệ với tổng đài" });

            var companyByEmail = await _companySvc.GetCompanyByEmail(request.Email);
            if (companyByEmail != null && companyByEmail.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Email đã được đăng ký, hãy nhập email khác hoặc liên hệ với tổng đài" });

            #region Move Image từ folder tạm qua folder chính

            string contentRootPath = _hostingEnvironment.ContentRootPath;

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
            if (isCheckCaptcha == false) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui lòng xác thực captcha" });

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
                // Render form đăng ký thành công và mời đăng nhập
                var html = await RenderViewToStringAsync("~/Views/Login/_Success.cshtml", company.Phonenumber);
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = html });
            }

            return ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Thêm mới doanh nghiệp thất bại" });
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

            // Kiểm tra session OTP
            var ssRegistry = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionRegistryOTP>(ConfigConstants.SS_REGISTRY_OTP_WEB);
            if (ssRegistry != null && ssRegistry.Company.Phonenumber == request.Phonenumber && ssRegistry.IsTimeOutOTP() == false)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Bạn vừa yêu cầu gửi OTP đăng ký tài khoản, vui lòng thử lại sau giây lát" });

            // Check sđt, check email, check mã số thuế đã đăng ký tài khoản chưa
            var companyByTax = await _companySvc.GetCompanyByTaxId(request.Taxid);
            if (companyByTax != null && companyByTax.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Mã số thuế đã được đăng ký, hãy nhập mã số thuế khác hoặc liên hệ với tổng đài" });

            var companyByPhone = await _companySvc.GetCompanyByPhone(request.Phonenumber);
            if (companyByPhone != null && companyByPhone.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Số điện thoại đã được đăng ký, hãy nhập số điện thoại khác hoặc liên hệ với tổng đài" });

            var companyByEmail = await _companySvc.GetCompanyByEmail(request.Email);
            if (companyByEmail != null && companyByEmail.Companyid > 0)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Email đã được đăng ký, hãy nhập email khác hoặc liên hệ với tổng đài" });


            #region Move Image từ folder tạm qua folder chính

            string contentRootPath = _hostingEnvironment.ContentRootPath;

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
            if (isCheckCaptcha == false) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui lòng xác thực captcha" });

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

            // Tạo OTP
            var otp = "2703";//WebHelpers.GenerateRandomCode();

            // Gửi OTP
            // Doing...

            // Lưu session
            var ssOTP = new SessionRegistryOTP()
            {
                Time = DateTime.Now,
                Company = company,
                OTP = otp,
                isResend = false
            };
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_REGISTRY_OTP_WEB, ssOTP);

            // Render form xác nhận OTP
            var html = await RenderViewToStringAsync("~/Views/Login/_SubmitOTP.cshtml", company.Phonenumber);

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = html });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public IActionResult ResendOTPRegistry(string Phonenumber) 
        {
            if (string.IsNullOrEmpty(Phonenumber)) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập số điện thoại" });

            // Kiểm tra session OTP
            var ssRegistry = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionRegistryOTP>(ConfigConstants.SS_REGISTRY_OTP_WEB);
            if (ssRegistry == null) ReturnData(new BaseResponseResult() { code = -1, errormessage = "Không tìm thấy yêu cầu, vui lòng thử lại" });
            if (ssRegistry.Company.Phonenumber == Phonenumber && ssRegistry.IsTimeOutOTP() == false)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Bạn vừa yêu cầu gửi OTP đăng ký tài khoản, vui lòng thử lại sau giây lát" });
            if (ssRegistry.isResend)
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Bạn yêu cầu gửi lại OTP quá nhiều, vui lòng thử lại sau" });

            // Tạo OTP
            var otp = "2703";//WebHelpers.GenerateRandomCode();

            // Gửi OTP
            // Doing...

            // Lưu session
            ssRegistry.OTP = otp;
            ssRegistry.Time = DateTime.Now;
            ssRegistry.isResend = true;
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_REGISTRY_OTP_WEB, ssRegistry);

            return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = "Đã gửi lại mã" });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitOTPRegistry(string OTP)
        {
            // Validate
            if (string.IsNullOrEmpty(OTP)) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mã OTP" });

            // Kiểm tra session OTP
            var ssRegistry = _httpContextAccessor.HttpContext.Session.GetObjectFromJson<SessionRegistryOTP>(ConfigConstants.SS_REGISTRY_OTP_WEB);
            if (ssRegistry == null) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui lòng lấy mã OTP để lấy xác thực thông tin" });
            if (ssRegistry.IsTimeOutOTP()) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Mã OTP đã hết hạn. Vui lòng lấy OTP khác để đăng nhập." });
            if (ssRegistry.CountFailed >= 5)
            {
                _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_REGISTRY_OTP_WEB, "");
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Bạn đã nhập sai OTP 5 lần, vui lòng thử lại sau." });
            }
            if (OTP.Equals(ssRegistry.OTP) == false)
            {
                ssRegistry.CountFailed++;
                _httpContextAccessor.HttpContext.Session.SetObjectAsJson(ConfigConstants.SS_REGISTRY_OTP_WEB, ssRegistry);
                return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Mã OTP chưa đúng, vui lòng nhập lại" });
            }

            // Insert company
            var resCode = await _companySvc.InsertCompany(ssRegistry.Company);

            if (resCode > 0)
            {
                // Render form đăng ký thành công và mời đăng nhập
                var html = await RenderViewToStringAsync("~/Views/Login/_Success.cshtml", ssRegistry.Company.Phonenumber);
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, data = html });
            }

            return ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Thêm mới doanh nghiệp thất bại" });
        }
    }
}