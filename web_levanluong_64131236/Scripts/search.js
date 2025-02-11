$(document).ready(function () {
    $("#searchInput").autocomplete({
        source: function (request, response) {
            $.ajax({
                url: "/HangHoas_64131236/GetSearchSuggestions",
                dataType: "json",
                data: { term: request.term },
                success: function (data) {
                    response(data);
                },
                error: function (xhr, status, error) {
                    console.error("Search error:", error);
                    response([]);
                }
            });
        },
        minLength: 1,
        select: function (event, ui) {
            if (ui.item && ui.item.id) {
                window.location.href = '/HangHoas_64131236/Details/' + ui.item.id;
                return false;
            }
        }
    }).autocomplete("instance")._renderItem = function (ul, item) {
        return $("<li>")
            .append(`<div class='product-suggestion'>
                <img src='/img/${item.image}' alt='${item.label}'
                     style='width:50px;height:50px;object-fit:cover;margin-right:10px'>
                <div>
                    <div style='font-weight:500'>${item.label}</div>
                    <div style='color:#ee4d2d'>${item.price.toLocaleString('vi-VN')}₫</div>
                </div>
            </div>`)
            .appendTo(ul);
    };

    $("#globalSearchForm").on("submit", function (e) {
        e.preventDefault();
        var searchTerm = $("#searchInput").val().trim();
        if (searchTerm) {
            window.location.href = '/HangHoas_64131236/ProductList?searchString=' + encodeURIComponent(searchTerm);
        }
    });
});