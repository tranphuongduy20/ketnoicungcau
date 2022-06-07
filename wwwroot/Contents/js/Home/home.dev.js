$(document).ready(function () {
    $('.slider-banner').owlCarousel({
        loop: true,
        items: 1,
        dots: false,
        nav: true,
        autoplay: true,
        autoplayTimeout: 4000,
        autoplayHoverPause: true,
        lazyload: true,
    });
    $('.banner-promo').owlCarousel({
        loop: true,
        items: 2,
        dots: true,
        nav: false,
        margin: 10,
        autoplay: true,
        autoplayTimeout: 4000,
        autoplayHoverPause: true,
        lazyload: true,
    });
    $('.listproduct-related').owlCarousel({
        loop: false,
        items: 4,
        dots: true,
        nav: true,
        margin: 26,
        slideBy: 4,
        autoplay: false,
        autoplayTimeout: 3000,
        autoplayHoverPause: true
    });
    $('.blog__list').owlCarousel({
        loop: true,
        items: 3,
        dots: true,
        nav: true,
        margin: 30,
        slideBy: 3,
        autoplay: false,
        autoplayTimeout: 3000,
        autoplayHoverPause: true,
    });
    $('.fotorama').fotorama({
        width: '100%',
        maxwidth: '100%',
        maxheight: '100%',
        nav: 'thumbs',
        thumbmargin: 10,
        thumbwidth: 143,
        thumbheight: 88,
    });

});

//method
function InitOwlCarousel() {
    //SLIDER
    //BANNER IMAGES
    let mainBannerContainer = $('.banner-home');
    let mainBannerSlider = mainBannerContainer.find('.slider-banner');
    InitBannerSlider(mainBannerContainer, mainBannerSlider);
    mainBannerSlider.owlCarousel({
        loop: true,
        items: 1,
        dots: false,
        autoplay: true,
        autoplayTimeout: 5000,
        autoplayHoverPause: true,
        lazyLoad: true,
        autoHeight: true
    });

    //BANNER PRODUCT BLOCK
    let bannerBlocks = $('.block__banner');
    bannerBlocks.each(function (e) {
        let container = $(this);
        let slider = container.find('.slider-bannerblock');
        InitBannerSlider(container, slider);
        slider.owlCarousel({
            loop: true,
            items: 1,
            dots: false,
            lazyLoad: true
        });
    });

    //BANNER Abouts
    let aboutBannerContainer = $('.store__about');
    let aboutBannerSlider = aboutBannerContainer.find('.slider-about');
    InitBannerSlider(aboutBannerContainer, aboutBannerSlider);
    aboutBannerSlider.owlCarousel({
        loop: true,
        items: 1,
        dots: false,
        lazyLoad: true
    });

    let storeImgContainer = $('.store-images');
    let storeImgSlider = storeImgContainer.find('.slider-about');
    InitBannerSlider(storeImgContainer, storeImgSlider);
    storeImgSlider.owlCarousel({
        loop: true,
        items: 1,
        dots: false,
        lazyLoad: true
    });

    if (!isMobile)
        InitProductSlider($(".listproduct"));
}

function InitBannerSlider(container, slider) {
    if (isMobile) {
        slider.on("initialized.owl.carousel changed.owl.carousel", function (e) {
            if (!e.namespace) {
                return;
            }
            container.find(".counter").text(
                e.relatedTarget.relative(e.item.index) + 1 + "/" + e.item.count
            );
        })
    } else {
        let trigger = container.find('.trigger');

        // counter images slider gallery
        slider.on("initialized.owl.carousel changed.owl.carousel", function (e) {
            if (!e.namespace) {
                return;
            }
            trigger.find(".nb-total").text(
                e.item.count
            );
            trigger.find(".nb-current").text(
                e.relatedTarget.relative(e.item.index) + 1
            );
        })
        //button next prev
        trigger.find('.btnowl-next').click(function () {
            slider.trigger('next.owl.carousel');
        });
        trigger.find('.btnowl-prev').click(function () {
            slider.trigger('prev.owl.carousel');
        });
    }
}

function InitProductSlider(slider) {
    slider.owlCarousel({
        items: 5,
        slideBy: 'page',
        rewind: true,
        dots: false,
        margin: 20,
        nav: true,
        lazyLoad: true
    });
}

function SetAllContentAttr() {
    var activeTabs = $('ul.box-common__tab li.active-tab');
    activeTabs.each(function () {
        var activeTab = $(this);
        var activeContent = activeTab.closest('.box-common').find('.box-common__content'
            + ':not([data-cate-id])'
            + ':not([data-prop-id])'
            + ':not([data-prop-value-ids])'
            + ':not([data-html-id])');
        if (activeContent != null) {
            if (activeTab.data('cate-id') != null)
                activeContent.attr('data-cate-id', activeTab.data('cate-id'));
            if (activeTab.data('prop-id') != null)
                activeContent.attr('data-prop-id', activeTab.data('prop-id'));
            if (activeTab.data('prop-value-ids') != null)
                activeContent.attr('data-prop-value-ids', activeTab.data('prop-value-ids'));
            if (activeTab.data('html-id') != null)
                activeContent.attr('data-html-id', activeTab.data('html-id'));
        }
    });
}

String.prototype.ToArray = function () {
    var array = [];
    if (this != null)
        array = JSON.parse('[' + this + ']');
    return array;
}

function ConvertToArray(input) {
    let array = [];
    if (input != null) {
        let invalidCharacterRegex = /[\u200B-\u200D\uFEFF]/g;
        array = JSON.parse('[' + input.toString().replace(invalidCharacterRegex, '') + ']');
    }
    return array;
}

$.fn.isInViewport = function () {
    let elementTop = $(this).offset().top;
    let elementBottom = elementTop + $(this).outerHeight();

    let viewportTop = $(window).scrollTop();
    let viewportBottom = viewportTop + $(window).height();

    return elementBottom > viewportTop && elementTop < viewportBottom;
};


var isViewmore = false;
var isFirst = true;
function viewMoreStoresHome(elm, pageIndex, pageSize, type) {
    if (isFirst) {
        var total = $('#btn_visit').data('total');
        var totalfirst = parseInt(total) - 6;
        var txt = "XEM THÊM " + totalfirst + " CỬA HÀNG";
        if (totalfirst > 0) {
            $("#btn_visit").text(txt);
        }
        else {
            $("#btn_visit").remove();
        }
        $(".visit__list .item").css({ "display": "block" });
        isFirst = false;
        
    }
    else {
        if (isViewmore) {
            return;
        }
        $.ajax({
            url: 'Home/ViewMoreStoreHome',
            type: 'GET',
            data: {
                pageIndex: pageIndex,
                pageSize: pageSize,
                type: type,
                isAjax: true,
            },
            cache: false,
            beforeSend: function () {
                $("#dlding").show();
                isViewmore = true;
            },
            success: function (res) {
                console.log(res);
                isViewmore = false;
                if (res != "") {
                    $("#visitId").append(res);
                    $(".visit__list .item").css({ "display": "block" });
                    $(elm).remove();
                }
                $("#dlding").hide();
            },
            error: function (e) {
                $("#dlding").hide();
            }
        });
    }
}