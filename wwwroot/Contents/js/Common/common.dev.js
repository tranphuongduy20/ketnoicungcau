var timmer;
var MIN_SSKEYWORD_LENGTH = 2;
var urlRoot = window.location.origin;
var searching = false;
var inValidChar = /:|;|!|@@|#|\$|%|\^|&|\*|' |"|>|<|,|\?|`|~|\+|=|_|-|\(|\)|{|}|\[|\]|\\|\|/gi;
var isMobile = false;

$(document).ready(function () {
    $.validator.addMethod("isPhonenumber", function (value) {
        var firstNumber = value.substring(0, 1);
        var secondNumber = value.substring(1, 2);
        return (firstNumber == 0 && secondNumber != 0);
    }, "Vui lòng nhập đúng định dạng số điện thoại");

    $.validator.addMethod("isTaxId", function (value) {
        const pattern = /^((\d{10}\-\d{3})|(\d{10}))$/;
        return pattern.test(value);
    }, "Vui lòng nhập đúng định dạng mã số thuế");


    $(".tab-commonlist li").click(function() {
    var data_id = $(this).attr("data-id");
    $('.tab-commonlist li').removeClass('is-active');
    $('.tab-content').removeClass('show-active');
    $(this).addClass('is-active');
        $("#" + data_id).addClass('show-active');
    });
    //$(".hassub").click(function () {
    //  $(this).toggleClass('rotate');
    //  $(".submenu-row").fadeToggle();
    //});

    //#region Suggest Search
    //step 1: type in
    $('.header-search input').keyup(function (e) {
        e.preventDefault();
        var searchResult = $('.search-result');
        var searchContainer = $('.search-show');
        var kw = $('.header-search input').val();
        if (kw != '')
            $('.clear-search').show();
        else
            $('.clear-search').hide();

        kw = kw.replace(inValidChar, '');

        var kwt = kw.trim().toLowerCase();
        if (kwt.length < MIN_SSKEYWORD_LENGTH) {
            searchContainer.hide();
            searchResult.html('');
            return;
        }
        if (e.which == 40 || e.which == 38) {
            UpDownSuggest(e.which);
            return;
        }
        else if (e.type == "submit" || e.which == 13)
            goToSearchPage(kw);
        else if (!searching) {
            clearTimeout(timmer);
            timmer = setTimeout(function () {
                callSuggestSearch(e);
            }, 600);
        }
    });

    //step 2: submt
    $('.submit-search').click(function (e) {
        e.preventDefault();
        var kw = $('.header-search input').val();
        kw = kw.replace(inValidChar, '');
        var kwt = kw.trim().toLowerCase();
        if (kwt.length < MIN_SSKEYWORD_LENGTH) {
            $('.search-result').html('');
            return;
        }
        goToSearchPage(kw);
    });

    //clear keyword
    $('.clear-search').click(function () {
        $('.header-search input').val('');
        $('.clear-search').hide();
        $('.search-show').hide();
        $('.search-result').html('');
        $('.header-search input').focus();
    });

    //close menu, search
    $('.bgcover').click(function () {
        $('body').removeClass('body--hidden');
        closeSearch();
        closeMenu();
        $(this).fadeOut();
    });
    $(document).on('click', function (e) {
        if ($(e.target).closest(".search-show").length === 0 &&
            $(e.target).closest(".wrap-menu").length === 0 &&
            $(e.target).closest(".list-menu").length === 0 &&
            $(e.target).closest(".header-search").length === 0) {
            closeSearch();
            closeMenu();
            hideBackground();
        }
    });
    //#endregion

    //#region Popup

    // Close popup notify
    $(".close-send").click(function () {
        $('.bgsend-overlay').fadeOut();
        $('.popup-send').fadeOut();
    });

    // Close popup by click background
    $(".bgsend-overlay").click(function () {
        $('.bgsend-overlay').fadeOut();
        $('.popup-send').fadeOut();
    });

    //#endregion
});

//method 
function closeSearch() {
    $('.header-search').removeClass('act');
    $('.search-show').fadeOut();
}

function closeMenu() {
    $('.list-menu > li > a').removeClass('active');
    $('.wrap-menu').fadeOut();
}

function showBackground() {
    $('.bgcover').fadeIn();
    $('body').addClass('body--hidden');
}

function hideBackground() {
    $('.bgcover').fadeOut();
    $('body').removeClass('body--hidden');
}

/*region suggest search*/
function callSuggestSearch(event) {
    if (searching) return;
    searching = true;
    event.preventDefault();
    var kw = $('.header-search input').val().replace(inValidChar, '');
    var kwt = kw.trim().toString().toLowerCase();
    var searchContainer = $('.search-show');
    var searchResult = $('.search-result');

    if (kwt.length < MIN_SSKEYWORD_LENGTH) {
        searchContainer.hide();
        searchResult.html('');
        searching = false;
        return;
    }

    if (kwt.length >= MIN_SSKEYWORD_LENGTH) {
        $.ajax({
            url: urlRoot + '/Common/SuggestSearch',
            type: 'GET',
            data: {
                keywords: kwt
            },
            cache: false,
            beforeSend: function () {
            },
            success: function (res) {
                if (res.trim() != "")
                    searchContainer.html(res);
                else
                    searchResult.html('');

                if ($('.search-item').length > 0) {
                    searchContainer.show();
                    searchResult.show();
                }
                else {
                    searchContainer.hide();
                    searchResult.hide();
                }
            },
            error: function (e) {
            }
        });
        searching = false;
    }
}


function goToSearchPage(s, cateId) {
    var tagUrl;
    //Dùng cho ?key trên Url
    var kwRep = s.toString().replace(inValidChar, '');
    var kwt = kwRep.trim();
    var keyUrl = encodeURIComponent(kwt).replace(/\./g, '+').replace(/%20/gi, '+').replace(/ /g, '+');
    tagUrl = "/tim-kiem?kw=" + keyUrl;
    //Lưu lịch sử tìm kiếm vào cookie
    //UpdateSearchKeywordHistory(kwt, urlRoot + tagUrl);

    location.href = tagUrl;
}

function slugify(s) {
    s = s.toString().toLowerCase()
        .replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a")
        .replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e")
        .replace(/ì|í|ị|ỉ|ĩ/g, "i")
        .replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o")
        .replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u")
        .replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y")
        .replace(/đ/g, "d")
        .replace(/!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\:|\;|\'|-|\"|\&|\#|\[|\]|~|$|_/g, "")
        .replace(/ /g, '-')
        .replace(/\s+/g, '-')           // Replace spaces with -
        .replace(/[^\w\-]+/g, '')       // Remove all non-word chars
        .replace(/\-\-+/g, '-')         // Replace multiple - with single -
        .replace(/^-+/, '')             // Trim - from start of text
        .replace(/-+$/, '');            // Trim - from end of text
    return s;
}

// Search Filter
function searchFilterCommon(me) {
    var term = $(me).val().toString().toLowerCase();
    $(me).parents('.filter-search').next().closest('.list-item').find("a").each(function (idx, elm) {
        var html = $(elm).text().toString().toLowerCase();
        if (stringToSlug(html).includes(stringToSlug(term))) {
            $(elm).parent('li').show();
        } else {
            $(elm).parent('li').hide();
        }
    });
    OffScrollCommon();
}

function OffScrollCommon() {
    $(".filter-main ul").each(function () {
        if ($(this).find(`li:not([style*="display: none"])`).length < 7)
            $(this).css("overflow-y", "unset");
        else $(this).css("overflow-y", "scroll");
    });
}

// Convert string thành slug
function stringToSlug(str) {
    str = str.replace(/à|á|ạ|ả|ã|â|ầ|ấ|ậ|ẩ|ẫ|ă|ằ|ắ|ặ|ẳ|ẵ/g, "a");
    str = str.replace(/è|é|ẹ|ẻ|ẽ|ê|ề|ế|ệ|ể|ễ/g, "e");
    str = str.replace(/ì|í|ị|ỉ|ĩ/g, "i");
    str = str.replace(/ò|ó|ọ|ỏ|õ|ô|ồ|ố|ộ|ổ|ỗ|ơ|ờ|ớ|ợ|ở|ỡ/g, "o");
    str = str.replace(/ù|ú|ụ|ủ|ũ|ư|ừ|ứ|ự|ử|ữ/g, "u");
    str = str.replace(/ỳ|ý|ỵ|ỷ|ỹ/g, "y");
    str = str.replace(/đ/g, "d");
    str = str.replace(/À|Á|Ạ|Ả|Ã|Â|Ầ|Ấ|Ậ|Ẩ|Ẫ|Ă|Ằ|Ắ|Ặ|Ẳ|Ẵ/g, "A");
    str = str.replace(/È|É|Ẹ|Ẻ|Ẽ|Ê|Ề|Ế|Ệ|Ể|Ễ/g, "E");
    str = str.replace(/Ì|Í|Ị|Ỉ|Ĩ/g, "I");
    str = str.replace(/Ò|Ó|Ọ|Ỏ|Õ|Ô|Ồ|Ố|Ộ|Ổ|Ỗ|Ơ|Ờ|Ớ|Ợ|Ở|Ỡ/g, "O");
    str = str.replace(/Ù|Ú|Ụ|Ủ|Ũ|Ư|Ừ|Ứ|Ự|Ử|Ữ/g, "U");
    str = str.replace(/Ỳ|Ý|Ỵ|Ỷ|Ỹ/g, "Y");
    str = str.replace(/Đ/g, "D");
    // Some system encode vietnamese combining accent as individual utf-8 characters
    // Một vài bộ encode coi các dấu mũ, dấu chữ như một kí tự riêng biệt nên thêm hai dòng này
    str = str.replace(/\u0300|\u0301|\u0303|\u0309|\u0323/g, ""); // ̀ ́ ̃ ̉ ̣  huyền, sắc, ngã, hỏi, nặng
    str = str.replace(/\u02C6|\u0306|\u031B/g, ""); // ˆ ̆ ̛  Â, Ê, Ă, Ơ, Ư
    // Remove extra spaces
    // Bỏ các khoảng trắng liền nhau
    str = str.replace(/ + /g, " ");
    str = str.trim();
    // Remove punctuations
    // Bỏ dấu câu, kí tự đặc biệt
    str = str.replace(/!|@|%|\^|\*|\(|\)|\+|\=|\<|\>|\?|\/|,|\.|\:|\;|\'|\"|\&|\#|\[|\]|~|\$|_|`|-|{|}|\||\\/g, " ");
    return str;
}

// Hàm lọc lại những giá trị bị trùng trong mảng
function unique(value, index, self) {
    return self.indexOf(value) === index;
}

function ShowPopupNotify(message, title = "Thông báo") {
    $(".popup-send .noti").text(title);
    $(".popup-send .txt").text(message);
    $('.bgsend-overlay').fadeIn();
    $('.popup-send').fadeIn();
}

function HidePreloader() {
    setTimeout(function () {
        $(".preloader").hide();
    }, 200);
}
