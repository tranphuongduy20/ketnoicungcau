

if (window.location.href.includes('&')) {
    if (window.location.href.split("&")[0] != undefined) {
        window.history.pushState("", "", window.location.href.split("&")[0]);
    }

}

$(document).ready(function () {
    $(".tab-commonlist li").click(function () {
        var data_id = $(this).attr("data-id");
        $('.tab-commonlist li').removeClass('is-active');
        $('.tab-content').removeClass('show-active');
        $(this).addClass('is-active');
        $("#" + data_id).addClass('show-active');
    });


    // Click tab chào bán/ chào mua / doanh nghiep
    $(".tab-commonlist li").click(function () {
        $('.tab-commonlist li').removeClass('is-active');
        $('.tab-content').removeClass('show-active');
        $(this).addClass('is-active');
        if ($(this).is("#tab-sell")) {  // Chào bán
            $("#tab1").addClass('show-active');
            if (
                checkClickTab(
                    ".listproduct")) // check xem có filter lại không, có thì call ajax, không thì chỉ hiển thị
            {

                filterProduct(typeSell);
            }
            else {
                if (totalProductSell > 0)
                    $('#total').text(genTextHaveAll(totalProductSell, 'chào bán'));
            } //gen lại tổng sản phẩm
            //updateURL("SearchType", typeSell);
        }
        else if ($(this).is("#tab-buy")) {  // Mua
            $("#tab2").addClass('show-active');
            if (checkClickTab(".listbuy")) {
                filterProduct(typeBuy);
            }

            else {
                if (totalProductBuy > 0)
                    $('#total').text(genTextHaveAll(totalProductBuy, 'chào mua'));
            }
            //updateURL("SearchType", typeBuy);
        }
        else if ($(this).is("#tab-company")) {  // DN
            $("#tab3").addClass('show-active');
            if (checkClickTab(".listcompany")) {
                filterCompany();
            }
            else {
                if (totalCompany > 0)
                    $('#total').text(genTextHaveAll(totalCompany, 'doanh nghiệp'));
            }
            //updateURL("SearchType", typeCompany);
        }
    });

});

function filterProduct(type) {
    query.PageIndex = 0;
    if (type === typeBuy) {
        query.SearchType = 1;
    } else {
        query.SearchType = 2;
    }
    $.ajax({
        url: '/Search/GetListProduct',
        type: 'POST',
        data: query,
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res == null || res.listProduct === '' || res.listProduct.trim() === '') {
                if (query.SearchType === typeBuy) {
                    var blockEmpty = '<li id="empty"><i class="empty"></i>Không có sản phẩm chào mua nào phù hợp với tiêu chí tìm kiếm!</li>';
                    $("#listproduct-buy").html(blockEmpty);
                    $('#total').text(genTextHaveAll(res.total, 'doanh nghiệp'));
                }
                else if (query.SearchType === typeSell) {
                    var blockEmpty = '<li id="empty"><i class="empty"></i>Không có sản phẩm chào bán nào phù hợp với tiêu chí tìm kiếm!</li>';
                    $("#listproduct-sell").html(blockEmpty);
                }
            } else {
                if (type == typeBuy) {
                    $('#listproduct-buy').html(res.listProduct);
                    $('#total').text(genTextHaveAll(res.total, 'chào mua')); //gen lại tổng sản phẩm
                    handleReadMore("#readmore-buy", res.left);
                    totalProductBuy = res.total;
                } else {
                    $('#listproduct-sell').html(res.listProduct);
                    $('#total').text(genTextHaveAll(res.total, 'chào bán')); //gen lại tổng sản phẩm
                    handleReadMore("#readmore-sell", res.left);
                    totalProductSell = res.total;
                }
            }
        },
        error: function (e) {
            HidePreloader();
        }
    });
}
function filterCompany() {
    query.PageIndex = 0;
    query.SearchType = typeCompany;
    $.ajax({
        url: 'Search/GetListCompany',
        type: 'POST',
        data: query,
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();

            if (res == null || res.listCompany === '' || res.listCompany.trim() === '') {
                var blockEmpty = '<li id="empty"><i class="empty"></i>Không có doanh nghiệp nào phù hợp với tiêu chí tìm kiếm!</li>';
                $(".listcompany").html(blockEmpty);
                $('#total').text(genTextHaveAll(0, 'doanh nghiệp'));
            } else {
                $('.listcompany').append(res.listCompany);
                $('#total').text(genTextHaveAll(res.total, 'doanh nghiệp')); //gen lại tổng sản phẩm
                handleReadMore("#readmore-company", res.left);
                totalCompany = res.total;
            }
        },
        error: function (e) {
            HidePreloader();
        }
    });
}

function viewMoreProduct(type) {
    query.PageIndex = query.PageIndex + 1;
    if (type === typeBuy) {
        query.SearchType = 1;
    } else {
        query.SearchType = 2;
    }
    $.ajax({
        url: 'Search/GetListProduct',
        type: 'POST',
        data: query,
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res != "") {

                if (type === typeBuy) {
                    $('#listproduct-buy').append(res.listProduct);
                    handleReadMore("#readmore-buy", res.left);
                } else {
                    $('#listproduct-sell').append(res.listProduct);
                    handleReadMore("#readmore-sell", res.left);
                }

                //updateURL("PageIndex", query.PageIndex);

            }
        },
        error: function (e) {
            HidePreloader();
            console.log("Lỗi gọi action");
        }
    });
}

function viewMoreCompany() {
    query.PageIndex = query.PageIndex + 1;
    query.SearchType = sizeCompany;
    $.ajax({
        url: 'Search/GetListCompany',
        type: 'POST',
        data: query,
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();

            if (res != "") {
                $('.listcompany').append(res.listCompany);
                handleReadMore("#readmore-company", res.left);
                //updateURL("PageIndex", query.PageIndex);
            }
        },
        error: function (e) {
            HidePreloader();
        }
    });
}


// Hàm xử lý button Xem thêm
function handleReadMore(obj, left) {
    if (left > 0) {
        $(obj).show();
        $(obj + " span").text(`Xem thêm ${left} sản phẩm`);
    }
    else
        $(obj).hide();
}

// Hàm generate text Có ... sản phẩm
function genTextHaveAll(total, type) {
    return `Có ${total} kết quả ${type} ‘${query.kw}’`;
}

function checkClickTab(obj) {
    return false || $(obj + " li").length === 0; //true = gọi ajax - false = không làm gì
}

function updateURL(key, value) {
    var url = window.location.href;
    var newUrl = updateQueryStringParameter(url, key, value);
    window.history.pushState("", "", newUrl);

}

function updateQueryStringParameter(uri, key, value) {
    var re = new RegExp("([?&])" + key + "=.*?(&|$)", "i");
    var separator = uri.indexOf('?') !== -1 ? "&" : "?";
    if (uri.match(re)) {
        return uri.replace(re, '$1' + key + "=" + value + '$2');
    }
    else {
        return uri + separator + key + "=" + value;
    }
}