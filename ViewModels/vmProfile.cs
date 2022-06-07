using ketnoicungcau.business.Model;
using ketnoicungcau.business.ModelWeb;
using System.Collections.Generic;

namespace ketnoicungcau.vn.ViewModels
{
    public class vmProfile
    {
        public List<ProductCategory> ListCate { get; set; }
        public Company Company { get; set; }
        public List<ProvinceCompany> CompanyProvince { get; set; }
        public string[] CompanyCategoryId { get; set; }

    }
}
