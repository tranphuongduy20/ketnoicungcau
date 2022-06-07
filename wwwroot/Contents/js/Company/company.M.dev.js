var isShow = false;
var isHasHideSubCateActive = false;
var firstRun = true;

$(document).ready(function () {
    if (query.IsMobile != undefined && query.IsMobile) {
        HandleSubCateFilter();
        $('#show-all-cate').click(function () {
            $(".quicklink").find("a:hidden").show();
            if (isShow === false) {
                $(this).text('Thu gọn');
                $(this).addClass('arrow-menuC');
                isShow = true;
            } else {
                $(this).text('Xem tất cả danh mục');
                $(this).removeClass('arrow-menuC');
                HandleSubCateFilter();
                isShow = false;
            }
        });
    }


    // TH vào bằng link
    var hash_value = document.location.hash;
    if (hash_value) {
        query.StrListCateId = hash_value.replace('#c=', '');
        var arrCateActive = hash_value.replace('#c=', '').split(',');
        if (arrCateActive !== undefined) {
            jQuery.each(arrCateActive, function (i, val) {
                $(".filter-company .quicklink").find("a[data-id='" + val + "']").addClass('active');
            });
        }
        getCompanyList();
        firstRun = true;
        HandleSubCateFilter();
    }

    $('.quicklink a.cate__filter').click(function () {
        $(this).toggleClass('active');

        var id = $(this).data('id');
        var url = $(this).data('url');

        if (id !== undefined && url !== undefined) {
            if (id == 0 && url == 'all') {
                if ($(this).hasClass('active')) {
                    window.location.href = '/doanh-nghiep';
                }
            }
            else {
                if ($(this).hasClass('active') && $(".filter-company .quicklink").find("a[data-id='" + 0 + "'].active").length > 0) {
                    $(".filter-company .quicklink").find("a[data-id='" + 0 + "']").removeClass('active');
                }
            }

            var $cate = $('.filter-company .quicklink a.active');
            if ($cate.length > 0) {
                if ($cate.length == 1) {
                    url = $('.filter-company .quicklink a.active').data('url');
                    window.location.href = '/doanh-nghiep/' + url;
                }
                else {
                    var numberArray = new Array();
                    $cate.each(function () {
                        numberArray.push($(this).data('id'));
                    });
                    numberArray = numberArray.filter(unique);
                    query.StrListCateId = numberArray.join();
                    getCompanyList();
                }
            }
            else {
                window.location.href = '/doanh-nghiep';
            }
        }
    });

    if ($('.content').height() > 300) {
        $('.content').addClass('short');
        $('.article a.btn-seemore').show();
    }
    else {
        $(".article .btn-seemore").remove();
        $(".article .bg-article").remove();
        $(".article .content").removeClass("short");
    }
});

function showAllContent() {
    $(".article .btn-seemore").remove();
    $(".article .bg-article").remove();
    $(".article .content").removeClass("short");
}


function HandleSubCateFilter() {
    var quicklinkPos = $('.quicklink').position();
    var cateHeight = $('a[data-url=all]').outerHeight(true);
    var area = quicklinkPos.top + cateHeight * 2;
    var isShowBtn = false;
    if (firstRun) {
        $('.cate__filter').each(function () {
            var boxPos = $(this).position();
            var y = boxPos.top;
            if (y >= area) {
                if ($(this).hasClass('active'))
                    isHasHideSubCateActive = true;
                //$(this).hide();
                //isShowBtn = true;
            }
        });
        firstRun = false;
    }
    else isHasHideSubCateActive = false;
    if (isHasHideSubCateActive == false) {
        $('.cate__filter').each(function () {
            var boxPos = $(this).position();
            var y = boxPos.top;
            if (y >= area) {
                //if ($(this).hasClass('active'))
                //    isHasHideSubCateActive = true;
                $(this).hide();
                isShowBtn = true;
            }
        });
    }
    else {
        $('#show-all-cate').text('Thu gọn');
        $('#show-all-cate').addClass('arrow-menuC');
        isShow = true;
    }
    if (isShowBtn) $('#show-all-cate').show();

}


// Hàm get company
var flagCompanyList = true;
function getCompanyList() {
    if (!flagCompanyList) return;
    flagCompanyList = false;
    query.IsFilterManyCate = '1';
    $.ajax({
        url: '/Company/GetCompanyList',
        type: 'POST',
        data: query,
        cache: false,
        async: query.PageIndex == 0,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (e) {
            $(".preloader").hide();
            flagCompanyList = true;
            $(".listcompany").html(e.listcompanys);
            var total = e.total;
            if (total !== undefined) {
                $('.filter-company .number-show').html(total + " Doanh nghiệp");
            }

            var hashOut = 'c=' + query.StrListCateId;
            document.location.hash = hashOut;
        },
        error: function () {
            console.log("Lỗi gọi action GetCompanyList");
        }
    });
}

// Hàm lọc lại những giá trị bị trùng trong mảng
function unique(value, index, self) {
    return self.indexOf(value) === index;
}

// Hàm xem thêm doanh nghiệp
var flagViewMoreCompany = true;
function viewMoreCompany() {
    if (!flagViewMoreCompany) return;
    flagViewMoreCompany = false;
    query.PageIndex += 1;
    $.ajax({
        url: '/Company/GetCompanyList',
        type: 'POST',
        data: query,
        cache: false,
        async: query.PageIndex == 0,
        beforeSend: function () {
            $("#preloader").show();
        },
        success: function (e) {
            $("#preloader").hide();
            flagViewMoreCompany = true;
            $(".listcompany").append($(e.listcompanys));
            var total = e.total;
            if (total !== undefined) {
                if (total > (query.PageIndex + 1) * query.PageSize) {
                    $('.viewmore_company a span').text('Xem thêm ' + total - (query.PageIndex + 1) * query.PageSize + ' doanh nghiệp');
                }
                else {
                    $('.viewmore_company').hide();
                }
            }

            //var hashOut = 'c=' + query.StrListCateId;
            //document.location.hash = hashOut;
        },
        error: function () {
            console.log("Lỗi gọi action GetCompanyList");
        }
    });
}

// Hàm xem thêm sản phẩm buy/sell
var flagViewMoreProduct = true;
function viewMoreProduct(productType) {
    if (!flagViewMoreProduct) return;
    flagViewMoreProduct = false;

    query.ProductType = productType;

    //ProductType: 1 = buy, 2 = sell
    if (productType == '1') {
        query.PageIndexBuy += 1;
    }
    else {
        query.PageIndexSell += 1;
    }

    $.ajax({
        url: '/Company/GetProductList',
        type: 'POST',
        data: query,
        cache: false,
        async: false,
        beforeSend: function () {
            //$("#preloader").show();
        },
        success: function (e) {
            //$("#preloader").hide();
            flagViewMoreProduct = true;
            var total = e.total;
            if (productType == '1') {
                $(".block__buy .listbuy").append($(e.listproducts));
                if (total !== undefined) {
                    if (total > (query.PageIndexBuy + 1) * query.PageSizeBuy) {
                        $('.block__buy a.btn-readmore span').text('Xem thêm ' + (total - (query.PageIndexBuy + 1) * query.PageSizeBuy) + ' sản phẩm chào mua');
                    }
                    else {
                        $('.block__buy a.btn-readmore').hide();
                    }
                }
            }
            else {
                $(".block__sell .listproduct").append($(e.listproducts));
                if (total !== undefined) {
                    console.log(total - (query.PageIndexSell + 1) * query.PageSizeSell);
                    if (total > (query.PageIndexSell + 1) * query.PageSizeSell) {
                        $('.block__sell a.btn-readmore span').text('Xem thêm ' + (total - (query.PageIndexSell + 1) * query.PageSizeSell) + ' sản phẩm chào bán');
                    }
                    else {
                        $('.block__sell a.btn-readmore').hide();
                    }
                }
            }

            //var hashOut = 'c=' + query.StrListCateId;
            //document.location.hash = hashOut;
        },
        error: function () {
            console.log("Lỗi gọi action GetCompanyList");
        }
    });
}