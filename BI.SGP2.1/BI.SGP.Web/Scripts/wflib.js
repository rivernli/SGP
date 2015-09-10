$.bi.wf = {
    skip: function (options) {
        var actionPath = options.submitUrl;
        var title, content;
        var redoandretuanid;
        if (options.toId == 0) {
            title = "Submit";
            content = "Are you sure you want to submit?"
        } else {
            title = "Skip";
            content = "Are you sure you want to<br> change the status to [" + options.toText + "]?"
        }
        var checkData = true;
        if (typeof(options.checkDataCondition) == "function") {
            checkData = options.checkDataCondition();
            if (checkData != false) {
                checkData = true;
            }
        }

        if (options.toId == 0) {

            if (options.wfcurrid == '103') {
                if (typeof (options.checkSelectVendor) == "function") {

                    if (options.checkSelectVendor() == false) {

                        $.bi.dialog.show({
                            title: 'Error', content: 'Please select a vendor quotation', buttons: []
                        });
                        return;
                    }
                }
            }

        
        }
        else{
            if (options.toId == '104') {
                if (typeof (options.checkSelectVendor) == "function") {

                    if (options.checkSelectVendor() == false) {

                        $.bi.dialog.show({
                            title: 'Error', content: 'Please select a vendor quotation', buttons: []
                        });
                        return;
                    }
                }
                 
            }
        
        }

        var waitAllChildComplated = options.toText != "Cancelled";
       
        $.bi.dialog.show({
            title: title, content: content, buttons: [
            {
                html: "<i class='icon-ok bigger-110'></i>&nbsp; Yes",
                "class": "btn btn-success btn-xs",
                click: function () {
                    $(this).dialog("destroy");
                    var postData = { templateName: options.templateName, entityId: options.entityId, toId: options.toId, fromId: options.fromId, toChildIds: options.toChildIds, checkData: checkData, waitAllChildComplated: waitAllChildComplated };
                    if (options.getDataFunction) {
                        var formData = { postData: JSON.stringify(options.getDataFunction()) }
                        postData = $.extend(formData, postData)
                    }
                    $.bi.overlay.show();
                    $.post(actionPath, postData, function (data) {
                        $.bi.overlay.hide();
                        if (options.onComplete) {
                            options.onComplete(data);
                        }
                        var wizardSteps = $(".wizard-steps");
                        if (wizardSteps.length > 0) {
                            wizardSteps.each(function () {
                                $(this).biWizard("reload");
                            });
                        }
                    }, "json");
                }
            },
            {
                html: "<i class='icon-remove bigger-110'></i>&nbsp; No",
                "class": "btn btn-xs",
                click: function () {
                    $(this).dialog("destroy");
                }
            }]
        });
    },
    showFromChildActivities: function (target, activities, toId, toText, options) {
        var funThis = this;
        var iHtml = '<div style="width:400px">';
        for (var i = 0; i < activities.length; i++) {
            var btn = '<a href="javascript:void(0)" actid="' + activities[i].id + '" class="wf-showFromChild"><i class="icon-reply light-green bigger-130"></i></a>';
            iHtml += '<div class="timeline-item clearfix">';
            iHtml += '<div class="widget-box transparent" style="margin-left:5px">';
            iHtml += '<div class="widget-body">';
            iHtml += '<div class="widget-main">';
            iHtml += '<b>' + activities[i].name + '</b>';
            iHtml += '<div class="pull-right">' + btn + '</div>';
            iHtml += '</div>';
            iHtml += '</div>';
            iHtml += '</div>';
            iHtml += '</div>';
        }
        iHtml += '</div>';
        var title = 'From Sub Step';
        $.bi.dialog.show({
            id: 'skipFromChild', title: title, content: iHtml, buttons: [{
                html: "<i class='icon-remove bigger-110'></i>&nbsp; Cancel",
                "class": "btn btn-xs",
                click: function () {
                    $(this).dialog("destroy");
                }
            }],
            create: function () {
                var dlg = this;
                $(".wf-showFromChild").each(function (index, element) {
                    var thisId = $(this).attr("actid");
                    $(this).children().unbind('click').click(function () {
                        $(dlg).dialog("destroy");
                        var opts = $.extend(options, { toId: toId, fromId: thisId, toChildIds: "", toText: toText, target: target });
                        funThis.skip(opts);
                    });
                });
            }
        });
    },
    showToChildActivities: function (target, activities, toId, toText, options) {
        var funThis = this;
        var iHtml = '<div style="width:400px">';
        for (var i = 0; i < activities.length; i++) {
            var btn = '<input type="checkbox" value="' + activities[i].id + '" class="wf-showFromChild">';
            iHtml += '<div class="timeline-item clearfix">';
            iHtml += '<div class="widget-box transparent" style="margin-left:5px">';
            iHtml += '<div class="widget-body">';
            iHtml += '<div class="widget-main">';
            iHtml += '<b>' + activities[i].name + '</b>';
            iHtml += '<div class="pull-right">' + btn + '</div>';
            iHtml += '</div>';
            iHtml += '</div>';
            iHtml += '</div>';
            iHtml += '</div>';
        }
        iHtml += '</div>';
        var title = 'To Sub Step';
        $.bi.dialog.show({
            id: 'skipToChild', title: title, content: iHtml, buttons: [{
                html: "<i class='icon-ok bigger-110'></i>&nbsp; Submit",
                "class": "btn btn-success btn-xs",
                click: function () {
                    var toChildIds = "";
                    $(".wf-showFromChild:checked").each(function () {
                        var toId = $(this).val();
                        if (toChildIds != "") toChildIds += ",";
                        toChildIds += toId;
                    });
                    if (toChildIds != "") {
                        $(this).dialog("destroy");
                        var opts = $.extend(options, { toId: toId, fromId: 0, toChildIds: toChildIds, toText: toText, target: target });

                        funThis.skip(opts);
                    }
                }
            },
            {
                html: "<i class='icon-remove bigger-110'></i>&nbsp; Cancel",
                "class": "btn btn-xs",
                click: function () {
                    $(this).dialog("destroy");
                }
            }]
        });
    }
};
(function ($) {
    function createWizard(target) {
        var opts = $.data(target, 'biWizard').options;
        var actionPath = opts.getWizardDataUrl || $.bi.getRootUrl() + 'Workflow/GetWizardData';
        $(target).removeClass("wizard-steps").addClass("wizard-steps");

        $.post(actionPath, { templateName: opts.templateName, entityId: opts.entityId }, function (data) {
            if (data != null) {
                render(target, opts, data);
            }

            if (opts.onCreateComplete != null) {
                opts.onCreateComplete();
            }
        }, "json");
    }

    function render(target, opts, data) {
        var iHtml = "";
        if (data.activites != null && data.activites.length > 0) {
            for (var i = 0; i < data.activites.length; i++) {
                var curId = 0;
                var stepclass = "";
                if (data.currentActivity != null) {
                    curId = data.currentActivity.id;
                    if (data.currentActivity.sort >= data.activites[i].sort) {
                        if (data.activites[i].activityType == "Finish") {
                            stepclass = 'class="complete"';
                        } else {
                            stepclass = 'class="active"';
                        }
                    }
                }
                iHtml += '<li curId="' + curId + '" actId="' + data.activites[i].id + '" data-target="#step' + (i + 1) + '" ' + stepclass + ' title="' + data.activites[i].name + ':' + data.activites[i].activityDeac + '"><span class="step">' + (i + 1) + '</span><span class="title">' + data.activites[i].name + '</span></li>';
            }
        }

        $(target).html(iHtml);

        if (!opts.readonly && (data.currentActivity == null || data.currentActivity.type != "Finish")) {
            regActAction(target, opts, data);
        }
    }

    function regActAction(target, opts, data) {
        $(target).children().each(function (index, element) {
            var curId = $(this).attr("curId");
            var thisId = $(this).attr("actid");
            var thisText = $(this).children()[1].innerText;
            if (curId != thisId) {
                $(this).children().css("cursor", "pointer");
                var actionPath = $.bi.getRootUrl() + 'Workflow/GetCurrentAndToActivity';
                $(this).children().unbind('click').click(function () {
                    $.bi.overlay.show();
                    $.post(actionPath, { templateName: opts.templateName, entityId: opts.entityId, toId: thisId }, function (data) {
                        $.bi.overlay.hide();
                        if (data.currentActivity.child != null && data.currentActivity.child.length > 0) {
                            if (data.currentActivity.child.length == 1) {
                                var curChildId = data.currentActivity.child[0].id;
                                if (curChildId == -999) {
                                    $.bi.dialog.show({ title: 'Message', content: data.currentActivity.child[0].desc, buttons: [] });
                                } else {
                                    skip(target, thisId, curChildId, "", thisText);
                                }
                            } else {
                                $.bi.wf.showFromChildActivities(target, data.currentActivity.child, thisId, thisText, opts);
                            }
                        } else if (data.toActivity.child != null && data.toActivity.child.length > 0 && (data.currentActivity.sort == null || data.toActivity.sort < data.currentActivity.sort)) {
                            if (data.toActivity.child.length == 1) {
                                skip(target, thisId, 0, data.toActivity.child[0].id, thisText);
                            } else {
                                $.bi.wf.showToChildActivities(target, data.toActivity.child, data.toActivity.id, thisText, opts);
                            }
                        } else {
                            skip(target, thisId, 0, "", thisText);
                        }
                    }, "json");
                });
            }
        });
    }

    function skip(target, toId, fromId, toChildIds, toText) {
        var opts = $.extend($.data(target, 'biWizard').options, { toId: toId, fromId: fromId, toChildIds: toChildIds, toText: toText, target: target })
        $.bi.wf.skip(opts);
    }

    function reload(target, param) {
        createWizard(target);
    }

    $.fn.biWizard = function (options, param) {
        if (typeof options == 'string') {
            var method = $.fn.biWizard.methods[options];
            if (method) {
                return method(this, param);
            }
        }

        options = options || {};
        return this.each(function () {
            var state = $.data(this, 'biWizard');
            if (state) {
                $.extend(state.options, options);
            } else {
                $.data(this, 'biWizard', {
                    options: $.extend({}, $.fn.biWizard.defaults, options)
                });
            }
            createWizard(this);
        });
    };

    $.fn.biWizard.methods = {
        reload: function (jq, param) {
            return jq.each(function () {
                reload(this, param);
            });
        }
    }

    $.fn.biWizard.defaults = {
        templateName: '',
        readonly: false,
        submitUrl: '',
        getWizardDataUrl: null,
        entityId: 0,
        getDataFunction: null,
        onComplete: null,
        checkDataCondition: null,
        onCreateComplete: null
    };
})(jQuery);

//(function ($) {
//    function createWizard(target) {
//        var opts = $.data(target, 'biWizard').options;
//        var actionPath = $.bi.getRootUrl() + 'Workflow/GetWizardData';
//        $(target).removeClass("wizard-steps").addClass("wizard-steps");

//        $.post(actionPath, { templateName: opts.templateName, entityId: opts.entityId }, function (data) {
//            if (data != null) {
//                render(target, opts, data);

//                $(target).resize(function (e) {
//                    render(target, opts, data);
//                });
//            }
//        }, "json");
//    }

//    function render(target, opts, data) {
//        if (data.activites != null && data.activites.length > 0) {
//            var cXAxis = opts.cXAxis;
//            var cYAxis = opts.cYAxis;
//            var qr = opts.qr;
//            var cHCount = opts.cHCount;
//            var cRadius = opts.cRadius;
//            var nHeight = opts.nHeight;
//            var nWidth = opts.nWidth;
//            var cstrokeWidth = opts.cstrokeWidth;
//            var pstrokeWidth = opts.pstrokeWidth;
//            var textFixed = opts.textFixed;
//            var ctextFixed = opts.ctextFixed;
//            var ctextStyle = opts.ctextStyle;
//            var textStyle = opts.textStyle;
//            var cCount = 1;
//            var _cx, _cy, _pm1, _pm2, _ph1, _ph2, _pv;

//            for (var i = 0; i < data.activites.length; i++) {
//                if (data.activites[i].child.length > cCount) {
//                    cCount = data.activites[i].child.length;
//                }
//            }

//            if (data.activites.length > 1) {
//                nWidth = ($(target).width() - (cXAxis * 2) - cRadius) / (data.activites.length - 1);
//            }

//            var canvasHeigth = nHeight * cCount + 20;

//            var qrWidth = (nWidth - ((qr + cRadius) * 2)) / 2;
//            var _ccy = cYAxis + ((nHeight * (cCount - 1)) / 2 + cRadius);

//            var _circleHtml = '';
//            var _circleTextHtml = '';
//            var _pathHtml = '';
//            var _textHtml = '';
//            for (var i = 0; i < data.activites.length; i++) {

//                var curId = 0;
//                var _stroke = "#ced1d6";
//                var _pathStroke = "#ced1d6";
//                var _textStroke = "#ced1d6";
//                if (opts.readonly) {
//                    _stroke = "#5293c4";
//                    _pathStroke = "#5293c4"
//                    _textStroke = "#000000";
//                }
//                else if (data.currentActivity != null && !opts.readonly) {
//                    curId = data.currentActivity.id;
//                    if (data.currentActivity.sort > data.activites[i].sort || data.currentActivity.type == "Finish") {
//                        _stroke = "#82af6f";
//                        _pathStroke = "#82af6f";
//                        _textStroke = "#000000";
//                    } else if (data.currentActivity.sort == data.activites[i].sort) {
//                        _stroke = "#5293c4";
//                        _pathStroke = "#ced1d6"
//                        _textStroke = "#d15b47";
//                    }
//                }

//                _cx = (cXAxis + nWidth * i)
//                if (data.activites[i].child.length == 0) {
//                    _circleHtml += '<circle cx="' + _cx + '" cy="' + _ccy + '" r="' + cRadius + '" stroke="' + _stroke + '" fill="transparent" stroke-width="' + cstrokeWidth + '" curId="' + curId + '" actId="' + data.activites[i].id + '" actText="' + data.activites[i].name + '" class="wf-step-node" />';
//                    _circleTextHtml += '<text x="' + _cx + '" y="' + (_ccy + (cRadius * 2) - cRadius - ctextFixed) + '" fill="' + _textStroke + '" style="' + ctextStyle + '" curId="' + curId + '" actId="' + data.activites[i].id + '" actText="' + data.activites[i].name + '" class="wf-step-node">' + (i + 1) + '</text>';
//                    _textHtml += '<text x="' + _cx + '" y="' + (_ccy + (cRadius * 2) - textFixed) + '" fill="' + _textStroke + '" style="' + textStyle + '">' + data.activites[i].name + '</text>';
//                } else {
//                    for (var j = 0; j < data.activites[i].child.length; j++) {
//                        var _childStroke;
//                        var _childTextStroke = _textStroke;
//                        if (data.currentActivity != null && data.currentActivity.sort == data.activites[i].sort) {
//                            _childStroke = "#82af6f";
//                            _childTextStroke = "#000000";
//                        } else {
//                            _childStroke = _stroke;
//                        }
                        
//                        if (data.currentActivity != null && data.currentActivity.id == data.activites[i].id) {
//                            for (var k = 0; k < data.currentActivity.child.length; k++) {
//                                if (data.activites[i].child[j].id == data.currentActivity.child[k].id) {
//                                    _childStroke = "#5293c4";
//                                    _childTextStroke = "#d15b47";
//                                }
//                            }
//                        }

//                        _cy = cYAxis + cRadius + (nHeight * j);
//                        _circleHtml += '<circle cx="' + _cx + '" cy="' + _cy + '" r="' + cRadius + '" stroke="' + _childStroke + '" fill="transparent" stroke-width="' + cstrokeWidth + '" curId="' + curId + '" actId="' + data.activites[i].id + '" actText="' + data.activites[i].name + '" class="wf-step-node" />';
//                        _circleTextHtml += '<text x="' + _cx + '" y="' + (_cy + (cRadius * 2) - cRadius - 20) + '" fill="' + _childTextStroke + '" style="' + ctextStyle + '" curId="' + curId + '" actId="' + data.activites[i].id + '" actText="' + data.activites[i].name + '" class="wf-step-node">' + (i + 1) + '</text>';
//                        _textHtml += '<text x="' + _cx + '" y="' + (_cy + 10) + '" fill="' + _childTextStroke + '" style="' + textStyle + '" curId="' + curId + '" actId="' + data.activites[i].id + '" actText="' + data.activites[i].name + '" class="wf-step-node">' + data.activites[i].child[j].name + '</text>';
//                    }
//                    _textHtml += '<text x="' + _cx + '" y="' + (_cy + (cRadius * 2) - textFixed) + '" fill="' + _textStroke + '" style="' + textStyle + '">' + data.activites[i].name + '</text>';
//                }

//                if ((i + 1) < data.activites.length) {
//                    _pm1 = (cXAxis + nWidth * i) + cRadius;
//                    if (data.activites[i].child.length < 2) {
//                        _pm2 = _ccy;
//                        _ph1 = (_pm1 + nWidth) / 2;
//                        _ph2 = _pm1 + nWidth - (cRadius * 2);
//                        if ((i + 1) < data.activites.length && data.activites[i + 1].child.length > 0) {
//                            for (var j = 0; j < data.activites[i + 1].child.length; j++) {
//                                _pv = cYAxis + cRadius + (nHeight * j);
//                                _pathHtml += '<path d="M' + _pm1 + ',' + _pm2 + ' H' + _ph1 + ' V' + _pv + ' H' + _ph2 + '" fill="none" stroke="' + _pathStroke + '" stroke-width="' + pstrokeWidth + '"></path>';
//                            }
//                        } else {
//                            _pathHtml += '<path d="M' + _pm1 + ',' + _pm2 + ' H' + _ph2 + '" fill="none" stroke="' + _pathStroke + '" stroke-width="' + pstrokeWidth + '"></path>';
//                        }
//                    } else if (data.activites[i].child.length > 0) {
//                        for (var j = 0; j < data.activites[i].child.length; j++) {
//                            _pm2 = cYAxis + cRadius + (nHeight * j);
//                            _pv = _ccy;
//                            _ph1 = _pm1 + (nWidth / 2) - cRadius;
//                            _ph2 = _pm1 + nWidth - (cRadius * 2);
//                            _pathHtml += '<path d="M' + _pm1 + ',' + _pm2 + ' H' + _ph1 + ' V' + _pv + ' H' + _ph2 + '" fill="none" stroke="' + _pathStroke + '" stroke-width="' + pstrokeWidth + '"></path>';
//                        }
//                    }
//                }
//            }

//            var iHtml = '<svg height="' + canvasHeigth + '" width="100%" id="wf-svg">' + _pathHtml + _circleHtml + _circleTextHtml + _textHtml + '</svg>';
//            $(target).html(iHtml);
//        }

//        if (!opts.readonly && (data.currentActivity == null || data.currentActivity.type != "Finish")) {
//            regActAction(target, opts, data);
//        }
//    }

//    function regActAction(target, opts, data) {
//        $(target).find(".wf-step-node").each(function (index, element) {
//            var curId = $(this).attr("curId");
//            var thisId = $(this).attr("actid");
//            var thisText = $(this).attr("actText");
//            if (curId != thisId) {
//                $(this).css("cursor", "pointer");
//                var actionPath = $.bi.getRootUrl() + 'Workflow/GetCurrentAndToActivity';
//                $(this).unbind('click').click(function () {
//                    $.bi.overlay.show();
//                    $.post(actionPath, { templateName: opts.templateName, entityId: opts.entityId, toId: thisId }, function (data) {
//                        $.bi.overlay.hide();
//                        if (data.currentActivity.child != null && data.currentActivity.child.length > 0) {
//                            if (data.currentActivity.child.length == 1) {
//                                var curChildId = data.currentActivity.child[0].id;
//                                if (curChildId == -999) {
//                                    $.bi.dialog.show({ title: 'Message', content: data.currentActivity.child[0].desc, buttons: [] });
//                                } else {
//                                    skip(target, thisId, curChildId, "", thisText);
//                                }
//                            } else {
//                                $.bi.wf.showFromChildActivities(target, data.currentActivity.child, thisId, thisText, opts);
//                            }
//                        } else if (data.toActivity.child != null && data.toActivity.child.length > 0 && (data.currentActivity.sort == null || data.toActivity.sort < data.currentActivity.sort)) {
//                            if (data.toActivity.child.length == 1) {
//                                skip(target, thisId, 0, data.toActivity.child[0].id, thisText);
//                            } else {
//                                $.bi.wf.showToChildActivities(target, data.toActivity.child, data.toActivity.id, thisText, opts);
//                            }
//                        } else {
//                            skip(target, thisId, 0, "", thisText);
//                        }
//                    }, "json");
//                });
//            }
//        });
//    }

//    function skip(target, toId, fromId, toChildIds, toText) {
//        var opts = $.extend($.data(target, 'biWizard').options, { toId: toId, fromId: fromId, toChildIds: toChildIds, toText: toText, target: target })
//        $.bi.wf.skip(opts);
//    }

//    function reload(target, param) {
//        createWizard(target);
//    }

//    $.fn.biWizard = function (options, param) {
//        if (typeof options == 'string') {
//            var method = $.fn.biWizard.methods[options];
//            if (method) {
//                return method(this, param);
//            }
//        }

//        options = options || {};
//        return this.each(function () {
//            var state = $.data(this, 'biWizard');
//            if (state) {
//                $.extend(state.options, options);
//            } else {
//                $.data(this, 'biWizard', {
//                    options: $.extend({}, $.fn.biWizard.defaults, options)
//                });
//            }

//            createWizard(this);
//        });
//    };

//    $.fn.biWizard.methods = {
//        reload: function (jq, param) {
//            return jq.each(function () {
//                reload(this, param);
//            });
//        }
//    }

//    $.fn.biWizard.defaults = {
//        templateName: '',
//        readonly: false,
//        submitUrl: '',
//        entityId: 0,
//        getDataFunction: null,
//        onComplete: null,
//        checkDataCondition: null,
//        cXAxis: 30,
//        cYAxis: 10,
//        qr: 10,
//        cHCount: 3,
//        cRadius: 18,
//        nHeight: 45,
//        nWidth: 150,
//        cstrokeWidth: 5,
//        pstrokeWidth: 4,
//        cCount: 3,
//        textFixed: 5,
//        ctextFixed: 13,
//        ctextStyle: "text-anchor: middle; font-size: 16px; font-family: monospace;",
//        textStyle: "text-anchor: middle; font-size: 12px; font-family: monospace;"
//    };
//})(jQuery);


(function ($) {

    function createSkip(target) {
        var opts = $.data(target, 'biSkip').options;
        var actionPath = $.bi.getRootUrl() + 'Workflow/GetWizardData';
        $(target).unbind('click').click(function () {
            $.post(actionPath, { templateName: opts.templateName, entityId: opts.entityId }, function (data) {
                if (data != null) {
                    render(target, opts, data);
                }
            }, "json");
        });
    }

    function render(target, opts, data) {
        if (data.activites != null && data.activites.length > 0) {
            var iHtml = '<div class="timeline-container" style="width:500px"><div class="timeline-items">';
            for (var i = 0; i < data.activites.length; i++) {
                var curId, btn, icoClass;
                if (data.currentActivity != null) {
                    curId = data.currentActivity.id;
                } else {
                    curId = 0;
                }
                if (curId > 0 && data.currentActivity.type == "Finish") {
                    btn = "";
                    icoClass = "btn-success";
                } else if (curId > 0 && curId == data.activites[i].id) {
                    btn = "";
                    icoClass = "btn-primary";
                }
                else {
                    btn = '<a href="javascript:void(0)" actid="' + data.activites[i].id + '" actname="' + data.activites[i].name + '" class="wf-showSkipStep"><i class="icon-reply light-green bigger-130"></i></a>';
                    icoClass = "";
                }

                iHtml += '<div class="timeline-item clearfix"><div class="timeline-info"><i class="timeline-indicator btn ' + icoClass + ' no-hover">' + data.activites[i].sort + '</i></div>';
                iHtml += '<div class="widget-box transparent">';
                iHtml += '<div class="widget-body">';
                iHtml += '<div class="widget-main">';
                iHtml += '<b>' + data.activites[i].name + '</b>';
                iHtml += '<div class="pull-right">' + btn + '</div>';
                iHtml += '</div>';
                iHtml += '</div>';
                iHtml += '</div>';
                iHtml += '</div>';
            }
            iHtml += '</div></div>';
            var title = 'Workflow Skip';
            $.bi.dialog.show({
                id: 'wf-main-skip', title: title, content: iHtml, buttons: [{
                    html: "<i class='icon-remove bigger-110'></i>&nbsp; Cancel",
                    "class": "btn btn-xs",
                    click: function () {
                        $(this).dialog("destroy");
                    }
                }]
            });

            regActAction(target, opts, data);
        }
    }

    function closeMainDialog() {
        $.bi.dialog.close('wf-main-skip');
    }

    function regActAction(target, opts, data) {
        var actionPath = $.bi.getRootUrl() + 'Workflow/GetCurrentAndToActivity';
        $(".wf-showSkipStep").each(function (index, element) {
            var thisId = $(this).attr("actid");
            var thisText = $(this).attr("actname");
            $(this).children().unbind('click').click(function () {
                closeMainDialog();
                $.bi.overlay.show();
                $.post(actionPath, { templateName: opts.templateName, entityId: opts.entityId, toId: thisId }, function (data) {
                    $.bi.overlay.hide();
                    if (data.currentActivity.child != null && data.currentActivity.child.length > 0) {
                        if (data.currentActivity.child.length == 1) {
                            var curChildId = data.currentActivity.child[0].id;
                            if (curChildId == -999) {
                                $.bi.dialog.show({ title: 'Message', content: data.currentActivity.child[0].desc, buttons: [] });
                            } else {
                                skip(target, thisId, curChildId, "", thisText);
                            }
                        } else {
                            $.bi.wf.showFromChildActivities(target, data.currentActivity.child, thisId, thisText, opts);
                        }
                    } else if (data.toActivity.child != null && data.toActivity.child.length > 0 && (data.currentActivity.sort == null || data.toActivity.sort < data.currentActivity.sort)) {
                        if (data.toActivity.child.length == 1) {
                            skip(target, thisId, 0, data.toActivity.child[0].id, thisText);
                        } else {
                            $.bi.wf.showToChildActivities(target, data.toActivity.child, data.toActivity.id, thisText, opts);
                        }
                    } else {
                        skip(target, thisId, 0, "", thisText);
                    }
                }, "json");
            });
        });
    }

    function skip(target, toId, fromId, toChildIds, toText) {;
        var opts = $.extend($.data(target, 'biSkip').options, { toId: toId, fromId: fromId, toChildIds: toChildIds, toText: toText, target: target });
        $.bi.wf.skip(opts);
    }

    function submit(target, param) {
        $.bi.overlay.show();
        var actionPath = $.bi.getRootUrl() + 'Workflow/GetCurrentAndToActivity';
        var opts = $.data(target, 'biSkip').options;
        $.post(actionPath, { templateName: opts.templateName, entityId: opts.entityId, toId: 0 }, function (data) {
            $.bi.overlay.hide();
            if (data.currentActivity.child != null && data.currentActivity.child.length > 0) {
                if (data.currentActivity.child.length == 1) {
                    var curChildId = data.currentActivity.child[0].id;
                    if (curChildId == -999) {
                        $.bi.dialog.show({ title: 'Message', content: data.currentActivity.child[0].desc, buttons: [] });
                    } else {
                        skip(target, 0, curChildId, "", "");
                    }
                } else {
                    $.bi.wf.showFromChildActivities(target, data.currentActivity.child, 0, '', opts);
                }
            } else {
                skip(target, 0, 0, "", "");
            }
        }, "json");
    }

    function cancel(target, param) {
        skipToActivity(target, { toActId: param, toActText: 'Cancelled' });
    }

    function skipToActivity(target, param) {
        var toActId = param.toActId;
        var toActText = param.toActText;
        if (toActId > 0) {
            $.bi.overlay.show();
            var actionPath = $.bi.getRootUrl() + 'Workflow/GetCurrentAndToActivity';
            var opts = $.data(target, 'biSkip').options;
            $.post(actionPath, { templateName: opts.templateName, entityId: opts.entityId, toId: 0 }, function (data) {
                $.bi.overlay.hide();
                if (data.currentActivity.child != null && data.currentActivity.child.length > 0) {
                    if (data.currentActivity.child.length == 1) {
                        var curChildId = data.currentActivity.child[0].id;
                        if (curChildId == -999) {
                            $.bi.dialog.show({ title: 'Message', content: data.currentActivity.child[0].desc, buttons: [] });
                        } else {
                            skip(target, toActId, curChildId, "", toActText);
                        }
                    } else {
                        $.bi.wf.showFromChildActivities(target, data.currentActivity.child, toActId, toActText, opts);
                    }
                } else {
                    skip(target, toActId, 0, "", toActText);
                }
            }, "json");
        }
    }

    $.fn.biSkip = function (options, param) {
        if (typeof options == 'string') {
            var method = $.fn.biSkip.methods[options];
            if (method) {
                return method(this, param);
            }
        }

        options = options || {};
        return this.each(function () {
            var state = $.data(this, 'biSkip');
            if (state) {
                $.extend(state.options, options);
            } else {
                $.data(this, 'biSkip', {
                    options: $.extend({}, $.fn.biSkip.defaults, options)
                });
            }
            createSkip(this);
        });
    };

    $.fn.biSkip.methods = {
        submit: function (jq, param) {
            return submit(jq[0], param);
        },
        skip: function (jq, param) {
            return skipToActivity(jq[0], param);
        },
        cancel: function (jq, param) {
            return cancel(jq[0], param);
        }
    }

    $.fn.biSkip.defaults = {
        templateName: '',
        submitUrl: '',
        entityId: 0,
        getDataFunction: null,
        onComplete: null,
        checkDataCondition: null
    };
})(jQuery);

$.bi.timeline = {
    show: function (templateId, entityId) {
        $.bi.overlay.show();
        var actionPath = $.bi.getRootUrl() + 'Workflow/GetTimelineData';
        $.post(actionPath, { templateId: templateId, entityId: entityId }, function (data) {
            $.bi.overlay.hide();
            if (data != null) {
                var timelineHtml = '<div class="timeline-container" style="width:530px;">';

                if (data.activites != null && data.activites.length > 0) {
                    var endIndex = data.activites.length - 1;
                    var isFinishClassIcon = "";
                    var isFinishClassHeader = "";
                    var finishBody = '';
                    if (data.activites[endIndex].activityType == "Finish") {
                        isFinishClassIcon = "btn-success";
                        isFinishClassHeader = " header-color-green";
                        finishBody = "";
                    } else {
                        isFinishClassIcon = "btn-primary";
                        isFinishClassHeader = " header-color-blue";
                        finishBody = '<div class="widget-body">'
                        if (data.currentActivity.subActivities != null && data.currentActivity.subActivities.length > 0) {
                            for (var i = 0; i < data.currentActivity.subActivities.length; i++) {
                                if (data.currentActivity.subActivities[i].subActivityComplated == 1) {
                                    finishBody += '<div class="widget-body">'
                                    finishBody += '<div class="widget-main">';
                                    finishBody += '<b>&nbsp;' + data.currentActivity.subActivities[i].activityName + '&nbsp;</b>';
                                    finishBody += '<div class="pull-right"><b>' + data.currentActivity.subActivities[i].actionUser + '</b> submit at ';
                                    finishBody += '<i class="icon-time bigger-110"></i>';
                                    finishBody += data.currentActivity.subActivities[i].actionTime;
                                    finishBody += '</div>';
                                    finishBody += '</div>';
                                    finishBody += '</div>';
                                } else {
                                    finishBody += '<div class="widget-box transparent" style="margin-left: 0px;"><div class="widget-header ' + isFinishClassHeader + ' widget-header-small"><h5 class="smaller">' + data.currentActivity.subActivities[i].activityName + ' </h5></div>';
                                    finishBody += '<div class="widget-main">Owner: <a title="Click to send email" href="mailto:';
                                    finishBody += data.currentActivity.subActivities[i].ownerUser + '?subject=SGP';
                                    if (data.currentActivity.subActivities[i].ccUser != null) finishBody += '&cc=' + data.currentActivity.subActivities[i].ccUser + '';
                                    finishBody += '" >';
                                    finishBody += data.currentActivity.subActivities[i].ownerUser + '</a>';
                                    finishBody += '<br>Copy To: ' + data.currentActivity.subActivities[i].ccUser + '</div></div>';
                                }
                            }
                        } else {
                            finishBody += '<div class="widget-main">Owner: <a title="Click to send email" href="mailto:';
                            finishBody += data.currentActivity.ownerUser + '?subject=SGP';
                            if (data.currentActivity.ccUser != null) finishBody += '&cc=' + data.currentActivity.ccUser + '';
                            finishBody += '" >';
                            finishBody += data.currentActivity.ownerUser + '</a>';
                            finishBody += '<br>Copy To: ' + data.currentActivity.ccUser + '</div>';
                        }

                        finishBody += '</div>';
                    }

                    timelineHtml += '<div class="timeline-item clearfix"><div class="timeline-info"> <i class="timeline-indicator btn ' + isFinishClassIcon + ' no-hover">' + data.currentActivity.sort + '</i></div>';
                    timelineHtml += '<div class="widget-box"><div class="widget-header ' + isFinishClassHeader + ' widget-header-small"><h5 class="smaller">' + data.currentActivity.activityName + ' (Current Stage)</h5></div>';
                    timelineHtml += finishBody;
                    timelineHtml += '</div></div>';

                    var iHtml = "";

                    for (var i = 0; i < data.activites.length; i++) {
                        var itemHtml = "";
                        itemHtml += '<div class="timeline-item clearfix"><div class="timeline-info"><i class="timeline-indicator btn btn-success no-hover">' + data.activites[i].sort + '</i></div>';
                        itemHtml += '<div class="widget-box transparent">';
                        if (data.activites[i].subActivities != null && data.activites[i].subActivities.length > 0) {
                            itemHtml += '<div class="widget-header  header-color-blue widget-header-small"><b>' + data.activites[i].activityName + '</b></div>';
                            for (var si = (data.activites[i].subActivities.length - 1) ; si >= 0; si--) {
                                itemHtml += '<div class="widget-body">'
                                itemHtml += '<div class="widget-main">';
                                itemHtml += '<b>&nbsp;' + data.activites[i].subActivities[si].activityName + '&nbsp;</b>';
                                itemHtml += '<div class="pull-right"><b>' + data.activites[i].subActivities[si].actionUser + '</b> submit at ';
                                itemHtml += '<i class="icon-time bigger-110"></i>';
                                itemHtml += data.activites[i].subActivities[si].actionTime;
                                itemHtml += '</div>';
                                itemHtml += '</div>';
                                itemHtml += '</div>';
                            }
                        } else {
                            itemHtml += '<div class="widget-body">';
                            itemHtml += '<div class="widget-main">';
                            itemHtml += '<b>' + data.activites[i].activityName + '</b>';
                            itemHtml += '<div class="pull-right"><b>' + data.activites[i].actionUser + '</b> submit at ';
                            itemHtml += '<i class="icon-time bigger-110"></i>';
                            itemHtml += data.activites[i].actionTime;
                            itemHtml += '</div>';
                            itemHtml += '</div>';
                            itemHtml += '</div>';
                        }

                        itemHtml += '</div>';
                        itemHtml += '</div>';
                        iHtml = itemHtml + iHtml;
                    }
                    timelineHtml += iHtml;
                    timelineHtml += '</div>';
                }
                timelineHtml += '</div>';
                var title = 'Workflow History';
                $.bi.dialog.show({ title: title, content: timelineHtml, width: 570, maxHeight: 500 });
            }
        }, "json");
    }
};