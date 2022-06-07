var pageIndexReceive = 0;
var pageIndexSend = 0;
var tabReceive = 1;
var tabSend = 2;
var currentTab = tabReceive;
var statusReceive = "0";
var statusSend = "0";

$(document).ready(function () {

    //#region Common Event

    $('#tab-receive').click(function () {
        $('.tab-click').removeClass('current');
        $('.tab-content').removeClass('current');

        $(this).addClass('current');
        $("#tab1").addClass('current');

        currentTab = tabReceive;
    });
    $('#tab-send').click(function () {
        $('.tab-click').removeClass('current');
        $('.tab-content').removeClass('current');

        $(this).addClass('current');
        $("#tab2").addClass('current');

        currentTab = tabSend;
    });

    $('#tab1 ul li input').click(function () {
        statusReceive = $(this).attr("data-status");
        filterDeals();
    });

    $('#tab2 ul li input').click(function () {
        statusSend = $(this).attr("data-status");
        filterDeals();
    });

    $(document).on("click", ".click-seedetail", function () {
        queryDetailDeals = {
            dealsid: $(this).data("dealsid")
        }
        $.ajax({
            url: '/Profile/GetDetailDeals',
            type: 'POST',
            cache: false,
            data: queryDetailDeals,
            beforeSend: function () {
                $(".preloader").show();
            },
            success: function (res) {
                HidePreloader();
                if (res !== null && res.code === 200) {
                    $("div#detail-deal").html(res.data);
                }
                else {
                    ShowPopupNotify(res.errormessage);
                }
            },
            error: function (jqXHR, textStatus, errorThrown) {
                HidePreloader();
                alert(defaultErrorMessage);
                location.reload();
            }
        });
    });

    // Tắt popup Detail
    $(document).on("click", ".close-popup, #btn-closedeals", function () {
        $('.popup-connect').removeClass('active');
    });

    // Từ chối
    $(document).on("click", "#btn-refusedeals", function () {
        updateDeals(3);
    });

    // Đồng ý
    $(document).on("click", "#btn-acceptdeals", function () {
        updateDeals(2);
    });

    // Hủy
    $(document).on("click", "#btn-canceldeals", function () {
        updateDeals(4);
    });

    // Xem thêm
    $(document).on("click", ".seemore-another", function () {
        viewMoreDeals();
    });
})

function viewMoreDeals() {
    if (currentTab == tabReceive) {
        pageIndexReceive = pageIndexReceive + 1;
        query.PageIndex = pageIndexReceive;
        query.Status = statusReceive;
    }
    else {
        pageIndexSend = pageIndexSend + 1;
        query.PageIndex = pageIndexSend;
        query.Status = statusSend;
    }
    query.Type = currentTab;
    $.ajax({
        url: '/Profile/GetListDeals',
        type: 'POST',
        data: query,
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res != "") {
                if (currentTab == tabReceive) {
                    if (isMobile) $('#tab1 .table-connect').append(res.listdeals);
                    else $('#tab1 tbody').append(res.listdeals);
                    handleReadMore("#tab1 .seemore-another", res.left);
                }
                else {
                    if (isMobile) $('#tab2 .table-connect').append(res.listdeals);
                    else $('#tab2 tbody').append(res.listdeals);
                    handleReadMore("#tab2 .seemore-another", res.left);
                }
            }
        },
        error: function (e) {
            HidePreloader();
            ShowPopupNotify(defaultErrorMessage);
        }
    });
}

function filterDeals() {
    if (currentTab == tabReceive) {
        pageIndexReceive = 0;
        query.PageIndex = pageIndexReceive;
        query.Status = statusReceive;
    }
    else {
        pageIndexSend = 0;
        query.PageIndex = pageIndexSend;
        query.Status = statusSend;
    }
    query.Type = currentTab;
    $.ajax({
        url: '/Profile/GetListDeals',
        type: 'POST',
        data: query,
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res != "") {
                if (currentTab == tabReceive) {
                    if (isMobile) $('#tab1 .table-connect').html(res.listdeals);
                    else $('#tab1 tbody').html(res.listdeals);
                    handleReadMore("#tab1 .seemore-another", res.left);
                }
                else {
                    if (isMobile) $('#tab2 .table-connect').html(res.listdeals);
                    else $('#tab2 tbody').html(res.listdeals);
                    handleReadMore("#tab2 .seemore-another", res.left);
                }
            }
        },
        error: function (e) {
            HidePreloader();
            ShowPopupNotify(defaultErrorMessage);
        }
    });
}

function updateDeals(status) {
    var dealsid = $(".detail-connect").data("dealsid");
    $.ajax({
        url: '/Profile/SubmitUpdateDeals',
        type: 'POST',
        data: {
            status: status,
            dealsid: dealsid
        },
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            $('.popup-connect').removeClass('active');

            if (res != "") {
                if (isMobile) {
                    $('.tab-content.current .table-connect .item-deals').each(function () {
                        // Cập nhật lại trạng thái deal vừa cập nhật
                        if ($(this).data('dealsid') == dealsid) {
                            if (status == "2") $(this).find(".status").html(`<span class="color-agree">Đồng ý</span>`);
                            else if (status == "3") $(this).find(".status").html(`<span class="color-refuse">Từ chối</span>`);
                            else $(this).find(".status").html(`<span class="color-refuse">Hủy</span>`);
                        }
                    });
                }
                else {
                    $('.tab-content.current tr').each(function () {
                        // Cập nhật lại trạng thái deal vừa cập nhật
                        if ($(this).data('dealsid') == dealsid) {
                            console.log("vô");
                            if (status == "2") $(this).find(".status").html(`<span class="color-agree">Đồng ý</span>`);
                            else if (status == "3") $(this).find(".status").html(`<span class="color-refuse">Từ chối</span>`);
                            else $(this).find(".status").html(`<span class="color-refuse">Hủy</span>`);
                        }
                    });
                }
                
            }

        },
        error: function (e) {
            HidePreloader();
            alert(defaultErrorMessage);
            location.reload();
        }
    });
}

function handleReadMore(obj, left) {
    if (left > 0) {
        $(obj).show();
        $(obj).text(`Xem thêm ${left} kết nối khác`);
    }
    else
        $(obj).hide();
}
