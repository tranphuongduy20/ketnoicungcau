var seemore = 0;
$(document).ready(function () {

    //#region Slide
    $('.slider-banner').owlCarousel({
        loop: true,
        items: 1,
        dots: true,
        nav: false,
        autoplay: true,
        autoplayTimeout: 3000,
        autoplayHoverPause: false
    });

    $('.banner-promo').owlCarousel({
        loop: true,
        items: 1,
        dots: true,
        nav: false,
        autoplay: true,
        autoplayTimeout: 3000,
        autoplayHoverPause: true
    });

    $('.slide-topnews').owlCarousel({
        items: 1,
        dots: true,
        margin: 10,
        nav: false,
        lazyload: true,
        loop: true,
        stagePadding: 30
    });

    $('.fotorama').fotorama({
        width: '100%',
        maxwidth: '100%',
        maxheight: '100%',
        nav: 'thumbs',
        thumbmargin: 6,
        thumbwidth: 90,
        thumbheight: 60,
    });
    //#endregion

    //#region Xem thêm

    var arrIndexes = [4, 5, 6, 7];

    if (totalSell <= 0) {
        $("#readmore-sell").hide();
    }
    else {
        // ẩn list chào bán
        $("#productsell .item").filter(function (index) {
            return arrIndexes.indexOf(index) > -1;
        }).hide();
    }

    if (totalBuy <= 0) {
        $("#readmore-buy").hide();
    }
    else {
        // ẩn list chào mua
        $(".listbuy .item").filter(function (index) {
            return arrIndexes.indexOf(index) > -1;
        }).hide();
    }

    if (totalRelate <= 0) {
        $("#readmore-relate").hide();
    }
    else {
        // ẩn list relate
        $(".listproduct-related .item").filter(function (index) {
            return arrIndexes.indexOf(index) > -1;
        }).hide();
    }

    if (totalNews <= 0) {
        $("#readmore-news").hide();
    }
    else {
        // ẩn list news
        $(".blog__list .item").filter(function (index) {
            return arrIndexes.indexOf(index) > -1;
        }).hide();
    }

    if (totalNews <= 0) {
        $("#readmore-company").hide();
    }
    else {
        // ẩn list company
        $(".listcompany .item").filter(function (index) {
            return arrIndexes.indexOf(index) > -1;
        }).hide();
    }

    // Xem thêm chào bán
    $(document).on("click", "#readmore-sell", function () {
        $(this).hide();
        $("#productsell .item, #readall-sell").show();
    });

    // Xem thêm chào mua
    $(document).on("click", "#readmore-buy", function () {
        $(this).hide();
        $(".listbuy .item, #readall-buy").show();
    });

    // Xem thêm liên quan
    $(document).on("click", "#readmore-relate", function () {
        $(this).hide();
        $(".listproduct-related .item, #readall-relate").show();
    });

    // Xem thêm tin tức
    $(document).on("click", "#readmore-news", function () {
        $(this).hide();
        $(".blog__list .item, #readall-news").show();
    });

    // Xem thêm company
    $(document).on("click", "#readmore-company", function () {
        $(this).hide();
        $(".listcompany .item, #readall-company").show();
    });
    //#endregion
})

function InitOwlCarousel() {
    //BANNER IMAGES
    $('.slider-banner').each(function (idx, elm) {
        CallowlCarousel(elm, 1);
    });

    $('.slider-about').each(function (idx, elm) {
        CallowlCarousel(elm, 1);
    });

    //BANNER IMAGES BLOCK
    $('.slider-bannerblock').each(function (idx, elm) {
        CallowlCarousel(elm, 1);
    });
}

function CallowlCarousel(elm, item) {
    $(elm).on("initialized.owl.carousel changed.owl.carousel", function (e) {
        if (!e.namespace) {
            return;
        }
        $(elm).next().text(
            e.relatedTarget.relative(e.item.index) + 1 + "/" + e.item.count
        );
    })
    //owlCarousel banner
    $(elm).owlCarousel({
        loop: true,
        items: item,
        dots: false,
        nav: false,
        autoplay: true,
        lazyLoad: true,
        autoplayTimeout: 5000,
        autoplayHoverPause: true
    });
}

