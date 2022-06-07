$(document).ready(function(){ 

  //Click icon menu mở ra menu lớn
  $(".menu-icon").click(function() {
    $(".menu-show").addClass('display');
  });
  //click đóng menu lớn
  $(".close-menu").click(function() {
    $(".menu-show").removeClass('display');
  });

  //click mở submenu
  $('.menu-main > li > a').on('click', function(event) {
    event.preventDefault();
    $(this).toggleClass('arrow-rotate');
    $(this).next('.menu-main .submenu').slideToggle();
  });

  // Click menu footer
  $(".footer-info-ttl").click(function() {
    $(this).toggleClass('arrow-cr');
    $(".footer-info-list").slideToggle();
  });

  // $('.footer-info-ttl').click(function() {
  //   $('html, body').animate({
  //     scrollTop: $("footer").offset().top
  //   }, 600)
  // });

  // tab common home + page
  $(".tab-commonlist li").click(function() {
    var data_id = $(this).attr("data-id");
    $('.tab-commonlist li').removeClass('is-active');
    $('.tab-content').removeClass('show-active');
    $(this).addClass('is-active');
    $("#"+data_id).addClass('show-active');
  });


  // show popup Search
  $(".header-search input").click(function() {
    $('.search-show').fadeToggle();
    $('body').css({"overflow": "hidden"});
    $('.submit-search').fadeToggle();
    $('.search-delete').fadeToggle();
  });
});


// Click mở sub submenu
$(function() {
  var Accordion = function(el, multiple) {
    this.el = el || {};
    this.multiple = multiple || false;
    
    var dropdownlink = this.el.find('.open-sub');
    dropdownlink.on('click',{ el: this.el, multiple: this.multiple },this.dropdown);
  };
  Accordion.prototype.dropdown = function(e) {
    var $el = e.data.el,
        $this = $(this),
        $next = $this.next();
    
    $next.slideToggle();
    $this.parent().toggleClass('arrow-trans');
    
    if(!e.data.multiple) {
      $el.find('.sub-submenu').not($next).slideUp().parent().removeClass('arrow-trans');
    }
  }
  var accordion = new Accordion($('.submenu'), false);

  // click mở ra thông tin tài khoản ở menu
  $(".profile-menu-ttl").click(function() {
    $(this).toggleClass('arrow-info');
    $(".profile-menu ul").slideToggle();
  });
})