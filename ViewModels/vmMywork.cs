using ketnoicungcau.business.Enums;
using ketnoicungcau.business.Model;
using ketnoicungcau.business.ModelWeb;
using System.Collections.Generic;

namespace ketnoicungcau.vn.ViewModels
{
    public class vmMywork
    {
        public Company Company { get; set; }
        public InfoTabDeals TabSend { get; set; }
        public InfoTabDeals TabReceive { get; set; }
    }

    public class InfoTabDeals
    {
        public DealsTypeCompany Type { get; set; }
        public int Total { get; set; }
        public int Left { get; set; }
        public List<DealsWeb> ListDeals { get; set; }
    }
}
