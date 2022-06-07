using ketnoicungcau.business.Model;
using ketnoicungcau.business.ModelWeb;
using System.Collections.Generic;

namespace ketnoicungcau.vn.ViewModels
{
    public class vmHome
    {
        public List<ProductWeb> ListBuy { get; set; }
        public List<ProductWeb> ListSell { get; set; }
        public List<ProductWeb> ListRelated { get; set; }
        public List<CompanyWeb> ListCompany { get; set; }
        public List<News> ListNew { get; set; }
        public List<News> ListNewTop1 { get; set; }
        public List<ProductImage> ListImage { get; set; }
    }
}
