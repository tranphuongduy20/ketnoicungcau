using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ketnoicungcau.business.ModelWeb;
using ketnoicungcau.vn.Controllers;

namespace ketnoicungcau.vn.ViewModels
{
    public class vmProductList
    {
        public List<ProductWeb> ProductList { get; set; }
        public RequestProductList Filter { get; set; }
        public int Total { get; set; }
        public int Left { get; set; }
    }

    public class RequestProductList
    {
        public string Keyword { get; set; }
        public int Type { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public RequestProductList()
        {
            Keyword = "";
            PageSize = 10;
        }
    }

    public class RequestProductListView
    {
        public List<ProductWeb> ListProduct { get; set; }
        public bool IsAjax { get; set; }
    }

}