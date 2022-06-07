var flagValidateFilter = false;

$(document).ready(function () {

    //#region Editor
    var heightEditor = 300;
    if (isMobile)
        heightEditor = 150;

    if (document.isCkeditor != undefined && document.isCkeditor == true && document.nametinyMCECkeditor != undefined && document.nameCkeditor != undefined) {
        $(document).ready(function () {
            document.nametinyMCECkeditor = null;
            tinymce.init({
                selector: `#${document.nameCkeditor}`,
                height: heightEditor,
                relative_urls: false,
                plugins: [
                    "advlist autolink lists link image charmap print preview hr anchor pagebreak",
                    "searchreplace wordcount visualblocks visualchars code fullscreen",
                    "insertdatetime media nonbreaking save table contextmenu directionality",
                    "emoticons template paste textcolor lineheight"
                ],
                menubar: "true",
                toolbar: "undo redo | formatselect | bold italic | pastetext alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link | imageupload | media",
                setup: function (editor) {
                    editor.addButton('imageupload', {
                        text: '',
                        icon: 'image',
                        tooltip: 'Insert/edit image',
                        onclick: function () {
                            $(`#${document.nameCkeditor}_upload`).trigger('click');
                            document.nametinyMCECkeditor = editor;
                        }
                    });
                },
                image_advtab: true,
                skin: 'lightgray',
                init_instance_callback: function (editor) {
                    editor.on('keyUp', function (e) {
                        $(`#${document.nameCkeditor}`).val(editor.getContent());
                    });
                }
            });
            $(`#${document.nameCkeditor}_upload`).html5Uploader({
                postUrl: '/Common/UploadImageCkEditor',
                onClientLoadEnd: function (e, file) {
                }, onServerLoadStart: function (e, file) {
                }, onServerProgress: function (e, file) {
                }, onServerLoad: function (e, file) {
                }, onSuccess: function (e) {
                    var data = $.parseJSON(e.currentTarget.response);
                    if (data.code != 200) {
                        alert(data.errormessage);
                        return;
                    } else {
                        if (document.nametinyMCECkeditor != null && document.nametinyMCECkeditor != undefined) {
                            document.nametinyMCECkeditor.insertContent('<img src="' + data.message + '" />');
                            console.log(data.message);
                            console.log('<img src="' + data.message + '" />');
                        }
                            
                    }
                }
            });
        });
    }
    //#endregion

    //#region Common Event

    OffScrollCommon();

    // Click filter
    $(document).on('click', ".filter__item input", function (e) {
        if ($(this).hasClass("filter-input")) {
            e.preventDefault();
            var notthis = $('.filter-input.current-filter').not(this);
            notthis.removeClass('.current-filter');
            notthis.toggleClass('current-filter').nextAll('.filter-main').fadeOut(300);
            $(this).toggleClass('current-filter').nextAll('.filter-main').fadeToggle("fast");
            $(this).parent(".filter__item").toggleClass('current-filter');
            if (isMobile)
                $(".bgsend-overlay").show();
        }
    });
    $(document).on("click", ".filter-main ul li", function () {
        $(this).toggleClass('act');
    });

    // Radio button
    //$(".radio-item").click(function () {
    //    $('.radio-item').removeClass('act-check');
    //    $(this).addClass('act-check');
    //});

    // Click dropdown
    $(".box-show").click(function () {
        $(this).toggleClass('rotate');
        $('.boxlist').fadeToggle();
    });

    // Search filter
    $(document).on('keyup', ".filter-search input", function (e) {
        searchFilterCommon($(this));
    });

    // Off popup khi click ra ngoài popup
    $(document).click(function (e) {
        closePopupFilter(e);
    });
    //#endregion

    //#region Click input chữ nhỏ lại
    $(".form-field").each(function () {
        if ($(this).find("input").val() != "")
            $(this).find("label").addClass("freeze");
        else
            $(this).find("label").removeClass("freeze");
        if ($(this).find("label").hasClass("always-freeze")) {
            $(this).find("label").addClass("freeze");
            console.log("freeze");
        }
            
    });
    var formFields = $('.form-field');
    formFields.each(function () {
        var field = $(this);
        var input = field.find('input');
        var label = field.find('label');

        function checkInput() {
            var valueLength = input.val().length;

            if (valueLength > 0) {
                label.addClass('freeze')
            }
            else {
                label.removeClass('freeze')
            }
        }
        input.change(function () {
            checkInput()
        })
    });
    //#endregion


    handleFilterText();
})

//#region Utilities

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

// Tắt popup Filter
function closePopupFilter(e) {

    if ($(e.target).closest(".filter__item").length == 0) {
        $(".filter-search input").val("");
        
        $('.filter-main').hide("fast");
        $('.filter__item').removeClass("current-filter");

        $(".filter-search input").each(function () {
            searchFilterCommon($(this));
        });
    }
    handleFilterText();
}

// Đặt lại label Filter
function handleFilterText() {

    var $cate = $('#list-cate .filter-main ul li.act');
    if ($cate.length > 0) {
        var textArray = new Array();
        $cate.each(function () {
            textArray.push($(this).text());
        });
        $("#list-cate label").addClass("freeze");
        $("#list-cate input:first").val(textArray.join(", "));
    } else {
        $("#list-cate label").removeClass("freeze");
        $("#list-cate input:first").val("");
    }

    var $province = $('#list-province .filter-main ul li.act');
    if ($province.length > 0) {
        var textArray = new Array();
        $province.each(function () {
            textArray.push($(this).text());
        });
        $("#list-province label").addClass("freeze");
        $("#list-province input:first").val(textArray.join(", "));
    } else {
        $("#list-province label").removeClass("freeze");
        $("#list-province input:first").val("");
    }

    if (flagValidateFilter) {
        if ($('#frmChangeInfo').length > 0) $('#frmChangeInfo').valid()
    }
}
//#endregion