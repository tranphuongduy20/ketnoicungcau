using ketnoicungcau.business.Model;
using ketnoicungcau.business.ModelWeb;
using System.Collections.Generic;
using ketnoicungcau.vn.Models;

namespace ketnoicungcau.vn.ViewModels
{
    public class vmAddProduct
    {
        public List<Province> ListProvince { get; set; }
        public List<ProductCategory> ListCate { get; set; }
        /// <summary>
        /// Đơn vị tần suất (kg/tháng, kg/năm,..)
        /// </summary>
        public List<ProductUnit> ListUnit { get; set; }
        /// <summary>
        /// Đơn vị cơ sở (kg, tạ , tấn)
        /// </summary>
        public List<ProductUnit> ListBaseUnit { get; set; }
        public List<ProductStandard> ListStandard { get; set; }
        public List<Specialties> ListSpecialties { get; set; }
        public int Type { get; set; }
        public Product Product { get; set; }
        public List<ProductImage> Images { get; set; }
        public List<ProductPrice> PriceItems { get; set; }
    }

    public class RequestBoxPrice
    {
        public  List<ProductUnit> ListUnit { get; set; }
        public int numId { get; set; }
        public ProductPrice Productpriceitem { get; set; }
        public bool GetButton { get; set; }

        public RequestBoxPrice()
        {
            GetButton = true;
        }
    }

}
