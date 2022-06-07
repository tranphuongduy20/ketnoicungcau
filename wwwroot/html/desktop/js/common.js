$(document).ready(function(){ 

  // tab
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
  });
  $("body").click(function() {
    $('.search-show').fadeOut();
  });
  $(".header-search").click(function(a) {
    a.stopPropagation()
  });

  // show popup Đăng nhập
  $(".boxitem-user").click(function() {
    $('.show-login').fadeToggle();
  });
  $("body").click(function() {
    $('.show-login').fadeOut();
  });
  $(".boxitem-user").click(function(a) {
    a.stopPropagation()
  });
});