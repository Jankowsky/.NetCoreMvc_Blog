// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

$(document).ready(function($) {
    $('#accordion').find('.accordion-toggle').click(function(){

        //Expand or collapse this panel
        $(this).next().slideToggle('fast');

        //Hide the other panels
        $(".accordion-content").not($(this).next()).slideUp('fast');

    });
});


$(document).on("click", ".addImage", function () {
    var myId = $(this).data('id');
    $(".modal-body #Id").val( myId );
    
});

$(document).on("click", ".addSubscriber", function () {
    var myId = $(this).data('id');
    $(".modal-body #Id").val( myId );

});


$(document).on("click", ".showPolicy", function () {
    var myId = $(this).data('id');
    $(".modal-body #Id").val( myId );

});



$(function() {
    $('#RulesAcceptance').click(function() {
        if ($(this).is(':checked')) {
            $('#submitNewsletter').removeAttr('disabled');
        } else {
            $('#submitNewsletter').attr('disabled', 'disabled');
        }
    });
});

$(function(){
    $("form[name='AddSubscriber']").validate({ 
        rules:{
            Name: "required",
            Email: {
                required: true,
                email: true
            }
        },

        messages: {
            Name: "Podaj imię",
            Email: "Podaj prawidłowy email"
        },

        submitHandler: function(form) {
            form.submit();
        }

    });
});

$(function(){
    $("form[name='DeleteSub']").validate({
        rules:{
           
            email: {
                required: true,
                email: true
            }
        },
        messages: {
            email: "Podaj prawidłowy email"
        },

        submitHandler: function(form) {
            form.submit();
        }

    });
});