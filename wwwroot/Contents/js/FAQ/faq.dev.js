$(document).ready(function() {
    $('.question').on('click', function(event) {
        event.preventDefault();
        $(this).toggleClass('active');
        $(this).next('.faqlist .answer').slideToggle();
    });

    //hien thi bai xuat hien scroll
    var totalHeightD = 270;
    $('.faqlistD li:nth-child(-n+10)').each(function(){
        totalHeightD+=$(this).outerHeight();
    })
    $('.faqlistD.height-scroll').css("height", totalHeightD);

    var totalHeightM = 25;
    $('.faqlistM li:nth-child(-n+6)').each(function(){
        totalHeightM+=$(this).outerHeight();
    })
    $('.faqlistM.height-scroll').css("height", totalHeightM);

    //validate
    $(".form-faq").validate({
        errorElement: 'div',
        errorClass:'error',
        rules: {
            content: {
                required: true,
                minlength: 5,
                maxlength: 100
            },
            fullName: {
                required: true,
                minlength: 1,
                maxlength: 40
            },
            phoneNumber: {
                required: true,
                digits: true,
                minlength: 10,
                maxlength: 10
            },
            email: {
                required: true,
                email: true,
            },
        },
        messages: {
            content: {
                required: "Vui lòng nhập góp ý của bạn",
                minlength: "Nội dung quá ngắn, vui lòng nhập tối thiểu 5 ký tự",
                maxlength: "Nội dung quá dài, vui lòng nhập tối đa 100 ký tự"
            },
            fullName: {
                required: "Vui lòng nhập họ và tên của bạn",
                minlength: "Tên quá ngắn",
                maxlength: "Tên đề quá dài"
            },
            email: {
                required: "Vui lòng nhập email của bạn",
                email: "Vui lòng nhập email hợp lệ",
            },
            phoneNumber: {
                required: "Vui lòng nhập số điện thoại của bạn",
                minlength: "Vui lòng nhập số điện thoại gồm 10 chữ số",
                digits: "Số điện thoại chỉ bao gồm chữ số",
                maxlength: "Vui lòng nhập số điện thoại gồm 10 chữ số"
            }
        }
    });

    $(".form-faq").submit(function (e) {
        if ($('.form-faq').valid()) {
            e.preventDefault();
            SendMailContact('.form-faq')
        }
    });

    $(".close-send").click(function () {
        if ($(".popup-send .noti").text() == "Đã gửi câu hỏi")
            location.reload()

        $('.bgsend-overlay').fadeOut();
        $('.popup-send').fadeOut();
    });
    
    $(".bgsend-overlay").click(function() {
        $('.bgsend-overlay').fadeOut();
        $('.popup-send').fadeOut();
    });
})

function SendMailContact() {
    var queryData = GetAllFormData();
    $.ajax({
        url: '/FAQ/SendEmailFaq',
        type: 'POST',
        cache: false,
        data: queryData,
        success: function (res) {
            if (res != null && res.code == 200) {
                $('.form-faq').trigger('reset');
                var title = "Đã gửi câu hỏi";
                var message = "Câu hỏi của bạn sẽ được trả lời bởi quản trị viên. Khi câu hỏi của bạn được trả lời, chúng tôi sẽ gửi email thông báo đến bạn.";
                ShowPopupNotify(message, title);
            }
            else {
                var title = "Gửi câu hỏi thất bại";
                ShowPopupNotify(res.errormessage, title);
            }
        },
        error: function() {
            alert(defaultErrorMessage);
            location.reload();
        }
    });
}

function GetAllFormData() {
    data = $(".form-faq").serializeArray();
    var obj;
    if (isLogin)
        obj = isMobile ? '.form-button #g-recaptcha-response' : '.form-button #g-recaptcha-response-1';
    else
        obj = isMobile ? '.form-button #g-recaptcha-response-1' : '.form-button #g-recaptcha-response-2';
    data.push({ name: 'Captcha', value: $(obj).val() });
    return data;
}