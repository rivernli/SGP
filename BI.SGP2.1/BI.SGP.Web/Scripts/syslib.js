(function ($, h, c) {
    var a = $([]),
    e = $.resize = $.extend($.resize, {}),
    i,
    k = "setTimeout",
    j = "resize",
    d = j + "-special-event",
    b = "delay",
    f = "throttleWindow";
    e[b] = 250;
    e[f] = true;
    $.event.special[j] = {
        setup: function () {
            if (!e[f] && this[k]) {
                return false;
            }
            var l = $(this);
            a = a.add(l);
            $.data(this, d, {
                w: l.width(),
                h: l.height()
            });
            if (a.length === 1) {
                g();
            }
        },
        teardown: function () {
            if (!e[f] && this[k]) {
                return false;
            }
            var l = $(this);
            a = a.not(l);
            l.removeData(d);
            if (!a.length) {
                clearTimeout(i);
            }
        },
        add: function (l) {
            if (!e[f] && this[k]) {
                return false;
            }
            var n;
            function m(s, o, p) {
                var q = $(this),
                r = $.data(this, d);
                r.w = o !== c ? o : q.width();
                r.h = p !== c ? p : q.height();
                n.apply(this, arguments);
            }
            if ($.isFunction(l)) {
                n = l;
                return m;
            } else {
                n = l.handler;
                l.handler = m;
            }
        }
    };
    function g() {
        i = h[k](function () {
            a.each(function () {
                var n = $(this),
                m = n.width(),
                l = n.height(),
                o = $.data(this, d);
                if (m !== o.w || l !== o.h) {
                    n.trigger(j, [o.w = m, o.h = l]);
                }
            });
            g();
        },
        e[b]);
    }
})(jQuery, this);

$(document).ready(function () {
    $.widget("ui.dialog", $.extend({}, $.ui.dialog.prototype, {
        _title: function (title) {
            var $title = this.options.title || '&nbsp;'
            title.html($title);
        }
    }));

    $.fn.onlyNum = function () {
        $(this).keydown(function () {

            var e = $(this).event || window.event;
            var code = parseInt(e.keyCode);
            if ((code >= 48 && code <= 57) ||(code >= 96 && code <= 105)|| code == 8 || code == 13 || code == 46 || code == 110 || code == 190 || code == 9 || ((event.ctrlKey) && (code == 86))) {

                return true;
            } else {
                return false;
            }
        })
    };
});

$.bi = {
    expiresDate: "",
    getRootUrl: function () {
        if (rootUrl != null) {
            return rootUrl;
        } else {
            return "/";
        }
    },
    getExpiresDate: function () {
        if (this.expiresDate == "") {
            var date = new Date();
            date.setFullYear(2099, 1, 1);
            this.expiresDate = date.toGMTString();
        }
        return this.expiresDate;
    },
    getUrl: function() {
        var url = window.location.pathname;
        var lastIndex = url.lastIndexOf("/");
        return url.substr(lastIndex + 1, url.length - lastIndex);
    },
    setCookie: function (key, value) {
        document.cookie = key + "=" + escape(value) + ";expires=" + this.getExpiresDate();
    },
    getCookie: function (key) {
        var arr = document.cookie.match(new RegExp("(^| )" + key + "=([^;]*)(;|$)"));
        if (arr != null) return unescape(arr[2]); return null;
    },
    overlay: {
        stack: 0,
        show: function (parentId) {
            if (parentId == null) {
                $(document.body).append('<div id="documentOverlady" class="widget-box-overlay"><i class="icon-spinner icon-spin icon-2x white"></i></div>');
            } else {
                $("#" + parentId).append('<div id="documentOverlady" class="widget-box-overlay"><i class="icon-spinner icon-spin icon-2x white"></i></div>');
            }
        },
        hide: function () {
            var obj = $("#documentOverlady");
            obj.remove();
        },
        showStack: function (parentId) {
            if ($.bi.overlay.stack == 0) {
                $.bi.overlay.show(parentId);
            }
            $.bi.overlay.stack = $.bi.overlay.stack + 1;
        },
        hideStack: function () {
            $.bi.overlay.stack = $.bi.overlay.stack -1;
            if ($.bi.overlay.stack <= 0) {
                $.bi.overlay.hide();
            }
        }
    },
    form: {
        getData: function (formId) {
            var serializeObj = {};
            var array = $("#" + formId).serializeArray();
            $(array).each(function () {
                if (serializeObj[this.name]) {
                    if ($.isArray(serializeObj[this.name])) {
                        serializeObj[this.name].push(this.value);
                    } else {
                        if ($.trim(this.value) != "") {
                            if ($.trim(serializeObj[this.name]) != "") {
                                serializeObj[this.name] = serializeObj[this.name] + ";" + this.value;
                            } else {
                                serializeObj[this.name] = this.value
                            }
                        }
                    }
                } else {
                    serializeObj[this.name] = $.trim(this.value);
                }
            });
            return serializeObj;
        },
        getCategoryData: function (formId) {
            data = {};
            $("#"+formId).find(":input").each(function () {
                var id = $(this).attr("id");
                if (id != null && id != "") {
                    var value = $.trim($(this).val());
                    if (data[id] != null) {
                        if ($.isArray(data[id])) {
                            data[id].push(value);
                        } else {
                            var preVal = data[id];
                            data[id] = new Array();
                            data[id].push(preVal);
                            data[id].push(value);
                        }
                    } else {
                        data[id] = $.trim(value);
                    }
                }
            });
            return data;
        },
        getAllCategoryData: function () {
            var data = {};
            $(".fm-category").each(function () {
                var categoryId = $(this).attr("id");
                categoryId = categoryId.replace("fm-", "");
                data[categoryId] = {};
                $(this).find(":input").each(function () {
                    var id = $(this).attr("id");
                    if (id != null && id != "") {
                        var value = $.trim($(this).val());
                        if (data[categoryId][id] != null) {
                            if ($.isArray(data[categoryId][id])) {
                                data[categoryId][id].push(value);
                            } else {
                                var preVal = data[categoryId][id];
                                data[categoryId][id] = new Array();
                                data[categoryId][id].push(preVal);
                                data[categoryId][id].push(value);
                            }
                        } else {
                            data[categoryId][id] = $.trim(value);
                        }
                    }
                });
            });
            return data;
        },
        //
        getCustomerData: function () {
            var data = {};
            $(".fm-category").each(function () {
                var categoryId = $(this).attr("id");
                categoryId = categoryId.replace("fm-", "");
                data[categoryId] = {};
                $(this).find(":input").each(function () {
                    var id = $(this).attr("id");
                    if (id != null && id != "") {
                        var value = $.trim($(this).val());
                        //去掉货币格式
                        if (value.indexOf('$') >= 0 && value.indexOf(',') >= 0) {
                            value = value.replace("$", "").replace(/,/g, "");
                        }

                        if (data[categoryId][id] != null) {
                            if ($.isArray(data[categoryId][id])) {
                                data[categoryId][id].push(value);
                            } else {
                                var preVal = data[categoryId][id];
                                data[categoryId][id] = new Array();
                                data[categoryId][id].push(preVal);
                                data[categoryId][id].push(value);
                            }
                        } else {
                            data[categoryId][id] = $.trim(value);
                        }
                    }
                });
            });
            return data;
        },

        pasteJoin: function (sender) {
            event.returnValue = false
            var clipboardData = event.clipboardData;
            if (clipboardData == null) {
                clipboardData = window.clipboardData;
            }
            var txt = clipboardData.getData('text');
            if (txt != null && txt != "") {
                var orgvalues = txt.split('\n');
                if (orgvalues.length > 50) {
                    alert("input limit (50)");
                    txt = "";
                } else {
                    var newValues = [];
                    if (orgvalues != null && orgvalues.length > 0) {
                        for (var i = 0; i < orgvalues.length; i++) {
                            var t = $.trim(orgvalues[i]);
                            if (t != "") {
                                newValues.push(t);
                            }
                        }
                    }
                    txt = newValues.join(";");
                }
            }
            sender.value = txt;
        },
        pasteJoinNumber: function (sender) {
            event.returnValue = false
            var txt = event.clipboardData.getData('text');
            if (txt != null && txt != "") {
                var orgvalues = txt.split('\n');
                var newValues = [];
                if (orgvalues != null && orgvalues.length > 0) {
                    for (var i = 0; i < orgvalues.length; i++) {
                        var t = $.trim(orgvalues[i]);
                        if (t != "" && !isNaN(t)) {
                            newValues.push(t);
                        }
                    }
                }
                txt = newValues.join(";");
            }
            sender.value = txt;
        }
    },
    setComponentEvent: function () {
        $(".chosen-select1").chosen();
        $('.date-picker').datepicker({ autoclose: true }).next().on(ace.click_event, function () {
            $(this).prev().focus();
        });
        $('.date-range-picker').daterangepicker({ showDropdowns: true }).prev().on(ace.click_event, function () {
            $(this).next().focus();
        });
        $(".NumberType1").onlyNum();
    }
};

$.bi.dialog = {
    defaults: {
        width: "auto",
        height: "auto",
        iconCss: "icon-ok",
        modal: true,
        resizable: false,
        title: "",
        content: "",
        okButtonHtml: "<i class='icon-ok bigger-110 green'></i>&nbsp; OK",
        okButtonCss: "btn btn-info btn-xs",
        closeButton: false,
        actionParam: null
    },
    show: function (options) {
        opts = $.extend({}, this.defaults, options);
        var buttons = opts.buttons;
        if (buttons == null) {
            buttons = [
                {
                    html: opts.okButtonHtml,
                    "class": opts.okButtonCss,
                    click: function () {
                        if (opts.okAction) {
                            if (opts.okAction(opts.actionParam)) {
                                $(this).dialog("destroy");
                            }
                        } else {
                            $(this).dialog("destroy");
                        }
                    }
                }
            ]
        }

        if (opts.closeButton) {
            buttons.push({
                html: "<i class='icon-remove bigger-110'></i>&nbsp; Cancel",
                "class": "btn btn-xs",
                click: function () {
                    $(this).dialog("destroy");
                }
            });
        }

        var mw = document.createElement('div');
        $(mw).attr("id", opts.id);
        mw.innerHTML = '<div class="space-6"></div><p class="bigger-110 bolder center grey" style="white-space:nowrap">' + opts.content + '</p>';

        $(mw).dialog({
            width: opts.width,
            height: opts.height,
            resizable: opts.resizable,
            modal:  opts.modal,
            minHeight: opts.minHeight,
            maxHeight: opts.maxHeight,
            title: "<div class='widget-header'><h4 class='smaller'><i class='" + opts.iconCss + "'></i> " + opts.title + "</h4></div>",
            title_html: true,
            close: function (event, ui) {
                $(this).dialog("destroy");
                if (typeof (opts.close) == "function") {
                    opts.close();
                }
            },
            beforeClose: function (event, ui) {
                if (typeof (opts.beforeClose) == "function") {
                    opts.beforeClose();
                }
            },
            create: opts.create,
            buttons: buttons
        });
    },
    showDelete: function (opts) {
        this.show($.extend({
            title: 'Delete',
            content: 'Delete selected record?',
            iconCss: 'icon-warning-sign red',
            closeButton: true,
            width: '250px',
            okButtonHtml: "<i class='icon-trash bigger-110'></i>&nbsp; Delete",
            okButtonCss: "btn btn-danger btn-xs"
        }, opts));
    },
    showErr: function (opts) {
        this.show($.extend({ iconCss: 'icon-warning-sign red' }, opts));
    },
    close: function (dialogId) {
        $("#" + dialogId).dialog("destroy");
    }
};

$.bi.fieldSetting = {
    showDialog: function (opts) {
        var content = '<table>';
        content += '<tr>';
        content += '<td valign="top">';
        content += '<div style="width:270px">';
        content += '<div class="widget-box">';
        content += '<div class="widget-header header-color-blue2">';
        content += '<h4 class="lighter smaller">All Fields</h4>';
        content += '</div>';
        content += '<div class="widget-body">';
        content += '<div class="widget-main padding-8">';
        content += '<div style="height:250px; overflow-y:auto">';
        content += '<ul id="easyTree" class="easyui-tree" data-options="animate:true,dnd:true"></ul>';
        content += '</div>';
        content += '</div>';
        content += '</div>';
        content += '</div>';
        content += '</div>';
        content += '</td>';
        content += '<td valign="top">';
        content += '<div style="width:270px">';
        content += '<div class="widget-box">';
        content += '<div class="widget-header header-color-green2">';
        content += '<h4 class="lighter smaller">Specify the Fields</h4>';
        content += '</div>';
        content += '<div class="widget-body">';
        content += '<div class="widget-main padding-8">';
        content += '<div style="height:250px; overflow-y:auto">';
        content += '<ul id="easyTree1" class="easyui-tree" data-options="animate:true,dnd:true"></ul>';
        content += '</div>';
        content += '</div>';
        content += '</div>';
        content += '</div>';
        content += '</div>';
        content += '</td>';
        content += '</tr>';
        content += '</table>';

        var categoryTreeAction = opts.rootUrl + 'Setting/GetAllFPCCategoryAndFields/' + opts.listName;
        var userTreeAction = opts.rootUrl + 'Setting/GetUserListFields/' + opts.listName;
        var saveAction = opts.rootUrl + 'Setting/SaveListFields';

        $.bi.dialog.show({
            title: opts.title,
            modal: true,
            content: content,
            width: 575,
            height: 450,
            okAction: function () {
                var postModel = { ListName: opts.listName, FieldIds: [] }
                var nodes = $('#easyTree1').tree('getChildren');
                if (nodes != null) {
                    for (var i = 0; i < nodes.length; i++) {
                        postModel.FieldIds.push(nodes[i].id);
                    }
                }

                var bResult;

                $.ajax({
                    url: saveAction,
                    data: postModel,
                    dataType: "json",
                    type: "POST",
                    traditional: true,
                    async: false,
                    success: function (data) {
                        if (data.success) {
                            bResult = true;
                            if (typeof (opts.callBack) == "function") {
                                opts.callBack();
                            }
                        } else {
                            if (!data.success) {
                                $.bi.dialog.showErr({ title: "Message", content: data.errMessage });
                            }
                            bResult = false;
                        }
                    }
                });
                return bResult;
            },
            create: function () {
                using("tree", function () {
                    $('#easyTree').tree({
                        onBeforeDrop: function (target, source, point) {
                            if ($('#easyTree').tree('getNode', target).IsCategory) {
                                return false;
                            }
                        },
                        url: categoryTreeAction,
                        lines: true
                    });

                    $('#easyTree1').tree({
                        onBeforeDrop: function (target, source, point) {
                            if (source.IsCategory) {
                                return false;
                            }
                        },
                        url: userTreeAction,
                        lines: true
                    });
                });
            }
        });
    }
};

(function ($) {

    function getDataUrl(options) {
        return options.dataUrl || $.bi.getRootUrl() + 'GridData/GetQueryTable';
    }

    function createSearchBox(target) {
        var state = $.data(target, 'biSearchBox');
        var groupName = state.options.groupName;
        var actionPath = getDataUrl(state.options);
        $.post(actionPath, { listName: groupName }, function (data) {
            if (data != null) {
                $("#searchGroup").remove();
                $("#query-" + groupName).remove();
                $(target).append(data);

                $.bi.setComponentEvent();

                var atc = $.extend({ OEM: $.bi.getRootUrl() + "Setting/GetAuotCompleteValue" }, state.options.autocomplete);

                for (var k in atc) {
                    var actInput = $(target).find("#" + k);
                    if (actInput.length > 0) {
                        actInput.autocomplete({ source: atc[k] });
                    }
                }

                if (state.options.searchButton != null) {
                    $(document).keydown(function (e) {
                        if (e.keyCode == 13) {
                            $("#" + state.options.searchButton).click();
                            e.preventDefault();
                        }
                    });
                }
            }

            if (state.options.onComplete != null) {
                state.options.onComplete();
            }

        }, "html");
    }

    function getData(target, param) {
        return $.bi.form.getCategoryData(target.attr("id"));//return $.bi.form.getData(target.attr("id"));
    }

    $.fn.biSearchBox = function (options, param) {
        if (typeof options == 'string') {
            var method = $.fn.biSearchBox.methods[options];
            if (method) {
                return method(this, param);
            }
        }

        options = options || {};
        return this.each(function () {
            var state = $.data(this, 'biSearchBox');
            if (state) {
                $.extend(state.options, options);
            } else {
                $.data(this, 'biSearchBox', {
                    options: $.extend({}, $.fn.biSearchBox.defaults, options)
                });
            }
            createSearchBox(this);
        });
    };

    $.fn.biSearchBox.methods = {
        getData: function (jq, param) {
            return getData(jq, param);
        }
    }

    $.fn.biSearchBox.defaults = {
        searchButton: '',
        autocomplete: {}
    };
})(jQuery);

/**
 * BI Grid
 * Dependencies:jgGrid
 */
(function ($) {

    function getColsUrl(options) {
        return options.colsUrl || $.bi.getRootUrl() + 'GridData/GetGridFields/' + options.groupName;
    }

    function getDataUrl(options) {
        return options.dataUrl || $.bi.getRootUrl() + 'GridData/GetGridData';
    }

    function createGrid(target) {

        var state = $.data(target, 'biGrid');
        var options = $.data(target, 'biGrid').options;
        var colsUrl = getColsUrl(options);
        var dataUrl = getDataUrl(options);

        $.post(colsUrl, function (data) {

            options.colNames = data.colNames;
            options.colModel = data.colModel;

            for (var i = 0; i < options.extColumns.length; i++) {
                if (options.extColumns[i].index == -1) {
                    options.colNames.push(options.extColumns[i].title);
                    options.colModel.push(options.extColumns[i].model);
                } else {
                    options.colNames.splice(options.extColumns[i].index, 0, options.extColumns[i].title);
                    options.colModel.splice(options.extColumns[i].index, 0, options.extColumns[i].model);
                }
            }

            for (var i = 0; i < options.colModel.length; i++) {
                if (options.formatters[options.colModel[i].name] != null) {
                    options.colModel[i].formatter = options.formatters[options.colModel[i].name];
                }
            }

            $.extend(options.postData, { groupName: options.groupName, extSqlColumns: options.extSqlColumns });

            $.extend(state.options, options);

            var cookieRowNum = 0;
            if (options.pager != null) {
                var rowNumCookieKey = "sgp_" + $.bi.getUrl() + "rowNum";
                cookieRowNum = $.bi.getCookie(rowNumCookieKey);
                if (cookieRowNum == null || cookieRowNum == "") {
                    cookieRowNum = 10;
                }
            }

            $(target).jqGrid({
                url: dataUrl,
                postData: options.postData,
                sortname: options.sortname,
                sortorder: options.sortorder,
                datatype: options.datatype,
                rownumbers: options.rownumbers,
                colNames: options.colNames,
                colModel: options.colModel,
                viewrecords: options.viewrecords,
                rowNum: cookieRowNum,
                multiselect: options.multiselect,
                multiboxonly: options.multiboxonly,
                rowList: options.rowList,
                pager: options.pager,
                altRows: options.altRows,
                onSelectRow: options.onSelectRow,
                afterInsertRow: options.afterInsertRow,
                loadComplete: function () {
                    var table = this;
                    $.bi.setCookie(rowNumCookieKey, $(target).jqGrid('getGridParam').rowNum);
                    setTimeout(function () {
                        updatePagerIcons();
                        var headerCheckBox = $("#jqgh_" + target.id + "_cb");
                        if (headerCheckBox[0] != null) {
                            headerCheckBox.attr("style", "text-align:center !important");
                        }
                        if (options.pager) {
                            $("#" + options.pager).find(".ui-pg-selbox").val($(target).jqGrid('getGridParam').rowNum);
                        }
                    }, 0);
                    
                    if (options.loadComplete != null) {
                        setTimeout(options.loadComplete, 0);
                    }
                },
                recordpos: options.recordpos,
                height: options.height,
                shrinkToFit: options.shrinkToFit,
                autowidth: options.autowidth
            });

            if (options.widthRelateElement != null) {
                $(target).jqGrid('setGridWidth', $("#" + options.widthRelateElement).width() + options.widthRelateOffset);
                $("#" + options.widthRelateElement).resize(function (e) {
                    $(target).jqGrid('setGridWidth', $("#" + options.widthRelateElement).width() + options.widthRelateOffset);
                });
            }
        });
    }

    function updatePagerIcons() {
        var replacement = {
            'ui-icon-seek-first': 'icon-double-angle-left bigger-140',
            'ui-icon-seek-prev': 'icon-angle-left bigger-140',
            'ui-icon-seek-next': 'icon-angle-right bigger-140',
            'ui-icon-seek-end': 'icon-double-angle-right bigger-140'
        };
        $('.ui-pg-table:not(.navtable) > tbody > tr > .ui-pg-button > .ui-icon').each(function () {
            var icon = $(this);
            var $class = $.trim(icon.attr('class').replace('ui-icon', ''));
            if ($class in replacement) icon.attr('class', 'ui-icon ' + replacement[$class]);
        });
    }

    function search(target, param) {
        var state = $.data(target, 'biGrid');
        $(target).jqGrid('setGridParam', {
            postData: $.extend(state.options.postData, param),
            page: 1
        }).trigger("reloadGrid");
    }

    $.fn.biGrid = function (options, param) {
        if (typeof options == 'string') {
            var method = $.fn.biGrid.methods[options];
            if (method) {
                return method(this, param);
            }
        }

        options = options || {};
        return this.each(function () {
            var state = $.data(this, 'biGrid');
            if (state) {
                $.extend(state.options, options);
            } else {
                $.data(this, 'biGrid', {
                    options: $.extend({}, $.fn.biGrid.defaults, options)
                });
            }
            createGrid(this);
        });
    };

    $.fn.biGrid.methods = {
        search: function (jq, param) {
            return jq.each(function () {
                search(this, param);
            });
        }
    }

    $.fn.biGrid.defaults = {
        sortorder: 'desc',
        datatype: "json",
        rownumbers: true,
        viewrecords: true,
        multiselect: true,
        multiboxonly: true,
        rowList: [10, 20, 50, 100],
        altRows: true,
        recordpos: 'left',
        height: 'auto',
        shrinkToFit: false,
        autowidth: true,
        formatters: {},
        extSqlColumns: '',
        extColumns: [],
        widthRelateElement: null,
        widthRelateOffset: -2,
        postData: {},
        onSelectRow: null,
        loadComplete: null,
        afterInsertRow: null
    };
})(jQuery);