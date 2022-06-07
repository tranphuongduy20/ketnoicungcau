var timmer;
var MIN_SSKEYWORD_LENGTH = 2;
var urlRoot = window.location.origin;
var searching = false;
var inValidChar = /:|;|!|@@|#|\$|%|\^|&|\*|' |"|>|<|,|\?|`|~|\+|=|_|-|\(|\)|{|}|\[|\]|\\|\|/gi;
var isMobile = false;
var idDel = 0;
var idActive = 0;
var isActive;
var elm;


$(document).ready(function () {
    $('.up-product').submit(function (e) {
        e.preventDefault();
    });

    //step 1: type in
    $('.click-find input').keyup(function (e) {
        e.preventDefault();
        var kw = $('.click-find input').val();
        //if (kw != '')
        //    $('.clear-search').show();
        //else
        //    $('.clear-search').hide();
        //debugger
        kw = kw.replace(inValidChar, '');
        var kwt = kw.trim().toLowerCase();
        if (kwt.length < MIN_SSKEYWORD_LENGTH) {
            return;
        }
        if (e.type == "submit" || e.which == 13) {
            query.PageIndex = 0;
            getProductList(kw);
        }
    });

    //step 2: submt
    $('.icon-search').click(function (e) {
        e.preventDefault();
        var kw = $('.click-find input').val();
        kw = kw.replace(inValidChar, '');
        var kwt = kw.trim().toLowerCase();
        if (kwt.length < MIN_SSKEYWORD_LENGTH) {
            return;
        }
        query.PageIndex = 0;
        getProductList(kw);
    });

    $('.click-del').click(function () {
        idDel = $(this).data('id');
        $('.popup-delsp').addClass('active');
    });
    $('.btn-back').click(function () {
        $('.popup-delsp').removeClass('active');
    });

    $(".btn-del").click(function (e) {
        $('.popup-delsp').removeClass('active');
        e.preventDefault();
        if (idDel > 0)
            SubmitDeleteProduct(idDel);
    });

    $(".close-send").click(function (e) {
        location.reload();
    });
    //$(".switch input[type=checkbox]").change(function (e) {
    //    elm = $(this);
    //    isActive = $(elm).is(":checked");
    //    idActive = $(elm).data('id');
    //    $(elm).prop('checked', !isActive);
    //    $('.popup-update-status').addClass('active');
    //});
    $('.btn-back-update').click(function () {
        $('.popup-update-status').removeClass('active');
    });

    $(".btn-update").click(function (e) {
        $.ajax({
            url: "/Profile/ActiveProduct",
            type: "POST",
            data: { "productId": idActive, "isActive": isActive },
            cache: false,
            beforeSend: function () {
                $(".preloader").show();
            },
            success: function (e) {
                HidePreloader();
                if (e.code === 200) {
                    $(elm).prop('checked', isActive);
                }
                else {
                    $(elm).prop('checked', !isActive);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                alert("Hệ thống đang cập nhật. Vui lòng thử lại sau ít phút");
                HidePreloader();
            }
        });
        $('.popup-update-status').removeClass('active');
    });

});

function ActiveSwitch(e) {
    elm = e;
    isActive = $(elm).is(":checked");
    idActive = $(elm).data('id');
    $(elm).prop('checked', !isActive);
    $('.popup-update-status').addClass('active');
};

function getProductList(kw) {
    if (searching) return;
    searching = true;
    //event.preventDefault();
    var kwt = kw.trim().toString().toLowerCase();
    //debugger 
    if (kwt.length < MIN_SSKEYWORD_LENGTH) {
        return;
    }

    if (kwt.length >= MIN_SSKEYWORD_LENGTH) {
        query.Keyword = kwt;
        $.ajax({
            url: 'Profile/GetProductList',
            type: 'POST',
            data: query,
            cache: false,
            beforeSend: function () {
                $(".preloader").show();
            },
            success: function (res) {
                HidePreloader();
                //debugger 
                if (res == null || res.listProduct === '' || res.listProduct.trim() === '') {
                    var blockEmpty = '<li id="empty"><i class="empty"></i>Không có sản phẩm nào phù hợp với tiêu chí tìm kiếm!</li>';
                    $(".table-buyandsell tbody").html(blockEmpty);
                } else {
                    $('.table-buyandsell tbody').html(res.listProduct);
                    handleReadMore(".seemore-another", res.left);
                }
            },
            error: function (e) {
                HidePreloader();
            }
        });
        searching = false;
    }
}


function viewMoreProduct() {
    query.PageIndex = query.PageIndex + 1;
    $.ajax({
        url: 'Profile/GetProductList',
        type: 'POST',
        data: query,
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            //debugger 
            if (res == null || res.listProduct === '' || res.listProduct.trim() === '') {
                //var blockEmpty = '<li id="empty"><i class="empty"></i>Không có sản phẩm nào phù hợp với tiêu chí tìm kiếm!</li>';
                //$(".listcompany").html(blockEmpty);
            } else {
                $('.table-buyandsell tbody').append(res.listProduct);
                handleReadMore(".seemore-another", res.left);
                //debugger 
            }

        },
        error: function (e) {
            HidePreloader();
        }
    });
}


function SubmitDeleteProduct(id) {
    $.ajax({
        url: '/Profile/DeleteProduct?id=' + id,
        type: 'POST',
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== "" && res.code === 200) {
                $("div.error-general").hide();
                ShowPopupNotify(res.message);
            } else {
                $("div .error-general").text(res.errormessage);
                $("div .error-general").show();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            HidePreloader();
            ShowPopupNotify(defaultErrorMessage);
        }
    });
}

// Hàm xử lý button Xem thêm
function handleReadMore(obj, left) {
    if (left > 0) {
        $(obj).show();
        $(obj).text(`Xem thêm ${left} sản phẩm`);
    }
    else
        $(obj).hide();
}
