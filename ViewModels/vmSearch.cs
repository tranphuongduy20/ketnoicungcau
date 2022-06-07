using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ketnoicungcau.business.Enums;
using ketnoicungcau.business.Model;
using ketnoicungcau.business.ModelWeb;
using ketnoicungcau.vn.Models;

namespace ketnoicungcau.vn.ViewModels
{
    public class vmSearch
    {
        public bool IsRunLive { get; set; }
        public bool IsMobile { get; set; }
        public string KeywordSearch { get; set; }
        public int Total { get; set; }
        public int Left { get; set; }
        public List<ProductWeb> ListProduct { get; set; }
        public List<CompanyWeb> ListCompany { get; set; }
        public SearchQuery Filter { get; set; }

        public vmSearch()
        {
            KeywordSearch = string.Empty;
            Total = 0;
            Left = 0;
            Filter = new SearchQuery();
        }

    }

}
