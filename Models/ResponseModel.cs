using ketnoicungcau.business.ModelWeb;
using System;

namespace ketnoicungcau.vn.Models
{
    //public interface IWorkContext
    //{
    //    Customer CurrentCustomer { get; set; }
    //}
    //[Serializable]
    public class BaseResponseResult
    {
        public dynamic data { get; set; }
        /// <summary>
        /// 
        /// <para>0 Thành công</para>
        /// </summary>
        public int code { get; set; }
        /// <summary>
        /// Nội dung thông báo hiển thị lên ứng dụng cho người dùng
        /// </summary>
        public string message { get; set; }
        /// <summary>
        /// Nội dung thông báo lỗi phục vụ cho dev (ko hiển thị lên cho người dùng)
        /// </summary>
        public string errormessage { get; set; }
        public string serverName { get; set; }
        /// <summary>
        /// Dữ liệu trả về
        /// </summary>
        public BaseResponseResult()
        {
            message = string.Empty;
            errormessage = string.Empty;
            serverName = string.Empty;
        }
    }
}
