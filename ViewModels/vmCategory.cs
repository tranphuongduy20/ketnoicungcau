using ketnoicungcau.business.Model;
using ketnoicungcau.business.ModelWeb;
using System.Collections.Generic;

namespace ketnoicungcau.vn.ViewModels
{
    public class vmCategory
    {
        public bool isSell { get; set; }
        public long CategoryId { get; set; }
        public string CategoryName { get; set; }
        public InfoTab InfoSell { get; set; }
        public InfoTab InfoBuy { get; set; }
        public List<Province> ListProvinceBuy { get; set; }
        public List<Province> ListProvinceSell { get; set; }
        public List<ProductCategory> ListCateBuy { get; set; }
        public List<ProductCategory> ListCateSell { get; set; }
        public List<ProductStandard> ListStandardBuy { get; set; }
        public List<ProductStandard> ListStandardSell { get; set; }
        public List<Specialties> ListSpecialistBuy { get; set; }
        public List<Specialties> ListSpecialistSell { get; set; }
    }

    public class InfoTab
    {
        public int Total { get; set; }
        public int Left { get; set; }
        public List<ProductWeb> ListProduct { get; set; }
    }
}
