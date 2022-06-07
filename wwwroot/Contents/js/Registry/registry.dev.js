var defaultErrorMessage = "Hệ thống đang cập nhật, vui lòng thử lại sau";
var logosrcPath = "";
var gpkdsrcPath = "";
var flagResendOTPRegistry = false;
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
                        if (document.nametinyMCECkeditor != null && document.nametinyMCECkeditor != undefined)
                            document.nametinyMCECkeditor.insertContent('<img src="' + data.message + '" />');
                    }
                }
            });
        });
    }
    //#endregion

    //#region Click input chữ nhỏ lại
    $(".form-field").each(function () {
        if ($(this).find("input").val() != "")
            $(this).find("label").addClass("freeze");
        else
            $(this).find("label").removeClass("freeze");
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

    //#region Common Event

    OffScrollCommon();

    // Click box filter
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
    // click filter
    $(document).on("click", ".filter-main ul li", function () {
        $(this).toggleClass('act');
    });

    // Radio button
    $(".radio-item").click(function () {
        $('.radio-item').removeClass('act-check');
        $(this).addClass('act-check');
    });


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

    //#region Create Account

    $("#Logosrc").on("change", function (e) {
        $("#logo-error").hide();
        var formData = new FormData();
        $($('#Logosrc[type=file]')[0].files).each(function (idx, img) {
            formData.append('files' + idx, img);
        });
        $.ajax({
            type: "POST",
            url: '/Login/UploadLogo',
            contentType: false,
            processData: false,
            data: formData,
            beforeSend: function (e) {
                $(".preloader").show();
            },
            success: function (res) {
                if (res.code == -1) {
                    $("#logo-error").text(res.errormessage);
                    $("#logo-error").show();;
                }
                else if (res.message != "") {
                    logosrcPath = res.message;
                }
                HidePreloader();
            },
            error: function (xhr, status, p3, p4) {
                HidePreloader();
                alert(defaultErrorMessage);
                location.reload();
            }
        });
    });

    $("#Gpkdsrc").on("change", function (e) {
        $("#gpkd-error").hide();
        var formData = new FormData();
        $($('#Gpkdsrc[type=file]')[0].files).each(function (idx, img) {
            formData.append('files' + idx, img);
        });
        $.ajax({
            type: "POST",
            url: '/Login/UploadBusinessLicense',
            contentType: false,
            processData: false,
            data: formData,
            beforeSend: function (e) {
                $(".preloader").show();
            },
            success: function (res) {
                if (res.code == -1) {
                    $("#gpkd-error").text(res.errormessage);
                    $("#gpkd-error").show();
                }
                else if (res.message != "") {
                    gpkdsrcPath = res.message;
                }
                HidePreloader();
            },
            error: function (xhr, status, p3, p4) {
                HidePreloader();
                alert(defaultErrorMessage);
                location.reload();

            }
        });
    });

    $(".register-form").validate({
        errorElement: 'div',
        errorClass: 'error',
        errorPlacement: function (error, element) {
            error.insertAfter(element.next('label'));
        },
        rules: {
            Companyname: {
                required: true,
            },
            Taxid: {
                required: true,
                isTaxId: true,
            },
            Phonenumber: {
                required: true,
                digits: true,
                minlength: 10,
                maxlength: 10,
                isPhonenumber: true
            },
            Password: {
                required: true,
                minlength: 6,
                maxlength: 20
            },
            ConfirmPassword: {
                required: true,
                equalTo: "#Password",
                minlength: 6,
                maxlength: 20
            },
            Listcategoryid: {
                required: true,
            },
            Provinceid: {
                required: true,
            },
            Address: {
                required: true,
                maxlength: 200,
            },
            Email: {
                required: true,
                email: true,
            },
            Zaloid: {
                required: true,
                digits: true,
                minlength: 5,
                maxlength: 20
            },
            Description: {
                required: true,
            },
            Representname: {
                required: true,
            },
            Representposition: {
                required: true,
            },
        },
        messages: {
            Companyname: {
                required: "Vui lòng nhập tên công ty",
            },
            Taxid: {
                required: "Vui lòng nhập mã số thuế",
            },
            Phonenumber: {
                required: "Vui lòng nhập số điện thoại",
                minlength: "Vui lòng nhập số điện thoại gồm 10 chữ số",
                digits: "Số điện thoại chỉ bao gồm chữ số",
                maxlength: "Vui lòng nhập số điện thoại gồm 10 chữ số"
            },
            Password: {
                required: "Vui lòng nhập mật khẩu mới",
                minlength: "Mật khẩu mới quá ngắn, vui lòng nhập tối thiểu 6 ký tự",
                maxlength: "Mật khẩu mới quá dài, vui lòng nhập tối đa 20 ký tự"
            },
            ConfirmPassword: {
                required: "Vui lòng nhập lại mật khẩu mới",
                minlength: "Mật khẩu mới quá ngắn, vui lòng nhập tối thiểu 6 ký tự",
                maxlength: "Mật khẩu mới quá dài, vui lòng nhập tối đa 20 ký tự",
                equalTo: "Vui lòng nhập mật khẩu trùng khớp nhau"
            },
            Listcategoryid: {
                required: "Vui lòng chọn ít nhất một danh mục sản phẩm",
            },
            Provinceid: {
                required: "Vui lòng chọn ít nhất một Tỉnh/ Thành phố",
            },
            Address: {
                required: "Vui lòng nhập địa chỉ",
                maxlength: "Địa chỉ quá dài, vui lòng nhập tối đa 200 ký tự"
            },
            Email: {
                required: "Vui lòng nhập email của bạn",
                email: "Vui lòng nhập email hợp lệ",
            },
            Zaloid: {
                required: "Vui lòng nhập số điện thoại liên kết Zalo",
                minlength: "Vui lòng nhập hotline nhiều hơn 5 chữ số",
                digits: "Số điện thoại chỉ bao gồm chữ số",
                maxlength: "Vui lòng nhập hotline bé hơn 20 chữ số"
            },
            Description: {
                required: "Vui lòng nhập mô tả sản phẩm",
            },
            Representname: {
                required: "Vui lòng nhập tên người đại diện",
            },
            Representposition: {
                required: "Vui lòng nhập chức vụ người đại diện",
            },
        }
    });

    $(".register-form").submit(function (e) {
        flagValidateFilter = true;
        if ($('.register-form').valid() && logosrcPath != "" && gpkdsrcPath != "") {
            e.preventDefault();
            SubmitCreateCompany();
        }
        if (logosrcPath == "") {
            $("#logo-error").text("Vui lòng upload logo doanh nghiệp");
            $("#logo-error").show();
        }
        else {
            $("#logo-error").hide()
        }
        if (gpkdsrcPath == "") {
            $("#gpkd-error").text("Vui lòng upload giấy phép kinh doanh");
            $("#gpkd-error").show();
        }
        else {
            $("#gpkd-error").hide();
        }
    });
    //#endregion

    //#region Submit OTP
    $(document).on('submit', ".otp .otp-form", function (e) {
        e.preventDefault();
        if ($(".otp-form input.input-field").val() == "") {
            $("#otp-error").prev().addClass("error");
            $("#otp-error").text("Vui lòng nhập mã OTP");
            $("#otp-error").show();
        }
        else {
            $("#otp-error").prev().removeClass("error");
            $("#otp-error").hide();
            SubmitOTPRegistry();
        }
    });

    // Send OTP again
    $(document).on("click", "#send-otp-again-registry", function (e) {
        ResendOTPRegistry();
    });

    //#endregion

    // Custom upload nhiều hình cùng lúc
    ImgUpload();

    handleFilterText();
})

//#region Ajax Submit

// Submit không cần OTP
function SubmitCreateCompany() {
    var data = $(".register-form").serializeArray();
    collectParam(data);
    $.ajax({
        url: '/Login/SubmitCreateCompanyNoOTP',
        type: 'POST',
        cache: false,
        data: data,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== null && res.code === 200) {
                /*ShowPopupNotify(res.message);*/
                $("div.register").remove();
                $("header").after(res.data);
                CountDownRegistry();
            } else {
                $(".register .error-general").text(res.errormessage);
                $(".register .error-general").show();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            HidePreloader();
            alert(defaultErrorMessage);
            location.reload();
        }
    });
}

//function SubmitCreateCompany() {
//    var data = $(".register-form").serializeArray();
//    collectParam(data);
//    $.ajax({
//        url: '/Login/SubmitCreateCompany',
//        type: 'POST',
//        cache: false,
//        data: data,
//        beforeSend: function () {
//            $(".preloader").show();
//        },
//        success: function (res) {
//            HidePreloader();
//            if (res !== null && res.code === 200) {
//                /*ShowPopupNotify(res.message);*/
//                $("div.register").remove();
//                $("header").after(res.data);
//                CountDownRegistry();
//            } else {
//                $(".register .error-general").text(res.errormessage);
//                $(".register .error-general").show();
//            }
//        },
//        error: function (jqXHR, textStatus, errorThrown) {
//            HidePreloader();
//            alert(defaultErrorMessage);
//            location.reload();
//        }
//    });
//}

function ResendOTPRegistry() {
    var data = $(".otp-form").serializeArray();
    $.ajax({
        url: '/Login/ResendOTPRegistry',
        type: 'POST',
        cache: false,
        data: data,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== null && res.code === 200) {
                flagResendOTPRegistry = true;
                CountDownRegistry();
            } else {
                $(".register .error-general").text(res.errormessage);
                $(".register .error-general").show();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            HidePreloader();
            alert(defaultErrorMessage);
            location.reload();
        }
    });
}

function SubmitOTPRegistry() {
    var data = $(".otp-form").serialize();
    $.ajax({
        url: '/Login/SubmitOTPRegistry',
        type: 'POST',
        cache: false,
        data: data,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== null && res.code === 200) {
                $("div.otp").remove();
                $("header").after(res.data);
                //InitJSLogin();
            } else {
                $("#otp-error").prev().addClass("error");
                $("#otp-error").text(res.errormessage);
                $("#otp-error").show();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            HidePreloader();
            alert(defaultErrorMessage);
            location.reload();
        }
    });
}

//#endregion

//#region Utilities
function CountDownRegistry() {
    $("#send-otp-again-registry").hide();
    $("#timer-registry").text("Thời gian còn lại: 1:00");
    $("#timer-registry").show();
    var sec = 60;
    var countDownSendOTP = setInterval(function () {
        if (sec < 10)
            $("#timer-registry").text("Thời gian còn lại: 0:0" + sec);
        else
            $("#timer-registry").text("Thời gian còn lại: 0:" + sec);
        sec--;
        if (sec == 00) {
            if (flagResendOTP) {
                $("#timer-registry").text("OTP đã hết hạn, vui lòng thử lại sau");
            }
            else {
                $("#send-otp-again-registry").show();
                $("#timer-registry").hide();
            }
            clearInterval(countDownSendOTP);
        }
    }, 1000);
}

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
    // Filter
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

    if (flagValidateFilter) $('.register-form').valid();

}

// Lấy giá trị Listcate và ListProvince và Description
function collectParam(data) {
    
    // Path logo & giấy phép kinh doanh
    data.push({ name: 'Logosrc', value: logosrcPath });
    data.push({ name: 'Gpkdsrc', value: gpkdsrcPath });
    data.push({ name: 'Description', value: tinyMCE.activeEditor.getContent() });
    
    // Param danh mục sản phẩm
    var $cate = $('#list-cate .filter-main ul li.act');
    if ($cate.length > 0) {
        var numberArray = new Array();
        $cate.each(function () {
            numberArray.push($(this).data('id'));
        });
        numberArray = numberArray.filter(unique);
        data.find(input => input.name == 'Listcategoryid').value = numberArray.join();
    } else {
        data.find(input => input.name == 'Listcategoryid').value = "";
    }

    // Param tỉnh thành
    var $province = $('#list-province .filter-main ul li.act');
    if ($province.length > 0) {
        var numberArray = new Array();
        $province.each(function () {
            numberArray.push($(this).data('id'));
        });
        numberArray = numberArray.filter(unique);
        data.find(input => input.name == 'Provinceid').value = numberArray.join();
    } else {
        data.find(input => input.name == 'Provinceid').value = "";
    }

    // Captcha
    var obj = isMobile ? '.register-form #g-recaptcha-response-1' : '.register-form #g-recaptcha-response-2';
    data.push({ name: 'Captcha', value: $(obj).val() });

    if ($('#Fburl').val().includes("https://") == false && $('#Fburl').val() != "")
        data.find(input => input.name == 'Fburl').value = "https://" + $('#Fburl').val();
    if ($('#Weburl').val().includes("http") == false && $('#Weburl').val() != "")
        data.find(input => input.name == 'Weburl').value = "https://" + $('#Weburl').val();
}
// Khởi tạo lại event Login
//function InitJSLogin() {

//    // Popup quên mật khẩu
//    $("a.forgot-password").click(function () {
//        $('.show-login').fadeOut();
//        $('#forgotpass').fadeIn();
//        $(".bgsend-overlay").fadeIn();
//    })

//    // Validate
//    $("#frmLogin").validate({
//        errorElement: 'div',
//        errorClass: 'error',
//        rules: {
//            Taxid: {
//                required: true,
//            },
//            Password: {
//                required: true,
//                minlength: 6,
//                maxlength: 20
//            },
//        },
//        messages: {
//            Taxid: {
//                required: "Vui lòng nhập mã số thuế",
//            },
//            Password: {
//                required: "Vui lòng nhập mật khẩu",
//                minlength: "Mật khẩu quá ngắn, vui lòng nhập tối thiểu 6 ký tự",
//                maxlength: "Mật khẩu quá dài, vui lòng nhập tối đa 20 ký tự"
//            },
//        }
//    });

//    // Submit
//    $("#frmLogin").submit(function (e) {
//        if ($('#frmLogin').valid()) {
//            e.preventDefault();
//            SubmitLogin();
//        }
//    });
//}
//#endregion