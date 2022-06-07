using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ketnoicungcau.vn.Models
{
    public class DataResponse
    {
        public int recordsFiltered { get; set; }
        public int recordsTotal { get; set; }
        public dynamic data { get; set; }
    }

    public class DataUser
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string Phonenumber { get; set; }
        public string Fullname { get; set; }
        public string Active { get; set; }
        public string Createddate { get; set; }
        public string Action { get; set; }
    }

    public class ProductCategoryResponse
    {
        public long Id { get; set; }
        public string Categoryname { get; set; }
        public string Alias { get; set; }
        public string Displayorder { get; set; }
        public string Description { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }

    public class NewsCategoryResponse
    {
        public long Id { get; set; }
        public string Categoryname { get; set; }
        public string Alias { get; set; }
        public string Displayorder { get; set; }
        public string Description { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }

    public class PlaceBannerResponse
    {
        public long Id { get; set; }
        public string Placebannername { get; set; }
        public string Sizedesktop { get; set; }
        public string Sizemobile { get; set; }
        public string Description { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }

    public class BannerResponse
    {
        public long Id { get; set; }
        public string Bannername { get; set; }
        public long Displayorder { get; set; }
        public string Placebanner { get; set; }
        public string Desktop { get; set; }
        public string Mobile { get; set; }
        public string Url { get; set; }
        public string Time { get; set; }
        public string Status { get; set; }
        public string Action { get; set; }
    }

    public class DemandResponse
    {
        public long Id { get; set; }
        public string Demandname { get; set; }
        public string Alias { get; set; }
        public string Position { get; set; }
        public string Description { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }

    public class NewsResponse
    {
        public long Id { get; set; }
        public string Coverimage { get; set; }
        public string Title { get; set; }
        public string Alias { get; set; }
        public string Category { get; set; }
        public string Displayorder { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }

    public class DataProductUnit
    {
        public long Id { get; set; }
        public string Unitname { get; set; }
        public string Type { get;set; }
        public string Alias { get; set; }
        public string Displayorder { get; set; }
        public string Description { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }

    public class DataSpecialties
    {
        public long Id { get; set; }
        public string Specialtiesname { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }

    public class ProvinceResponse
    {
        public long Id { get; set; }
        public string Provincename { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }

    public class CompanyResponse
    {
        public long Id { get; set; }
        public string Logo { get; set; }
        public string Companyinfo { get; set; }
        public string Registerdate { get; set; }
        public string Companycontact { get; set; }
        public string Representinfo { get; set; }
        public string Saleproduct { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }
    public class TopCompanyResponse
    {
        public long Id { get; set; }
        public string Companyname { get; set; }
        public string Displayorder { get; set; }
        public string Startdate { get; set; }
        public string Enddate { get; set; }
        public string Status { get; set; }
        public string Action { get; set; }
    }

    public class DealsCompanyResponse
    {
        public long Id { get; set; }
        public string Buyer { get; set; }
        public string Seller { get; set; }
        public string Product { get; set; }
        public string Status { get; set; }
        public string Description { get; set; }
        public string Action { get; set; }
    }

    public class ProductResponse
    {
        public long Id { get; set; }
        public string Avatar { get; set; }
        public string Productname { get; set; }
        public string Company { get; set; }
        public string Demand { get; set; }
        public string Price { get; set; }
        public string Packing { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }

    public class DataPermissionResponse
    {
        public int recordsFiltered { get; set; }
        public int recordsTotal { get; set; }
        public IEnumerable<DataPermission> data { get; set; }
    }
    public class DataPermission
    {
        public long Id { get; set; }
        public string Function { get; set; }
        public string Permissionname { get; set; }
        public string Description { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }

    public class DataGroupPermissionResponse
    {
        public int recordsFiltered { get; set; }
        public int recordsTotal { get; set; }
        public IEnumerable<DataGroupPermission> data { get; set; }
    }
    public class DataGroupPermission
    {
        public long Id { get; set; }
        public string Groupname { get; set; }
        public string Description { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }

    public class DataProductStandard
    {
        public long Id { get; set; }
        public string Standardname { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }
    public class DataPermissionGroupResponse
    {
        public int recordsFiltered { get; set; }
        public int recordsTotal { get; set; }
        public IEnumerable<DataPermissionGroup> data { get; set; }
    }
    public class DataPermissionGroup
    {
        public string Grouppermission { get; set; }
        public string Permission { get; set; }
    }
    public class DataPromoteWebsite
    {
        public long Id { get; set; }
        public string Logo { get; set; }
        public string Websitename { get; set; }
        public string Url { get; set; }
        public string Productcategory { get; set; }
        public string Displayorder { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }
    public class DataFaq
    {
        public long Id { get; set; }
        public string Contentask { get; set; }
        public string Fullnameask { get; set; }
        public string Status { get; set; }
        public string Contentrep { get; set; }
        public string Fullnamerep { get; set; }
        public string Displayorder { get; set; }
        public string Active { get; set; }
        public string Action { get; set; }
    }
}

