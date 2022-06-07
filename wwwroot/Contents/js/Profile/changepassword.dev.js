$(document).ready(function () {

    $.validator.addMethod("notEqual", function (value, element, param) {
        return this.optional(element) || value != $(param).val();
    }, "Mật khẩu mới không được trùng mật khẩu cũ");

    $(".popup-send .close-send").click(function (e) {
        if ($(".popup-send .txt").text() == "Cập nhật mật khẩu thành công") {
            window.location.href = "/logout?requirelogin=true";
        }
    });

    $("#frmChangePassword").validate({
        errorElement: 'div',
        errorClass: 'error',
        rules: {
            OldPassword: {
                required: true,
                minlength: 6,
                maxlength: 20
            },
            NewPassword: {
                required: true,
                notEqual: "#pass-current",
                minlength: 6,
                maxlength: 20
            },
            ConfirmNewPassword: {
                required: true,
                equalTo: "#pass-new",
                minlength: 6,
                maxlength: 20
            },
        },
        messages: {
            OldPassword: {
                required: "Vui lòng nhập mật khẩu hiện tại",
                minlength: "Mật khẩu hiện tại quá ngắn, vui lòng nhập tối thiểu 6 ký tự",
                maxlength: "Mật khẩu hiện tại quá dài, vui lòng nhập tối đa 20 ký tự"
            },
            NewPassword: {
                required: "Vui lòng nhập mật khẩu mới",
                minlength: "Mật khẩu mới quá ngắn, vui lòng nhập tối thiểu 6 ký tự",
                maxlength: "Mật khẩu mới quá dài, vui lòng nhập tối đa 20 ký tự"
            },
            ConfirmNewPassword: {
                required: "Vui lòng nhập lại mật khẩu mới",
                minlength: "Mật khẩu mới quá ngắn, vui lòng nhập tối thiểu 6 ký tự",
                maxlength: "Mật khẩu mới quá dài, vui lòng nhập tối đa 20 ký tự",
                equalTo: "Vui lòng nhập mật khẩu trùng khớp nhau"
            },
        }
    });

    $("#frmChangePassword").submit(function (e) {
        console.log("ab");
        if ($('#frmChangePassword').valid()) {
            e.preventDefault();
            SubmitChangePassword();
        }
    });

})

function SubmitChangePassword() {
    var data = $("#frmChangePassword").serialize();
    $.ajax({
        url: '/Profile/SubmitChangePassword',
        type: 'POST',
        cache: false,
        data: data,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== "" && res.code === 200) {
                ShowPopupNotify(res.message);
            } else {
                //ShowPopupNotify(res.errormessage);
                $(".error-general").text(res.errormessage);
                $(".error-general").show();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            HidePreloader();
            alert(defaultErrorMessage);
            location.reload();
        }
    });
}
