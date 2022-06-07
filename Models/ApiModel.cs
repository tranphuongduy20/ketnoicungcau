using ketnoicungcau.business.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ketnoicungcau.vn.Models
{
    public class ApiModel
    {
        [Serializable]
        public class SessionForgotPasswordOTP
        {
            public long Companyid { get; set; }
            public string Taxid { get; set; }
            public string Phonenumber { get; set; }
            public string OTP { get; set; }
            public int CountFailed { get; set; }
            public bool isResend { get; set; }
            public DateTime Time { get; set; }
            public bool IsTimeOutOTP(int minutes = 1)
            {
                return (DateTime.Now - this.Time).Minutes >= minutes;
            }
        }

        [Serializable]
        public class SessionLogin
        {
            public long Companyid { get; set; }
            public string Taxid { get; set; }
            public int CountFailed { get; set; }
            public DateTime Time { get; set; }
        }
        [Serializable]
        public class SessionRegistryOTP
        {
            public Company Company { get; set; }
            public int CountFailed { get; set; }
            public DateTime Time { get; set; }
            public string OTP { get; set; }
            public bool isResend { get; set; }
            public bool IsTimeOutOTP(int minutes = 1)
            {
                return (DateTime.Now - this.Time).Minutes >= minutes;
            }
        }
    }
}
