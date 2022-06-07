using ketnoicungcau.business.Model;
using ketnoicungcau.business.ModelWeb;
using System.Collections.Generic;

namespace ketnoicungcau.vn.ViewModels
{
    using System;

    public class vmNewsDetail
    {
        public News News { get; set; }
        public List<News> ListOtherNews { get; set; }
        public List<ProductImage> ListImageGallery { get; set; }

    }
}