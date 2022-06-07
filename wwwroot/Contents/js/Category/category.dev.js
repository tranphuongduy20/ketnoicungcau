var pageIndexSell = 0;
var pageIndexBuy = 0;
var flagQueryChangeSell = false;
var flagQueryChangeBuy = false;
var root = window.location.origin;
var isLoadAjax = false;
var minItemSort = 6;

$(document).ready(function () {

    //#region OwlCarousel
    $('.catSlider').owlCarousel({
        loop: false,
        items: 2,
        dots: false,
        nav: true,
        margin: 20,
        slideBy: 2,
        autoplay: true,
        autoplayTimeout: 3000,
        autoplayHoverPause: true
    });

    $('.banner-promo').owlCarousel({
        loop: true,
        items: isMobile == true ? 1 : 2,
        dots: true,
        nav: false,
        margin: 10,
        autoplay: true,
        autoplayTimeout: 3000,
        autoplayHoverPause: true
    });
    //#endregion

    //#region Filter

    OffScroll();

    // Click box filter
    $(document).on('click', ".filter-txt", function (e) {
        e.preventDefault();
        var notthis = $('.current-filter').not(this);
        notthis.toggleClass('current-filter').next('.filter-main').fadeToggle(300);
        $(this).toggleClass('current-filter').next().fadeToggle("fast");
        CheckOverlayFilter();
    });

    $(document).on('click', ".cate-overlay", function (e) {
        $(".cate-overlay").fadeOut(200);

    });

    // Chọn filter Cate
    $(document).on("click", "#list-cate .filter-main ul li", function () {
        $(this).toggleClass('act');
        setFlagQueryChange();
        loadCheckOneCate();
        filterProduct();
        /*loadCheckAjax();*/
    });

    // Chọn filter khác
    $(document).on("click", "#list-specialist .filter-main ul li, #list-province .filter-main ul li, #list-standard .filter-main ul li", function () {
        $(this).toggleClass('act');
        setFlagQueryChange();
        isLoadAjax = true;
        filterProduct();
    });

    // Search filter
    $(document).on('keyup', ".filter-search input", function (e) {
        searchFilter($(this));
    });

    // Off popup khi click ra ngoài popup
    $(document).click(function (e) {
        closePopupFilter(e);
    });

    //#endregion

    //#region Soft

    OffSort();

    // Click box sắp xếp - sort
    $(document).on('click', ".click-sort", function () {
        $(".sort-main").fadeToggle(300);
        $(this).toggleClass('sort-rotate');
    });

    // Cick chọn loại sort
    $(document).on("click", ".sort-main p", function () {
        if ($(this).hasClass('active')) return;
        isLoadAjax = true;
        $(".sort-main p.active").removeClass('active');
        $(this).addClass('selected').siblings().removeClass('selected');
        query.OrderType = $(this).data('id');
        setFlagQueryChange()
        filterProduct();
        replaceTextFilterSort($(this));
    });

    //#endregion

    // Apply hash to query rồi gọi lại hàm replace sản phẩm
    if (location.hash != '') {
        var hasIn = location.hash.replace('#', '');
        if (hasIn != '') {
            if (currentType == typeSell)
                flagQueryChangeBuy = true;
            else
                flagQueryChangeSell = true;
            callFilterFromHash(hasIn);
        }
    }
    handleFilterText();

    // Click tab chào bán/ chào mua
    $(".tab-commonlist li").click(function () {
        $('.tab-commonlist li').removeClass('is-active');
        $('.tab-content').removeClass('show-active');
        $(this).addClass('is-active');

        if ($(this).is("#tab-sell")) {  // Chào bán
            window.history.pushState("", "Danh mục chào bán", "/chao-ban");
            currentType = typeSell;
            $("#tab1").addClass('show-active');
        }
        else {  // Mua
            window.history.pushState("", "Danh mục chào mua", "/chao-mua");
            currentType = typeBuy;
            $("#tab2").addClass('show-active');
        }

        collectParam();
        var hasOut = toHash(query);
        if (hasOut != "")
            document.location.hash = hasOut;
        /*loadCheckAjax();*/
        loadCheckEmptyList(); //check xem list có đang empty do đi link từ cate k có sản phẩm hay không, nếu empty thì fill sản phẩm vào
    });

    // Click xem thêm 
    $(document).on("click", ".btn-readmore", function () {
        viewMoreProduct();
    });

    // Hiện search null nếu không có list item
    if ($("#tab1 .listproduct li").length <= 0) {
        $("#tab1 .list-body").hide();
        $("#tab1 .search-empty").show();
        if ($('#tab1 #list-cate .filter-main ul li.act').length <= 0) // Nếu truy cập cate không có sản phẩm hoặc inactive 
            flagEmptySell = true;
            
    }
    if ($("#tab2 .listbuy li").length <= 0) {
        $("#tab2 .list-body").hide();
        $("#tab2 .search-empty").show();
        if ($('#tab1 #list-cate .filter-main ul li.act').length <= 0) // Nếu truy cập cate không có sản phẩm hoặc inactive
            flagEmptyBuy = true;
    }

    $("#total-buy").text(genTextHaveAll($("#total-buy").data("total"), typeBuy));
    $("#total-sell").text(genTextHaveAll($("#total-sell").data("total"), typeSell));
});

//#region Ajax

// Hàm chính gọi replace sản phẩm
function filterProduct() {
    collectParam();
    /*query.PageIndex = 0;*/
    query.IsGetAll = true;
    $.ajax({
        url: root + '/Category/GetListProduct',
        type: 'POST',
        data: query,
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
            $(".search-empty").hide();
        },
        success: function (res) {
            HidePreloader();
            if (res != "") {

                if (res == null || res.listproducts === '' || res.listproducts.trim() === '') {
                    if (currentType == typeBuy) {
                        $("#tab2 .list-body").hide();
                        $("#tab2 .search-empty").show();
                    }
                    else {
                        $("#tab1 .list-body").hide();
                        $("#tab1 .search-empty").show();
                    }
                }
                else {
                    if (currentType == typeBuy) {
                        $("#tab2 .list-body").show();
                        $("#tab2 .search-empty").hide();
                        $('.listbuy').html(res.listproducts);
                        if (isMobile === false)
                            $('#tab2 .number').text(genTextHaveAll(res.total)); //gen lại tổng sản phẩm
                        handleReadMore("#readmore-buy", res.left);
                        flagQueryChangeBuy = false;
                    }
                    else {
                        $("#tab1 .list-body").show();
                        $("#tab1 .search-empty").hide();
                        $('.listproduct').html(res.listproducts);
                        if (isMobile === false)
                            $('#tab1 .number').text(genTextHaveAll(res.total));
                        handleReadMore("#readmore-sell", res.left);
                        flagQueryChangeSell = false;
                    }
                }
               
                OffSort();

                if (isLoadAjax) {
                    var hasOut = toHash(query);
                    if (hasOut != "")
                        document.location.hash = hasOut;
                    else
                        history.replaceState(null, null, ' ');
                }
            }
        },
        error: function (e) {
            HidePreloader();
        }
    });
}

// Hàm xem thêm sản phẩm
function viewMoreProduct() {
    console.log("viewmore product");
    genPageIndex();
    collectParam();
    query.IsGetAll = false;
    $.ajax({
        url: root + '/Category/GetListProduct',
        type: 'POST',
        data: query,
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();

            if (res != "") {
                if (currentType == typeBuy) {
                    $('.listbuy').append(res.listproducts);
                    handleReadMore("#readmore-buy", res.left);
                }
                else {
                    $('.listproduct').append(res.listproducts);
                    handleReadMore("#readmore-sell", res.left);
                }
            }
            var hasOut = toHash(query);
            document.location.hash = hasOut;

        },
        error: function (e) {
            HidePreloader();
        }
    });
}

//#endregion

//#region Hash - Query

// Hàm collect tất cả param gán vô biến query 
function collectParam() {
    query.SearchType = currentType;
    var obj = "#tab2";
    if (currentType == typeSell) {
        obj = "#tab1";
        var $oder = $('.sort-main p.selected');
        if ($oder.length > 0) {
            query.OrderType = $('.sort-main p.selected').data('id');
        } else {
            query.OrderType = 1;
        }
        query.PageIndex = pageIndexSell;
    }
    else {
        query.PageIndex = pageIndexBuy;
        query.OrderType = 1;
    }
 
    var $cate = $(obj + ' #list-cate .filter-main ul li.act');
    if ($cate.length > 0) {
        var numberArray = new Array();
        $cate.each(function () {
            numberArray.push($(this).data('id'));
        });
        numberArray = numberArray.filter(unique);
        query.StrListCategory = numberArray.join();
    } else {
        query.StrListCategory = '';
    }

    var $specialist = $(obj + ' #list-specialist .filter-main ul li.act');
    if ($specialist.length > 0) {
        var numberArray = new Array();
        $specialist.each(function () {
            numberArray.push($(this).data('id'));
        });
        numberArray = numberArray.filter(unique);
        query.StrListSpecialist = numberArray.join();
    } else {
        query.StrListSpecialist = '';
    }

    var $province = $(obj + ' #list-province .filter-main ul li.act');
    if ($province.length > 0) {
        var numberArray = new Array();
        $province.each(function () {
            numberArray.push($(this).data('id'));
        });
        numberArray = numberArray.filter(unique);
        query.StrListProvince = numberArray.join();
    } else {
        query.StrListProvince = '';
    }

    var $standard = $(obj + ' #list-standard .filter-main ul li.act');
    if ($standard.length > 0) {
        var numberArray = new Array();
        $standard.each(function () {
            numberArray.push($(this).data('id'));
        });
        numberArray = numberArray.filter(unique);
        query.StrListStandard = numberArray.join();
    } else {
        query.StrListStandard = '';
    }

}

// Apply Hash
function applyHash() {
    if (location.hash != '') {
        var hasIn = location.hash.replace('#', '');
        if (hasIn != '') {
            callFilterFromHash(hasIn);
        }
    }
}

// Hàm update lại query và call ajax update lại data khi load trang theo link có hash
function callFilterFromHash(hasIn) {
    getValueFromHashKey(hasIn);
    mapQueryToFilter();
    filterProduct();
}

// Hàm get value from hash khi mới vào trang có link hash
function getValueFromHashKey(hashIn) {
    if (hashIn === undefined || hashIn == '') hashIn = location.hash.replace('#', '');
    if (hashIn === undefined || hashIn == '') return null;
    var arr = hashIn.split('&');
    if (arr.length == 0) return null;

    for (var i in arr) {
        var item = arr[i];
        var val = item.split('=');
        if (val.length != 2) continue;
        if (val[0] == 'c') {
            query.StrListCategory = val[1];
        }
        else if (val[0] == 's') {
            query.StrListSpecialist = val[1];
        }
        else if (val[0] == 'p') {
            query.StrListProvince = val[1];
        }
        else if (val[0] == 't') {
            query.StrListStandard = val[1];
        }
        else if (val[0] == 'o') {
            query.OrderType = parseInt(val[1]);
        }
        else if (val[0] == 'pi') {
            query.PageIndex = parseInt(val[1]);
            console.log(query);
        }
    }
}

// Hàm add active element from query
function mapQueryToFilter() {

    var obj = "#tab2";
    pageIndexBuy = query.PageIndex;
    if (currentType == typeSell) {
        obj = "#tab1";
        if (query.OrderType !== undefined && query.OrderType > 1) {
            $('.sort-main p').each(function () {
                if ($(this).data('id') == query.OrderType) {
                    $('.sort-main p.selected').removeClass('selected');
                    $(this).addClass('selected');
                    replaceTextFilterSort($(this));
                }
            });
        }
        pageIndexSell = query.PageIndex;
    }
        
    if (query.StrListCategory !== undefined && query.StrListCategory != null && query.StrListCategory != '') {
        var arr = (query.StrListCategory + "").split(',');
        $(obj + ' #list-cate .filter-main ul li').each(function () {
            if (arr.indexOf($(this).data('id') + '') > -1) {
                $(this).addClass('act');
            } else {
                $(this).removeClass('act');
            }
        });
    }

    if (query.StrListSpecialist !== undefined && query.StrListSpecialist != null && query.StrListSpecialist != '') {
        var arr = (query.StrListSpecialist + "").split(',');
        $(obj + ' #list-specialist .filter-main ul li').each(function () {
            if (arr.indexOf($(this).data('id') + '') > -1) {
                $(this).addClass('act');
            } else {
                $(this).removeClass('act');
            }
        });
    }

    if (query.StrListProvince !== undefined && query.StrListProvince != null && query.StrListProvince != '') {
        var arr = (query.StrListProvince + "").split(',');
        $(obj + ' #list-province .filter-main ul li').each(function () {
            if (arr.indexOf($(this).data('id') + '') > -1) {
                $(this).addClass('act');
            } else {
                $(this).removeClass('act');
            }
        });
    }

    if (query.StrListStandard !== undefined && query.StrListStandard != null && query.StrListStandard != '') {
        var arr = (query.StrListStandard + "").split(',');
        $(obj + ' #list-standard .filter-main ul li').each(function () {
            if (arr.indexOf($(this).data('id') + '') > -1) {
                $(this).addClass('act');
            } else {
                $(this).removeClass('act');
            }
        });
    }
   
}

// Hàm gán lại hash từ querystring
function toHash(queryIn) {
    if (queryIn === undefined) return '';
    var hashOut = '';
    if (queryIn.StrListCategory !== undefined && queryIn.StrListCategory != '') {
        hashOut += 'c=' + queryIn.StrListCategory + '&';
    }
    if (queryIn.StrListSpecialist !== undefined && queryIn.StrListSpecialist != '') {
        hashOut += 's=' + queryIn.StrListSpecialist + '&';
    }
    if (queryIn.StrListProvince !== undefined && queryIn.StrListProvince != '') {
        hashOut += 'p=' + queryIn.StrListProvince + '&';
    }
    if (queryIn.StrListStandard !== undefined && queryIn.StrListStandard != '') {
        hashOut += 't=' + queryIn.StrListStandard + '&';
    }
    if (queryIn.OrderType !== undefined && queryIn.OrderType > 1) {
        hashOut += 'o=' + queryIn.OrderType + '&';
    }
    if (queryIn.PageIndex !== undefined && queryIn.PageIndex > 0) {
        hashOut += 'pi=' + queryIn.PageIndex + '&';
    }
    if (hashOut[hashOut.length - 1] == '&') {
        hashOut = hashOut.substring(0, hashOut.length - 1);
    }
    return hashOut;
}

//#endregion

//#region TextHandle
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
function genTextHaveAll(total, type = -1) {

    var cates = "";
    var specials = "";
    var provinces = "";

    var obj = ".tab-all .show-active ";
    if (type == typeBuy)
        obj = "#tab2 ";
    else if (type == typeSell)
        obj = "#tab1 ";
    if ($(obj + '#list-cate .filter-main ul li.act').length > 0)
        cates = $(obj + "#list-cate .filter-txt").text();
    if ($(obj + '#list-specialist .filter-main ul li.act').length > 0)
        specials = $(obj + "#list-specialist .filter-txt").text();
    if ($(obj + '#list-province .filter-main ul li.act').length > 0)
        provinces = $(obj + "#list-province .filter-txt").text();
    if ($(obj + '#list-standard .filter-main ul li.act').length > 0)
        standards = $(obj + "#list-standard .filter-txt").text();

    var stringHaveAll = `Có ${total} sản phẩm`;

    if (cates != "") {
        console.log(cates);
        stringHaveAll += ` ${cates}`;
        if (specials != "") stringHaveAll += `, ${specials}`
    }
    else {
        if (specials != "") stringHaveAll += ` ${specials}`
    }

    if (provinces != "") {
        stringHaveAll += ` ở ${provinces}`;
    }
    
    return stringHaveAll;
}

// Hàm replace text active filter sắp xếp
function replaceTextFilterSort(e) {
    if (e !== undefined) {
        if (e.text() !== '') {
            $(".sort-show").html(e.text());
        }
    } else if ($(".sort-main p.selected").length > 0) {
        $(".sort-show").html($(".sort-main p.selected").text());
    }
}

//#endregion

//#region Utilities

// Đặt lại label Filter
function handleFilterText() {

    templateHandleText("#tab1");

    templateHandleText("#tab2");
}

// Template Handle text filter
function templateHandleText(obj) {

    var $cate = $(obj + ' #list-cate .filter-main ul li.act');
    if ($cate.length > 0) {
        var textArray = new Array();
        $cate.each(function () {
            textArray.push($(this).text());
        });
        $(obj + " #list-cate .filter-txt").addClass("selected");
        $(obj + " #list-cate .filter-txt").text(textArray.join(", "));
    } else {
        $(obj + " #list-cate .filter-txt").removeClass("selected");
        $(obj + " #list-cate .filter-txt").text("Danh mục sản phẩm");
    }

    var $specialist = $(obj + ' #list-specialist .filter-main ul li.act');
    if ($specialist.length > 0) {
        var textArray = new Array();
        $specialist.each(function () {
            textArray.push($(this).text());
        });
        $(obj + " #list-specialist .filter-txt").addClass("selected");
        $(obj + " #list-specialist .filter-txt").text(textArray.join(", "));
    } else {
        $(obj + " #list-specialist .filter-txt").removeClass("selected");
        $(obj + " #list-specialist .filter-txt").text("Đặc sản vùng miền");
    }

    var $province = $(obj + ' #list-province .filter-main ul li.act');
    if ($province.length > 0) {
        var textArray = new Array();
        $province.each(function () {
            textArray.push($(this).text());
        });
        $(obj + " #list-province .filter-txt").addClass("selected");
        $(obj + " #list-province .filter-txt").text(textArray.join(", "));
    } else {
        $(obj + " #list-province .filter-txt").removeClass("selected");
        $(obj + " #list-province .filter-txt").text("Tỉnh thành");
    }

    var $standard = $(obj + ' #list-standard .filter-main ul li.act');
    if ($standard.length > 0) {
        var textArray = new Array();
        $standard.each(function () {
            textArray.push($(this).text());
        });
        $(obj + " #list-standard .filter-txt").addClass("selected");
        $(obj + " #list-standard .filter-txt").text(textArray.join(", "));
    } else {
        $(obj + " #list-standard .filter-txt").removeClass("selected");
        $(obj + " #list-standard .filter-txt").text("Tiêu chuẩn");
    }
}

// Search 
function searchFilter(me) {
    var term = $(me).val().toString().toLowerCase();
    $(me).parents('.filter-search').next().closest('.list-distrist').find("a").each(function (idx, elm) {
        var html = $(elm).text().toString().toLowerCase();
        if (stringToSlug(html).includes(stringToSlug(term))) {
            $(elm).parent('li').show();
        } else {
            $(elm).parent('li').hide();
        }
    });
    OffScroll();
}

// Gen pageIndex
function genPageIndex() {
    if (currentType == typeSell) {
        pageIndexSell = pageIndexSell + 1;
    }
    else {
        pageIndexBuy = pageIndexBuy + 1;
    }
}

// Hàm gắn flag khi change Query
function setFlagQueryChange() {
    flagQueryChangeBuy = true;
    flagQueryChangeSell = true;
}

// Hàm kiểm tra khi nào gọi link, khi nào gọi ajax
//function loadCheckAjax() {

//    collectParam();

//    if (flagEmptyBuy && currentType == typeBuy) {
//        isLoadAjax = true;
//        filterProduct();
//        flagEmptyBuy = false;
//        console.log("vô 1");
//    }

//    else if (flagEmptySell && currentType == typeSell) {
//        isLoadAjax = true;
//        filterProduct();
//        flagEmptySell = false;
//        console.log("vô 2");
//    }
    
//    else if (query.StrListProvince !== undefined && query.StrListProvince != null && query.StrListProvince != '') {
//        isLoadAjax = true;
//        filterProduct();
//        console.log("1");
//    }
//    else if (query.StrListSpecialist !== undefined && query.StrListSpecialist != null && query.StrListSpecialist != '') {
//        isLoadAjax = true;
//        filterProduct();
//        console.log("2");
//    }
//    else if (query.StrListCategory !== undefined && query.StrListCategory != null && query.StrListCategory != '') {
//        if (query.StrListCategory.split(',').length == 1) {
//            $('#list-cate .filter-main ul li').each(function () {
//                if (query.StrListCategory == $(this).data('id')) {
//                    if ($(this).data('href') !== undefined && $(this).data('href') !== "") {
//                        console.log("3");
//                        isLoadAjax = false;
//                        reloadURL($(this).data('href'));
//                        filterProduct();
//                    }
//                }
//            });
//        }
//        else {
//            console.log("4");
//            isLoadAjax = true;
//            filterProduct();
//        }
//    }
//    else {
//        console.log("5");
//        isLoadAjax = false;
//        reloadURL();
//        filterProduct();
//    }
//}

// Check xem có chọn duy nhất 1 cate hay không

function loadCheckOneCate() {

    collectParam();

    if (query.StrListProvince !== undefined && query.StrListProvince != null && query.StrListProvince != '') {
        return;
    }
    else if (query.StrListSpecialist !== undefined && query.StrListSpecialist != null && query.StrListSpecialist != '') {
        return;
    }
    else if (query.StrListCategory !== undefined && query.StrListCategory != null && query.StrListCategory != '') {
        if (query.StrListCategory.split(',').length == 1) {
            $('#list-cate .filter-main ul li').each(function () {
                if (query.StrListCategory == $(this).data('id')) {
                    if ($(this).data('href') !== undefined && $(this).data('href') !== "") {
                        var path = $(this).data('href');
                        if (currentType == typeBuy) path += "-chao-mua";
                        else path += "-chao-ban";
                        history.pushState({}, '', path);
                    }
                }
            });
        }
        else {
            var hasOut = toHash(query);
            if (hasOut != "")
                document.location.hash = hasOut;
        }
    }
}

// Hàm kiểm tra có cần fill lại data khi đi link đến cate k có sản phẩm
function loadCheckEmptyList() {

    collectParam();

    if (flagEmptyBuy && currentType == typeBuy) {
        isLoadAjax = true;
        filterProduct();
        flagEmptyBuy = false;
    }

    else if (flagEmptySell && currentType == typeSell) {
        isLoadAjax = true;
        filterProduct();
        flagEmptySell = false;
    }
}
// Hàm reload lại trang theo url
function reloadURL(urlredirect) {
    var typeUrl = currentType == typeSell ? "chao-ban" : "chao-mua";
    if (urlredirect !== undefined && urlredirect != null && urlredirect != '') {
        var url = "/" + urlredirect + "-" + typeUrl;
        if (window.location.pathname != url || location.hash != '') {
            history.pushState({}, '', url);
            /*location.reload();*/
        }
    }
    else if (window.location.pathname != "/" + typeUrl) {
        history.pushState({}, '', "/" + typeUrl);
        /*location.reload();*/
    }
}

// Scroll khi xem thêm
function scrollToReadmore() {
    $('html, body').animate({
        scrollTop: $(".footer-main").offset().top
    }, 1000);
}

// Close popup
function closePopupFilter(e) {

    $target = $(e.target).parent();

    // Filter
    if ($(e.target).closest(".filter__item").length == 0) {
        $(".filter-search input").val("");

        $('.filter-main').hide(300);
        $('.filter-txt').removeClass("current-filter");

        $(".filter-search input").each(function () {
            searchFilter($(this));
        });
    }

    // Soft
    if ($(e.target).closest(".sort").length == 0) {
        $(".sort-main").hide(300);
        $(".click-sort").removeClass('sort-rotate');
    }

    handleFilterText();

    CheckOverlayFilter();
}


function OffScroll() {
    $(".filter-main ul").each(function () {
        if ($(this).find(`li:not([style*="display: none"])`).length < 7)
            $(this).css("overflow-y", "unset");
        else $(this).css("overflow-y", "scroll");
    });
}

function OffSort() {
    if ($(".listproduct li").length <= minItemSort)
        $(".sort").hide();
    else $(".sort").show();
}

function CheckOverlayFilter() {
    if (isMobile) {
        if ($(".current-filter").length + $(".sort-rotate").length > 0)
            $(".cate-overlay").fadeIn(200);
        else
            $(".cate-overlay").fadeOut(200);
    }
}
//#endregion