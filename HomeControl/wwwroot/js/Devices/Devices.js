function executeDeviceFeature(dataset) {
    let data = {};

    for ([targetName, boundPropertyValue] of iterateBoundPropertiesWithPrefix(model, Object.entries(dataset), clickBindingPrefix)) {
        Object.defineProperty(data, targetName, { value: boundPropertyValue, enumerable: true });
    }

    $.ajax({
        method: "POST",
        url: model.pageInfo.url + "?handler=ExecuteFeature",
        data: data,
        headers: { RequestVerificationToken: document.getElementById("RequestVerificationToken").value },
        success: function (responseModel) {
            model = responseModel;
            rebind();
        }
    });
}