deviceOptionActionTypeChanged();

function openRemoveDeviceOptionActionQuestionDialog(actionIdToRemove) {
	let actionIdToRemoveInput = document.getElementById("ActionIdToRemove");

	actionIdToRemoveInput.value = actionIdToRemove;

	showDialogById("RemoveDeviceOptionActionQuestionDialog");
}

function deviceOptionActionTypeChanged() {
	let select = document.getElementById("DeviceOptionActionType");

	let editor = document.getElementById("DeviceOptionActionEditor");

	deviceOptionActionEditorSetVisibility(select.value, editor);
}

function deviceOptionActionEditorSetVisibility(selectedValue, editor) {
	for (child of editor.children) {
		if (child.id == selectedValue) {
			child.classList.remove("hidden");
		}
		else {
			child.classList.add("hidden");
		}
	}
}

function submitCreateDeviceOptionActionForm() {
	
	let form = document.getElementById("CreateDeviceOptionAction");

	let deviceOptionActionType = document.getElementById("DeviceOptionActionType").value;

	let deviceOptionActionTypeData = newDeviceOptionDataInformation[deviceOptionActionType];

	let actionData = {};

	Object.assign(actionData, deviceOptionActionTypeData);

	let dataContext = document.getElementById(deviceOptionActionType);

	bindToSourceWithPrefix(actionData, dataContext, "action");

	form["NewDeviceOptionActionData"].value = JSON.stringify(actionData);

	form.submit();
}