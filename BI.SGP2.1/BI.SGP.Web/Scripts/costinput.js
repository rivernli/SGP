$.cost = {
    common: {
        showQuery: function (obj, costItem, onSelected) {
            var trObj = $(obj).parent().parent();
            var onQuerySelected = onSelected || $.cost.common.onQuerySelected;
            $.post(rootUrl + 'CostIO/GetTableKeyByCostItem', { costItem: costItem }, function (data) {
                openQuery({ costItem: costItem, tableKey: data, onSelect: onQuerySelected, multiselect: false, trObj: trObj })
            });
        },
        getTrData: function(trObj) {
            var jsonData = {}
            $(trObj).find(".form-field").each(function (alldata) {
                var id = $(this).eq(0).prop("id")
                var value = $(this).val();
                jsonData[id] = value;
            });
            return jsonData;
        },
        onQuerySelected: function (options, status, rowData, costItem) {
            var trObj = options.trObj;
            trObj.find("#MaterialID").val(rowData["ID"]);
            trObj.find("#ManualPrice").val(rowData["USDPrice"]);
            trObj.find("#MaterialDescription").val(rowData["Description"]);
            trObj.find("#Unit").val(rowData["PriceUnit"]);
        },
        initRFQNumber: function() {
            $("#RFQNumber").unbind('change').on('change', function (e) {
                $.cost.common.setRFQInfo();
            });
        },
        setRFQInfo: function() {
            var rfqNumber = $("#RFQNumber").val();
            if (rfqNumber.length > 11) {
                $.post(rootUrl + 'CostIO/GetRFQInfo', { rfqNumber: rfqNumber }, function (data) {
                    if (data != null && data.length > 0) {
                        $.cost.common.fillRFQ(data[0]);
                    }
                }, "json");
            } else {
                $.cost.common.fillRFQ(null);
            }
        },
        fillRFQ: function (data) {
            if (data == null) {
                $("#OEM").val("");
                $("#CEM").val("");
                $("#CustomerPartNumber").val("");
                $("#CustomerPartRevision").val("");
                $("#ProductApplication").val("");
                $("#EAU").val("");
            } else {
                $("#OEM").val(data["OEM"]);
                $("#CEM").val(data["CEM"]);
                $("#CustomerPartNumber").val(data["CustomerPartNumber"]);
                $("#CustomerPartRevision").val(data["Revision"]);
                $("#ProductApplication").val(data["Application"]);
                $("#EAU").val(data["VolumePerMonth"]);
            }
        },
        initPanelSize: function (plant) {
            $.post(rootUrl + 'CostIO/GetPanelSize', { plant: plant }, function (data) {
                var panelSizeValue = $("#PanelSize").val();
                var td = $("#PanelSize").parent();
                $(td).html('');
                $(td).html('<select class="form-control form-field" style="width:100% !important" name="PanelSize" id="PanelSize"></select>');
                $("#PanelSize").append("<option value=''></option>");
                for (var i = 0; i < data.length; i++) {
                    var selected = data[i]["PanelSize"] == panelSizeValue ? "selected" : "";
                    $("#PanelSize").append("<option value='" + data[i]["PanelSize"] + "' " + selected + ">" + data[i]["PanelSize"] + "</option>");
                }

                $("#PanelSize").unbind('change').on('change', function (e) {
                    panelSizeValue = $("#PanelSize").val();
                    if (panelSizeValue != '') {
                        $.post(rootUrl + 'CostIO/GetPanelArea', { plant: plant, panelSize: panelSizeValue }, function (data) {
                            $("#PanelArea").val(data);
                            $.cost.spec.calcAllSpecTime();
                        });
                    } else {
                        $("#PanelArea").val('');
                        $.cost.spec.calcAllSpecTime();
                    }
                });
            }, "json");
        },
        setProductType: function() {
            var pt = $("#ProductType");
            if (pt.length > 0) {
                var vs = $.trim($("#ViaStructure").val());
                var lc = $.trim($("#LayerCount").val());
                if (vs != "" && lc != "") {
                    $.post(rootUrl + 'CostIO/GetProductType', { category: vs, layerCount: lc, pcbType: 'Rigid' }, function (data) {
                        $("#ProductType").val(data);
                        $.cost.common.initProcFlowByProdType();
                    });
                } else {
                    $("#ProductType").val("");
                    $.cost.common.initProcFlowByProdType();
                }
            }
        },
        initProcFlowByProdType: function () {
            $.cost.procFlow.showConfirm();
            $("#tb-ProcessFlow tr:gt(0)").remove();
            var prodType = $("#ProductType").val();
            var viaStructure = $("#ViaStructure").val();
            var plant = $("#CostingBasedOn").val();
            var version = $("#CostVersion").val();
            if (prodType != "" && plant != "" && pcbType != "FPC") {
                $.post(rootUrl + 'CostIO/GetProcFlowByProdType', { plant: plant, prodType: prodType, viaStructure: viaStructure, version: version }, function (data) {
                    var flowData = {};
                    for (var i = 0; i < data.flow.length; i++) {
                        var layerName = data.flow[i].Layer;
                        if(layerName == null || layerName == "") {
                            layerName = "NoLayer";
                        }
                        if (flowData[layerName] == null) {
                            flowData[layerName] = [];
                        }
                        flowData[layerName].push(data.flow[i]);
                    }

                    for (var obj in flowData) {
                        var times = 1;
                        for (var i = 0; i < data.times.length; i++) {
                            if (obj.toLocaleLowerCase() == data.times[i].Layer.toLocaleLowerCase()) {
                                times = data.times[i].Times;
                                break;
                            }
                        }

                        for (var j = 0; j < times; j++) {
                            for (var i = 0; i < flowData[obj].length; i++) {
                                    var tr = addProcessFlowDetail('ProcessFlow', null, -1);
                                    var trObj = $("#tb-ProcessFlow tr:last");
                                    $(trObj).find("#Layer").val(flowData[obj][i]["Layer"]);
                                    $(trObj).find("#WorkCenter").val(flowData[obj][i]["WorkCenter"]);
                                    $(trObj).find("#DescriptionEnglish").val(flowData[obj][i]["DescEnglish"]);
                                    $(trObj).find("#DescriptionChinese").val(flowData[obj][i]["DescChinese"]);
                            }
                        }
                    }
                    
                    $.cost.procFlow.resetFlowStep();
                }, "json");
            }
        },
        boundEvent: function() {
            $.cost.procFlow.boundEvent();
            $.cost.bom.boundEvent();
        },
        removeAfter: function (subDataType) {
            switch(subDataType)
            {
                case "ProcessFlow":
                    $.cost.procFlow.resetFlowStep();
                    break;
            }
        }
    },
    procFlow: {
        boundEvent: function () {
            $("#tb-ProcessFlow").find("input[name='WorkCenter']").unbind('change').on('change', function (sender) {
                $.cost.procFlow.fillDesc($(this).parent().parent());
            });
            $("#tb-ProcessFlow").find("select[name='SubcontractPlant']").unbind('change').on('change', function (sender) {
                $.cost.procFlow.fillDesc($(this).parent().parent());
            });
            $("#tb-ProcessFlow").find("input[name='WorkCenter']").unbind('paste').on('paste', function (sender) {
                event.returnValue = false
                var idx = 0;
                var firstVal = "";
                var nextTr = null;
                var txt = event.clipboardData.getData('text');
                if (txt != null && txt != "") {
                    var orgvalues = txt.split('\n');
                    if (orgvalues != null && orgvalues.length > 0) {
                        for (var i = 0; i < orgvalues.length; i++) {
                            var t = $.trim(orgvalues[i]);
                            if (t != "") {
                                if ($.trim(this.value) == "" && idx == 0) {
                                    firstVal = t;
                                } else {
                                    if (nextTr == null) {
                                        nextTr = $(this).parent().parent();
                                    }
                                    $(nextTr).after(subdata_ProcessFlow);
                                    nextTr = $(nextTr).next();
                                    $(nextTr).find("#WorkCenter").val(t);
                                    $.cost.procFlow.fillDesc(nextTr);
                                }
                                idx++;
                            }
                        }
                    }
                }
                if ($.trim(this.value) == "" && firstVal != "") {
                    this.value = firstVal;
                    $.cost.procFlow.fillDesc($(this).parent().parent())
                }
                $.bi.setComponentEvent();
                $.cost.procFlow.resetFlowStep();
            });
        },
        resetFlowStep: function () {
            $("#tb-ProcessFlow tr").each(function (i) {
                $(this).find("#Step").val(i * 10);
            })
        },
        fillDesc: function (trObj) {
            var wc = $(trObj).find("#WorkCenter").val();
            var plant = $(trObj).find("#SubcontractPlant").val();
            if ($.trim(plant) == "") {
                plant = $("#CostingBasedOn").val();
            }
            if ($.trim(wc) != "") {
                $.post(rootUrl + 'CostIO/GetWCDesc', { wc: wc, plant: plant }, function (data) {
                    if (data != null && data.length > 0) {
                        $(trObj).find("#DescriptionEnglish").val(data[0]["DescEnglish"]);
                        $(trObj).find("#DescriptionChinese").val(data[0]["DescChinese"]);
                    } else {
                        $(trObj).find("#DescriptionEnglish").val('');
                        $(trObj).find("#DescriptionChinese").val('');
                    }
                }, "json");
            }
        },
        addFlowGroup: function() {
            var procGroupName = $("#ProcGroupLisst").val();
            var insertStep = $.trim($("#InsertStep").val());

            if (procGroupName == "") {
                $.bi.dialog.showErr({ title: "Message", content: "please select a process group", iconCss: "icon-warning-sign red" });
            } else {
                $.post(rootUrl + 'CostIO/GetProcGroup', { procGroup: procGroupName }, function (data) {
                    if (insertStep != null && insertStep != "") {
                        insertStep = Math.floor((insertStep * 1 - 1) / 10);
                        if (insertStep < 0) insertStep = 0;
                    } else {
                        insertStep = -1;
                    }

                    for (var i = 0; i < data.length; i++) {
                        if (insertStep == 0) {
                            var trObj = addProcessFlowDetail('ProcessFlow');
                            $(trObj).find("#Layer").val(data[i]["Layer"]);
                            $(trObj).find("#WorkCenter").val(data[i]["WorkCenter"]);
                            $.cost.procFlow.fillDesc(trObj);
                            insertStep++;
                        } else {
                            var inTr = null;
                            var inIndex = -1;

                            if (insertStep > 0) {
                                var ttr = $("#tb-ProcessFlow tr:eq(" + insertStep + ")");
                                if (ttr.length > 0) {
                                    inTr = ttr.find("#Step");
                                    inIndex = insertStep;
                                }
                            }

                            var trObj = addProcessFlowDetail('ProcessFlow', inTr, inIndex);
                            if (insertStep > 0) {
                                insertStep++;
                            }
                            $(trObj).find("#Layer").val(data[i]["Layer"]);
                            $(trObj).find("#WorkCenter").val(data[i]["WorkCenter"]);
                            $.cost.procFlow.fillDesc(trObj);
                        }
                        
                    }
                    $.cost.procFlow.resetFlowStep();
                }, "json");
            }
        },
        removeAllFlow: function() {
            $("#tb-ProcessFlow tr:gt(0)").remove();
        },
        confirmFlow: function (trObj) {
            var cbo = $("#CostingBasedOn").val();
            var wcData = [];
            $("#tb-ProcessFlow tr:gt(0)").each(function () {
                var wc = $(this).find("#WorkCenter").val();
                var pt = $(this).find("#SubcontractPlant").val();
                if (pt == null || pt == "") {
                    pt = cbo;
                }
                var objData = eval("({" + pt + ":'" + wc + "'})");
                wcData.push(objData);
            });
            $.bi.overlay.showStack();
            $.post(rootUrl + 'CostIO/GetWCMapping', { pcbType: pcbType, wcData: JSON.stringify(wcData) }, function (result) {
                $.bi.overlay.hideStack();
                $.cost.procFlow.removeAllCategoryRow();
                if (result.sysMsg.isPass) {
                    var data = result.data;
                    if (data != null && data.length > 0) {
                        $("#tb-ProcessFlow tr").each(function (index) {
                            var step = $(this).find("#Step").val();
                            var wcElem = $(this).find("#WorkCenter");
                            var sPlant = $.trim($(this).find("#SubcontractPlant").val());
                            var mPlant = $("#CostingBasedOn").val();
                            var plant = sPlant == "" ? mPlant : sPlant;
                            var wc = $.trim($(wcElem).val()).toUpperCase();
                            if (wc != "") {
                                $(wcElem).val(wc);
                                for (var i = 0; i < data.length; i++) {
                                    var mwc = $.trim(data[i]["MainWorkCenter"]).toUpperCase();
                                    var swc = $.trim(data[i]["SubWorkCenter"]).toUpperCase();
                                    var ewcs = $.trim(data[i]["ExceptWC"]).toUpperCase().split('|');
                                    var isExcept = false;
                                    if (ewcs != null && ewcs.length > 0) {
                                        for (var j = 0; j < ewcs.length; j++) {
                                            var ewc = ewcs[j];
                                            if (ewc != null && ewc != "" && wc.indexOf(ewc) != -1) {
                                                isExcept = true;
                                                break;
                                            }
                                        }
                                    }
                                    var dp = $.trim(data[i]["Plant"]).toUpperCase();
                                    if (!isExcept && plant == dp && (wc == mwc || wc == swc)) {
                                        $.cost.procFlow.addCategoryRow(step, wc, data[i], this);
                                        break;
                                    }
                                }
                            }
                        });
                        $.cost.procFlow.hideConfirm();
                    }
                }
                else {
                    $.bi.dialog.show({ title: 'Message', content: result.sysMsg.MessageString, buttons: [] });
                }
            }, "json");
        },
        removeAllCategoryRow: function(){
            $("#tb-BOM tr:gt(0)").remove();
            $("#tb-DryFilm tr:gt(0)").remove();
            $("#tb-SMCM tr:gt(0)").remove();
            $("#tb-DrillBit tr:gt(0)").remove();
            $("#tb-Router tr:gt(0)").remove();
            $("#tb-LAS tr:gt(0)").remove();
            $("#tb-LAC tr:gt(0)").remove();
            $("#tb-Punching tr:gt(0)").remove();
            $("#tb-SpeaProc tr:gt(0)").remove();
            $("#tb-Gold tr:gt(0)").remove();
        },
        addCategoryRow: function (step, wc, data, flowTr) {
            var form = $("form[categoryname='" + data["Category"] + "']");
            if (form != null && form.length > 0) {
                var plant = $.trim($(flowTr).find("#SubcontractPlant").val());
                if (plant == null || plant == "") {
                    plant = $("#CostingBasedOn").val();
                }

                var newTrData = {
                    step: step,
                    plant: plant,
                    mwc: data["MainWorkCenter"],
                    wc: wc,
                    pfid: $(flowTr).index() + 1
                }

                var subdatatype = $(form).attr("subdatatype");
                eval("add" + subdatatype + "Detail('" + subdatatype + "', null, -1, newTrData)");
            }
        },
        hideConfirm: function () {
            $("#FlowConfirm").val("1");
            $(".procflow-confirm").hide();
            $(".procflow-modify").show();
            $("#tb-ProcessFlow tr").find("td:eq(0),th:eq(0)").hide();
            $("#tb-ProcessFlow tr").find("td:gt(1)").children().attr("disabled", "disabled");
        },
        showConfirm: function () {
            $("#FlowConfirm").val("0");
            $(".procflow-confirm").show();
            $(".procflow-modify").hide();
            $("#tb-ProcessFlow tr").find("td:eq(0),th:eq(0)").show();
            $("#tb-ProcessFlow tr").find("td:gt(1)").children().removeAttr("disabled", "disabled");
        }
    },
    bom: {
        showQuery: function (obj) {
            var trObj = $(obj).parent().parent();
            $.cost.common.showQuery(obj, $(trObj).find("#MaterialType").val());
        },
        clearTR: function(trObj) {
            $(trObj).find("#MaterialDescription").val('');
            $(trObj).find("#ManualPrice").val('');
            $(trObj).find("#Amount").val('');
        },
        boundEvent: function() {
            $("#tb-BOM").find("select[name='MaterialType']").unbind('change').on('change', function (e) {
                $.cost.bom.clearTR($(this).parent().parent());
            });
        },
        setMaterialMapping: function() {
            $("#tb-BOM tr:gt(0)").each(function () {
                $.cost.bom.setTrMaterialMapping(this);
            });
        },
        setTrMaterialMapping: function (thisTr) {
            var mwc = $(thisTr).find("#MainWorkCenter").val();
            $.post(rootUrl + 'CostIO/GetWCBOMMapping', { pcbType: pcbType, workCenter: mwc }, function (data) {
                var mos = [];
                $(thisTr).find("#MaterialType option").each(function () {
                    mos.push({ name: $(this).text(), exists: 0 });
                });
                if (data != null) {
                    for (var i = 0; i < data.length; i++) {
                        for (var j = 0; j < mos.length; j++) {
                            if (mos[j].name.toUpperCase() == data[i]["Material"].toUpperCase()) {
                                mos[j].exists = 1;
                            }
                        }
                    }

                    for (var i = 0; i < mos.length; i++) {
                        if (mos[i].exists == 0) {
                            $(thisTr).find("#MaterialType option").each(function () {
                                if (mos[i].name.toUpperCase() == $(this).text().toUpperCase()) {
                                    $(this).remove();
                                }
                            });
                        }
                    }
                }
            }, "json");
        }
    },
    edm: {
        showQuery: function (obj, costItem) {
            $.cost.common.showQuery(obj, costItem, $.cost.edm.onQuerySelected)
        },
        onQuerySelected: function (options, status, rowData, costItem) {
            var trObj = options.trObj;
            trObj.find("#MaterialID").val(rowData["ID"]);
            trObj.find("#Size").val(rowData["Diameter"]);
            trObj.find("#ManualPrice").val(rowData["USDPrice"]);
            trObj.find("#MaterialDescription").val(rowData["Description"]);
            trObj.find("#Unit").val(rowData["PriceUnit"]);
        },
        setSMCMTrEnable: function (tr) {
            var mwc = $(tr).find("#MainWorkCenter").val();
            if (mwc == "COM") {
                $(tr).find("#Thickness").attr("disabled", "disabled");
                $(tr).find("#Area").attr("disabled", "disabled");
            } else {
                $(tr).find("#SidePanel").attr("disabled", "disabled");
            }
        }
    },
    spec: {
        showLASQuery: function (obj) {
            var trObj = $(obj).parent().parent();
            openQuery({ costItem: '', tableKey: 'SCLaserProcParams', onSelect: $.cost.spec.onLASSelected, multiselect: false, trObj: trObj })
        },
        onLASSelected: function (options, status, rowData, costItem) {
            var trObj = options.trObj;
            trObj.find("#MaterialID").val(rowData["ID"]);
            trObj.find("#Size").val(rowData["Diameter"]);
            trObj.find("#LaserType").val(rowData["Type"]);
            trObj.find("#LaserMethod").val(rowData["Method"]);
        },
        showLACQuery: function (obj) {
            var trObj = $(obj).parent().parent();
            openQuery({ costItem: '', tableKey: 'SCLaserCutingParams', onSelect: $.cost.spec.onLACSelected, multiselect: false, trObj: trObj })
        },
        onLACSelected: function (options, status, rowData, costItem) {
            var trObj = options.trObj;
            trObj.find("#MaterialID").val(rowData["ID"]);
            trObj.find("#LaserType").val(rowData["Type"]);
            trObj.find("#LaserMethod").val(rowData["Method"]);
        },
        setSpecEnable: function (categoryName) {
            if (pcbType == "FPC" && categoryName.indexOf("SCISMCMFPC") != -1) {
                $("#tb-SMCM tr:gt(0)").each(function (index) {
                    $.cost.edm.setSMCMTrEnable(this);
                });
            }

            if (categoryName.indexOf("SCSpeacialProcess") != -1) {
                $("#tb-SpeaProc tr:gt(0)").each(function (index) {
                    $.cost.spec.setSpecMaterial(this);
                });
            }
        },
        boundPUNEvent: function(categoryName) {
            if (categoryName == null || categoryName.indexOf("Punching") > 0) {
                $("#tb-Punching").find("input[name='PunchTimesPerPnl']").unbind('change').on('change', function (sender) {
                    var pt = $(this).val();
                    var tpt = '';
                    if ($.trim(pt) != '') {
                        tpt = 1 * $(this).parent().parent().find("#PunchTime").val() * pt;
                    }
                    $(this).parent().parent().find("#TotalCycleTime").val(tpt)
                });
            }
        },
        boundSpecEvent: function (categoryName) {
            if (categoryName == null || categoryName.indexOf("SCSpeacialProcess") > 0) {
                $("#tb-SpeaProc").find("select[name='Material']").unbind('change').on('change', function (sender) {
                    var tr = $(this).parent().parent();
                    $(tr).find("#Second").val($.cost.spec.getSpecMaterialDefault($(tr).find("#Material").val()));
                    $.cost.spec.calcSpecTime($(this).parent().parent());
                });

                $("#tb-SpeaProc").find("input[name='TimesPerPnl']").unbind('change').on('change', function (sender) {
                    $.cost.spec.calcSpecTime($(this).parent().parent());
                });

                $("#tb-SpeaProc").find("input[name='Second']").unbind('change').on('change', function (sender) {
                    $.cost.spec.calcSpecTime($(this).parent().parent());
                });
            }
        },
        setSpecMaterial: function (tr) {
            var mwc = $(tr).find("#MainWorkCenter").val();
            if (mwc != "ATT") {
                $(tr).find("#Material").parent().html('<input style="width:100% !important" id="Material" name="Material" type="text" class="form-field" value="" disabled="disabled">');
                $(tr).find("#TimesPerPnl").attr("disabled", "disabled");
            }
        },
        setSpecDefault: function (tr) {
            var mwc = $(tr).find("#MainWorkCenter").val();
            if (mwc == "ATT") {
                $(tr).find("#Unit").val("/time");
                $(tr).find("#Second").val($.cost.spec.getSpecMaterialDefault($(tr).find("#Material").val()));
            } else {
                $(tr).find("#Unit").val("/sqft");
                $(tr).find("#TimesPerPnl").val("1");
                switch (mwc) {
                    case "QC1":
                        $(tr).find("#Second").val("5");
                        break;
                    case "FQC":
                        $(tr).find("#Second").val("730");
                        break;
                    case "FQA":
                        $(tr).find("#Second").val("730");
                        break;
                    case "FIN":
                        $(tr).find("#Second").val("100");
                        break;
                }
            }
        },
        getSpecMaterialDefault: function (material) {
            switch (material) {
                case "Coverlay":
                    return "25";
                case "Shield Film":
                    return "6";
                case "Stiffener":
                    return "6";
                case "Adhesive":
                    return "8";
                case "Remove Unwanted Material":
                    return "8";
            }
        },
        calcSpecTime: function (tr) {
            var mwc = $(tr).find("#MainWorkCenter").val();
            var tct = '';
            var tpp = $(tr).find("#TimesPerPnl").val();
            var sec = $(tr).find("#Second").val();
            if (mwc == "ATT") {
                if ($.trim(tpp) != "" && $.trim(sec) != "") {
                    tct = 1 * tpp * sec / 60;
                    tct = tct.toFixed(4);
                }
            } else {
                var pfid = $(tr).find("#PFID").val();
                var pa = $("#tb-ProcessFlow tr:eq(" + pfid + ")").find("#PanelArea").val();
                if ($.trim(pa) == "") {
                    pa = $("#tabBasicInformation").find("#PanelArea").val();
                }
                if ($.trim(tpp) != "" && $.trim(sec) != "" && $.trim(pa) != "") {
                    tct = 1 * tpp * sec * pa / 60;
                    tct = tct.toFixed(4);
                }
            }
            
            $(tr).find("#TotalCycleTime").val(tct);
        },
        calcAllSpecTime: function () {
            $("#tb-SpeaProc tr").each(function () {
                $.cost.spec.calcSpecTime(this);
            });
        }
    }
}
