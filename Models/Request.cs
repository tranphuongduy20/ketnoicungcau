using ketnoicungcau.business;
using ketnoicungcau.business.Enums;
using ketnoicungcau.business.Helpers;
using ketnoicungcau.business.Helpers.Interface;
using ketnoicungcau.business.infrastructure;
using ketnoicungcau.business.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ketnoicungcau.business.Service.Interface;

namespace ketnoicungcau.vn.Models
{
    public class RequestLogin
    {
        public string Taxid { get; set; }
        public string Password { get; set; }
        public BaseResponseResult Validate()
        {
            if (this == null) return new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT };
            if (string.IsNullOrEmpty(Taxid)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mã số thuế" };
            if (UtilitiesExtensions.IsTaxId(Taxid) == false) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mã số thuế đúng định dạng" };
            if (string.IsNullOrEmpty(Password)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mật khẩu" };
            return null;
        }
    }
    public class RequestForgotPassword
    {
        public string Phonenumber { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string OTP { get; set; }
        public BaseResponseResult Validate()
        {
            var userHelpers = EngineContext.Current.Resolve<IUserHelpers>();
            if (this == null) return new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT };
            if (string.IsNullOrEmpty(Phonenumber)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập số điện thoại" };
            if (UtilitiesExtensions.IsPhoneNumber(Phonenumber) == false) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập đúng định dạng số điện thoại" };
            if (string.IsNullOrEmpty(OTP) || OTP.Length != 4) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng xác nhập OTP có 4 chữ số" };
            if (string.IsNullOrEmpty(Password)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mật khẩu mới" };
            else if (userHelpers.CheckPasswordUser(Password) == false) return new BaseResponseResult() { code = -1, errormessage = "Mật khẩu tối thiểu 6 ký tự, tối đa 20 ký tự, có chứa chữ in hoa, chữ thường, số và ký tự đặt biệt" };
            if (string.IsNullOrEmpty(ConfirmPassword)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng xác nhận mật khẩu mới" };
            else if (userHelpers.CheckPasswordUser(ConfirmPassword) == false) return new BaseResponseResult() { code = -1, errormessage = "Mật khẩu tối thiểu 6 ký tự, tối đa 20 ký tự, có chứa chữ in hoa, chữ thường, số và ký tự đặt biệt" };
            if (ConfirmPassword != Password) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mật khẩu trùng khớp nhau" };
            return null;
        }
    }
    public class RequestChangePassword
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }

        public BaseResponseResult Validate()
        {
            var userHelpers = EngineContext.Current.Resolve<IUserHelpers>();
            if (this == null) return new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT };
            if (string.IsNullOrEmpty(OldPassword)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mật khẩu cũ" };
            if (string.IsNullOrEmpty(NewPassword)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mật khẩu mới" };
            else if (userHelpers.CheckPasswordUser(NewPassword) == false) return new BaseResponseResult() { code = -1, errormessage = "Mật khẩu tối thiểu 6 ký tự, tối đa 20 ký tự, có chứa chữ in hoa, chữ thường, số và ký tự đặt biệt" };
            if (string.IsNullOrEmpty(ConfirmNewPassword)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng xác nhận mật khẩu mới" };
            else if (userHelpers.CheckPasswordUser(ConfirmNewPassword) == false) return new BaseResponseResult() { code = -1, errormessage = "Mật khẩu tối thiểu 6 ký tự, tối đa 20 ký tự, có chứa chữ in hoa, chữ thường, số và ký tự đặt biệt" };
            if (ConfirmNewPassword != NewPassword) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mật khẩu trùng khớp nhau" };
            if (OldPassword == NewPassword) return new BaseResponseResult() { code = -1, errormessage = "Mật khẩu mới không được trùng mật khẩu cũ" };
            return null;
        }
    }
    public class RequestUpdateCompany
    {
        public long Companyid { get; set; }
        public string Companyname { get; set; }
        public string Taxid { get; set; }
        public string Provinceoldid { get; set; }
        public string Provinceid { get; set; }
        public string Address { get; set; }
        public string Phonenumber { get; set; }
        public string Logosrc { get; set; }
        public string Gpkdsrc { get; set; }
        public string Email { get; set; }
        public string Zaloid { get; set; }
        public string Fburl { get; set; }
        public string Saleproduct { get; set; }
        public string Listcategoryid { get; set; }
        public string Representname { get; set; }
        public string Representposition { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string Description { get; set; }
        public string Weburl { get; set; }
        public bool Isshow { get; set; }
        public BaseResponseResult Validate(int type = 0) // 0 là update, 1 là insert
        {
            if (this == null) return new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT };
            if (string.IsNullOrEmpty(Companyname)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập tên doanh nghiệp" };
            if (string.IsNullOrEmpty(Taxid)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mã số thuế" };
            else if (UtilitiesExtensions.IsTaxId(Taxid) == false) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mã số thuế đúng định dạng" };
            if (string.IsNullOrEmpty(Provinceid)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng chọn tỉnh thành" };
            if (string.IsNullOrEmpty(Address)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập địa chỉ" };
            if (string.IsNullOrEmpty(Phonenumber)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập số điện thoại" };
            if (UtilitiesExtensions.IsPhoneNumber(Phonenumber) == false) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập đúng định dạng số điện thoại" };
            if (string.IsNullOrEmpty(Logosrc)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng upload logo" };
            if (string.IsNullOrEmpty(Gpkdsrc)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng upload giấy phép kinh doanh" };
            if (string.IsNullOrEmpty(Email)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập email" };
            if (UtilitiesExtensions.IsEmail(Email) == false) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập đúng định dạng email" };
            if (string.IsNullOrEmpty(Zaloid)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập zalo" };
            if (!string.IsNullOrEmpty(Fburl) && !UtilitiesExtensions.IsValidUrlV2(Fburl)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập link facebook đúng định dạng" };
            if (!string.IsNullOrEmpty(Weburl) && !UtilitiesExtensions.IsValidUrlV2(Weburl)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập link website đúng định dạng" };

            if (string.IsNullOrEmpty(Saleproduct)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập sản phẩm kinh doanh" };
            if (string.IsNullOrEmpty(Listcategoryid)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng chọn danh mục kinh doanh" };
            if (string.IsNullOrEmpty(Representname)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập tên người đại diện" };
            if (string.IsNullOrEmpty(Representposition)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập chức vụ người đại diện" };
            if (type == 1) // Validate mật khẩu area
            {
                var userHelpers = EngineContext.Current.Resolve<IUserHelpers>();
                if (string.IsNullOrEmpty(Password)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mật khẩu" };
                else if (userHelpers.CheckPasswordUser(Password) == false) return new BaseResponseResult() { code = -1, errormessage = "Mật khẩu tối thiểu 6 ký tự, tối đa 20 ký tự, có chứa chữ in hoa, chữ thường, số và ký tự đặt biệt" };
                if (string.IsNullOrEmpty(ConfirmPassword)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng xác nhận mật khẩu" };
                else if (userHelpers.CheckPasswordUser(ConfirmPassword) == false) return new BaseResponseResult() { code = -1, errormessage = "Mật khẩu tối thiểu 6 ký tự, tối đa 20 ký tự, có chứa chữ in hoa, chữ thường, số và ký tự đặt biệt" };
                if (ConfirmPassword != Password) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mật khẩu trùng khớp nhau" };
            }
            return null;
        }
    }
    public class RequestInsertRenewUser
    {
        public string Companyname { get; set; }
        public string Taxid { get; set; }
        public string Note { get; set; }
        public string Phonenumber { get; set; }
        public string Confirmimage { get; set; }
        public string Licenseimage { get; set; }
        public string Email { get; set; }
     
        public BaseResponseResult Validate()
        {
            if (this == null) return new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT };
            if (string.IsNullOrEmpty(Taxid)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mã số thuế" };
            else if (UtilitiesExtensions.IsTaxId(Taxid) == false) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập mã số thuế đúng định dạng" };
            if (string.IsNullOrEmpty(Companyname)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập tên công ty" };
            if (string.IsNullOrEmpty(Phonenumber)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập số điện thoại" };
            if (UtilitiesExtensions.IsPhoneNumber(Phonenumber) == false) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập đúng định dạng số điện thoại" };
            if (string.IsNullOrEmpty(Confirmimage)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng upload logo" };
            if (string.IsNullOrEmpty(Licenseimage)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng upload giấy phép kinh doanh" };
            if (string.IsNullOrEmpty(Email)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập email" };
            if (UtilitiesExtensions.IsEmail(Email) == false) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập đúng định dạng email" };
            return null;
        }
    }
    public class RequestAddDeals
    {
        public long Seller { get; set; }
        public long Buyer { get; set; }
        public long Product { get; set; }
        public long Quantity { get; set; }
        public long Unit { get; set; }
        public double Value { get; set; }
        public long Status { get; set; }
        public string Description { get; set; }
        public BaseResponseResult Validate()
        {
            if (this == null) return new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT };
            if (Seller <= 0) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng chọn bên mua" };
            if (Buyer <= 0) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng chọn bên bán" };
            if (Seller == Buyer) return new BaseResponseResult() { code = -1, errormessage = "Bên mua và bên bán không được trùng nhau" };
            if (Product <= 0) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng chọn sản phẩm" };
            //if (Quantity <= 0) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập số lượng" };
            //if (Unit <= 0) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng chọn đơn vị tính" };
            //if (Status <= 0) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng chọn trạng thái kết nối" };
            return null;
        }
    }
    public class RequestUpdateProduct
    {
        public short Producttype { get; set; }
        public long Productid { get; set; }
        public string Productname { get; set; }
        public long Companyid { get; set; }
        public string Categoryid { get; set; }
        public string Shortdescription { get; set; }
        public string Description { get; set; }
        public string Standardid { get; set; }
        public string Begindate { get; set; }
        public string Url { get; set; }
        public string Specialtiesid { get; set; }
        public string Orderprocedure { get; set; }
        public int Quantity { get; set; }
        public int Unitid { get; set; }
        public short IsBuy { get; set; }
        public string Productimage { get; set; }
        public short Gender { get; set; }
        public string Fullname { get; set; }
        public string Phonenumber { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public bool Isshow { get; set; }
        public bool Isshowprice { get; set; }
        public int Frequency { get; set; }
        public long Provinceid { get; set; }
        public string Urlyoutube { get; set; }
        public List<ProductPriceItem> Productpriceitem { get; set; }
        public async Task<BaseResponseResult> Validate(IProductService _productSvc)
        {
            //Check param
            if (this == null) return new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT };
            if (string.IsNullOrEmpty(Productname)) return new BaseResponseResult() { code = -1, errormessage = "Tên sản phẩm không được bỏ trống" };
            else if (Productname.Length > 200) return new BaseResponseResult() { code = -1, errormessage = "Tên sản phẩm không được vượt quá 200 ký tự" };
            if (string.IsNullOrEmpty(Categoryid)) return new BaseResponseResult() { code = -1, errormessage = "Danh mục sản phẩm không được bỏ trống" };
            if (string.IsNullOrEmpty(Shortdescription)) return new BaseResponseResult() { code = -1, errormessage = "Mô tả ngắn không được bỏ trống" };
            else if (Shortdescription.Length > 200) return new BaseResponseResult() { code = -1, errormessage = "Mô tả ngắn không được vượt quá 200 ký tự" };
            if (Producttype == (int)ProductType.Buy)
            {
                if (Quantity <= 0) return new BaseResponseResult() { code = -1, errormessage = "Sản lượng mua không được bỏ trống" };
                if (IsBuy <= 0) return new BaseResponseResult() { code = -1, errormessage = "Nhu cầu mua không được bỏ trống" };
                //if (Frequency <= 0) return new BaseResponseResult() { code = -1, errormessage = "Tần suất mua không được bỏ trống" };
                if (!string.IsNullOrEmpty(Begindate))
                {
                    if (DateTime.ParseExact(UtilitiesExtensions.RemoveHTMLTag(Begindate), "dd/MM/yyyy", new CultureInfo("vi-VN")) < DateTime.Now)
                    {
                        return new BaseResponseResult() { code = -1, errormessage = "Thời gian mua phải từ ngày hôm nay" };
                    }
                }
            }
            else if (Producttype == (int)ProductType.Sell)
            {
                if (Provinceid <= 0) return new BaseResponseResult() { code = -1, errormessage = "Khu vực không được bỏ trống" };
                if (string.IsNullOrEmpty(Productimage) || Productimage.Trim() == "|") return new BaseResponseResult() { code = -1, errormessage = "Vui lòng upload hình ảnh sản phẩm" };
            }
            //else if (Producttype == (int)ProductType.Service)
            //{
            //    if (Provinceid < 0) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng chọn khu vực" };
            //}
            if (string.IsNullOrEmpty(Description)) return new BaseResponseResult() { code = -1, errormessage = "Mô tả không được bỏ trống" };

            ////nếu chọn show giá, thì phải có ít nhất 1 khoảng giá
            if (Isshowprice == true && (Productpriceitem == null || Productpriceitem.Any() == false))
                return new BaseResponseResult() { code = -1, errormessage = "Nếu chọn hiển thị giá, phải có ít nhất 1 khoảng giá" };

            var unitPriceBO = new ProductUnit();
            ////Nếu có chọn khoảng giá
            if (Isshowprice)
            {
                if (Productpriceitem != null && Productpriceitem.Any() && Productpriceitem.FirstOrDefault().Unitprice <= 0) 
                    return new BaseResponseResult() { code = -1, errormessage = "Đơn vị cơ sở không được bỏ trống" };
                if (Productpriceitem != null && Productpriceitem.Any())
                {

                    //kiểm tra trong từng khoảng giá phải có đầy đủ các đơn vị
                    foreach (var item in Productpriceitem)
                    {
                        if (item.Fromquantily <= 0 || item.Toquantily <= 0 || item.PriceSell <= 0 || item.FrequencyUnitId <= 0)
                        {
                            return new BaseResponseResult() { code = -1, errormessage = $"Vui lòng nhập đầy đủ thông tin của khoảng giá {item.Displayorder}" };
                        }

                        if (item.Fromquantily >= item.Toquantily)
                        {
                            return new BaseResponseResult() { code = -1, errormessage = $"Giá từ phải nhỏ hơn giá đến của khoảng giá {item.Displayorder}" };
                        }

                        if (item.Toquantily <= item.Fromquantily)
                        {
                            return new BaseResponseResult() { code = -1, errormessage = $"Giá đến phải lớn hơn giá từ của khoảng giá {item.Displayorder}" };
                        }

                        if (item.Fromquantily < 1)
                            return new BaseResponseResult() { code = -1, errormessage = $"Giá từ của khoảng giá {item.Displayorder} nhỏ nhất là 1" };

                        if (item.Toquantily < 1)
                            return new BaseResponseResult() { code = -1, errormessage = $"Giá đến của khoảng giá {item.Displayorder} nhỏ nhất là 1" };

                        if (item.PriceSell < 1)
                            return new BaseResponseResult() { code = -1, errormessage = $"Giá bán của khoảng giá {item.Displayorder} nhỏ nhất là 1" };

                        var frequencyUnit = await _productSvc.GetProductUnitById(item.FrequencyUnitId);
                        if (frequencyUnit == null || frequencyUnit.Unitid <= 0 || frequencyUnit.Isactived == 0)
                        {
                            return new BaseResponseResult() { code = -1, errormessage = $"Đơn vị tính của khoảng giá {item.Displayorder} không tồn tại" };
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(Urlyoutube) && !UtilitiesExtensions.IsValidUrlV2(Urlyoutube)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập link youtube đúng định dạng" };
            if (string.IsNullOrEmpty(Fullname)) return new BaseResponseResult() { code = -1, errormessage = "Họ và tên người đại diện không được bỏ trống" };
            else if (Fullname.Length > 100) return new BaseResponseResult() { code = -1, errormessage = "Họ tên không được vượt quá 100 ký tự" };
            else if (string.IsNullOrEmpty(ValidateHelper.RemoveSpecialCharacters(Fullname))) return new BaseResponseResult() { code = -1, errormessage = "Họ tên không được chứa ký tự đặc biệt" };
            if (string.IsNullOrEmpty(Position)) return new BaseResponseResult() { code = -1, errormessage = "Chức vụ người đại diện không được bỏ trống" };
            else if (Position.Length > 100) return new BaseResponseResult() { code = -1, errormessage = "Chức vụ không được vượt quá 100 ký tự" };
            else if (string.IsNullOrEmpty(ValidateHelper.RemoveSpecialCharacters(Position))) return new BaseResponseResult() { code = -1, errormessage = "Chức vụ không được chứa ký tự đặc biệt" };
            var refError = "";
            if (string.IsNullOrEmpty(Phonenumber) || UtilitiesExtensions.IsPhoneNumberNew(Phonenumber, ref refError) == false) return new BaseResponseResult() { code = -1, errormessage = "Số điện thoại người đại diện không được bỏ trống" };
            if (!string.IsNullOrEmpty(Email) && UtilitiesExtensions.IsEmail(Email) == false) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập email đúng định dạng" };

            return null;
        }
    }
    public class ProductPriceItem
    {
        public int Displayorder { get; set; }
        public long Fromquantily { get; set; }
        public long Toquantily { get; set; }
        public long FrequencyUnitId { get; set; }
        public decimal PriceSell { get; set; }
        public long Unitprice { get; set; }
    }
    public class RequestUpdatePromoteWebsite
    {
        public long Siteid { get; set; }
        public string Sitename { get; set; }
        public string Url { get; set; }
        public string Logo { get; set; }
        public long Categoryid { get; set; }
        public int Displayorder { get; set; }
        public bool Isshow { get; set; }
    }
    public class RequestInsertFaq
    {
        public string fullName { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string content { get; set; }
        public BaseResponseResult Validate()
        {
            if (this == null) return new BaseResponseResult() { code = -1, errormessage = ConfigConstants.ERROR_DEFAULT };
            if (string.IsNullOrEmpty(fullName)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập họ và tên của bạn" };
            if (string.IsNullOrEmpty(phoneNumber)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập số điện thoại của bạn" };
            if (UtilitiesExtensions.IsPhoneNumber(phoneNumber) == false) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập đúng định dạng số điện thoại" };
            if (string.IsNullOrEmpty(email)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập email của bạn" };
            if (UtilitiesExtensions.IsEmail(email) == false) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập đúng định dạng email" };
            if (string.IsNullOrEmpty(content)) return new BaseResponseResult() { code = -1, errormessage = "Vui lòng nhập góp ý của bạn" };
            return null;
        }
    }
}
