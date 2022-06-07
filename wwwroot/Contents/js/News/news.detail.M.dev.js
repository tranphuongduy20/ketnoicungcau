$(document).ready(function () {


    //Slider Thumnail chi tiết
    // counter images slider gallery
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

});

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
    let step = 8;
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