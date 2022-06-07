//------ define-----------------
var imgArray = [];
var numUnit = 2;
var timmer;
var typing = false;
var productImg = "";
/*var flagFirstHandleTextAddProduct = true;*/

if (query.Productid > 0) {
    if (query.ProductImg !== "") {
        productImg = query.ProductImg;
        console.log(productImg);
        console.log(imgArray);
    }

    numUnit = query.NumUnit;

}
//--------------------------------



$(document).ready(function() {

    //#region Common Event
    // Tab click
    $('.tab-click').click(function() {
        var tab_id = $(this).attr('data-tab');

        $('.tab-click').removeClass('current');
        $('.tab-content').removeClass('current');

        $(this).addClass('current');
        $("#" + tab_id).addClass('current');
    });
    $('.wait-connect').click(function() {
        $('.popup-connect').addClass('active');
    });
    $('.close-popup').click(function() {
        $('.popup-connect').removeClass('active');
    });
    $('.btn-connect a.refuse').click(function() {
        $('.popup-connect').removeClass('active');
    });
    $('.bg-connect').click(function() {
        $('.popup-connect').removeClass('active');
        $('.popup-delsp').removeClass('active');
    });
    $('.click-del').click(function() {
        $('.popup-delsp').addClass('active');
    });
    $('.btn-back').click(function() {
        $('.popup-delsp').removeClass('active');
    });
    $(".close-send").click(function () {
        window.location.href = '/productlist?type=' + query.AddType;
    });
    $("#show").click(function () {
        $('#hide').removeClass('act-check');
        $(this).addClass('act-check');
    });
    $("#hide").click(function () {
        $('#show').removeClass('act-check');
        $(this).addClass('act-check');
    });
    $("#showPrice").click(function () {
        $('#hidePrice').removeClass('act-check');
        $(this).addClass('act-check');
    });
    $("#hidePrice").click(function () {
        $('#showPrice').removeClass('act-check');
        $(this).addClass('act-check');
    });

    //#region addproduct

    $("#frmAddProductSell").validate({
        errorElement: 'div',
        errorClass: 'error',
        errorPlacement: function (error, element) {
            error.insertAfter(element.next('label'));
        },
        rules: {
            Productname: {
                required: true,
            },
            Categoryid: {
                required: true,
            },
            Provinceid: {
                required: true,
            },
            Fullname: {
                required: true,
            },
            Position: {
                required: true,
            },
            Phonenumber: {
                required: true,
                digits: true,
                minlength: 10,
                maxlength: 10,
                isPhonenumber: true
            },
            Shortdescription: {
                required: true,
            },
            Description: {
                required: true,
            },
            Orderprocedure: {
            }
        },
        messages: {
            Productname: {
                required: "Tên sản phẩm không được bỏ trống",
            },
            Categoryid: {
                required: "Danh mục sản phẩm không được bỏ trống",
            },
            Phonenumber: {
                required: "Số điện thoại người đại diện không được bỏ trống",
                minlength: "Vui lòng nhập số điện thoại gồm 10 chữ số",
                digits: "Số điện thoại chỉ bao gồm chữ số",
                maxlength: "Vui lòng nhập số điện thoại gồm 10 chữ số"
            },
            Provinceid: {
                required: "Tỉnh/ Thành phố không được bỏ trống",
            },
            Fullname: {
                required: "Tên người đại diện không được bỏ trống",

            },
            Position: {
                required: "Chức vụ người đại diện không được bỏ trống",

            },
            Shortdescription: {
                required: "Mô tả ngắn không được bỏ trống",
            },
            Description: {
                required: "Mô tả không được bỏ trống",
            },
            Orderprocedure: {
            },
        }
    });

    $("#frmAddProductBuy").validate({
        errorElement: 'div',
        errorClass: 'error',
        errorPlacement: function (error, element) {
            error.insertAfter(element.next('label'));
        },
        rules: {
            Productname: {
                required: true,
            },
            Categoryid: {
                required: true,
            },
            //Provinceid: {
            //    required: true,
            //},
            Fullname: {
                required: true,
            },
            Position: {
                required: true,
            },
            Phonenumber: {
                required: true,
                digits: true,
                minlength: 10,
                maxlength: 10,
                isPhonenumber: true
            },
            Shortdescription: {
                required: true,
            },
            Description: {
                required: true,
            },
            //Frequency: {
            //    required: true,
            //},
            Isbuy: {
                required: true,
            },
            Quantity: {
                required: true,
                min: 1
            },
            Unitid : {
                required: true,
            },
            Orderprocedure: {
            }
        },
        messages: {
            Productname: {
                required: "Tên sản phẩm không được bỏ trống",

            },
            Categoryid: {
                required: "Danh mục sản phẩm không được bỏ trống",
            },
            Phonenumber: {
                required: "Số điện thoại người đại diện không được bỏ trống",
                minlength: "Vui lòng nhập số điện thoại gồm 10 chữ số",
                digits: "Số điện thoại chỉ bao gồm chữ số",
                maxlength: "Vui lòng nhập số điện thoại gồm 10 chữ số"
            },
            //Provinceid: {
            //    required: "Tỉnh/ Thành phố không được bỏ trống",
            //},
            Fullname: {
                required: "Tên người đại diện không được bỏ trống",
            },
            Position: {
                required: "Chức vụ người đại diện không được bỏ trống",
            },
            Shortdescription: {
                required: "Mô tả ngắn không được bỏ trống",
            },
            Description: {
                required: "Mô tả không được bỏ trống",
            },
            //Frequency: {
            //    required: "Tần suất mua không được bỏ trống",
            //},
            Isbuy: {
                required: "Nhu cầu mua không được bỏ trống",
            },
            Quantity: {
                required: "Sản lượng mua không được bỏ trống",
                min: "Sản lượng mua nhỏ nhất là 1",
            },
            Unitid: {
                required: "Đơn vị không được bỏ trống",
            },
            Orderprocedure: {
            }
        }
    });

    $("#frmAddProductSell").submit(function(e) {
        e.preventDefault();
        flagValidateFilter = true;
        if ($('#frmAddProductSell').valid()) {
            SubmitAddProduct();
        }
       
    });

    $("#frmAddProductBuy").submit(function (e) {
        e.preventDefault();
        flagValidateFilter = true;
        if ($('#frmAddProductBuy').valid()) {
            SubmitAddProduct();
        }

    });

    //#endregion

    // Custom upload nhiều hình cùng lúc
    ImgUploadAddProduct();

});

// click dropdown form
$('#select-unit').on('click',
    function (event) {
        event.preventDefault();
        $(this).toggleClass('current');
        $(this).next('.select-list').fadeToggle();
    });

$(document).on("click",
    "#unit-select ul li",
    function () {
        var id = $(this).closest('.filter__item').attr("id");;
        //console.log(id);
        $(this).toggleClass('act');
        var selector = '#' + id + ' .filter-main ul';
        $(selector).find('li.act').removeClass('act');
    });

$(document).on("click",
    "#isfrequency-select ul li",
    function () {
        $(this).toggleClass('act');
        $('#isfrequency-select ul').find('li.act').removeClass('act');
    });

$(document).on("click",
    "#isbuy-select ul li",
    function () {
        $(this).toggleClass('act');
        $('#isbuy-select ul').find('li.act').removeClass('act');
    });

$(document).on("click",
    "#province-select ul li",
    function () {
        $(this).toggleClass('act');
        $('#province-select ul').find('li.act').removeClass('act');
    });

$(document).on("click",
    "#baseUnit-select ul li",
    function () {
        $(this).toggleClass('act');
        $('#baseUnit-select ul').find('li.act').removeClass('act');
    });

//#region Ajax Submit

function SubmitAddProduct() {
    //UploadImg();
    var formdata;
    if (query.AddType == 2) {
        formdata = $('#frmAddProductSell').serializeArray();
    } else {
        formdata = $('#frmAddProductBuy').serializeArray();
    }
    //debugger 
    collectParam(formdata);
    var data = {};
    $(formdata).each(function (index, obj) {
        data[obj.name] = obj.value;
    });
    
    //debugger 
    $.ajax({
        url: '/Profile/UpdateProduct',
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
                $("#error-general").text(res.errormessage);
                $("#error-general").show();
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            HidePreloader();
            ShowPopupNotify(defaultErrorMessage);
        }
    });
}


function AddBoxPrice(e) {
    $.ajax({
        url: '/Profile/GetBoxPrice?numId=' + numUnit,
        type: 'GET',
        cache: false,
        beforeSend: function () {
            $(".preloader").show();
        },
        success: function (res) {
            HidePreloader();
            if (res != null || res.boxPrice !== '' || res.boxPrice.trim() !== '') {
                e.remove();
                $('.price').append(res.boxPrice);
                numUnit++;
            }
            if ($('.price-main').length >= 5)
                $('.btn-addprice').hide();
        },
        error: function (jqXHR, textStatus, errorThrown) {
            HidePreloader();
        }
    });
}

function DeleteBoxPrice(e) {
    $(e).parent().parent().remove();
}

//#endregion

//#region Utilities

function ImgUploadAddProduct() {
    var imgWrap = "";

    $('.uploadmulti__inputfile').each(function () {
        $(this).on('change', function (e) {
            imgWrap = $(this).closest('.uploadmulti__box').find('.uploadmulti__img-wrap');
            var maxLength = $(this).attr('data-max_length');
            var isCallAjax = true;

            var files = e.target.files;
            var filesArr = Array.prototype.slice.call(files);
            var iterator = 0;
            filesArr.forEach(function (f, index) {

                if (!f.type.match('image.*')) {
                    isCallAjax = false;
                    return;
                }

                if (imgArray.length > maxLength) {
                    $('#error-maxLen').text('Chỉ có thể tải lên tối đa 30 ảnh');
                    $('#error-maxLen').show();
                    isCallAjax = false;
                    return false;
                } else {
                    $('#error-maxLen').hide();
                    var len = 0;
                    for (var i = 0; i < imgArray.length; i++) {
                        if (imgArray[i].file !== undefined) {
                            len++;
                        }
                    }
                    if (len > maxLength) {
                        $('#error-maxLen').text('Chỉ có thể tải lên tối đa 30 ảnh');
                        $('#error-maxLen').show();
                        isCallAjax = false;
                        return false;
                    } else {
                        $('#error-maxLen').hide();
                        var item = {
                            file: f,
                            path: "",
                            isUpload: false,
                        }
                        var isExist = false;
                        for (var i = 0; i < imgArray.length; i++) {
                            if (imgArray[i].file !== undefined) {
                                if (imgArray[i].file.name === f.name)
                                    isExist = true;
                            }
                        }

                        if (!isExist) {
                            imgArray.push(item);
                        }
                    }
                }
            });

            if (isCallAjax) {
                var formData = new FormData();
                for (var i = 0; i < imgArray.length; i++) {
                    if (imgArray[i].isUpload === false) {
                        formData.append('files', imgArray[i].file);
                        imgArray[i].isUpload = true;
                    }
                }
                $.ajax({
                    type: "POST",
                    url: '/Profile/UploadProductImg',
                    contentType: false,
                    processData: false,
                    data: formData,
                    beforeSend: function (e) {
                        $(".preloader").show();
                        $("#error-maxLen").hide();
                    },
                    success: function (res) {
                        if (res.code == -1) {
                            $("#error-maxLen").text(res.errormessage);
                            $("#error-maxLen").show();
                        }
                        else if (res.message !== "") {
                            if (productImg.length > 0) {
                                productImg = productImg + "|" + res.message;
                            } else {
                                productImg = res.message;
                            }

                            if (res.data !== null & res.data.length > 0) {
                                for (var i = 0; i < res.data.length; i++) {
                                    for (var j = 0; j < imgArray.length; j++) {
                                        if (res.data[i].name === imgArray[j].file.name) {
                                            imgArray[j].path = res.data[i].path;
                                        }
                                    }
                                    var html = "<div class='uploadmulti__img-box'><div style='background-image: url(" + res.data[i].path + ")' data-number='" + $(".uploadmulti__img-close").length + "' data-file='" + res.data[i].name + "' class='img-bg'><div class='uploadmulti__img-close'><i class='icon-clearfile'></i></div></div></div>";
                                    imgWrap.prepend(html);
                                }
                            }

                            // Ẩn nút upload ảnh sản phẩm chào mua (vì limit 1 ảnh)
                            $(".btn-upload-buy").hide();

                            console.log(productImg);
                        }
                        HidePreloader();
                    },
                    error: function (jqXHR, exception) {
                        //var msg = '';
                        //if (jqXHR.status === 0) {
                        //    msg = 'Not connect.\n Verify Network.';
                        //} else if (jqXHR.status == 404) {
                        //    msg = 'Requested page not found. [404]';
                        //} else if (jqXHR.status == 500) {
                        //    msg = 'Internal Server Error [500].';
                        //} else if (exception === 'parsererror') {
                        //    msg = 'Requested JSON parse failed.';
                        //} else if (exception === 'timeout') {
                        //    msg = 'Time out error.';
                        //} else if (exception === 'abort') {
                        //    msg = 'Ajax request aborted.';
                        //} else {
                        //    msg = 'Uncaught Error.\n' + jqXHR.responseText;
                        //}
                        //$('#error-maxLen').text(msg);
                        //$('#error-maxLen').show();
                        HidePreloader();
                        alert(defaultErrorMessage);
                        location.reload();
                    }
                });
            }


        });
    });
    
    $('body').on('click', ".uploadmulti__img-close", function (e) {

        // Hiện nút upload ảnh sản phẩm chào mua (vì limit 1 ảnh)
        $(".btn-upload-buy").show();

        if (query.Productid <= 0) {
            var file = $(this).parent().data("file");
            if (imgArray.length > 1) {
                for (var i = 0; i < imgArray.length; i++) {
                    if (imgArray[i].file.name === file) {
                        var pathArr = productImg.split('|');
                        for (var j = 0; j < pathArr.length; j++) {
                            if (pathArr[j] === imgArray[i].path) {
                                pathArr.splice(j, 1);
                            }
                        }
                        if (pathArr.length > 1) {
                            productImg = pathArr.join('|');
                        } else {
                            productImg = pathArr[0];
                        }

                        imgArray.splice(i, 1);
                        break;
                    }
                }
            } else {
                imgArray.splice(0, 1);
                productImg = "";
            }
            console.log(productImg);
            $('#Productimage').val('');
            $(this).parent().parent().remove();
        } else {

            var filePath = $(this).parent().data("path");

            if (filePath !== undefined) {
                var pathArr = productImg.split('|');
                for (var j = 0; j < pathArr.length; j++) {
                    if (pathArr[j] === filePath) {
                        pathArr.splice(j, 1);
                    }
                }
                if (pathArr.length > 1) {
                    productImg = pathArr.join('|');
                } else {
                    productImg = pathArr[0];
                }
                if (productImg === undefined)
                    productImg = "";
                console.log(productImg);
                $('#Productimage').val('');
                $(this).parent().parent().remove();
            } else {
                debugger 
                var file = $(this).parent().data("file");
                for (var i = 0; i < imgArray.length; i++) {
                    if (imgArray[i].file.name === file) {
                        var pathArr = productImg.split('|');
                        for (var j = 0; j < pathArr.length; j++) {
                            if (pathArr[j] === imgArray[i].path) {
                                pathArr.splice(j, 1);
                            }
                        }
                        if (pathArr.length > 1) {
                            productImg = pathArr.join('|');
                        } else {
                            productImg = pathArr[0];
                        }
                        if (productImg === undefined)
                            productImg = "";
                        imgArray.splice(i, 1);
                        break;
                    }
                }
                console.log(productImg);
                $('#Productimage').val('');
                $(this).parent().parent().remove();
            }
        }
    });
}

// Đặt lại label Filter
function handleFilterText() {

    var $date = $('#Date');
    if ($date.find("label").hasClass("always-freeze")) {
        $date.find("label").addClass("freeze");
    }

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

    var $province = $('#province .filter-main ul li.act');
    if ($province.length > 0) {
        var textArray = new Array();
        $province.each(function () {
            textArray.push($(this).text());
        });
        $("#province label").addClass("freeze");
        $("#province input:first").val(textArray.join(", "));
    } else {
        $("#province label").removeClass("freeze");
        $("#province input:first").val("");
    }

    var specialties = $('#specialties .filter-main ul li.act');
    if (specialties.length > 0) {
        var textArray = new Array();
        specialties.each(function () {
            textArray.push($(this).text());
        });
        $("#specialties label").addClass("freeze");
        $("#specialties input:first").val(textArray.join(", "));
    } else {
        $("#specialties label").removeClass("freeze");
        $("#specialties input:first").val("");
    }

    var $standard = $('#standard .filter-main ul li.act');
    if ($standard.length > 0) {
        var textArray = new Array();
        $standard.each(function () {
            textArray.push($(this).text());
        });
        $("#standard label").addClass("freeze");
        $("#standard input:first").val(textArray.join(", "));
    } else {
        $("#standard label").removeClass("freeze");
        $("#standard input:first").val("");
    }

    var $isbuy = $('#isbuy .filter-main ul li.act');
    if ($isbuy.length > 0) {
        var textArray = new Array();
        $isbuy.each(function () {
            textArray.push($(this).text());
        });
        $("#isbuy label").addClass("freeze");
        $("#isbuy input:first").val(textArray.join(", "));
    } else {
        $("#isbuy label").removeClass("freeze");
        $("#isbuy input:first").val("");
    }

    var $isfrequency = $('#isfrequency .filter-main ul li.act');
    if ($isfrequency.length > 0) {
        var textArray = new Array();
        $isfrequency.each(function () {
            textArray.push($(this).text());
        });
        $("#isfrequency label").addClass("freeze");
        $("#isfrequency input:first").val(textArray.join(", "));
    } else {
        $("#isfrequency label").removeClass("freeze");
        $("#isfrequency input:first").val("");
    }

    var $baseUnit = $('#baseUnit .filter-main ul li.act');
    if ($baseUnit.length > 0) {
        var textArray = new Array();
        $baseUnit.each(function () {
            textArray.push($(this).text());
        });
        $("#baseUnit label").addClass("freeze");
        $("#baseUnit input:first").val(textArray.join(", "));
    } else {
        $("#baseUnit label").removeClass("freeze");
        $("#baseUnit input:first").val("");
    }

    for (var i = 0; i < numUnit; i++) {
        var selector = '#unit' + i + ' .filter-main ul li.act';
        var label = '#unit' + i + ' label';
        var firstInput = '#unit' + i + ' input:first';
        var $unit = $(selector);
        if ($unit.length > 0) {
            var textArray = new Array();
            $unit.each(function () {
                textArray.push($(this).text());
            });
            $(label).addClass("freeze");
            $(firstInput).val(textArray.join(", "));
        }
    }

    if (flagValidateFilter) {
        if ($('#frmAddProductBuy').length > 0) $('#frmAddProductBuy').valid();
        if ($('#frmAddProductSell').length > 0) $('#frmAddProductSell').valid();
    }

}

// Lấy giá trị 
function collectParam(data) {

    var id = {
        name: "Productid",
        value: query.Productid,
    }
    data.push(id);

    var producttype = {
        name: "Producttype",
        value: query.AddType,
    }
    data.push(producttype);

    var $cate = $('#list-cate .filter-main ul li.act');
    if ($cate.length > 0) {
        var numberArray = new Array();
        $cate.each(function () {
            numberArray.push($(this).data('id'));
        });
        numberArray = numberArray.filter(unique);
        data.find(input => input.name == 'Categoryid').value = numberArray.join();
    } else {
        data.find(input => input.name == 'Categoryid').value = "";
    }

    var $province = $('#province .filter-main ul li.act');
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

    var $specialties = $('#specialties .filter-main ul li.act');
    if ($specialties.length > 0) {
        var numberArray = new Array();
        $specialties.each(function () {
            numberArray.push($(this).data('id'));
        });
        numberArray = numberArray.filter(unique);
        data.find(input => input.name == 'Specialtiesid').value = numberArray.join();
    } else {
        data.find(input => input.name == 'Specialtiesid').value = "";
    }

    var isShow = $(".radio #show").hasClass('act-check');
    var Isshow = {
        name: 'Isshow',
        value: isShow
    }
    data.push(Isshow);

    var Isshowprice = $(".radio #showPrice").hasClass('act-check');
    data.push({
        name: 'Isshowprice',
        value: Isshowprice
    });

    var img = {
        name: 'Productimage',
        value: productImg
    }
    data.push(img);

    data.push({ name: 'Description', value: tinyMCE.activeEditor.getContent() });

    var $standard = $('#standard .filter-main ul li.act');
    if ($standard.length > 0) {
        var numberArray = new Array();
        $standard.each(function () {
            numberArray.push($(this).data('id'));
        });
        numberArray = numberArray.filter(unique);
        data.find(input => input.name == 'Standardid').value = numberArray.join();
    } else {
        data.find(input => input.name == 'Standardid').value = "";
    }

    if (query.AddType == 2) {
        var listProductPrice = new Array();
        $('.price-main').each(function (i) {
            var from;
            var to;
            var price;
            var Unitprice;
            var selectorId;
            if (isMobile) {
                from = $(this).children('.itm').children('.first-row').children('.frames01').children('#from').val();
                to = $(this).children('.itm').children('.first-row').children('.frames02').children('#to').val();
                price = $(this).children('.itm').children('.second-row').children('#price').val();
                selectorId = $(this).children('.itm').children('.first-row').children('.frames03').children('.form-field')
                    .children('.filter__item').attr('id');
            } else {
                from = $(this).children('.itm').children('.frames01').children('#from').val();
                to = $(this).children('.itm').children('.frames02').children('#to').val();
                price = $(this).children('.itm').children('#price').val();
                Unitprice;
                selectorId = $(this).children('.itm').children('.frames03').children('.form-field')
                    .children('.filter__item').attr('id');
            }
            
            var selector = '#' + selectorId + ' .filter-main ul li.act';
            var $unit = $(selector);
            if ($unit.length > 0) {
                var numberArray = new Array();
                $unit.each(function() {
                    numberArray.push($(this).data('id'));
                });
                numberArray = numberArray.filter(unique);
                Unitprice = numberArray.join();
            } else {
                Unitprice = "0";
            }

            var $baseUnit = $('#baseUnit .filter-main ul li.act');
            var baseUnitValue = 0;
            if ($baseUnit.length > 0) {
                var numberArray = new Array();
                $baseUnit.each(function () {
                    numberArray.push($(this).data('id'));
                });
                numberArray = numberArray.filter(unique);
                baseUnitValue = numberArray.join();
            } else {
                baseUnitValue = 0;
            }

            var Productpriceitem = {
                Displayorder: i + 1,
                Fromquantily: parseInt(from),
                Toquantily: parseInt(to),
                FrequencyUnitId: parseInt(Unitprice),
                PriceSell: price,
                Unitprice: baseUnitValue,
            };
            listProductPrice.push(Productpriceitem);
        });
        var item = {
            name: 'Productpriceitem',
            value: listProductPrice
        }
        data.push(item);

    } else {

        var $baseUnit = $('#baseUnit .filter-main ul li.act');
        if ($baseUnit.length > 0) {
            var numberArray = new Array();
            $baseUnit.each(function () {
                numberArray.push($(this).data('id'));
            });
            numberArray = numberArray.filter(unique);
            data.find(input => input.name == 'Unitid').value = numberArray.join();
        } else {
            data.find(input => input.name == 'Unitid').value = "";
        }


        var $isbuy = $('#isbuy .filter-main ul li.act');
        if ($isbuy.length > 0) {
            var numberArray = new Array();
            $isbuy.each(function () {
                numberArray.push($(this).data('id'));
            });
            numberArray = numberArray.filter(unique);
            data.find(input => input.name == 'Isbuy').value = numberArray.join();
        } else {
            data.find(input => input.name == 'Isbuy').value = "";
        }

        var $isfrequency = $('#isfrequency .filter-main ul li.act');
        if ($isfrequency.length > 0) {
            var numberArray = new Array();
            $isfrequency.each(function () {
                numberArray.push($(this).data('id'));
            });
            numberArray = numberArray.filter(unique);
            data.find(input => input.name == 'Frequency').value = numberArray.join();
        } else {
            data.find(input => input.name == 'Frequency').value = "";
        }

        var quantity = {
            name: 'quantity',
            value: '1',
        }
        data.push(quantity);
    }

}

//#endregion