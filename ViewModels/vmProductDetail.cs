using ketnoicungcau.business.Model;
using ketnoicungcau.business.ModelWeb;
using System.Collections.Generic;

namespace ketnoicungcau.vn.ViewModels
{
    public class vmProductDetail
    {
        public Company UserCompany { get; set; }
        public ProductWeb Product { get; set; }
        public List<SellUnit> ListPrices { get; set; }
        public Deals Deal { get; set; }
        public string StringSpecialties { get; set; }
        public string StringProductStandard { get; set; }
        public string Provincename { get; set; }
        public bool IsLike { get; set; }
        public int TotalLike { get; set; }
    }
}
