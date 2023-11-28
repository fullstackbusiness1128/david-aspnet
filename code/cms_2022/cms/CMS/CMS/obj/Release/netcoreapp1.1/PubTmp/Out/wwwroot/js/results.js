    $(function () {

        init_paging();
        function init_paging() {
            $('.pagination li a').on('click', function () {
                refresh(this, 1);
            });

            $('.btnsort').on('click', function () {
                refresh(this, 3);
            });
        }

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
            var pageNumber = 1;
            var pageSize = $("#results-table").data("size");
            var sortField = $("#results-table").data("sortfield");
            var sortOrder = $("#results-table").data("sortorder");
            var searchValue = $("#results-table").data("searchvalue");

            if (trigger === 1) {
                pageNumber = $(obj).data("page");
            } else if (trigger === 2) {
                searchValue = $("#search-email").val();
            } else if (trigger === 3) {
                sortField = $(obj).data("field");
                sortOrder = $(obj).data("order");
            } else if (trigger === 4) {
                pageSize = $(obj).text();
            }

            var datax = { "pageNumber": pageNumber, "pageSize": pageSize, "totalResults": "0", "sortOrder": sortOrder, "sortField": sortField, "searchValue": searchValue }; 
            
            $.ajax(endpoint, {
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
                    $('.resultspanel').html(datax);
                    $('.pagelink_' + pageNumber).addClass("active");
                    $('.fa-spinner').toggle().removeClass("fa-spin").removeClass("text-success");
                    if (trigger === 3) {
                        var sortField = $(obj).data("field");
                        var sortOrder = $(obj).data("order");

                        if (sortOrder === "asc") {
                            $("*[data-field='"+sortField+"']").attr("data-order", "desc");
                        } else {
                            $("*[data-field='" + sortField + "']").attr("data-order", "asc");
                        }
                    }
                    init_paging();
                },
                error: function () {
                    console.log("error");
                }
            });
        }
    })
