using ketnoicungcau.business.Model;
using ketnoicungcau.business.ModelWeb;
using System.Collections.Generic;

namespace ketnoicungcau.vn.ViewModels
{
    using System;

    public class vmNews
    {
        public List<NewsCategory> ListNewsCate { get; set; }
        public List<News> ListNews { get; set; }
        public int PageSize { get; set; }
        public NewsCategory SelectedCate { get; set; }
    }
}