$(document).ready(function () {
    //$(".sidebar ul li ul li").each(function () {        
    //    $(this).click(function () {
    //        $(this).parent().each(function () {//去掉非点中样式    
    //            alert(1)
    //            $('.sidebar ul li ul li').removeClass("active");
    //        });
    //        alert(2)
    //        $(this).addClass("active");//增加点中的样式  
    //    })
    //})

    $(".sidebar ul li ul li a").each(function () {
        if ($(this)[0].href == String(window.location)) {
            $(this).parents("li").parents("li").addClass("active open");
            $(this).parents("li").addClass("active");
        }
    });
});