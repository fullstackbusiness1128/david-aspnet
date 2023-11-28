$(function () {

    init_paging();
    init_paging_history();
    function init_paging() {
        $('.pagination.current li a').on('click', function () {
            refresh(this, 1);
        });

        $('.btnsort').on('click', function () {
            refresh(this, 3);
        });

        $("[id^='show_']").click(function () {
            loadlog(this);
        });
    }

    function init_paging_history() {
        $('.pagination.history li a').on('click', function () {
            refreshhistory(this);
        });
    }

    $('.resultfilter').on('change', function () {
        refresh(this, 5);
    });

    $('#search-filtera').on('change', function () {
        refresh(this, 6);
    });

    $('.custom-search-form input[type="text"]').keypress(function (e) {
        if (e.which === 13) {
            refresh(this, 2);
        }
    });

    $('.custom-search-form .btn').click(function (e) {
        refresh(this, 2);
    });

    $(".dropdown-menu li a").click(function () {
        $("button.dropdown-toggle span.info").text($(this).text());
        $("#results-table").data("size", $(this).text());
        refresh(this, 4);
    });

    function refresh(obj, trigger) {

        var forgeryId = $('[name="__RequestVerificationToken"]').val();
        var userid = $("input[name = 'userid']").val();

     

        var pageNumber = 1;
        var pageSize = $("#results-table").data("size");
        var sortField = $("#results-table").data("sortfield");
        var sortOrder = $("#results-table").data("sortorder");
        var searchValue = $("#results-table").data("searchvalue");
        var searchField = $("#results-table").data("searchfield");
        var groupby = $("#results-table").data("groupby");
        var datefrom = $("#results-table").data("datefrom");
        var dateto = $("#results-table").data("dateto");
        var filtera = $("#results-table").data("filtera");
        var filterb = $("#results-table").data("filterb");
        var filterc = $("#results-table").data("filterc");

        if (trigger === 1) {
            pageNumber = $(obj).data("page");
        } else if (trigger === 2) {
            searchValue = $("#search-email").val();
        } else if (trigger === 3) {
            sortField = $(obj).data("field");
            sortOrder = $(obj).data("order");
        } else if (trigger === 4) {
            pageSize = $(obj).text();
        } else if (trigger === 5) {
            groupby = $(obj).val();
        } else if (trigger === 6) {
            filtera = $(obj).val();
        }

        var datax = { "Results": { "pageNumber": pageNumber, "pageSize": pageSize, "totalResults": "0", "sortOrder": sortOrder, "sortField": sortField, "searchValue": searchValue, "searchField": searchField }, "Filter": { "userid": userid, "GroupBy": groupby, "DateFrom": datefrom, "DateTo": dateto, "FilterA": filtera, "FilterB": filterb, "FilterC": filterc } };

        $.ajax(endpoint, {
            method: "POST",
            data: JSON.stringify(datax),
            contentType: 'application/json; charset=utf-8',
            headers: {
                'RequestVerificationToken': forgeryId
            },
            beforeSend: function () {
                $('.fa-spinner').css('display', 'inline-block').addClass("fa-spin");
            },
            success: function (datax) {
                $('.resultspanel').html(datax);

                // _UserResults.cshtml
                configure();

                function configure() {
                    $("[id^='Status_']").click(function () {
                        if ($(this).prop('checked') == true) {
                            $(this).prop('checked', true);
                        } else {
                            $(this).prop('checked', false);
                        }
                        ToggleStatus($(this).data("id"));
                    });
                }

                function ToggleStatus(id) {
                    var forgeryId = $('[name="__RequestVerificationToken"]').val();
                    var datax = { "Id": id };
                    $.ajax("/resellers/status", {
                        method: "POST",
                        data: JSON.stringify(datax),
                        contentType: 'application/json; charset=utf-8',
                        headers: {
                            'RequestVerificationToken': forgeryId
                        },
                        success: function (datax) {
                        },
                        error: function () {
                        }
                    });
                }



                $('select[name="reseller_status"]').change(function () {
                    var forgeryId = $('[name="__RequestVerificationToken"]').val();
                    var user_id = $(this).parent().parent().data('id');
                    var status = $(this).val();
                    var data = { "Id": user_id, 'DocStatus': status }

                    if (status) {
                        $.ajax({
                            url: 'resllers/doc_status',
                            method: 'post',
                            data: JSON.stringify(data),
                            contentType: 'application/json; charset=utf-8',
                            headers: {
                                'RequestVerificationToken': forgeryId
                            },
                            success: function (datax) {
                                if (datax != null) {
                                    // alert('success');
                                    if (datax.status === 1) {
                                        // green status
                                        $('#Status_' + user_id).prop("checked", true);

                                    } else {
                                        // default status
                                        $('#Status_' + user_id).prop("checked", false);

                                    }
                                } else {
                                    alert("fail");
                                }
                            },
                            error: function () {
                                alert("fail");
                            }
                        });
                    }

                });

                // End using in _UserResults.cshtml

                $('.pagelink_' + pageNumber + '.current').addClass("active");
                $('.fa-spinner').css('display', 'none').removeClass("fa-spin");
                if (trigger === 3) {
                    var sortField = $(obj).data("field");
                    var sortOrder = $(obj).data("order");

                    if (sortOrder === "asc") {
                        $("*[data-field='" + sortField + "']").attr("data-order", "desc");
                    } else {
                        $("*[data-field='" + sortField + "']").attr("data-order", "asc");
                    }
                }
                init_paging();
                InitActions(pageNumber);
            },
            error: function () {
                console.log("error");
            }
        });
    }

    function refreshhistory(obj) {

        var forgeryId = $('[name="__RequestVerificationToken"]').val();
        var pageSize = $("#history-table").data("size");
        var searchValue = $(".historypanel").data("searchvalue");
        pageNumber = $(obj).data("page");

        var datax = { "Results": { "pageNumber": pageNumber, "pageSize": pageSize, "totalResults": "0", "searchValue": searchValue }, "Identifier": searchValue };

        $.ajax(endpointhistory, {
            method: "POST",
            data: JSON.stringify(datax),
            contentType: 'application/json; charset=utf-8',
            headers: {
                'RequestVerificationToken': forgeryId
            },
            beforeSend: function () {
                $('.fa-spinner').toggle().addClass("fa-spin").addClass("text-success");
            },
            success: function (datax) {
                $('.historypanel').html(datax);
                $('.pagelink_' + pageNumber + '.history').addClass("active");
                $('.fa-spinner').toggle().removeClass("fa-spin").removeClass("text-success");
                init_paging_history();
            },
            error: function () {
                console.log("error");
            }
        });
    }

    $(".export").click(function () {
        exportdata();
    });

    function exportdata() {

        var forgeryId = $('[name="__RequestVerificationToken"]').val();
        var pageNumber = 1;
        var pageSize = 10000;

        window.location.href = endpointExp + '?p=' + pageNumber + '&s=' + pageSize;
    }
});
