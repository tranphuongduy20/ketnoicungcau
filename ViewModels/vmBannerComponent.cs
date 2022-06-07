namespace ketnoicungcau.vn.ViewModels
{
    using ketnoicungcau.business.Model;
    using System.Collections.Generic;

    public class vmBannerComponent
    {
        public string ClassName { get; set; }
        public List<Banner> banners { get; set; }
        public bool IsRunLive { get; set; }
    }
}