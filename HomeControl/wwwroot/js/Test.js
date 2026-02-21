function testAjaxPost(data) {
    $.ajax({
        method: "POST",
        url: pageInfo.url + "?handler=TestAjaxPost",
        data: data,
        headers: { RequestVerificationToken: document.getElementById("RequestVerificationToken").value },
        success: function (responseModel) {
            model = responseModel;
            bindFromModel();
        }
    });
}