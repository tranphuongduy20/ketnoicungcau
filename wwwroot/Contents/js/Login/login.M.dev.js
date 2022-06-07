var defaultErrorMessage = "Hệ thống đang cập nhật, vui lòng thử lại sau";
var confirmPath = "";
var licensePath = "";
var flagResendOTP = false;

$(document).ready(function () {

    //#region Upload Images form
    $(".file-upload").each(function () {
        var fI = $(this).children('input'),
            btn = $(this).children('.btn-upload'),
            i1 = $(this).children('.i-pic-upload'),
            i2 = $(this).children('.i-deselect'),
            bT = $(this).find('.text-browse');

        btn.click(function (b) {
            b.preventDefault();
            fI.click();
        });
        function readURL(input) {
            if (input.files && input.files[0]) {
                var reader = new FileReader();
                reader.onload = function (e) {
                    // i1.css("background-image", "url("+e.target.result+")").fadeIn();
                    i1.append('<img src="' + e.target.result + ' " alt="" title="" />').addClass('show');
                }
                reader.readAsDataURL(input.files[0]);
            }
        }
        fI.change(function (e) {
            readURL(this);
            var fN = e.target.files[0].name;
            i2.fadeIn(200);
            btn.hide();
        });
        $('.i-deselect').click(function () {
            console.log($(this));
            if ($(this).hasClass("del-logo")) logosrcPath = "";
            if ($(this).hasClass("del-gpkd")) gpkdsrcPath = "";
            if ($(this).hasClass("del-license")) licensePath = "";
            if ($(this).hasClass("del-confirm")) confirmPath = "";
            $target = $(this).parents(".file-upload");
            window.setTimeout(function () {
                $target.find("img").remove();
                $target.find(".i-pic-upload").removeClass('show');
                $target.find(".btn-upload").show();
                fI.val(null);
            }, 200);
        });
    });
    //#endregion

    //#region Popup

    // Popup Đăng nhập
    $(".boxitem-user").click(function (e) {
        if ($(e.target).closest(".show-login").length == 0 || $(e.target).parent(".boxitem-user").length == 1) {
            $('.show-login').fadeToggle();
        }
    });
    $("body").click(function () {
        $('.show-login').fadeOut();
    });
    $(".boxitem-user").click(function (a) {
        a.stopPropagation()
    });

    // Show popup ForgotPassword
    $("a.forgot-password").click(function () {
        $('.show-login').fadeOut();
        $('#forgotpass').fadeIn();
    })

    // Show popup ForgotPasswordByTax
    $("#forgot-password-bytax").click(function () {
        $('#forgotpass').fadeOut();
        $('#submit-forgotpass-bytax').fadeIn();
    })

    // Close Popup
    $(document).on("click", ".box-button .btn-close, .bgsend-overlay", function (e) {
        e.preventDefault();
        HideAllPopup();
    });
    //#endregion

    //#region Login
    $("#frmLogin").validate({
        errorElement: 'div',
        errorClass: 'error',
        rules: {
            Taxid: {
                required: true,
                isTaxId: true,
            },
            Password: {
                required: true,
                minlength: 6,
                maxlength: 20
            },
        },
        messages: {
            Taxid: {
                required: "Vui lòng nhập mã số thuế",
            },
            Password: {
                required: "Vui lòng nhập mật khẩu",
                minlength: "Mật khẩu quá ngắn, vui lòng nhập tối thiểu 6 ký tự",
                maxlength: "Mật khẩu quá dài, vui lòng nhập tối đa 20 ký tự"
            },
        }
    });

    $("#frmLogin").submit(function (e) {
        if ($('#frmLogin').valid()) {
            e.preventDefault();
            SubmitLogin();
        }
    });
    //#endregion

    //#region Login Popup
    $(".show-login-wrap").validate({
        errorElement: 'div',
        errorClass: 'error',
        rules: {
            Taxid: {
                required: true,
                isTaxId: true,
            },
            Password: {
                required: true,
                minlength: 6,
                maxlength: 20
            },
        },
        messages: {
            Taxid: {
                required: "Vui lòng nhập mã số thuế",
            },
            Password: {
                required: "Vui lòng nhập mật khẩu",
                minlength: "Mật khẩu quá ngắn, vui lòng nhập tối thiểu 6 ký tự",
                maxlength: "Mật khẩu quá dài, vui lòng nhập tối đa 20 ký tự"
            },
        }
    });

    $(".show-login-wrap").submit(function (e) {
        if ($('.show-login-wrap').valid()) {
            e.preventDefault();
            SubmitLoginPopup();
        }
    });
    //#endregion

    //#region Forget Password
    $("#forgotpass form").submit(function (e) {
        e.preventDefault();
        var strPhone = $('#forgotpass form .Phonenumber').val().toString().length;
        if (strPhone != 10) {
            $("#forgotpass form .Phonenumber").addClass("error");
            $("#forgotpass form #Phonenumber-error").text("Vui lòng nhập số điện thoại có 10 chữ số");
            $("#forgotpass form #Phonenumber-error").show();
        }
        else {
            $("#forgotpass form input.Phonenumber").removeClass("error");
            $("#forgotpass form #Phonenumber-error").hide(); 
            ForgotPassword();
        }
    });

    // Send OTP again
    $(document).on("click", "#send-otp-again", function (e) {
        ResendOTPForgotPassword();
    });

    //#endregion

    //#region Submit Forget Password
    $("#submit-forgotpass form").validate({
        errorElement: 'div',
        errorClass: 'error',
        rules: {
            OTP: {
                required: true,
                minlength: 4,
                maxlength: 4
            },
            Password: {
                required: true,
                minlength: 6,
                maxlength: 20
            },
            ConfirmPassword: {
                required: true,
                equalTo: "#submit-forgotpass .Password",
                minlength: 6,
                maxlength: 20
            },
        },
        messages: {
            OTP: {
                required: "Vui lòng nhập mã OTP",
                minlength: "Vui lòng nhập mã OTP có 4 chữ số",
                maxlength: "Vui lòng nhập mã OTP có 4 chữ số"
            },
            Password: {
                required: "Vui lòng nhập mật khẩu",
                minlength: "Mật khẩu quá ngắn, vui lòng nhập tối thiểu 6 ký tự",
                maxlength: "Mật khẩu quá dài, vui lòng nhập tối đa 20 ký tự"
            },
            ConfirmPassword: {
                required: "Vui lòng nhập mật khẩu",
                minlength: "Mật khẩu quá ngắn, vui lòng nhập tối thiểu 6 ký tự",
                maxlength: "Mật khẩu quá dài, vui lòng nhập tối đa 20 ký tự",
                equalTo: "Vui lòng nhập mật khẩu trùng khớp nhau"
            },
        }
    });

    $("#submit-forgotpass form").submit(function (e) {
        if ($('#submit-forgotpass form').valid()) {
            e.preventDefault();
            SubmitForgotPassword();
        } 
    });
    //#endregion

    //#region Submit Forget Password By Taxid

    $("#Licenseimage").on("change", function (e) {
        $("#license-error").hide();
        var formData = new FormData();
        $($('#Licenseimage[type=file]')[0].files).each(function (idx, img) {
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
                    $("#license-error").text(res.errormessage);
                    $("#license-error").show();
                }
                else if (res.message != "") {
                    licensePath = res.message;
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

    $("#Confirmimage").on("change", function (e) {
        $("#confirm-error").hide();
        var formData = new FormData();
        $($('#Confirmimage[type=file]')[0].files).each(function (idx, img) {
            formData.append('files' + idx, img);
        });
        $.ajax({
            type: "POST",
            url: '/Login/UploadConfirmImage',
            contentType: false,
            processData: false,
            data: formData,
            beforeSend: function (e) {
                $(".preloader").show();
            },
            success: function (res) {
                if (res.code == -1) {
                    $("#confirm-error").text(res.errormessage);
                    $("#confirm-error").show();;
                }
                else if (res.message != "") {
                    confirmPath = res.message;
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

    $("#submit-forgotpass-bytax form").validate({
        errorElement: 'div',
        errorClass: 'error',
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
                maxlength: 10
            },
            Email: {
                required: true,
                email: true,
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
            Email: {
                required: "Vui lòng nhập email của bạn",
                email: "Vui lòng nhập email hợp lệ",
            },
        }
    });

    $("#submit-forgotpass-bytax form").submit(function (e) {
        if ($('#submit-forgotpass-bytax form').valid() && confirmPath != "" && licensePath != "") {
            e.preventDefault();
            SubmitForgotPasswordByTax();
        }
        if (confirmPath == "") {
            $("#confirm-error").text("Vui lòng upload bản scan xác nhận tài khoản");
            $("#confirm-error").show();
        }
        else {
            $("#confirm-error").hide()
        }
        if (licensePath == "") {
            $("#license-error").text("Vui lòng upload giấy phép kinh doanh");
            $("#license-error").show();
        }
        else {
            $("#license-error").hide();
        }
    });
    //#endregion
})

function CountDown() {
    $("#send-otp-again").hide();
    $("#timer").text("Thời gian còn lại: 1:00");
    $("#timer").show();
    var sec = 60;
    var countDownSendOTP = setInterval(function () {
        if (sec < 10)
            $("#timer").text("Thời gian còn lại: 0:0" + sec);
        else
            $("#timer").text("Thời gian còn lại: 0:" + sec);
        sec--;
        if (sec == 00) {
            if (flagResendOTP) {
                $("#timer").text("OTP đã hết hạn, vui lòng thử lại sau");
            }
            else {
                $("#send-otp-again").show();
                $("#timer").hide();
            }
            clearInterval(countDownSendOTP);
        }
    }, 1000);
}

function SubmitLogin() {
    var data = $("#frmLogin").serializeArray();
    data.push({ name: 'Captcha', value: $('#frmLogin #g-recaptcha-response-1').val() });
    $.ajax({
        url: '/Login/SubmitLogin',
        type: 'POST',
        cache: false,
        data: data,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== null && res.code === 200) {
                if (document.referrer != "") {
                    const url = new URL(document.referrer)
                    if (url.pathname == "/doi-mat-khau") window.location.href = "/";
                    else history.go(-1);
                }
                else window.location.href = "/";
            } else {
                $("#frmLogin .error-general").text(res.errormessage);
                $("#frmLogin .error-general").show();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            HidePreloader();
            alert(defaultErrorMessage);
            location.reload();
        }
    });
}

function SubmitLoginPopup() {
    var data = $(".show-login-wrap").serializeArray();
    data.push({ name: 'Captcha', value: $('.show-login-wrap #g-recaptcha-response').val() });
    $.ajax({
        url: '/Login/SubmitLogin',
        type: 'POST',
        cache: false,
        data: data,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== null && res.code === 200) {
                window.location.href = "/profile";
            } else {
                $(".show-login-wrap .error-general").text(res.errormessage);
                $(".show-login-wrap .error-general").show();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            HidePreloader();
            alert(defaultErrorMessage);
            location.reload();
        }
    });
}

function ForgotPassword() {
    var data = $("#forgotpass form").serializeArray();
    data.push({ name: 'Captcha', value: $('.show-forgotpass #g-recaptcha-response').val() });
    $.ajax({
        url: '/Login/ForgotPassword',
        type: 'POST',
        cache: false,
        data: data,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== null && res.code === 200) {
                $("#submit-forgotpass form p b").text($("#forgotpass .Phonenumber").val());
                $("#submit-forgotpass .Phonenumber").val($("#forgotpass .Phonenumber").val());
                ShowPopupSubmitForgotPassword();
                CountDown();
            } else {
                $("#forgotpass form #Phonenumber-error").text(res.errormessage);
                $("#forgotpass form #Phonenumber-error").show();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            HidePreloader();
            alert(defaultErrorMessage);
            location.reload();
        }
    });
}

function ResendOTPForgotPassword() {
    var data = $("#forgotpass form").serializeArray();
    console.log(data);
    $.ajax({
        url: '/Login/ResendOTPForgotPassword',
        type: 'POST',
        cache: false,
        data: data,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== null && res.code === 200) {
                flagResendOTP = true;
                CountDown();
            } else {
                $("#submit-forgotpass .error-general").text(res.errormessage);
                $("#submit-forgotpass .error-general").show();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            HidePreloader();
            alert(defaultErrorMessage);
            location.reload();
        }
    });
}

function SubmitForgotPassword() {
    var data = $("#submit-forgotpass form").serializeArray();
    console.log(data);
    $.ajax({
        url: '/Login/SubmitForgotPassword',
        type: 'POST',
        cache: false,
        data: data,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== null && res.code === 200) {
                HideAllPopup();
                ShowPopupNotify(res.message);
            } else {
                $("#submit-forgotpass .error-general").text(res.errormessage);
                $("#submit-forgotpass .error-general").show();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            HidePreloader();
            alert(defaultErrorMessage);
            location.reload();
        }
    });
}

function SubmitForgotPasswordByTax() {

    var data = $("#submit-forgotpass-bytax form").serializeArray();
    // Path logo & giấy phép kinh doanh
    data.push({ name: 'Licenseimage', value: licensePath });
    data.push({ name: 'Confirmimage', value: confirmPath });

    $.ajax({
        url: '/Login/SubmitRenewUserByTax',
        type: 'POST',
        cache: false,
        data: data,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== null && res.code === 200) {
                HideAllPopup();
                $(".successtax").fadeIn();
            } else {
                $("#submit-forgotpass-bytax .error-general").text(res.errormessage);
                $("#submit-forgotpass-bytax .error-general").show();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            HidePreloader();
            alert(defaultErrorMessage);
            location.reload();
        }
    });
}

function ShowPopupSubmitForgotPassword() {
    $('#forgotpass').fadeOut();
    $('#submit-forgotpass').fadeIn();
}

function HideAllPopup() {
    // Hide
    $('#forgotpass').fadeOut();
    $('#submit-forgotpass').fadeOut();
    $('#submit-forgotpass-bytax').fadeOut();
    $('.popup-connection').fadeOut();
    // Reset Text
    $('#forgotpass form').trigger('reset');
    $('#submit-forgotpass form').trigger('reset');
    $('#submit-forgotpass-bytax form').trigger('reset');
    // Reset Validate
    $("#submit-forgotpass form").data('validator').resetForm();
    //$("#submit-forgotpass-bytax form").data('validator').resetForm();
    $("div.error-general").hide();
}

function MaxLength(e) {
    if (e.value.length > e.maxLength) e.value = e.value.slice(0, e.maxLength);
}
