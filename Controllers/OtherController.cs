using System.Threading.Tasks;
using ketnoicungcau.business.Framework;
using ketnoicungcau.business.Helpers;
using ketnoicungcau.business.Model;
using ketnoicungcau.business.ServiceWeb.Interface;
using ketnoicungcau.vn.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ketnoicungcau.vn.Controllers
{
    public class OtherController : BaseController
    {
        #region Variable
        private readonly IOptions<AppSettings> _settings;
        private readonly IWorkContext _workContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISystemService _systemSvc;
        private readonly INewsServiceWeb _newsSvc;

        #endregion

        #region Constructor
        public OtherController(
            IOptions<AppSettings> settings,
            IWorkContext workContext,
            IHttpContextAccessor httpContextAccessor,
            ISystemService systemSvc,
            INewsServiceWeb newsSvc,
            IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _settings = settings;
            _workContext = workContext;
            _newsSvc = newsSvc;
            _httpContextAccessor = httpContextAccessor;
            _systemSvc = systemSvc;
        }
        #endregion

        #region Action
        [Route("gioi-thieu")]
        public async Task<IActionResult> IntroAsync()
        {
            return View($"Index{(_workContext.IsMobile ? ".M" : "")}", await GetViewModel(50));
        }

        [Route("huong-dan-dang-ky")]
        public async Task<IActionResult> InstructionAsync()
        {
            return View($"Index{(_workContext.IsMobile ? ".M" : "")}", await GetViewModel(54));
        }

        [Route("chinh-sach")]
        public async Task<IActionResult> PolicyAsync()
        {
            return View($"Index{(_workContext.IsMobile ? ".M" : "")}", await GetViewModel(53));
        }

        [Route("quy-dinh")]
        public async Task<IActionResult> RegulationAsync()
        {
            return View($"Index{(_workContext.IsMobile ? ".M" : "")}", await GetViewModel(51));
        }
        #endregion

        #region Method
        public async Task<vmNewsDetail> GetViewModel(int htmlId)
        {
            var news = new News();
            news = await _newsSvc.GetNewsById(htmlId);
            var vm = new vmNewsDetail()
            {
                News = news,
            };
            return vm;
        }
        #endregion
    }
}
