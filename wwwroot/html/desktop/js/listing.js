$(document).ready(function(){ 

    // sort by
    $(".click-sort").click(function() {
        $(".sort-main").fadeToggle(300);
        $(this).toggleClass('sort-rotate');
    });
    $('.sort-main p').on('click', function(){
        $(this).addClass('selected').siblings().removeClass('selected');
    });

    // filter
    $('.filter-txt').click(function(e) {
        e.preventDefault();
        var notthis = $('.current-filter').not(this);
        notthis.toggleClass('current-filter').next('.filter-main').fadeToggle(300);
        $(this).toggleClass('current-filter').next().fadeToggle("fast");
        // if($('.jsTitle').hasClass('current-filter')) {
        //   $('.breadcrumb').addClass('heighter');
        // } else {
        //   $('.breadcrumb').removeClass('heighter');
        // }
    });
});