$(document).ready(function () {
    $('.btn-readmore').click(function () {
        let btn = $(this);
        let ul = btn.parent().find('.listblog');
        let index = parseInt(btn.attr('data-index')) + 1;
        $.ajax({
            url: "/News/GetNews",
            data: {
                pageIndex: index,
                pageSize: parseInt(btn.attr('data-size'))
            }
        }).done(function (data) {
            ul.append(data);
            let remain = parseInt(btn.attr('data-total')) - ul.children().length - 3;
            if (remain > 0) {
                btn.attr('data-index', index);
                btn.find('span').text('Xem them ' + remain + ' tin tuc');
            }
            else
                btn.remove();
        });
    });

    $('.scrolling').on('scroll', function () {
        var parent_div = $(this).parent();
        if ($(this).scrollLeft() + $(this).innerWidth() >= $(this)[0].scrollWidth) {
            if (parent_div.hasClass('scroll-right')) { parent_div.removeClass('scroll-right'); }
        } else if ($(this).scrollLeft() === 0) {
            if (parent_div.hasClass('scroll-left')) { parent_div.removeClass('scroll-left'); }
        } else {
            if (!parent_div.hasClass('scroll-right')) { parent_div.addClass('scroll-right'); }
            if (!parent_div.hasClass('scroll-left')) { parent_div.addClass('scroll-left'); }
        }
    });
    scrolling_tables = document.getElementsByClassName('scrolling');
    for (var i = 0; i < scrolling_tables.length; i++) {
        scrolling_div = scrolling_tables[i];
        if (scrolling_div.offsetWidth < scrolling_div.scrollWidth) {
            if (!$('.scrolling_inner').hasClass('scroll-right')) {
                $('.scrolling_inner').addClass('scroll-right');
            }
        }
    }
});
