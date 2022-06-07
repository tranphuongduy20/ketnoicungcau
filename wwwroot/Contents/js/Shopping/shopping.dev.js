var isShow = false;

$(document).ready(function () {

    if (query.IsMobile != undefined && query.IsMobile) {
        $('#show-all-cate').hide();
        HandleSubCateFilter();
        $('#show-all-cate').click(function () {
            $(".quicklink").find("a:hidden").show();
            if (isShow === false) {
                $(this).text('Thu gọn');
                $(this).addClass('arrow-menuB');
                isShow = true;
            } else {
                $(this).text('Xem tất cả danh mục');
                $(this).removeClass('arrow-menuB');
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
        $(".quicklink").find("a[data-id='" + 0 + "']").removeClass('active');
        if (arrCateActive !== undefined) {
            jQuery.each(arrCateActive, function (i, val) {
                $(".quicklink").find("a[data-id='" + val + "']").addClass('active');
            });
        }
        getWebsiteList();
    }

    $('.quicklink a.cate__filter').click(function () {
        $(this).toggleClass('active');

        if (IsHasTabActive() === false) {
            $(".quicklink").find("a[data-id='" + 0 + "']").addClass('active');
            window.location.href = '/mua-sam-online/' + query.CategoryUrl;
        } else {
            var id = $(this).data('id');
            var url = $(this).data('url');

            if (id !== undefined && url !== undefined) {
                if (id == 0 && url == 'all') {
                    if ($(this).hasClass('active')) {
                        window.location.href = '/mua-sam-online/' + query.CategoryUrl;
                    }
                }
                else {
                    if ($(this).hasClass('active') && $(".quicklink").find("a[data-id='" + 0 + "'].active").length > 0) {
                        $(".quicklink").find("a[data-id='" + 0 + "']").removeClass('active');
                    }
                }

                var $cate = $('.quicklink a.active');
                if ($cate.length > 0) {
                    var numberArray = new Array();
                    $cate.each(function () {
                        numberArray.push($(this).data('id'));
                    });
                    numberArray = numberArray.filter(unique);
                    query.StrListCateId = numberArray.join();
                    getWebsiteList();
                }
                else {
                    query.StrListCateId = '';
                }
            }
        }

        
    });
});

function HandleSubCateFilter() {
    var quicklinkPos = $('.quicklink').position();
    var cateHeight = $('a[data-url=all]').outerHeight(true);
    var area = quicklinkPos.top + cateHeight * 2;
    var isShowBtn = false;
    $('.cate__filter').each(function () {
        var boxPos = $(this).position();
        var y = boxPos.top;
        if (y >= area) {
            $(this).hide();
            isShowBtn = true;
        }
    });
    if (isShowBtn) $('#show-all-cate').show();

}

function IsHasTabActive() {
    if ($(".quicklink").find("a.active").length > 0)
        return true;
    return false;
}

// Hàm get company
var flagCompanyList = true;
function getWebsiteList() {
    if (!flagCompanyList) return;
    flagCompanyList = false;
    query.IsGetAll = false;
    query.IsFilterManyCate = '1';
    query.PageIndex = 0;
    //query.PageSize = 2;
    $.ajax({
        url: '/Shopping/GetWebsiteList',
        type: 'POST',
        data: query,
        cache: false,
        async: query.PageIndex == 0,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (e) {
            HidePreloader();
            flagCompanyList = true;

            if (e == null || e.listWebsite == null || e.listWebsite == '' || e.listWebsite.trim() == '') {
                var blockEmpty = '<li id="empty"><i class="empty"></i>Không có website nào phù hợp với tiêu chí tìm kiếm!</li>';
                $(".listonline").html(blockEmpty);
            }
            else {
                /*debugger*/
                $(".listonline").html(e.listWebsite);
                handleReadMore('#viewMore', e.left);
                //var total = e.total;
                //if (total <= 20) {
                //    /*$('#paging').hide();*/
                //} else {
                //    //genPaging(total, query.PageSize, query.PageIndex);
                //    //$('#paging').show();
                //}

                //if (total !== undefined) {
                //    $('.filter-company .number-show').html(total + " Doanh nghiệp");
                //} else {
                //    /*$('.boxtop .txt02').hide();*/
                //}
            }

            if (e.left <= 0)
                $('#viewMore').hide();
            else $('#viewMore').show();

            //showOrOffFilterSort();

            var hashOut = 'c=' + query.StrListCateId;
            document.location.hash = hashOut;
        },
        error: function () {
            console.log("Lỗi gọi action");
        }
    });
}

function viewMoreWebsiteList() {
    query.PageIndex = query.PageIndex + 1;
    //query.PageSize = 2;
    query.IsFilterManyCate = '1';
    //debugger
    //if (query.StrListCateId.length > 0)
    //    query.StrListCateId = query.StrListCateId + "," + query.CategoryId;
    //else query.StrListCateId = query.CategoryId;
    query.IsGetAll = false;
    if ($("a[data-id='" + 0 + "']").hasClass('active'))
        query.IsGetAll = true;
    $.ajax({
        url: '/Shopping/GetWebsiteList',
        type: 'POST',
        data: query,
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (e) {
            HidePreloader();
            flagCompanyList = true;

            if (e == null || e.listWebsite == null || e.listWebsite == '' || e.listWebsite.trim() == '') {
                //var blockEmpty = '<li id="empty"><i class="empty"></i>Không có website nào phù hợp với tiêu chí tìm kiếm!</li>';
                //$(".listonline").html(blockEmpty);
            }
            else {
                $(".listonline").append(e.listWebsite);
                handleReadMore('#viewMore', e.left);
            }

            if (e.left <= 0)
                $('#viewMore').hide();
            else $('#viewMore').show();
        },
        error: function () {
            console.log("Lỗi gọi action");
        }
    });
}

// Hàm lọc lại những giá trị bị trùng trong mảng
function unique(value, index, self) {
    return self.indexOf(value) === index;
}

// Hàm xử lý button Xem thêm
function handleReadMore(obj, left) {
    if (left > 0) {
        $(obj).show();
        $(obj + " span").text(`Xem thêm ${left} website ${query.CategoryName}`);
    }
    else
        $(obj).hide();
}