using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using ketnoicungcau.business.Helpers;
using ketnoicungcau.business.ModelWeb;
using System;
using System.IO;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ketnoicungcau.business.Service.Interface;
using System.Text.RegularExpressions;
using ketnoicungcau.business.infrastructure;
using ketnoicungcau.business.Extensions;
using ketnoicungcau.vn.Models;
using ketnoicungcau.business;
using RestSharp;
using ketnoicungcau.business.Framework;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using ImageMagick;

namespace ketnoicungcau.vn.Controllers
{
    public abstract class BaseController : Controller
    {
        protected string ErrorMessageDefault = "Hệ thống đang cập nhật. Vui lòng thử lại sau ít phút";
        private readonly IWebHostEnvironment _hostingEnvironment;
        public BaseController(IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        #region Rendering
        /// <summary>
        /// Render component to string
        /// </summary>
        /// <param name="componentName">Component name</param>
        /// <param name="arguments">Arguments</param>
        /// <returns>Result</returns>
        protected virtual string RenderViewComponentToString(string componentName, object arguments = null)
        {
            //original implementation: https://github.com/aspnet/Mvc/blob/dev/src/Microsoft.AspNetCore.Mvc.ViewFeatures/Internal/ViewComponentResultExecutor.cs
            //we customized it to allow running from controllers

            if (string.IsNullOrEmpty(componentName))
                throw new ArgumentNullException(nameof(componentName));

            var actionContextAccessor = HttpContext.RequestServices.GetService(typeof(IActionContextAccessor)) as IActionContextAccessor;
            if (actionContextAccessor == null)
                throw new Exception("IActionContextAccessor cannot be resolved");

            var context = actionContextAccessor.ActionContext;

            var viewComponentResult = ViewComponent(componentName, arguments);

            var viewData = ViewData;
            if (viewData == null)
            {
                throw new NotImplementedException();
            }

            var tempData = TempData;
            if (tempData == null)
            {
                throw new NotImplementedException();
            }

            using var writer = new StringWriter();
            var viewContext = new ViewContext(
                context,
                NullView.Instance,
                viewData,
                tempData,
                writer,
                new HtmlHelperOptions());

            // IViewComponentHelper is stateful, we want to make sure to retrieve it every time we need it.
            var viewComponentHelper = context.HttpContext.RequestServices.GetRequiredService<IViewComponentHelper>();
            (viewComponentHelper as IViewContextAware)?.Contextualize(viewContext);

            var result = viewComponentResult.ViewComponentType == null ?
                viewComponentHelper.InvokeAsync(viewComponentResult.ViewComponentName, viewComponentResult.Arguments) :
                viewComponentHelper.InvokeAsync(viewComponentResult.ViewComponentType, viewComponentResult.Arguments);

            result.Result.WriteTo(writer, HtmlEncoder.Default);
            return writer.ToString();
        }
        #endregion

        #region Upload Image
        public static bool CheckIsImage(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);

            List<string> jpg = new List<string> { "FF", "D8" };
            List<string> bmp = new List<string> { "42", "4D" };
            List<string> gif = new List<string> { "47", "49", "46" };
            List<string> png = new List<string> { "89", "50", "4E", "47", "0D", "0A", "1A", "0A" };
            List<List<string>> imgTypes = new List<List<string>> { jpg, bmp, gif, png };

            List<string> bytesIterated = new List<string>();

            for (int i = 0; i < 8; i++)
            {
                string bit = stream.ReadByte().ToString("X2");
                bytesIterated.Add(bit);

                bool isImage = imgTypes.Any(img => !img.Except(bytesIterated).Any());
                if (isImage)
                {
                    return true;
                }
            }

            return false;
        }
        public static void ResizeImageWithAspectRatio(string filePath, int width, int height, string newPath)
        {
            using (var imageMagick = new MagickImage(filePath))
            {
                var ratioX = width / (double)imageMagick.Width;
                var ratioY = height / (double)imageMagick.Height;
                var ratio = ratioX < ratioY ? ratioX : ratioY;

                if (ratio < 1)
                {
                    var newHeight = Convert.ToInt32(imageMagick.Height * ratio);
                    var newWidth = Convert.ToInt32(imageMagick.Width * ratio);
                    imageMagick.Resize(newWidth, newHeight);
                }

                imageMagick.Write(newPath);
            }
        }
        public async Task<BaseResponseResult> UploadImage(IFormFile file, string folderPath, ConfigConstants.UploadImageType imageType, int width = 0, int height = 0, int resizeWidth = 0, int resizeHeight = 0, long maxLength = 0)
        {
            try
            {
                if (file == null || string.IsNullOrEmpty(file.FileName) || string.IsNullOrEmpty(folderPath.Trim())) return new BaseResponseResult() { code = -1, errormessage = ErrorMessageDefault };

                //kiểm tra image hợp lệ
                var stream = file.OpenReadStream();
                if (!CheckIsImage(stream))
                    return new BaseResponseResult() { code = -1, errormessage = "Hình ảnh không hợp lệ" };

                //kiểm tra định dạng image
                var extent = file.FileName.Substring(file.FileName.LastIndexOf('.'), file.FileName.Length - file.FileName.LastIndexOf('.')).ToLower();
                var extents = new List<string>() { ".jpg", ".png", ".gif", ".jpeg" };
                var formatImage = CheckFormatImage(file.FileName, extent, extents);
                if (formatImage.code == -1)
                    return new BaseResponseResult() { code = -1, errormessage = formatImage.errormessage };

                //kiểm tra dung lượng ảnh (nếu có)
                maxLength = maxLength * 1024 * 1024;
                if (maxLength > 0 && file.Length > maxLength)
                    return new BaseResponseResult() { code = -1, errormessage = $"Dung lượng tối đa cho phép là {maxLength}MB. Vui lòng nén ảnh trước khi upload." };


                //đổi filename sang dạng url
                var fileName = UtilitiesExtensions.ToUnsignedVietnamese(file.FileName.Replace(extent, ""));
                fileName = fileName.ToURL();

                //tạo path và ktra tồn tại path chưa
                string contentRootPath = _hostingEnvironment.ContentRootPath;
                var absolutePath = Path.Combine(contentRootPath + folderPath + fileName + extent);

                if (!Directory.Exists(contentRootPath + folderPath))
                    Directory.CreateDirectory(contentRootPath + folderPath);

                //resize ảnh (nếu có)
                if (resizeWidth > 0 && resizeHeight > 0)
                {
                    //var idx = 1;
                    var resizePath = Path.Combine(contentRootPath + folderPath + fileName + $"-{resizeWidth}x{resizeHeight}" + extent);

                    var idx = DateTime.Now.ToString("hhmmssddMMyyyy");
                    resizePath = Path.Combine(contentRootPath + folderPath + fileName + $"-{resizeWidth}x{resizeHeight}-{idx}" + extent);
                    ResizeImageWithAspectRatio(absolutePath, resizeWidth, resizeHeight, resizePath);
                }
                else
                {
                    var idx = DateTime.Now.ToString("hhmmssddMMyyyy");
                    absolutePath = Path.Combine(contentRootPath + folderPath + fileName + $"-{idx}" + extent);
                }
                //lưu ảnh
                using (Stream fileStream = new FileStream(absolutePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                //kiếm tra kích thước ảnh (nếu có)
                if (width > 0 || height > 0)
                {
                    using (var image = new MagickImage(absolutePath))
                    {
                        if (image.Width != width || image.Height != height)
                        {
                            //remove ảnh nếu ko thỏa điều kiện
                            //tránh phình file trên server
                            if (System.IO.File.Exists(absolutePath))
                            {
                                System.IO.File.Delete(absolutePath);
                            }
                            return new BaseResponseResult() { code = -1, errormessage = $"Vui lòng upload hình ảnh đúng kích thước (chiều rộng: {width}px - chiều cao: {height}px)" };
                        }
                    }
                }

                return new BaseResponseResult() { code = (int)HttpStatusCode.OK, message = absolutePath.Replace(contentRootPath, "").Replace("/wwwroot", "") };
            }
            catch (Exception ex)
            {
                return new BaseResponseResult() { code = -1, errormessage = "Ex - " + ex.ToString() };
                //throw;
            }
        }

        public BaseResponseResult CheckFormatImage(string fileName, string fileNameExtend = "", List<string> extents = null, bool checkExist = false)
        {
            if (string.IsNullOrEmpty(fileName))
                return new BaseResponseResult() { code = -1, errormessage = "Không tìm thấy hình ảnh" };
            if (fileName.ToLower().StartsWith("http://") || fileName.ToLower().StartsWith("https://"))
                return new BaseResponseResult() { code = -1, errormessage = "Hình ảnh không đúng định dạng" };

            var extent = fileNameExtend;
            if (string.IsNullOrEmpty(extent))
                extent = fileName.Substring(fileName.LastIndexOf('.'), fileName.Length - fileName.LastIndexOf('.')).ToLower();

            var lstExtents = extents;
            if (lstExtents == null || lstExtents.Any() == false)
                lstExtents = new List<string>() { ".jpg", ".png", ".gif", ".jpeg" };

            if (!lstExtents.Any(x => x == extent))
                return new BaseResponseResult() { code = -1, errormessage = "File không đúng định dạng (chỉ hỗ trợ .jpg|.jpeg|.png|.gif)" };

            if (checkExist)
            {
                string contentRootPath = _hostingEnvironment.ContentRootPath;
                var absolutePath = "";
                if (fileName.StartsWith("/cms/"))
                {
                    absolutePath = Path.Combine(contentRootPath.Replace("ketnoicungcau.vn", "cms.ketnoicungcau.vn/wwwroot") + fileName.Replace("/cms/", "/"));
                }
                else
                {
                    absolutePath = Path.Combine(contentRootPath + "/wwwroot" + fileName);
                }

                if (System.IO.File.Exists(absolutePath) == false)
                    return new BaseResponseResult() { code = -1, errormessage = "Hình ảnh không tồn tại" };
            }

            return new BaseResponseResult() { code = 1, message = "" };
        }


        #endregion

        public async Task<string> RenderViewToStringAsync<TModel>(string viewNamePath, TModel model, ViewDataDictionary viewData = null)
        {
            if (string.IsNullOrEmpty(viewNamePath))
                viewNamePath = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;
            if (viewData != null)
            {
                ViewData.Clear();
                foreach (var vd in viewData)
                {
                    ViewData.Add(vd.Key, vd.Value);
                }
            }

            using (StringWriter writer = new StringWriter())
            {
                try
                {
                    IViewEngine viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;

                    ViewEngineResult viewResult = null;

                    if (viewNamePath.EndsWith(".cshtml"))
                        viewResult = viewEngine.GetView(viewNamePath, viewNamePath, false);
                    else
                        viewResult = viewEngine.FindView(ControllerContext, viewNamePath, false);

                    if (!viewResult.Success)
                        return $"A view with the name '{viewNamePath}' could not be found";

                    ViewContext viewContext = new ViewContext(
                        ControllerContext,
                        viewResult.View,
                        ViewData,
                        TempData,
                        writer,
                        new HtmlHelperOptions()
                    );

                    await viewResult.View.RenderAsync(viewContext);

                    return writer.GetStringBuilder().ToString().Trim();
                }
                catch (Exception ex)
                {
                    var logger = EngineContext.Current.Resolve<ILoggerService>();
                    logger.LogErrorException(ex);
                    return string.Empty;
                }
            }
        }
        public async Task<string> GetHTML(string URL)
        {
            string connectionString = URL.Contains("http") ? URL : URL.Replace("//cdn", "http://cdn");
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(connectionString);
                WebResponse webResponse = myRequest.GetResponse();
                Stream respStream = webResponse.GetResponseStream();
                StreamReader ioStream = new StreamReader(respStream);
                string pageContent = ioStream.ReadToEnd();
                ioStream.Close();
                respStream.Close();
                return pageContent;
            }
            catch (Exception ex)
            {
                var logger = EngineContext.Current.Resolve<ILoggerService>();
                logger.LogErrorException(ex, URL);
            }
            return null;
        }
        public async Task<string> GetHTMLNew(string URL)
        {
            string connectionString = URL.Contains("http") ? URL : URL.Replace("//cdn", "http://cdn");
            try
            {
                var client = new RestClient(connectionString);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "*/*");
                request.AddHeader("User-Agent", "PostmanRuntime");
                client.UserAgent = "PostmanRuntime";
                IRestResponse response = client.Execute(request);
                return response.Content;
            }
            catch (Exception ex)
            {
                var logger = EngineContext.Current.Resolve<ILoggerService>();
                logger.LogErrorException(ex, URL);
            }
            return null;
        }
        public JsonResult ReturnJson(string message, int status = -1)
        {
            return Json(new
            {
                status = status,
                message = message
            });
        }
        protected JsonResult ReturnData(BaseResponseResult model)
        {
            model.serverName = Environment.MachineName;
            return Json(model);
        }
        public string LoadFullAddressByCookies(Customer customer)
        {
            var address = "Hồ Chí Minh";
            var newcustomer = customer;
            Regex trimmer = new Regex(@"\s\s+"); // Xóa khoảng trắng thừa trong chuỗi
            address = string.IsNullOrEmpty(customer.ProvinceName) ? address : customer.ProvinceName.Replace("TP.", "");
            var districtName = customer.DistrictType;
            var wardTypeOld = customer.WardType;
            var districtType = "";
            var districtId = 0;
            if (!string.IsNullOrEmpty(districtName) && customer.DistrictId > 0)
            {
                if (districtName.ToLower().Contains("quận"))
                    districtType = "Q.";
                else if (districtName.ToLower().Contains("thị xã") || districtName.ToLower().Contains("tx."))
                    districtType = "TX.";
                else if (districtName.ToLower().Contains("tp.") || districtName.ToLower().Contains("thành phố"))
                    districtType = "TP.";
                else if (districtName.ToLower().Contains("huyện"))
                    districtType = "H.";
                address = districtType + trimmer.Replace(customer.DistrictName.Trim(), " ").ToCapitalize() + ", " + address;
                Int32.TryParse(customer.DistrictName, out districtId);
            }
            var wardType = "";
            if (!string.IsNullOrEmpty(wardTypeOld) && customer.WardId > 0)
            {
                if (wardTypeOld.ToLower().Contains("phường"))
                    wardType = "P.";
                else if (wardTypeOld.ToLower().Contains("xã"))
                    wardType = "X.";
                else if (wardTypeOld.ToLower().Contains("thị trấn"))
                    wardType = "TT.";
                else if (!string.IsNullOrEmpty(wardTypeOld))
                    wardType = "P.";
                var wardName = customer.WardName.Trim();
                var afterTrim = !string.IsNullOrEmpty(wardName) ? trimmer.Replace(wardName, " ").ToString() : trimmer.ToString();
                address = wardType + afterTrim.ToCapitalize() + ", " + address;
                if (districtId == 0 && !string.IsNullOrEmpty(districtType))
                {
                    address = address.Replace(districtType, " ");
                }
                if (!string.IsNullOrEmpty(customer.Address))
                {
                    address = customer.Address + ", " + address;
                }
            }
            return trimmer.Replace(address.Trim(), " ");
        }

        #region Session

        public void SetSession(ISession session, string key, object value)
        {
            try
            {
                session.SetSession(key, value);
            }
            catch (Exception ex)
            {
                var logger = EngineContext.Current.Resolve<ILoggerService>();
                if (logger != null)
                    logger.LogErrorException(ex, $"key {key} ");
            }
        }
        public T GetSession<T>(ISession session, string key)
        {
            try
            {
                return session.GetSession<T>(key);
            }
            catch (Exception ex)
            {
                var logger = EngineContext.Current.Resolve<ILoggerService>();
                if (logger != null)
                    logger.LogErrorException(ex, $"key {key} ");
            }
            return default(T);
        }
        public void DelSession(ISession session, string key)
        {
            try
            {
                session.Remove(key);
            }
            catch (Exception ex)
            {
                var logger = EngineContext.Current.Resolve<ILoggerService>();
                if (logger != null)
                    logger.LogErrorException(ex, $"key {key} ");
            }
        }

        #endregion

        #region Cookie
        public string GetCookie(string name)
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            return CookiesHelpers.GetCookie(Request, name);
        }
        /// <summary>  
        /// set the cookie  
        /// </summary>  
        /// <param name="key">key (unique indentifier)</param>  
        /// <param name="value">value to store in cookie object</param>  
        public void SetCookie(string key, string value, int? minute)
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            CookiesHelpers.SetCookie(Response, key, value, minute);
        }
        /// <summary>  
        /// Delete the key  
        /// </summary>  
        /// <param name="key">Key</param>  
        public void RemoveCookies(string key)
        {
            var workContext = EngineContext.Current.Resolve<IWorkContext>();
            CookiesHelpers.RemoveCookies(Response, key);
        }
        #endregion
    }
}
