using ketnoicungcau.business.Caching;
using ketnoicungcau.business.ServiceWeb.Interface;
using ketnoicungcau.vn.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using ketnoicungcau.business.Framework;
using System.Collections.Generic;
using ketnoicungcau.business.Model;
using Microsoft.AspNetCore.Routing;
using System.Linq;
using ketnoicungcau.business;
using Microsoft.AspNetCore.Hosting;
using ketnoicungcau.business.Helpers;
using ketnoicungcau.business.Extensions;

namespace ketnoicungcau.vn.Controllers
{
    public class NewsController : BaseController {

        #region Constructors
        private readonly ILogger<NewsController> _logger;
        private readonly ICache _cache;
        private readonly INewsServiceWeb _newsSvc;
        private readonly IWorkContext _workContext;
        private readonly IProductServiceWeb _productSvc;

        public NewsController(ILogger<NewsController> logger,
            ICache cache,
            INewsServiceWeb newsSvc,
            IProductServiceWeb productSvc,
            IWorkContext workContext,
            IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _logger = logger;
            _cache = cache;
            _newsSvc = newsSvc;
            _productSvc = productSvc;
            _workContext = workContext;
        }
        #endregion

        [Route("tin-tuc")]
        [Route("tin-tuc/{CategoryUrl}")]
        public async Task<IActionResult> Index(string CategoryUrl = "")
        {
            string viewName =  $"Index{(_workContext.IsMobile ? ".M" : "")}";
            int pageSize = 10;
            var newsCate = await _newsSvc.GetCate("", 0, 10);
            NewsCategory activedCate = null;

            if(CategoryUrl != "")
            {
                var dataCate = newsCate.Find(e => e.Url == CategoryUrl);
                if (dataCate?.Categoryid > 0)
                    activedCate = dataCate;
                else
                    return NotFound();
            }
            var vm = new vmNews() {
                ListNewsCate = await _newsSvc.GetCate("", 0, 10, 1),
                ListNews = await _newsSvc.GetNews("", 0, pageSize, 1, 1, activedCate?.Categoryid > 0 ? activedCate.Categoryid : -1),
                PageSize = pageSize,
                SelectedCate = activedCate
            };

            #region remove cate trang tinh
            if (vm.ListNewsCate?.Count > 0)
            {
                var cate = vm.ListNewsCate.FirstOrDefault(c => c.Categoryid == 24);
                vm.ListNewsCate.Remove(cate);
            }
            #endregion

            #region cat chuoi video
            foreach (var item in vm.ListNews)
            {
                if (item.Video.Contains("https://www.youtube.com/watch?v="))
                {
                    var str = "https://img.youtube.com/vi/";
                    item.Video = item.Video.Replace("https://www.youtube.com/watch?v=", string.Empty);
                    if (item.Video.Contains("&ab_channel="))
                    {
                        var idx = item.Video.IndexOf("&");
                        item.Video = item.Video.Substring(0, idx);
                    }
                    item.Video = str + item.Video + "/maxresdefault.jpg";
                }
                item.ContentString = UtilitiesExtensions.RemoveHarmfulCharacterV0(item.Content);
            }
            #endregion

            foreach (var item in vm.ListNewsCate)
            {
                item.ListNews = vm.ListNews.Where(i => i.Categoryid == item.Categoryid).ToList();
            }
            return View(viewName, vm);
        }

        [Route("tin-tuc/{NewsUrl}-{NewsId:int}")]
        public async Task<IActionResult> Detail(string NewsUrl, int NewsId)
        {
            string viewName = $"Detail{(_workContext.IsMobile ? ".M" : "")}";
            var news = new News();
            var otherNews = new List<News>();
            var dataNews = await _newsSvc.GetNewsById(NewsId);

            #region Redirect

            if (dataNews.Newsid <= 0 || dataNews == null)
                return Redirect("/tin-tuc");
            if (SEOHelper.GenSEOUrl(dataNews.Url) != NewsUrl)
                return Redirect(dataNews.GenNewsUrl());

            #endregion

            news = dataNews;
            otherNews = await _newsSvc.GetNews("", 0, 3, 1, 1, news.Categoryid);
            if (otherNews?.Count > 0)
                otherNews.RemoveAll(e => e.Newsid == news.Newsid);
            var vm = new vmNewsDetail()
            {
                News = news,
                ListOtherNews = otherNews,
                ListImageGallery = await _productSvc.GetImageByType(NewsId, 2)
            };
            vm.News.ListImage = vm.ListImageGallery.Where(i => i.Objectid == vm.News.Newsid).ToList();

            #region cat chuoi video
            if (vm.News.Video.Contains("https://www.youtube.com/watch?v="))
            {
                var str = "https://www.youtube.com/embed/";
                vm.News.Video = vm.News.Video.Replace("https://www.youtube.com/watch?v=", string.Empty);
                if (vm.News.Video.Contains("&ab_channel="))
                {
                    var idx = vm.News.Video.IndexOf("&");
                    vm.News.Video = vm.News.Video.Substring(0, idx);
                }
                vm.News.Video = str + vm.News.Video;
            }
            #endregion

            #region cat chuoi video cho list
            foreach (var item in vm.ListOtherNews)
            {
                if (item.Video.Contains("https://www.youtube.com/watch?v="))
                {
                    var str = "https://img.youtube.com/vi/";
                    item.Video = item.Video.Replace("https://www.youtube.com/watch?v=", string.Empty);
                    if (item.Video.Contains("&ab_channel="))
                    {
                        var idx = item.Video.IndexOf("&");
                        item.Video = item.Video.Substring(0, idx);
                    }
                    item.Video = str + item.Video + "/maxresdefault.jpg";
                }
            }
            #endregion

            return View(viewName, vm); 
        }

        public async Task<IActionResult> GetNews(int pageIndex, int pageSize)
        {
            string viewName = $"~/Views/News/Partial/_ListNews.cshtml";
            var newsList = await _newsSvc.GetNews("", pageIndex, pageSize, 1, 1);
            #region cat chuoi video cho list
            foreach (var item in newsList)
            {
                if (item.Video.Contains("https://www.youtube.com/watch?v="))
                {
                    var str = "https://img.youtube.com/vi/";
                    item.Video = item.Video.Replace("https://www.youtube.com/watch?v=", string.Empty);
                    if (item.Video.Contains("&ab_channel="))
                    {
                        var idx = item.Video.IndexOf("&");
                        item.Video = item.Video.Substring(0, idx);
                    }
                    item.Video = str + item.Video + "/maxresdefault.jpg";
                }
            }
            #endregion
            return PartialView(viewName, newsList);
        }
    }
}