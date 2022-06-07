$(document).ready(function() {

    // click input chữ nhỏ lại
    var formFields = $('.form-field');
    formFields.each(function() {
      var field = $(this);
      var input = field.find('input');
      var label = field.find('label');
      
      function checkInput() {
        var valueLength = input.val().length;
        
        if (valueLength > 0 ) {
          label.addClass('freeze')
        }
        else {
          label.removeClass('freeze')
        }
      }
      input.change(function() {
        checkInput()
      })
    });

    // upload Images form
    $(".file-upload").each(function() {
      var fI  = $(this).children('input'), 
          btn = $(this).children('.btn-upload'), 
          i1  = $(this).children('.i-pic-upload'),
        i2  = $(this).children('.i-deselect'), 
        bT  = $(this).find('.text-browse');

        btn.click(function(b) {
          b.preventDefault(); 
          fI.click();
        });
        function readURL(input) {
          if (input.files && input.files[0]) {
            var reader = new FileReader();
              reader.onload = function(e) {
              // i1.css("background-image", "url("+e.target.result+")").fadeIn();
              i1.append('<img src="'+e.target.result+' " alt="" title="" />').addClass('show');
            }
            reader.readAsDataURL(input.files[0]);
          }
        }
        fI.change(function(e) {
          readURL(this); 
          var fN = e.target.files[0].name; 
          i2.fadeIn(200); 
          btn.hide();
        });
        $('.i-deselect').click(function() {
          window.setTimeout(function() {
            i1.removeClass('show');
            btn.show();
            fI.val(null);
        }, 200);
      });
    });

    // click dropdown form
    $('.select-ttl').on('click', function(event) {
      event.preventDefault();
      $(this).toggleClass('current');
      $(this).next('.select-list').fadeToggle();
    });

    //radio button
    $(".radio-item").click(function() {
      $('.radio-item').removeClass('act-check');
      $(this).addClass('act-check');
    });


    // click dropdown
    $(".box-show").click(function() {
      $(this).toggleClass('rotate');
      $('.boxlist').fadeToggle();
    });

    // Custom upload nhiều hình cùng lúc
    ImgUpload();
  });

// Custom upload nhiều hình cùng lúc
function ImgUpload() {
  var imgWrap = "";
  var imgArray = [];

  $('.uploadmulti__inputfile').each(function () {
    $(this).on('change', function (e) {
      imgWrap = $(this).closest('.uploadmulti__box').find('.uploadmulti__img-wrap');
      var maxLength = $(this).attr('data-max_length');

      var files = e.target.files;
      var filesArr = Array.prototype.slice.call(files);
      var iterator = 0;
      filesArr.forEach(function (f, index) {

        if (!f.type.match('image.*')) {
          return;
        }

        if (imgArray.length > maxLength) {
          return false
        } else {
          var len = 0;
          for (var i = 0; i < imgArray.length; i++) {
            if (imgArray[i] !== undefined) {
              len++;
            }
          }
          if (len > maxLength) {
            return false;
          } else {
            imgArray.push(f);

            var reader = new FileReader();
            reader.onload = function (e) {
              var html = "<div class='uploadmulti__img-box'><div style='background-image: url(" + e.target.result + ")' data-number='" + $(".uploadmulti__img-close").length + "' data-file='" + f.name + "' class='img-bg'><div class='uploadmulti__img-close'><i class='icon-clearfile'></i></div></div></div>";
              imgWrap.prepend(html);
              iterator++;
            }
            reader.readAsDataURL(f);
          }
        }
      });
    });
  });
  $('body').on('click', ".uploadmulti__img-close", function (e) {
    var file = $(this).parent().data("file");
    for (var i = 0; i < imgArray.length; i++) {
      if (imgArray[i].name === file) {
        imgArray.splice(i, 1);
        break;
      }
    }
    $(this).parent().parent().remove();
  });
}