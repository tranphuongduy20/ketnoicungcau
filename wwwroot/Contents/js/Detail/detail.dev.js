let step;

$(document).ready(function () {

    //#region Gallery
    if ($('.thubmail-slide').length) {
        $("#slider-defaults").on("initialized.owl.carousel changed.owl.carousel", function (e) {
            if (!e.namespace) {
                return;
            }
            $(".counter").text(
                e.relatedTarget.relative(e.item.index) + 1 + "/" + e.item.count
            );
        })
        // owl-carousel gallery
        $(".slider-img").owlCarousel({
            items: 1,
            margin: 0,
            stagePadding: 0,
            autoplay: false,
            nav: true,
        });

        //total item de show or hide next prev thumbnail
        var numberItem = document.querySelectorAll('.thubmail-slide .owl-dots button').length;
        if (numberItem == 1) {
            $('.thubmail-slide').addClass('fullImages');
        }
        else if (numberItem > 1 && numberItem < 7) {
            $('.thubmail-slide').addClass('hasSlider');
        }
        else {
            $('.thubmail-slide').addClass('hasArrow');
        }
        galleryRow();
    }

    $('.item-img img').each(function () {
        $(this).attr("alt", $(".item-img").data("alt"));
    });

    if ($(".thubmail-slide .counter").text() == "1/1")
        $(".thubmail-slide .counter").hide();
    //#endregion

    //#region Kết nối với doanh nghiêp
    $(".btn-connection.connect").click(function () {
        if ($(this).hasClass("sent") == false) {
            if (isLogin == false) {
                showPopupRequireLogin();
            }
            else {
                connectCompany();
            }
        }
    });
    $(".close-connection").click(function () {
        $('.bgconnection-overlay').fadeOut();
        $('.popup-connection').fadeOut();
    });
    $(".bgconnection-overlay").click(function () {
        $('.bgconnection-overlay').fadeOut();
        $('.popup-connection').fadeOut();
    });

    //#endregion
    if ($(".detail-content").height() > 500) {
        $(".detail-content").addClass("bg-hide");
        $("a.readmore").closest("div").show();
    }

    $("a.readmore").click(function () {
        $(".detail-content").removeClass("bg-hide");
        $(this).hide();
    });

    $(".btn-wish").click(function () {
        if (isLogin) {
            var id = $(this).data('id');
            if ($(this).hasClass("love")) {
                DeleteWishlistProduct(id);
            }
            else {
                AddWishlistProduct(id);
            }
        }
        else {
            showPopupRequireLogin();
        }
    });

});

function AddWishlistProduct(productid) {
    $.ajax({
        url: '/Product/AddWishlist',
        type: 'POST',
        data: {
            productid: productid,
        },
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== null && res.code === 200) {
                if (res.data !== null && res.data > 0) {
                    $('.btn-wish').addClass("love");
                    $(".btn-wish span").text(res.data);
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

function DeleteWishlistProduct(productid) {
    $.ajax({
        url: '/Product/DelWishlist',
        type: 'POST',
        data: {
            productid: productid,
        },
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== null && res.code === 200) {
                if (res.data !== null && res.data >= 0) {
                    $('.btn-wish').removeClass("love");
                    $(".btn-wish span").text(res.data);
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

function galleryRow() {
    // scroll mouse
    const slider = document.querySelector('.owl-dots');
    let isDown = false;
    let startX;
    let scrollLeft;
    slider.addEventListener('mousedown', (e) => {
        isDown = true;
        slider.classList.add('active');
        startX = e.pageX - slider.offsetLeft;
        scrollLeft = slider.scrollLeft;
    });
    slider.addEventListener('mouseleave', () => {
        isDown = false;
        slider.classList.remove('active');
    });
    slider.addEventListener('mouseup', () => {
        isDown = false;
        slider.classList.remove('active');
    });
    slider.addEventListener('mousemove', (e) => {
        if (!isDown) return;
        e.preventDefault();
        const x = e.pageX - slider.offsetLeft;
        const walk = (x - startX) * 3; //scroll-fast
        slider.scrollLeft = scrollLeft - walk;
    });

    // custom dots owl-carousel => images
    dotcount = 1;
    jQuery('.owl-dot').each(function () {
        jQuery(this).addClass('dotnumber' + dotcount);
        jQuery(this).attr('data-info', dotcount);
        let dot = $(this);
        dot.attr('data-position-left', dot.position().left);
        dotcount = dotcount + 1;
    });
    slidecount = 1;
    jQuery('.owl-item').not('.cloned').each(function () {
        jQuery(this).addClass('slidenumber' + slidecount);
        slidecount = slidecount + 1;
    });
    jQuery('.owl-dot').each(function () {
        grab = jQuery(this).data('info');
        slidegrab = jQuery('.slidenumber' + grab);

        let itemVideo = $('.slidenumber' + grab).find('.item-video');
        let itemImg = $('.slidenumber' + grab).find('.item-img');
        if (itemVideo.length > 0) {
            $(this).html($('<div class="owl-video-play-icon"></div>'));
            $(this).append($('<img>', { class: 'theImg', src: itemVideo.find('a').data('thumb') }));
        }
        if (itemImg.length > 0)
            $(this).html($('<img>', { class: 'theImg', src: itemImg.find('img').attr('src') }));

    });
    amount = $('.owl-dot').length;
    gotowidth = 100 / amount;
    jQuery('.owl-dot').css("height", gotowidth + "%");


    $(".slider-img").on('translate.owl.carousel', function (event) {
        let container = $(event.target).find(".owl-dots");
        let nextDot = container.find(".owl-dot").eq(event.item.index);
        container.animate({
            scrollLeft: nextDot.data('position-left') - container.outerWidth() / 2 + nextDot.outerWidth() / 2
        }, 300);
    });
    $(".slider-img").on('translated.owl.carousel', function (event) {
        showHideThumbNav(slideContainer, dotContainer);
    });

    $(".thubmail-slide .owl-carousel .owl-dots").on('scroll', function () {
        showHideThumbNav(slideContainer, dotContainer);
    });


    //next

    if ($('.thubmail-slide-product').length) {
        let step = 9;
    }
    else {
        let step = 4;
    }
    let slideContainer = $('.thubmail-slide');
    let dotContainer = $('.owl-dots');
    let duration = 500;
    showHideThumbNav(slideContainer, dotContainer);

    $('.thubmail-slide .next').click(function () {
        let lastInviewDot = getInViewElement(dotContainer, isFirst = false);
        if (dotContainer.children().last().data('info') != lastInviewDot.data('info')) {
            let nextDot = lastInviewDot.next();
            dotContainer.animate({
                scrollLeft: dotContainer.scrollLeft() + (nextDot.offset().left - dotContainer.offset().left)
            }, {
                duration: duration,
                complete: function () {
                    showHideThumbNav(slideContainer, dotContainer);
                }
            });
        }
    });

    //prev
    $('.thubmail-slide .prev').click(function () {
        let firstInviewDot = getInViewElement(dotContainer, isFirst = true);
        if (dotContainer.children().first().data('info') != firstInviewDot.data('info')) {
            let prevDot = dotContainer.find('.owl-dot[data-info=' + (firstInviewDot.data('info') - step > 1 ? firstInviewDot.data('info') - step : 1) + ']');
            dotContainer.animate({
                scrollLeft: dotContainer.scrollLeft() + (prevDot.offset().left - dotContainer.offset().left)
            }, {
                duration: duration,
                complete: function () {
                    showHideThumbNav(slideContainer, dotContainer);
                }
            });
        }
    });
    function checkInView(elem, partial) {
        var container = $(".owl-dots");
        var contwidth = container.width();
        var contLeft = container.scrollLeft();
        var contRight = contLeft + contwidth;

        var elemLeft = $(elem).offset().left - container.offset().left;
        var elemRight = elemLeft + $(elem).width();

        var isTotal = (elemLeft >= 0 && elemRight <= contwidth);
        var isPart = ((elemLeft < 0 && elemRight > 0) || (elemLeft > 0 && elemLeft <= container.width())) && partial;

        return isTotal || isPart;
    }


    function showHideThumbNav(slideContainer, dotContainer) {
        let prev = slideContainer.find('.prev');
        let next = slideContainer.find('.next');
        if (checkInView(dotContainer.children().first(), false)
            && !checkInView(dotContainer.children().last(), false)) {
            prev.addClass('disabled');
            next.removeClass('disabled');
        }
        else if (!checkInView(dotContainer.children().first(), false)
            && checkInView(dotContainer.children().last(), false)) {
            prev.removeClass('disabled');
            next.addClass('disabled');
        }
        else if (checkInView(dotContainer.children().first(), false)
            && checkInView(dotContainer.children().last(), false)) {
            prev.addClass('disabled');
            next.addClass('disabled');
        }
        else {
            prev.removeClass('disabled');
            next.removeClass('disabled');
        }
    }

    function getInViewElement(container, isFirst) {
        let result;
        container.children().each(function () {
            if (checkInView($(this), true)) {
                result = $(this);
                if (isFirst)
                    return false;
            }
        });
        return result;
    }


}

function connectCompany() {
    dealQuery.__RequestVerificationToken = $('.detail-main input[name="__RequestVerificationToken"]').val()
    $.ajax({
        url: '/Product/AddDeals',
        type: 'POST',
        data: dealQuery,
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== null && res.code === 200) {
                showPopupSuccess();
                $(".btn-connection").removeClass("connect");
                $(".btn-connection").addClass("sent");
                $(".btn-connection span").text("Đã gửi kết nối, đang chờ phản hồi");
            }
            else {
                showPopupFail(res.errormessage);
            }
        },
        error: function (e) {
            HidePreloader();
            alert(defaultErrorMessage);
            location.reload();
        }
    });
}

function showPopupSuccess() {
    $('.popup-connection .txt-noti').show();
    $('.popup-connection .txt').html("Đã gửi kết nối.<br>\n Vui lòng đợi phản hồi từ doanh nghiệp.");
    $('.bgconnection-overlay').fadeIn();
    $('.popup-connection').fadeIn();
}

function showPopupFail(errormessage) {
    $('.popup-connection .txt-noti').hide();
    $('.popup-connection .txt').html(errormessage);
    $('.bgconnection-overlay').fadeIn();
    $('.popup-connection').fadeIn();
}

function showPopupRequireLogin() {
    $('.popup-connection .txt-noti').hide();
    $('.popup-connection .txt').html(`Vui lòng <a href="/dang-nhap" style="color: #1c67d8"> đăng nhập </a> để sử dụng chức năng này`);
    $('.bgconnection-overlay').fadeIn();
    $('.popup-connection').fadeIn();
}