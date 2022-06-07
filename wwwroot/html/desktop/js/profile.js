$(document).ready(function() {
	$('.tab-click').click(function(){
		var tab_id = $(this).attr('data-tab');

		$('.tab-click').removeClass('current');
		$('.tab-content').removeClass('current');

		$(this).addClass('current');
		$("#"+tab_id).addClass('current');
	});
	$('.wait-connect').click(function(){
		$('.popup-connect').addClass('active');
	});
	$('.close-popup').click(function(){
		$('.popup-connect').removeClass('active');
	});
	$('.btn-connect a.refuse').click(function(){
		$('.popup-connect').removeClass('active');
	});
	$('.bg-connect').click(function(){
		$('.popup-connect').removeClass('active');
		$('.popup-delsp').removeClass('active');
	});
	$('.click-del').click(function(){
		$('.popup-delsp').addClass('active');
	});
	$('.btn-back').click(function(){
		$('.popup-delsp').removeClass('active');
	});
})