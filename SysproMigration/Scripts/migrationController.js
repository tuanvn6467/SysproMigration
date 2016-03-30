
(function (window, $) {
    //Define plugins
    $.fn.migrationController = function (options) {
        var defaultOptions = {};
        // Establish our default settings
        var settings = $.extend(defaultOptions, options);
        $.extend(migrationController, settings);
        return this.each(function () {
            //code here
        });
    };
    window.migrationController = {
        ui: {
            btnSourceGetListCompany: '#btnSourceGetListCompany',
            txtSourceServerName: '#SourceServerName',
            txtSourceUserLogin: '#SourceUserLogin',
            txtSourceUserPassword: '#SourceUserPassword',

            dbSourceCompany: '#dbSourceCompany',

            dbDestinationCompany: '#dbDestinationCompany',

            btnMigrate: '#btnMigrate',
            checkboxSecuritySystem: '#checkboxSecuritySystem',
            divSecuritySystem: '.divSecuritySystem',
            fromdate: '#fromdate',
            todate: '#todate',
            progressbar: '#progressbar',
            progressLabel: '.progress-label',

            btnSourceCheckExist: '#btnSourceCheckExist',
            btnDestinationCheckExist: '#btnDestinationCheckExist',
            btnDestinationGetListCompany: '#btnDestinationGetListCompany',

            txtDestinationUserLogin: '#DestinationUserLogin',
            txtDestinationUserPassword: '#DestinationUserPassword',
            IsMigrateCustomData: "#IsMigrateCustomData"
        },
        totalTableMigrateCount: 0,
        tableMigratedCount: 0,
        progressTimer: {},
        logFileDownload: null,
        logFileName: null,
        loadLogInteraval: -1,
        init: function () {
            var ui = migrationController.ui;
            var dialogButtons =
                [
                    {
                        text: "Cancel Migrate",
                        disabled: true,
                    }
                ];

            var dialog = $("#dialog").dialog({
                autoOpen: false,
                closeOnEscape: false,
                resizable: false,
                buttons: dialogButtons,
                open: function () {
                    $(".ui-dialog-titlebar-close", ui.dialog | ui).hide();
                    migrationController.progressTimer = setTimeout(migrationController.progress, 2000);
                },
                beforeClose: function () {
                    $(ui.btnMigrate).removeAttr('disabled');
                }
            });
            $(ui.btnDestinationGetListCompany).on('click', function() {
                $.ajax({
                    url: '/Migration/GetListCompanyDestination',
                    type: "POST",
                    data: {
                        user: $(ui.txtDestinationUserLogin).val(),
                        password: $(ui.txtDestinationUserPassword).val()
                    },
                    success: function (result) {
                        var htmlDestinationLstCompany = "";
                        for (var i = 0; i < result.lstCompany.length ; i++) {
                            htmlDestinationLstCompany += "<option value='" + result.lstCompany[i].DatabaseID + "' physicalName='" + result.lstCompany[i].PhysicalName + "' tennantID='" + result.lstCompany[i].TenantID + "'>" + result.lstCompany[i].DatabaseName + "</option>";
                        }
                        $(ui.dbDestinationCompany).html(htmlDestinationLstCompany);
                    },
                    error: function (e) {
                    }
                });
            });
            $(ui.btnSourceGetListCompany).on('click', function () {
                $.ajax({
                    url: '/Migration/GetListCompanySource',
                    type: "POST",
                    data: {
                        sqlServerName: $(ui.txtSourceServerName).val()
                    },
                    success: function (result) {
                        var htmlSourceLstCompany = "";
                        for (var i = 0; i < result.lstCompany.length ; i++) {
                            htmlSourceLstCompany += "<option value='" + result.lstCompany[i].DatabaseID + "'>" + result.lstCompany[i].DatabaseName + "</option>";
                        }
                        $(ui.dbSourceCompany).html(htmlSourceLstCompany);
                    },
                    error: function (e) {
                    }
                });
            });

            $(ui.btnSourceCheckExist).on('click', function() {
                migrationController.checkDatabaseExist(
                    $(ui.dbSourceCompany + ' option:selected').text(),
                    '',
                    true,
                    function () {
                        $(ui.dbSourceCompany).val('');
                    });
            });
            $(ui.btnDestinationCheckExist).on('click', function () {
                migrationController.checkDatabaseExist(
                    $(ui.dbDestinationCompany + ' option:selected').text(),
                    $(ui.dbDestinationCompany + ' option:selected').attr('physicalName'),
                    false,
                    function () {
                        $(ui.dbDestinationCompany).val('');
                    });
            });
            
            $(ui.btnMigrate).on('click', function () {
                migrationController.migrateData(false, dialog);
            });
            $(ui.fromdate).datepicker({
                /*showOn: "button",
                buttonImage: "images/calendar.gif",
                buttonImageOnly: true,
                buttonText: "Select date"*/

            });
            $(ui.todate).datepicker({
                /*showOn: "button",
                buttonImage: "Images/calendar.gif",
                buttonImageOnly: true,
                buttonText: "Select date"*/
                onSelect: function (value) {
                    var dateFrom = $(ui.fromdate).val();
                    if (dateFrom != '') {
                        var fromDate = new Date(dateFrom);
                        if (new Date(value) < fromDate) {
                            alert("To Date can't less than From Date");
                            $(ui.todate).val('');
                        }
                    }
                }
            });

        },
        migrateData: function (isConvertAgain, dialog) {
            var ui = migrationController.ui;
            if (migrationController.validateBeforeMigrate()) {
                $.ajax({
                    url: '/Migration/MigrateData',
                    type: "POST",
                    data: {
                        sourceSQLServerName: $(ui.txtSourceServerName).val(),
                        sourceDb: $(ui.dbSourceCompany + ' option:selected').text(),
                        targetDb: $(ui.dbDestinationCompany + ' option:selected').attr("physicalName"),
                        userName: $(ui.txtDestinationUserLogin).val(),
                        userPassword: $(ui.txtDestinationUserPassword).val(),
                        tennantID: $(ui.dbDestinationCompany + ' option:selected').attr("tennantID"),
                        databaseID: $(ui.dbDestinationCompany + ' option:selected').val(),
                        isMigrateCustomData:$(ui.IsMigrateCustomData).is(":checked"),
                        convertAgain: isConvertAgain
                    },
                    beforeSend: function() {
                        migrationController.tableMigratedCount = 0;
                        $(ui.btnMigrate).attr('disabled', 'disabled');
                        $(ui.progressbar).progressbar({
                            value: false,
                            change: function() {
                                $(ui.progressLabel).text("Current Progress: " + $(ui.progressbar).progressbar("value") + "%");
                            },
                            complete: function() {
                                $(ui.progressLabel).text("Complete!");
                                dialog.dialog("option", "buttons", [
                                   {
                                       text: "Close",
                                       click: migrationController.closeMigrate
                                   }
                                ]);
                                $(".ui-dialog button").last().focus();
                            }
                        });
                        dialog.dialog("open");
                    },
                    success: function(result) {
                        if (result.Status <= 0) {
                            if (result.IsMigrated) {
                                if (confirm("Source company and Target company is converted, Click 'OK' to convert againt and target database to be wipe out")) {
                                    migrationController.migrateData(true, dialog);
                                } else {
                                    dialog.dialog("close");
                                    clearInterval(migrationController.loadLogInteraval);
                                }
                            } else {
                                alert(result.Message);
                                dialog.dialog("close");
                            }
                        }
                    },
                    error: function(e) {
                    }
                });
                if (!isConvertAgain)
                migrationController.loadLogInteraval = setInterval(migrationController.getTableMigratedCount, 5000);
            }
        },
        validateBeforeMigrate: function () {
            var ui = migrationController.ui;
            var check1 = true;
            var check2 = true;
            if ($(ui.txtSourceServerName).val() == '') {
                alert('Please input SQL Server Name');
                return false;
            }
            if ($(ui.dbSourceCompany).val() == '' || $(ui.dbSourceCompany).val() == null) {
                alert('Please select Source Company ');
                return false;
            } else {
                check1 =
                migrationController.checkDatabaseExist(
                    $(ui.dbSourceCompany + ' option:selected').text(),
                    '',
                    true,
                    function() {
                        $(ui.dbSourceCompany).val('');
                    },
                    false);
            }
            if ($(ui.txtDestinationUserLogin).val() == '') {
                alert('Please input User Login');
                return false;
            }
            if ($(ui.txtDestinationUserPassword).val() == '') {
                alert('Please input User Password');
                return false;
            }
            if ($(ui.dbDestinationCompany).val() == '' || $(ui.dbDestinationCompany).val() == null) {
                alert('Please select Destination Company');
                return false;
            } else {
                check2 =
                migrationController.checkDatabaseExist(
                    $(ui.dbDestinationCompany + ' option:selected').text(),
                    $(ui.dbDestinationCompany + ' option:selected').attr('physicalName'),
                    false,
                    function () {
                        $(ui.dbDestinationCompany).val('');
                    },
                    false);
            }
            return check1 && check2;
        },
        checkDatabaseExist: function (dbSelected, physicalName, fromSource, functionPrivate, isCheckByButton) {
            var resultCheck = false;
            var ui = migrationController.ui;
            isCheckByButton = isCheckByButton == undefined ? true : isCheckByButton;
            $.ajax({
                url: '/Migration/CheckExistDatabase',
                type: "POST",
                data: {
                    dbSelected: dbSelected,
                    sqlServerName: $(ui.txtSourceServerName).val(),
                    fromSource: fromSource,
                    physicalName: physicalName,
                },
                async: false,
                success: function (result) {
                    var isExist = result.IsExist;
                    if (!isExist) {
                        alert('Physical ' + (fromSource ? 'Source' : 'Destination') + ' database is not exist');
                        if (functionPrivate != undefined) {
                            functionPrivate();
                        }
                        resultCheck = false;
                    } else {
                        if (result.IsHasEmptyEmail) {
                            alert('User ' + result.UserEmptyEmail + " has empty email address. Please fill it in legacy first");
                            resultCheck = false;
                        } else {
                            if (isCheckByButton) {
                                alert('Connection Successful');
                            }
                            resultCheck = true;
                        }
                    }
                },
                error: function (e) {
                }
            });
            return resultCheck;
        },
        getTableMigratedCount: function () {
            var ui = migrationController.ui;
            $.ajax({
                url: '/Migration/GetPercentageDone',
                data: {
                    sourceDb: $(ui.dbSourceCompany + ' option:selected').text(),
                    targetDb: $(ui.dbDestinationCompany + ' option:selected').attr("physicalName"),
                    serverName: $(ui.txtSourceServerName).val()
                },
                async:false,
                success: function (response) {
                    migrationController.tableMigratedCount = response.TableMigratedCount;
                    if (response.TableMigratedCount == migrationController.totalTableMigrateCount) {
                        //
                    }
                    if (response.Success) {
                        clearInterval(migrationController.loadLogInteraval);
                        migrationController.logFileDownload = response.fileDownload;
                        migrationController.logFileName = response.filename;
                    }
                    if (response.Error) {
                        alert('Migrate Error, please check again or contact admin');
                        $("#dialog").dialog("close");
                        clearInterval(migrationController.loadLogInteraval);
                    }
                },
                error: function(e) {
                }
            });
        },
        progress: function () {
            var ui = migrationController.ui;
            var val = $(ui.progressbar).progressbar("value") || 0;

            var mathPlus = Math.floor(Math.random() * 3);
            if(val + mathPlus <= (migrationController.tableMigratedCount/migrationController.totalTableMigrateCount) * 100)
            $(ui.progressbar).progressbar("value", val + mathPlus);

            if (val <= 99) {
                migrationController.progressTimer = setTimeout(migrationController.progress, 50);
            }
        },
        closeMigrate: function () {
            var ui = migrationController.ui;
            clearTimeout(migrationController.progressTimer);
            var dialogButtons = [
                {
                    text: "Cancel Migrate",
                    click: migrationController.closeMigrate
                }
            ];
            $("#dialog").dialog("option", "buttons", dialogButtons)
              .dialog("close");
            window.location =
                "/Download/ToolExportFileNew" + "?title=" + migrationController.logFileDownload + "&fileName=" + migrationController.logFileName;
            $(ui.progressbar).progressbar("value", false);
            $(ui.progressLabel)
              .text("Starting Migrate...");
            //downloadButton.focus();
        }
    };
})(window, jQuery);