

$.rfqDetail = {
    rootUrl: '/',
    load: function (url) {
        this.rootUrl = url;
    },
   

    GetValue: function (e) {
        var posturl = rootUrl + "Pricing/GetComputedValue";
        // alert(posturl);
       
        $.ajax({
            type: 'Get',
            url: posturl,
            data: {
                UnitType: $('#UnitType').val(),
                UnitPrice1: $('#UnitPrice1').val(),
                UnitPrice2: $('#UnitPrice2').val(),
                UnitPrice3: $('#UnitPrice3').val(),
                UnitPrice4: $('#UnitPrice4').val(),
                UnitPrice5: $('#UnitPrice5').val(),
                UnitPrice6: $('#UnitPrice6').val(),
                UnitPrice7: $('#UnitPrice7').val(),
                UnitPrice8: $('#UnitPrice8').val(),
                UnitPrice9: $('#UnitPrice9').val(),
                UnitPrice10: $('#UnitPrice10').val(),
                FixedCost: $('#FixedCost').val(),
                MaterialCost: $('#MaterialCost').val(),
                VariableCost: $('#VariableCost').val(),
                UnitPerWorkingPanel: $('#UnitPerWorkingPanel').val(),
                ArraySizeWidth: $('#ArraySizeWidth').val(),
                ArraySizeLength: $('#ArraySizeLength').val(),
                PanelSizeWidth: $('#PanelSizeWidth').val(),
                PanelSizeLength: $('#PanelSizeLength').val(),
                TargetPrice: $('#TargetPrice').val(),
                ExchangRatePerUSD: $('#ExchangRatePerUSD').val(),
                ShipTermsAdder: $('#ShipTermsAdder').val(),
                MinSqInch: $('#MinSqInch').val(),
                LayerCount: $('#LayerCount').val(),
                UnitPerArray: $('#UnitPerArray').val(),
                VolumePerMonth: $('#VolumePerMonth').val()

            },
            datatype: 'json',
            success: function (data) {
                $('#OP').val(data.OP);
                $('#MP').val(data.MP);
                $('#OP1').val(data.OP1);
                $('#MP1').val(data.MP1);
                $('#OP2').val(data.OP2);
                $('#MP2').val(data.MP2);
                $('#OP3').val(data.OP3);
                $('#MP3').val(data.MP3);
                $('#OP4').val(data.OP4);
                $('#MP4').val(data.MP4);
                $('#OP5').val(data.OP5);
                $('#MP5').val(data.MP5);
                $('#OP6').val(data.OP6);
                $('#MP6').val(data.MP6);
                $('#OP7').val(data.OP7);
                $('#MP7').val(data.MP7);
                $('#OP8').val(data.OP8);
                $('#MP8').val(data.MP8);
                $('#OP9').val(data.OP9);
                $('#MP9').val(data.MP9);
                $('#OP10').val(data.OP10);
                $('#MP10').val(data.MP10);
                $('#TargetASP').val(data.TargetASP);
                $('#MinASP').val(data.MinASP);
                $('#TargetASPL').val(data.TargetASPL);
                $('#MinASPL').val(data.MinASPL);
                $('#TargetSqIn').val(data.TargetSqIn);
                $('#MinSqInch').val(data.MinSqInch);
                $('#TargetCLsqin').val(data.TargetCLsqin);
                $('#MinCLsqin').val(data.MinCLsqin);
                $('#TargetVSActucal').val(data.TargetVSActucal);
                $('#TotalCost').val(data.TotalCost);
                $('#PanelUtilization').val(data.PanelUtilization);

                if ($("#RequestVVI").val() == "VVI") {
                  
                    $("#TechnicalQuoting").val("Wendy Xu");
                }
                var UnitPrice = [$('#UnitPrice1').val(), $('#UnitPrice2').val(), $('#UnitPrice3').val(), $('#UnitPrice4').val(), $('#UnitPrice5').val()];

                var filterarray = $.grep(UnitPrice, function (value) {
                    return value > 0;
                });

                Array.min = function (p) {
                    return Math.min.apply({}, p);
                }

                if (filterarray.length > 0) {

                    var minprice = Array.min(filterarray);

                    var eau = (minprice * $("#VolumePerMonth").val()) / 1000;
                    $("#EstQuoteSize").val(eau);

                }
                

            }
            , error: function (e) {
                //  alert('error:' + e);
            }
            , complete: function () {
                //  alert('completed');
            }
        });
    },

    ComputedEAU : function(){
             
        var UnitPrice = [$('#UnitPrice1').val(), $('#UnitPrice2').val(), $('#UnitPrice3').val(), $('#UnitPrice4').val(), $('#UnitPrice5').val()];

        var filterarray = $.grep(UnitPrice, function (value) {
            return value > 0;
        });

        Array.min = function (p) {
            return Math.min.apply({}, p);
        }

        if (filterarray.length > 0) {

            var minprice = Array.min(filterarray);

            var eau = (minprice * $("#VolumePerMonth").val()) / 1000;
            $("#EstQuoteSize").val(eau);

        }
        
    },
        
    Init: function () {
        $.fn.onlyNum = function () {
            $(this).keydown(function () {

                var e = $(this).event || window.event;
                var code = parseInt(e.keyCode);
                if ((code >= 48 && code <= 57 )||(code >= 96 && code <= 105)|| code == 8 || code == 13 || code == 46 || code == 110 || code == 190 || code == 9||((code==86) && (e.ctrlKey))) {

                    return true;
                } else {
                    return false;
                }
            })
        };


        if ($("#EstQuoteSize").val() <= 0) {

            var UnitPrice = [$('#UnitPrice1').val(), $('#UnitPrice2').val(), $('#UnitPrice3').val(), $('#UnitPrice4').val(), $('#UnitPrice5').val()];

            var filterarray = $.grep(UnitPrice, function (value) {
                return value > 0;
            });

            Array.min = function (p) {
                return Math.min.apply({}, p);
            }

            if (filterarray.length > 0) {

                var minprice = Array.min(filterarray);

                var eau = (minprice * $("#VolumePerMonth").val()) / 1000;
                $("#EstQuoteSize").val(eau);

            }
        }

       
       
        $(".NumberType1").onlyNum();


        $("#OEM").autocomplete({
            source: this.rootUrl + "Setting/GetAuotCompleteValue"
        });
        $("#ShipmentTerms").autocomplete({
            source: this.rootUrl + "Setting/GetAuotCompleteValueForShipmentTerms"
        });
        $("#Location").autocomplete({
            source: this.rootUrl + "Setting/GetAuotCompleteValueForLocation"
        });

        $('#FixedCost').on('change', this.GetValue);
        $('#TotalCost').on('change', this.GetValue);
        $('#VariableCost').on('change', this.GetValue);
        $('#MaterialCost').on('change', this.GetValue);
        $('#UnitPerWorkingPanel').on('change', this.GetValue);
        $('#ArraySizeWidth').on('change', this.GetValue);
        $('#ArraySizeLength').on('change', this.GetValue);
        $('#PanelSizeWidth').on('change', this.GetValue);
        $('#PanelSizeLength').on('change', this.GetValue);
        $('#TargetPrice').on('change', this.GetValue);
        $('#ExchangRatePerUSD').on('change', this.GetValue);
        $('#ShipTermsAdder').on('change', this.GetValue);
        $('#MinSqInch').on('change', this.GetValue);
        $('#LayerCount').on('change', this.GetValue);
        $('#UnitPerArray').on('change', this.GetValue);
        $('#UnitType').on('change', this.GetValue);

        $('#UnitPrice1').on('change', this.GetValue);
        $('#UnitPrice2').on('change', this.GetValue);
        $('#UnitPrice3').on('change', this.GetValue);
        $('#UnitPrice4').on('change', this.GetValue);
        $('#UnitPrice5').on('change', this.GetValue);
        $('#VolumePerMonth').on('change', this.GetValue);
        $('#RequestVVI').on('change', this.GetValue);
        $('#TechnicalQuoting').on('change',this.GetValue);

        $('#id-disable-check').on('click', function () {
            var inp = $('#form-input-readonly').get(0);
            if (inp.hasAttribute('disabled')) {
                inp.setAttribute('readonly', 'true');
                inp.removeAttribute('disabled');
                inp.value = "This text field is readonly!";
            }
            else {
                inp.setAttribute('disabled', 'disabled');
                inp.removeAttribute('readonly');
                inp.value = "This text field is disabled!";
            }
        });
        
        $(".chosen-select").chosen();
        $('#chosen-multiple-style').on('click', function (e) {
            var target = $(e.target).find('input[type=radio]');
            var which = parseInt(target.val());
            if (which == 2) $('#form-field-select-4').addClass('tag-input-style');
            else $('#form-field-select-4').removeClass('tag-input-style');
        });

        
       
        $('[data-rel=tooltip]').tooltip({ container: 'body' });
        $('[data-rel=popover]').popover({ container: 'body' });

        $('textarea[class*=autosize]').autosize({ append: "\n" });
        $('textarea.limited').inputlimiter({
            remText: '%n character%s remaining...',
            limitText: 'max allowed : %n.'
        });

        $.mask.definitions['~'] = '[+-]';
        $('.input-mask-date').mask('99/99/9999');
        $('.input-mask-phone').mask('(999) 999-9999');
        $('.input-mask-eyescript').mask('~9.99 ~9.99 999');
        $(".input-mask-product").mask("a*-999-a999", { placeholder: " ", completed: function () { alert("You typed the following: " + this.val()); } });



        $("#input-size-slider").css('width', '200px').slider({
            value: 1,
            range: "min",
            min: 1,
            max: 8,
            step: 1,
            slide: function (event, ui) {
                var sizing = ['', 'input-sm', 'input-lg', 'input-mini', 'input-small', 'input-medium', 'input-large', 'input-xlarge', 'input-xxlarge'];
                var val = parseInt(ui.value);
                $('#form-field-4').attr('class', sizing[val]).val('.' + sizing[val]);
            }
        });

        $("#input-span-slider").slider({
            value: 1,
            range: "min",
            min: 1,
            max: 12,
            step: 1,
            slide: function (event, ui) {
                var val = parseInt(ui.value);
                $('#form-field-5').attr('class', 'col-xs-' + val).val('.col-xs-' + val);
            }
        });


        $("#slider-range").css('height', '200px').slider({
            orientation: "vertical",
            range: true,
            min: 0,
            max: 100,
            values: [17, 67],
            slide: function (event, ui) {
                var val = ui.values[$(ui.handle).index() - 1] + "";

                if (!ui.handle.firstChild) {
                    $(ui.handle).append("<div class='tooltip right in' style='display:none;left:16px;top:-6px;'><div class='tooltip-arrow'></div><div class='tooltip-inner'></div></div>");
                }
                $(ui.handle.firstChild).show().children().eq(1).text(val);
            }
        }).find('a').on('blur', function () {
            $(this.firstChild).hide();
        });

        $("#slider-range-max").slider({
            range: "max",
            min: 1,
            max: 10,
            value: 2
        });

        $("#eq > span").css({ width: '90%', 'float': 'left', margin: '15px' }).each(function () {
            // read initial values from markup and remove that
            var value = parseInt($(this).text(), 10);
            $(this).empty().slider({
                value: value,
                range: "min",
                animate: true

            });
        });


        $('#id-input-file-1 , #id-input-file-2').ace_file_input({
            no_file: 'No File ...',
            btn_choose: 'Choose',
            btn_change: 'Change',
            droppable: false,
            onchange: null,
            thumbnail: false //| true | large
            //whitelist:'gif|png|jpg|jpeg'
            //blacklist:'exe|php'
            //onchange:''
            //
        });

        $('#id-input-file-3').ace_file_input({
            style: 'well',
            btn_choose: 'Drop files here or click to choose',
            btn_change: null,
            no_icon: 'icon-cloud-upload',
            droppable: true,
            thumbnail: 'small'//large | fit
            //,icon_remove:null//set null, to hide remove/reset button
            /**,before_change:function(files, dropped) {
                //Check an example below
                //or examples/file-upload.html
                return true;
            }*/
            /**,before_remove : function() {
                return true;
            }*/
            ,
            preview_error: function (filename, error_code) {
                //name of the file that failed
                //error_code values
                //1 = 'FILE_LOAD_FAILED',
                //2 = 'IMAGE_LOAD_FAILED',
                //3 = 'THUMBNAIL_FAILED'
                //alert(error_code);
            }

        }).on('change', function () {
            //console.log($(this).data('ace_input_files'));
            //console.log($(this).data('ace_input_method'));
        });


        //dynamically change allowed formats by changing before_change callback function
        $('#id-file-format').removeAttr('checked').on('change', function () {
            var before_change
            var btn_choose
            var no_icon
            if (this.checked) {
                btn_choose = "Drop images here or click to choose";
                no_icon = "icon-picture";
                before_change = function (files, dropped) {
                    var allowed_files = [];
                    for (var i = 0 ; i < files.length; i++) {
                        var file = files[i];
                        if (typeof file === "string") {
                            //IE8 and browsers that don't support File Object
                            if (!(/\.(jpe?g|png|gif|bmp)$/i).test(file)) return false;
                        }
                        else {
                            var type = $.trim(file.type);
                            if ((type.length > 0 && !(/^image\/(jpe?g|png|gif|bmp)$/i).test(type))
                                    || (type.length == 0 && !(/\.(jpe?g|png|gif|bmp)$/i).test(file.name))//for android's default browser which gives an empty string for file.type
                                ) continue;//not an image so don't keep this file
                        }

                        allowed_files.push(file);
                    }
                    if (allowed_files.length == 0) return false;

                    return allowed_files;
                }
            }
            else {
                btn_choose = "Drop files here or click to choose";
                no_icon = "icon-cloud-upload";
                before_change = function (files, dropped) {
                    return files;
                }
            }
            var file_input = $('#id-input-file-3');
            file_input.ace_file_input('update_settings', { 'before_change': before_change, 'btn_choose': btn_choose, 'no_icon': no_icon })
            file_input.ace_file_input('reset_input');
        });




        $('#spinner1').ace_spinner({ value: 0, min: 0, max: 200, step: 10, btn_up_class: 'btn-info', btn_down_class: 'btn-info' })
        .on('change', function () {
            //alert(this.value)
        });
        $('#spinner2').ace_spinner({ value: 0, min: 0, max: 10000, step: 100, touch_spinner: true, icon_up: 'icon-caret-up', icon_down: 'icon-caret-down' });
        $('#spinner3').ace_spinner({ value: 0, min: -100, max: 100, step: 10, on_sides: true, icon_up: 'icon-plus smaller-75', icon_down: 'icon-minus smaller-75', btn_up_class: 'btn-success', btn_down_class: 'btn-danger' });



        $('.date-picker').datepicker({ autoclose: true }).next().on(ace.click_event, function () {
            $(this).prev().focus();
        });
        $('input[name=date-range-picker]').daterangepicker().prev().on(ace.click_event, function () {
            $(this).next().focus();
        });

        //$('#DueDate').timepicker({
        //    minuteStep: 1,
        //    showSeconds: true,
        //    showMeridian: false
        //}).next().on(ace.click_event, function () {
        //    $(this).prev().focus();
        //});

        $('#colorpicker1').colorpicker();
        $('#simple-colorpicker-1').ace_colorpicker();


        $(".knob").knob();


        //we could just set the data-provide="tag" of the element inside HTML, but IE8 fails!
        var tag_input = $('#form-field-tags');
        if (!(/msie\s*(8|7|6)/.test(navigator.userAgent.toLowerCase()))) {
            tag_input.tag(
              {
                  placeholder: tag_input.attr('placeholder'),
                  //enable typeahead by specifying the source array
                  source: ace.variable_US_STATES,//defined in ace.js >> ace.enable_search_ahead
              }
            );
        }
        else {
            //display a textarea for old IE, because it doesn't support this plugin or another one I tried!
            tag_input.after('<textarea id="' + tag_input.attr('id') + '" name="' + tag_input.attr('name') + '" rows="3">' + tag_input.val() + '</textarea>').remove();
            //$('#form-field-tags').autosize({append: "\n"});
        }




        /////////
        $('#modal-form input[type=file]').ace_file_input({
            style: 'well',
            btn_choose: 'Drop files here or click to choose',
            btn_change: null,
            no_icon: 'icon-cloud-upload',
            droppable: true,
            thumbnail: 'large'
        })

        //chosen plugin inside a modal will have a zero width because the select element is originally hidden
        //and its width cannot be determined.
        //so we set the width after modal is show
        $('#modal-form').on('shown.bs.modal', function () {
            $(this).find('.chosen-container').each(function () {
                $(this).find('a:first-child').css('width', '210px');
                $(this).find('.chosen-drop').css('width', '210px');
                $(this).find('.chosen-search input').css('width', '200px');
            });
        })
        /**
        //or you can activate the chosen plugin after modal is shown
        //this way select element becomes visible with dimensions and chosen works as expected
        $('#modal-form').on('shown', function () {
            $(this).find('.modal-chosen').chosen();
        })
        */







    }

}


