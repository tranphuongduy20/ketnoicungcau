using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ketnoicungcau.business.Model;
using ketnoicungcau.business.ModelWeb;

namespace ketnoicungcau.vn.ViewModels
{
    [Serializable]
    public class vmCompany
    {
        public int TotalCompany { get; set; }
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int PageSizeBuy { get; set; }
        public int PageSizeSell { get; set; }
        public int PageIndexSell { get; set; }
        public int PageIndexBuy { get; set; }
        public long CategoryId { get; set; }
        public int CompanyId { get; set; }
        public List<CompanyWeb> Companys { get; set; }
        public CompanyWeb Company { get; set; }
        public List<ProductWeb> ListBuy { get; set; }
        public List<ProductWeb> ListSell { get; set; }
        public List<ProductCategory> ProductCategories { get; set; }
    }
}