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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace ketnoicungcau.vn.Controllers
{
    public class FAQController : BaseController {

        #region Constructors
        private readonly ILogger<FAQController> _logger;
        private readonly ICache _cache;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFAQServiceWeb _faqSvc;
        private readonly IWorkContext _workContext;

        public FAQController(ILogger<FAQController> logger,
            ICache cache,
            IFAQServiceWeb faqSvc,
            IWorkContext workContext,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _logger = logger;
            _cache = cache;
            _faqSvc = faqSvc;
            _workContext = workContext;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion
        public async Task<IActionResult> Index()
        {
            string viewName =  $"Index{(_workContext.IsMobile ? ".M" : "")}";
            var vm = new vmFAQ() 
            {
                ListFAQ = await _faqSvc.GetFAQ(0,100)
            };
            //TO DO: get data db
            return View(viewName, vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendEmailFaq(RequestInsertFaq request)
        {
            request.fullName = UtilitiesExtensions.RemoveHarmfulCharacter(request.fullName);
            request.content = UtilitiesExtensions.RemoveHarmfulCharacter(request.content);

            var res = request.Validate();
            if (res != null)
                return ReturnData(res);

            // validate captcha
            var isCheckCaptcha = ValidateHelper.ValidateRecaptcha(_httpContextAccessor.HttpContext.Request);
            if (isCheckCaptcha == false) return ReturnData(new BaseResponseResult() { code = -1, errormessage = "Vui lòng xác thực captcha" });

            var resCode = await _faqSvc.InsertFAQ(request.fullName, request.email, request.phoneNumber, request.content);
            if (resCode > 0)
                return ReturnData(new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = $"Góp ý của bạn gửi thành công" });

            return ReturnData(new BaseResponseResult() { code = -1, errormessage = $"Góp ý của bạn gửi thất bại" });

        }
    }
}