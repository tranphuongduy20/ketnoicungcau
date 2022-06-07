using Microsoft.AspNetCore.Mvc;
using ketnoicungcau.business.Caching;
using System;
using System.Threading.Tasks;
using ketnoicungcau.vn.Models;
using ketnoicungcau.business.ModelWeb;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ketnoicungcau.vn.ViewComponents;
using ketnoicungcau.business.Helpers;
using ketnoicungcau.business;
using Constants = ketnoicungcau.business.Caching.Constants;
using ketnoicungcau.business.ServiceWeb.Interface;
using ketnoicungcau.business.Framework;
using Microsoft.AspNetCore.Http;
using System.Net;
using Microsoft.AspNetCore.Hosting;

namespace ketnoicungcau.vn.Controllers
{
    public class CommonController : BaseController
    {
        private readonly ILogger<CommonController> _logger;
        private readonly IOptions<AppSettings> _settings;
        private readonly ICache _cache;
        private readonly ISearchServiceWeb _searchSvc;
        private readonly IWorkContext _workContext;

        public CommonController(
            ILogger<CommonController> logger,
            ICache cache,
            IOptions<AppSettings> settings,
            ISearchServiceWeb searchSvc,
            IWorkContext workContext,
            IWebHostEnvironment hostEnvironment) : base(hostEnvironment)
        {
            _logger = logger;
            _settings = settings;
            _cache = cache;
            _searchSvc = searchSvc;
            _workContext = workContext;
        }

        [Route("getheaderfooter")]
        public async Task<string> GetHeaderFooter(int type = 0, bool isMobile = false, int provinceId = 0, string keysecret = "")
        {
            if (string.IsNullOrEmpty(keysecret) || keysecret != "mwgqazwsxedcrfv")
            {
                return string.Empty;
            }
            var keycache = _cache.CreateKey(isMobile, type, provinceId.GetProvinceName());
            var data = _cache.Get<string>(keycache);
            if (data != null)
                return data;
            var timeCache = Constants.TimeCache.dateHour;
            var version = isMobile ? ".M" : "";
            var urlSite = string.Format("{0}://{1}", Request.Scheme, Request.Host);
            var requestUrl = Request.Host.ToString();
            var masterPage = new HeaderFooter();
            try
            {
                var header = RenderViewComponentToString("MasterPage", new MasterPageViewComponent.ReqMasterPage { viewName = "Header", isHeader = true, isMobile = isMobile, ProvinceId = provinceId });
                var footer = RenderViewComponentToString("MasterPage", new MasterPageViewComponent.ReqMasterPage { viewName = "Footer", isMobile = isMobile });
                if (!string.IsNullOrEmpty(_settings.Value.CdnUrl))
                {
                    urlSite = _settings.Value.CdnUrl;
                }
                var urlcss = CommonHelper.GetFileVersion(urlSite, string.Format("/css/bundle/global" + version + ".min.css"));
                var css = await GetHTMLNew(urlcss);
                var urljs = CommonHelper.GetFileVersion(urlSite, string.Format("/js/bundle/global" + version + ".min.js"));
                var js = await GetHTMLNew(urljs);
                masterPage.Header = header;
                masterPage.Footer = footer;
                masterPage.Css = css;
                masterPage.Js = js;
                if (string.IsNullOrEmpty(header) || string.IsNullOrEmpty(footer) || string.IsNullOrEmpty(css) || string.IsNullOrEmpty(js))
                {
                    _logger.LogInformation("Thieu du lieu css,js tu ham getheaderfooter", urlcss + urljs);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                timeCache = Constants.TimeCache.dateNull;
                masterPage = new HeaderFooter();
                _logger.LogErrorException(ex);
            }
            data = JsonConvert.SerializeObject(masterPage);
            _cache.Set(keycache, data, timeCache);
            return data;
        }

        #region Suggest Search
        [HttpGet]
        public async Task<ActionResult> SuggestSearch(string keywords = "")
        {
            if (string.IsNullOrEmpty(keywords) || keywords.Length < 2)
                return PartialView(new SuggestSearch());

            keywords = keywords.HandelMaxLenghtKwSearch();
            //var provinceId = _workContext.CurrentCustomer.ProvinceId;
            var keycache = _cache.CreateKey(keywords.GetKeyWordName(false));
            var data = _cache.Get<SuggestSearch>(keycache);
            if (data != null)
                return PartialView(data);
            var timeCache = Constants.TimeCache.dateDefault;
            var suggestSearch = await _searchSvc.GetSuggestSearch(keywords);
            data = suggestSearch ?? new SuggestSearch();

            data.Keyword = keywords;

            _cache.Set(keycache, data, timeCache);
            return PartialView(data);
        }

        //public ActionResult UpdateSearchKeywordHistory(string keyword, string url)
        //{
        //    if (url.IsValidUrl() == false)
        //        return ReturnJson("Du lieu khong hop le");

        //    string value = string.Format("{0}|{1}", keyword, ValidateHelper.Sanitize(url));
        //    int mins = 525948766;
        //    if (GetCookie(CookiesHelpers.SearchKeywordHis) == null)
        //    {
        //        SetCookie(CookiesHelpers.SearchKeywordHis, CryptoHelper.Encrypt(value), mins);
        //    }
        //    else
        //    {
        //        string cookieValue = CryptoHelper.Decrypt(GetCookie(CookiesHelpers.SearchKeywordHis));
        //        if (!string.IsNullOrEmpty(cookieValue))
        //        {
        //            var lst = cookieValue.Split(',').ToList();
        //            if (lst != null && lst.Count > 0)
        //            {
        //                lst = lst.Distinct().ToList();
        //                if (!lst.Contains(value))
        //                {
        //                    lst.Insert(0, value);
        //                }
        //                cookieValue = string.Join(",", lst.Take(5));// up date toi da 5 phan tu vao cookies
        //            }

        //            //cookieValue = string.Format("{0},{1}", cookieValue, value);
        //        }
        //        else
        //            cookieValue = value;
        //        SetCookie(CookiesHelpers.SearchKeywordHis, CryptoHelper.Encrypt(cookieValue), mins);
        //    }
        //    return ReturnJson("Update thanh cong keyword moi ", 1);
        //}

        //public ActionResult ViewSearchKeywordHistory()
        //{
        //    List<SearchHistoryCookie> result = new List<SearchHistoryCookie>();
        //    string cookieValue = CryptoHelper.Decrypt(GetCookie(CookiesHelpers.SearchKeywordHis));
        //    if (!string.IsNullOrEmpty(cookieValue))
        //    {
        //        var lstValue = cookieValue.Split(',').ToList();
        //        if (lstValue != null && lstValue.Count > 0)
        //        {
        //            foreach (var item in lstValue)
        //            {
        //                if (item.Contains("|"))
        //                {
        //                    var searchHistory = new SearchHistoryCookie
        //                    {
        //                        Keyword = item.Substring(0, item.IndexOf("|")),
        //                        Url = item.Substring(item.IndexOf("|") + 1)
        //                    };
        //                    if (searchHistory.IsValidate())
        //                    {
        //                        searchHistory.Keyword = UtilitiesExtensions.RemoveHarmfulCharacter(searchHistory.Keyword);
        //                        result.Add(searchHistory);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    var data = new SuggestSearch();

        //    if (result != null && result.Any())
        //    {
        //        data.SearchHistoryCookie = result;
        //    }
        //    return PartialView("SuggestSearch", data);
        //}
        #endregion

        public async Task<BaseResponseResult> UploadImageCkEditor(IFormCollection collection)
        {
            if (collection == null || collection.Files == null || collection.Files.Count <= 0)
                return new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT };

            var files = collection.Files;
            var path = $"/wwwroot/images/{DateTime.Now.Year}/{DateTime.Now.Month}/{DateTime.Now.Day}/contents/";

            var image = await UploadImage(files[0], path, ConfigConstants.UploadImageType.Contents);
            if (image != null && image.code == (int)HttpStatusCode.OK)
                return new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = image.message };

            return new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT };
        }
    }
}
