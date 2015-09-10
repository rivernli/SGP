$(document).ready(function () {

  


    $('#btnSave').click(function () {

        var url = '@Url.Content("~/Pricing/Detail")';
        
        $(":input").each(function (alldata) {
            var a = $(this).get(0).
            var b = $(this).val();
            alert(a);
            alert(b);
        });
        //$.ajax({
        //    type: 'Post',
        //    url: url,
        //    data: {},
        //    datatype: 'json',
        //    success: function (data) {
        //        $('#Number').val(data.Number);
        //        if (data.isPass == false) {
        //            $.bi.dialog.show({ title: 'Error', content: data.MessageString, width: 800 });
        //        } else {
        //            $.bi.dialog.show({ title: 'Save Success', content: 'Save Success!', width: 800 });
        //        }

        //    }
        //});
    });
});