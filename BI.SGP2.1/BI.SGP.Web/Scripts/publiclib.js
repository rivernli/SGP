$(document).ready(function () {
    $.widget("ui.dialog", $.extend({}, $.ui.dialog.prototype, {
        _title: function (title) {
            var $title = this.options.title || '&nbsp;'
            title.html($title);
        }
    }));
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
    }
};


$.bi.setCookie = function (key, value) {
    document.cookie = key + "=" + escape(value) + ";expires=" + $.bi.getExpiresDate();
}

$.bi.getCookie = function (key) {
    var arr = document.cookie.match(new RegExp("(^| )" + key + "=([^;]*)(;|$)"));
    if (arr != null) return unescape(arr[2]); return null;
}

$.bi.form = {
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
                if ($.trim(this.value).indexOf('$') >= 0) {
                    serializeObj[this.name] = $.trim(this.value).replace("$", "").replace(/,/g, "");
                } else {
                    serializeObj[this.name] = $.trim(this.value);
                }       
            }
        });        
        return serializeObj;
    },
    generateQuery: function (opts) {
        var actionPath = $.bi.getRootUrl() + 'GridData/GetQueryTable';
        $.post(actionPath, { listName: opts.listName }, function (data) {
            if (data != null) {
                $("#searchGroup").remove();
                $("#query-" + opts.listName).remove();
                $("#" + opts.parentId).append(data);
                $(".chosen-select1").chosen();
                $("#OEM").autocomplete({
                    source: '@Url.Content("~/Setting/GetAuotCompleteValue")'
                });
                $('.date-range-picker').daterangepicker({ showDropdowns: true }).prev().on(ace.click_event, function () {
                    $(this).next().focus();
                });
            }
        }, "html");
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
    },
}

$.bi.overlay = {
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
    }
}

$.bi.dialog = {
    show: function (opts) {
        //icon-warning-sign red
        var dwidth = opts.width == null ? "auto" : opts.width;
        var dheight = opts.height == null ? "auto" : opts.height;
        var iconCss = opts.iconCss == null ? "icon-ok" : opts.iconCss;
        var modal = opts.modal == null ? true : opts.modal;
        var buttons = opts.buttons;
        if (buttons == null) {
            buttons = [
                {
                    html: "<i class='icon-ok bigger-110 green'></i>&nbsp; OK",
                    "class": "btn btn-info btn-xs",
                    click: function () {
                        if (opts.okAction) {
                            if (opts.okAction()) {
                                $(this).dialog("destroy");
                            }
                        } else {
                            $(this).dialog("destroy");
                        }
                    }
                }
            ]
        }

        var mw = document.createElement('div');
        $(mw).attr("id", opts.id);
        mw.innerHTML = '<p class="bigger-110 bolder center grey" style="white-space:nowrap">' + opts.content + '</p>';
        $(mw).dialog({
            width: dwidth,
            height: dheight,
            resizable: false,
            modal: modal,
            minHeight: opts.minHeight,
            maxHeight: opts.maxHeight,
            title: "<div class='widget-header'><h4 class='smaller'><i class='" + iconCss + "'></i> " + opts.title + "</h4></div>",
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
    showErr: function (opts) {
        this.show($.extend({ iconCss: 'icon-warning-sign red' }, opts));
    },
    close: function (dialogId) {
        $("#" + dialogId).dialog("destroy");
    }
};

$.bi.action = {
    edit: function (gid, rowId, keyValue) {
        if ($.bi.grid.setting[gid].editAction != null) {
            $.bi.grid.setting[gid].editAction(gid, rowId, keyValue);
        }
    },
    down: function (gid, rowId, keyValue) {
        if ($.bi.grid.setting[gid].downAction != null) {
            $.bi.grid.setting[gid].downAction(gid, rowId, keyValue);
        }
    },
    history: function (gid, rowId, keyValue) {
        if ($.bi.grid.setting[gid].history != null) {
            $.bi.grid.setting[gid].history(gid, rowId, keyValue);
        }
    },
    del: function (gid, rowId, keyValue) {
        if ($.bi.grid.setting[gid].delAction != null) {
            var mw = document.createElement('div');
            mw.innerHTML = '<div class="space-6"></div><p class="bigger-110 bolder center grey">Delete selected record?</p>';

            $(mw).dialog({
                width: '250px',
                resizable: false,
                modal: true,
                title: "<div class='widget-header'><h4 class='smaller'><i class='icon-warning-sign red'></i> Delete</h4></div>",
                title_html: true,
                close: function (event, ui) {
                    $(this).dialog("destroy");
                },
                buttons: [
                    {
                        html: "<i class='icon-trash bigger-110'></i>&nbsp; Delete",
                        "class": "btn btn-danger btn-xs",
                        click: function () {
                            $.bi.grid.setting[gid].delAction(gid, rowId, keyValue);
                            $(this).dialog("destroy");
                        }
                    },
                    {
                        html: "<i class='icon-remove bigger-110'></i>&nbsp; Cancel",
                        "class": "btn btn-xs",
                        click: function () {
                            $(this).dialog("destroy");
                        }
                    }
                ]
            });
        }
    }
};

$.bi.formatter = {
    actionFormatter: function (cellvalue, options, rowObject, setting) {
        if ($.bi.grid.setting[setting.grid_selector].rowObject == null) {
            $.bi.grid.setting[setting.grid_selector].rowObject = [];
        }
        $.bi.grid.setting[setting.grid_selector].rowObject[options.rowId] = rowObject;
        //alert(JSON.stringify(cellvalue));
        //alert(JSON.stringify(options));
        //alert(JSON.stringify(setting));
        
        var returtString = '';
        if (setting.editAction != null) {
            returtString += '<div title="Edit" style="float:left;cursor:pointer;" class="ui-pg-div ui-inline-edit" onclick="$.bi.action.edit(\'' + setting.grid_selector + '\',\'' + options.rowId + '\', \'' + rowObject[setting.key] + '\');" onmouseover="jQuery(this).addClass(\'ui-state-hover\');" onmouseout="jQuery(this).removeClass(\'ui-state-hover\')"><span class="ui-icon ui-icon-pencil"></span></div>';
        }

        var keyusers = ["mcntwang", "gsssagov", "gssravku", "gssubhat", "gssrsing", "hkgmalai", "mcnozhan","mcnszhan"];

       

        if (setting.delAction != null) {
            var strclickevent;
            var strenablecss;
            var strtitle;

            if ($.inArray(setting.UserID, keyusers)>0) {

                strenablecss = "red";
                strtitle = "delete";
                strclickevent = '$.bi.action.del(\'' + setting.grid_selector + '\',\'' + options.rowId + '\', \'' + rowObject[setting.key] + '\');';
            }
            else {
                if (rowObject["WFStatus"] != "Launch") {
                    strclickevent = "";
                    strenablecss = "gray";
                    strtitle = "can't delete";
                }
                else {
                    strenablecss = "red";
                    strtitle = "delete";
                    strclickevent = '$.bi.action.del(\'' + setting.grid_selector + '\',\'' + options.rowId + '\', \'' + rowObject[setting.key] + '\');';
                }
            }
            returtString += '<div title="' + strtitle + '" style="float:left;margin-left:2px;" class="ui-pg-div ui-inline-del"  onclick="' + strclickevent + '" onmouseover="jQuery(this).addClass(\'ui-state-hover\');" onmouseout="jQuery(this).removeClass(\'ui-state-hover\');"><span style="color:' + strenablecss + '" class="ui-icon ui-icon-trash ' + strenablecss + '"></span></div>';
           // returtString += '<div title="Delete" style="float:left;margin-left:2px;" class="ui-pg-div ui-inline-del" onclick="$.bi.action.del(\'' + setting.grid_selector + '\',\'' + options.rowId + '\', \'' + rowObject[setting.key] + '\');" onmouseover="jQuery(this).addClass(\'ui-state-hover\');" onmouseout="jQuery(this).removeClass(\'ui-state-hover\');"><span class="ui-icon ui-icon-trash"></span></div>';
        }

        if (setting.downAction != null) {

            var strclickevent;
            var strenablecss;
            var strtitle;
            if (rowObject["WFStatus"] == "Launch" || rowObject["WFStatus"] == "Technical Costing" || rowObject["WFStatus"] == "Pricing" || rowObject["WFStatus"] == "Approval") {
                strclickevent = "";
                strenablecss = "gray";
                strtitle = "No PDF can be generated until after Stage 4";
            }
            else {
                strenablecss = "green";
                strtitle = "Download PDF";
                strclickevent = '$.bi.action.down(\'' + setting.grid_selector + '\',\'' + options.rowId + '\', \'' + rowObject[setting.key] + '\');';
            }
            returtString += '<div title="' + strtitle + '" style="float:left;margin-left:2px;" class="ui-pg-div ui-inline-del"  onclick="' + strclickevent + '" onmouseover="jQuery(this).addClass(\'ui-state-hover\');" onmouseout="jQuery(this).removeClass(\'ui-state-hover\');"><span style="color:' + strenablecss + '" class="ui-icon icon-file-alt ' + strenablecss + '"></span></div>';
        }
        if (setting.history != null) {
            returtString += '<div title="history" style="float:left;margin-left:2px;" class="ui-pg-div ui-inline-edit" onclick="$.bi.action.history(\'' + setting.grid_selector + '\',\'' + options.rowId + '\', \'' + rowObject[setting.key] + '\');" onmouseover="jQuery(this).addClass(\'ui-state-hover\');" onmouseout="jQuery(this).removeClass(\'ui-state-hover\');"><span class="ui-icon icon-calendar orange"></span></div>';
        }
        return returtString;
    },
    isNewFormatter: function (cellvalue, options, rowObject) {
        if (cellvalue == null) {
            cellvalue = "";
        }
        
        cellvalue = "<a href='" + $.bi.getRootUrl() + "Pricing/Detail?RFQID=" + rowObject["RFQID"] + "' target='_blank'>" + cellvalue + "</a>"

        //if (rowObject["StatusID"] != "9") {
        //    return cellvalue + ' <span class="label label-sm label-warning arrowed arrowed-righ">New</span>';
        //} else {
            return cellvalue;
        //}
    },
    wfFormatter: function (cellvalue, options, rowObject) {
        if (cellvalue == null || cellvalue == "") {
            return "";
        } else {
            var labelClass;
            if (rowObject["StatusID"] == "9") {
                labelClass = "label-success";
            } else {
                labelClass = "label-info";
            }
            return '<span class="label ' + labelClass + ' arrowed arrowed-right" style="cursor:pointer" onclick="$.bi.timeline.show(1,\'' + rowObject["RFQID"] + '\')">' + cellvalue + '</span>';
        }
    }
};

$.bi.grid = {
    rootUrl: "",
    setting: {},
    load: function (opts) {
        this.rootUrl = opts.rootUrl;
        var gridObj = $(".bi-grid");
        for (var i = 0; i < gridObj.length; i++) {
            var gridId = "#" + $(gridObj[i]).attr("id");
            if (gridId != null) {
                this.setting[gridId] = new Object();
                this.setting[gridId].grid_selector = gridId;
                this.setting[gridId].pager_selector = "#" + $(gridObj[i]).attr("pager");
                this.setting[gridId].listName = $(gridObj[i]).attr("listName");
                this.setting[gridId].editAction = eval($(gridObj[i]).attr("editAction"));
                this.setting[gridId].delAction = eval($(gridObj[i]).attr("delAction"));
                this.setting[gridId].downAction = eval($(gridObj[i]).attr("downAction"));
                this.setting[gridId].history = eval($(gridObj[i]).attr("history"));
                this.setting[gridId].key = $(gridObj[i]).attr("key");
                this.setting[gridId].UserID = $(gridObj[i]).attr("UserID");
                this.setting[gridId].rowNum = $(gridObj[i]).attr("rowNum");
                this.setting[gridId].multiselect = $(gridObj[i]).attr("multiselect");
                this.setting[gridId].dataUrl = opts.rootUrl + "GridData/GetData/" + this.setting[gridId].listName;
                this.setting[gridId].colsUrl = opts.rootUrl + "GridData/GetGridFields/" + this.setting[gridId].listName;
                this.setting[gridId].formatters = {};
                var actionFormatter = $(gridObj[i]).attr("actionFormatter");
                if (actionFormatter != null && actionFormatter != "") {
                    var formatters = actionFormatter.split(',');
                    if (formatters != null && formatters.length > 0) {
                        for (var i = 0; i < formatters.length; i++) {
                            var fmKeyValue = formatters[i].split(':');
                            this.setting[gridId].formatters[$.trim(fmKeyValue[0])] = $.trim(fmKeyValue[1]);
                        }
                    }
                }
                this.initGrid(this.setting[gridId]);
            }
        }
    },
    reload: function (gridId) {
        if (gridId != null) {
            this.initGrid(this.setting["#" + gridId]);
        } else {
            for (var o in this.setting) {
                this.initGrid(this.setting[o]);
            }
        }
    },
    initGrid: function (setting) {
        var thisGrid = this;
        $.post(setting.colsUrl, function (data) {

            var opts = setting;
            opts.colNames = data.colNames;
            opts.colModel = data.colModel;

            //var opts = $.extend({
            //    colModel: data.colNames,
            //    colModel: data.colModel
            //}, setting);

            var actionWidth = 5;
            if (opts.editAction != null) {
                actionWidth += 24;
            }

            if (opts.downAction != null) {
                actionWidth += 24;
            }

            if (opts.delAction != null) {
                actionWidth += 24;
            }

            if (opts.history != null) {
                actionWidth += 24;
            }

            if (actionWidth > 5) {
                opts.colNames.unshift(" ");
                opts.colModel.unshift({
                    name: 'myac', index: '', width: actionWidth, fixed: true, sortable: false, resize: false, align: 'center',
                    formatter: function (cellvalue, options, rowObject) {
                        return $.bi.formatter.actionFormatter(cellvalue, options, rowObject, setting);
                    }
                });
            }

            for (var i = 0; i < opts.colModel.length; i++) {
                if (opts.formatters[opts.colModel[i].name] != null) {
                    opts.colModel[i].formatter = eval(opts.formatters[opts.colModel[i].name]);
                }
            }

            $(opts.grid_selector).GridUnload();
            thisGrid.createGrid(opts);
        });
    },
    createGrid: function (gridOptions) {
        var thisGrid = this;
        var rowNumCookieKey = "sgp_" + gridOptions.grid_selector + "rowNum";
        var cookieRowNum = $.bi.getCookie(rowNumCookieKey);
        if (cookieRowNum == null || cookieRowNum == "") {
            cookieRowNum = 10;
        }
        $(gridOptions.grid_selector).jqGrid({
            url: gridOptions.dataUrl,
            sortname: gridOptions.key,
            sortorder: 'desc',
            datatype: "json",
            rownumbers: true,
            colNames: gridOptions.colNames,
            colModel: gridOptions.colModel,
            viewrecords: true,
            rowNum: cookieRowNum,
            multiselect: gridOptions.multiselect == null ? true : (gridOptions.multiselect == "false" ? false : true),
            multiboxonly: true,
            rowList: [10, 20, 50, 100],
            pager: gridOptions.pager_selector,
            altRows: true,
            loadComplete: function () {
                var table = this;
                $.bi.setCookie(rowNumCookieKey, $(gridOptions.grid_selector).jqGrid('getGridParam').rowNum);
                setTimeout(function () {
                    thisGrid.updatePagerIcons(table);
                    var headerCheckBox = $("#jqgh_grid-table_cb");
                    if (headerCheckBox[0] != null) {
                        headerCheckBox.attr("style", "text-align:center !important");
                    }
                    $(gridOptions.pager_selector).find(".ui-pg-selbox").val($(gridOptions.grid_selector).jqGrid('getGridParam').rowNum);
                }, 0);
            },
            recordpos: "left",
            height: "auto",
            shrinkToFit: false,
            autowidth: true
        });
    },
    updatePagerIcons: function (table) {
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
    },
    search: function (postData, gridId) {
        if (gridId != null) {
            var grid_selector = "#" + gridId;
            $(grid_selector).jqGrid('setGridParam', {
                url: this.setting["#" + gridId].dataUrl,
                postData: postData,
                page: 1
            }).trigger("reloadGrid");
        } else {
            for (var o in this.setting) {
                $(this.setting[o].grid_selector).jqGrid('setGridParam', {
                    url: this.setting[o].dataUrl,
                    postData: postData,
                    page: 1
                }).trigger("reloadGrid");
            }
        }
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

        var categoryTreeAction = opts.rootUrl + 'Setting/GetAllCategoryAndFields/' + opts.listName;
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

$.bi.wizard = {
    create: function (opts) {
        if (opts.rootUrl != null && opts.wizardId != null && opts.templateName != null) {
            var actionPath = opts.rootUrl + 'Workflow/GetWizardData';
            var wizardId = "#" + opts.wizardId;
            $(wizardId).removeClass("wizard-steps").addClass("wizard-steps");
            var iHtml = "";
            $.post(actionPath, { templateName: opts.templateName, entityId: opts.entityId }, function (data) {
                if (data != null) {
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
                }
                $(wizardId).html(iHtml);

                if (opts.clickCallback) {
                    opts.clickCallback();
                }
            }, "json");
        }
    }
};

$.bi.wf = {};

$.bi.wf.skip = {
    show: function (templateName, entityId, func) {
        var actionPath = $.bi.getRootUrl() + 'Workflow/GetWizardData';
        $.post(actionPath, { templateName: templateName, entityId: entityId }, function (data) {
            if (data != null) {
                if (data.activites != null && data.activites.length > 0) {
                    var iHtml = '<div class="timeline-container" style="width:500px"><div class="timeline-items">';
                    for (var i = 0; i < data.activites.length; i++) {
                        var curId, btn, icoClass;
                        if (data.currentActivity != null) {
                            curId = data.currentActivity.id;
                          
                        } else {
                            curId = 0;
                        }
                        if (curId > 0 && curId == data.activites[i].id) {
                            btn = "";
                            icoClass = "btn-primary";
                        } else {
                            btn = '<a href="javascript:void(0)" onclick="' + func + '(\'' + curId + '\',\'' + data.activites[i].id + '\')"><i class="icon-reply light-green bigger-130"></i></a>';
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
                        id: 'skip' + entityId, title: title, content: iHtml, buttons: [{
                            html: "<i class='icon-remove bigger-110'></i>&nbsp; Cancel",
                            "class": "btn btn-xs",
                            click: function () {
                                $(this).dialog("destroy");
                            }
                        }]
                    });
                }
            }
        }, "json");
    },
    close: function (entityId) {
        $.bi.dialog.close('skip' + entityId);
    }
};




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
                        if (data.activites[i].subActivities != null) {
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