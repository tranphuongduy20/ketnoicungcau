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
                btn.find('span').text('Xem thêm ' + remain + ' tin tức');
            }
            else
                btn.remove();
        });
    });
});
