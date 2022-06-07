$(document).ready(function () {

    //#region Change info

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

    $("#frmChangeInfo").validate({
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
            Listcategoryid: {
                required: true,
            },
            Provinceid: {
                required: true,
            },
            Address: {
                required: true,
                maxlength: 200
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
                maxlength: "Vui lòng nhập hotline ít hơn 20 chữ số"
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

    $("#frmChangeInfo").submit(function (e) {
        flagValidateFilter = true;
        e.preventDefault();
        if ($('#frmChangeInfo').valid() && logosrcPath != "" && gpkdsrcPath != "") {
            SubmitChangeInfo();
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

    ImgUpload();

    // Gán dữ liệu Provinceoldid
    // Param tỉnh thành
    var $province = $('#list-province .filter-main ul li.act');
    if ($province.length > 0) {
        var numberArray = new Array();
        $province.each(function () {
            numberArray.push($(this).data('id'));
        });
        numberArray = numberArray.filter(unique);
        Provinceoldid = numberArray.join();
    }
})

//#region Ajax Submit

function SubmitChangeInfo() {
    var data = $("#frmChangeInfo").serializeArray();
    collectParamChangeInfo(data);
    $.ajax({
        url: '/Profile/SubmitChangeInfo',
        type: 'POST',
        cache: false,
        data: data,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res !== "" && res.code === 200) {
                $("div.error-general").hide();
                ShowPopupNotify(res.message);
            } else {
                $(".profile .error-general").text(res.errormessage);
                $(".profile .error-general").show();
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

// Lấy giá trị Listcate và ListProvince
function collectParamChangeInfo(data) {

    data.push({ name: 'Logosrc', value: logosrcPath });
    data.push({ name: 'Gpkdsrc', value: gpkdsrcPath });
    data.push({ name: 'Description', value: tinyMCE.activeEditor.getContent() });
    data.push({ name: 'Provinceoldid', value: Provinceoldid });

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
    if ($('#Fburl').val().includes("https://") == false && $('#Fburl').val() != "")
        data.find(input => input.name == 'Fburl').value = "https://" + $('#Fburl').val();
    if ($('#Weburl').val().includes("http") == false && $('#Weburl').val() != "")
        data.find(input => input.name == 'Weburl').value = "https://" + $('#Weburl').val();
}

//#endregion