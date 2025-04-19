function testAjaxPost(data) {
    $.ajax({
        method: "POST",
        url: model.pageInfo.url + "?handler=TestAjaxPost",
        data: data,
        headers: { RequestVerificationToken: document.getElementById("RequestVerificationToken").value },
        success: function (responseModel) {
            model = responseModel;
            bindFromModel();
        }
    });
}