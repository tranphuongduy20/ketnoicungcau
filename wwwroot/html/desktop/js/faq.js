$(document).ready(function() {
    $('.question').on('click', function(event) {
        event.preventDefault();
        $(this).toggleClass('active');
        $(this).next('.faqlist .answer').slideToggle();
    });

    // Popup thông báo đã kết nối với doanh nghiêp
    $(".send-faq").click(function() {
        $('.bgsend-overlay').fadeIn();
        $('.popup-send').fadeIn();
    });
    $(".close-send").click(function() {
        $('.bgsend-overlay').fadeOut();
        $('.popup-send').fadeOut();
    });
    $(".bgsend-overlay").click(function() {
        $('.bgsend-overlay').fadeOut();
        $('.popup-send').fadeOut();
    });

    var totalHeight = 270;
    $('.faqlist li:nth-child(-n+10)').each(function(){
        totalHeight+=$(this).outerHeight();
    })
    $('.faqlist.height-scroll').css("height", totalHeight);
})