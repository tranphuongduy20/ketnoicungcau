using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ketnoicungcau.business.ServiceWeb.Interface;
using ketnoicungcau.business.Extensions;
using ketnoicungcau.business.Helpers;
using ketnoicungcau.vn.ViewModels;
using ketnoicungcau.business.Model;
using ketnoicungcau.business;
using System.Threading.Tasks;

namespace ketnoicungcau.vn.ViewComponents
{
    public class BannerViewComponent : ViewComponent
    {
        private readonly IBannerServiceWeb _bannerService;
        private readonly IOptions<AppSettings> _settings;

        public BannerViewComponent(IBannerServiceWeb bannerService, IOptions<AppSettings> settings)
        {
            _bannerService = bannerService;
            _settings = settings;
        }

        public async Task<IViewComponentResult> InvokeAsync(ReqBanner reqBanner)
        {
            string _viewName = $"{reqBanner.viewName}{(reqBanner.useViewMobile ? ".M" : "")}";
            var model = new vmBannerComponent()
            {
                IsRunLive = reqBanner.IsRunLive,
                banners = await _bannerService.GetBanners(reqBanner.PlaceId),
                ClassName = reqBanner.className,
            };

            if (model.banners != null && model.banners.Any())
            {
                if (reqBanner.TakeBanner > 0)
                {
                    model.banners = model.banners.Take(reqBanner.TakeBanner).ToList();
                }
            }

            return View(_viewName, model);
        }

        public class ReqBanner
        {
            public long v_bannerplaceid;
            public string v_keyword = "";
            public long v_currentpage = 0;
            public long v_recordperpage = 5;
            public short v_isactive = 1;
            public int v_bannerid;
            public long PlaceId;
            public int PlaceIdPromote;
            public int CategoryId;
            public string viewName = "";
            public string className = "";
            public bool useViewMobile = false;
            public int ProductId = 0;
            public int ManuId = 0;
            public bool IsRunLive = false;
            /// <summary>
            /// Lay so luong banner theo yeu cau
            /// </summary>
            public int TakeBanner = 0;
            public bool newRuleBannerRandom = false;
        }
    }
}
