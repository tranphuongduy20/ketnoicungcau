function SubmitLogin() {
    var data = $("#frmLogin").serializeArray();
    $.ajax({
        url: '/Login/SubmitLoginV2',
        type: 'POST',
        cache: false,
        data: data,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== null && res.code === 200) {
                history.go(-1);
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