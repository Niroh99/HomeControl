routineTriggerTypeChanged();
routineActionTypeChanged();
routineActionDeviceChanged();

function openRemoveRoutineTriggerQuestionDialog(triggerIdToRemove) {
	let triggerIdToRemoveInput = document.getElementById("TriggerIdToRemove");

	triggerIdToRemoveInput.value = triggerIdToRemove;

	showDialogById("RemoveRoutineTriggerQuestionDialog");
}

function routineTriggerTypeChanged() {
	let select = document.getElementById("RoutineTriggerType");

	let templateContainer = document.getElementById("RoutineTriggerDataTemplateContainer");

	templateContainer.textContent = "";

	let template = document.getElementById(select.value + "Template");

	if (template == undefined || template == null) return;

	let templateContent = template.content.cloneNode(true);

	templateContainer.appendChild(templateContent);

	weekDaySelectionChanged();
}

function weekDaySelectionChanged() {
	let weekDaySelectionInput = document.getElementById("WeekDaySelection");

	if (weekDaySelectionInput == undefined || weekDaySelectionInput == null) return;

	let newValue = [];

	if (document.getElementById("Monday").checked) newValue.push(1);
	if (document.getElementById("Tuesday").checked) newValue.push(2);
	if (document.getElementById("Wednesday").checked) newValue.push(3);
	if (document.getElementById("Thursday").checked) newValue.push(4);
	if (document.getElementById("Friday").checked) newValue.push(5);
	if (document.getElementById("Saturday").checked) newValue.push(6);
	if (document.getElementById("Sunday").checked) newValue.push(0);

	weekDaySelectionInput["value"] = JSON.stringify(newValue);
}

function inputTimeToTimeOnly(time) {
	return `${time}:00`;
}

function minutesToTimespan(minutes) {
	return `00:${minutes.padStart(2, "0")}:00`;
}

function submitCreateRoutineTriggerForm() {
	let form = document.getElementById("CreateRoutineTrigger");

	let routineTriggerType = document.getElementById("RoutineTriggerType").value;

	let routineTriggerTypeData = newRoutineTriggerDataInformation[routineTriggerType];

	let triggerData = {};

	Object.assign(triggerData, routineTriggerTypeData);

	let dataContext = document.getElementById(routineTriggerType);
	
	bindToSourceWithPrefix(triggerData, dataContext, "trigger");

	form["NewRoutineTriggerData"].value = JSON.stringify(triggerData);

	form.submit();
}

function openRemoveRoutineActionQuestionDialog(actionIdToRemove) {
	let actionIdToRemoveInput = document.getElementById("ActionIdToRemove");

	actionIdToRemoveInput.value = actionIdToRemove;

	showDialogById("RemoveRoutineActionQuestionDialog");
}

function routineActionTypeChanged() {
	let select = document.getElementById("ActionType");

	let templateContainer = document.getElementById("RoutineActionDataTemplateContainer");

	templateContainer.textContent = "";

	let template = document.getElementById(select.value + "Template");

	if (template == undefined || template == null) return;

	let templateContent = template.content.cloneNode(true);

	templateContainer.appendChild(templateContent);
}

function routineActionDeviceChanged() {
	let deviceSelect = document.getElementById("NewActionDeviceId");

	let featureNameSelect = document.getElementById("FeatureName");

	if (featureNameSelect == undefined || featureNameSelect == null) return;

	for (let i = 0; i < featureNameSelect.options.length; i++) featureNameSelect.options.remove(featureNameSelect.options.length - 1);

	let deviceId = deviceSelect.value;

	let device = model.devices.find((deviceInfo) => deviceInfo.device.id == deviceId);

	if (device == null) return;

	for (let deviceFeature of device.integrationDevice.features) {
		let option = document.createElement("option");
		option.setAttribute("value", deviceFeature.name);
		option.innerText = deviceFeature.name;

		featureNameSelect.options.add(option);
	}
}

function submitCreateRoutineActionForm() {
	console.log("submit");
	let form = document.getElementById("CreateRoutineAction");

	let routineActionType = document.getElementById("ActionType").value;

	let routineActionTypeData = newRoutineActionDataInformation[routineActionType];

	let actionData = {};

	Object.assign(actionData, routineActionTypeData);

	let dataContext = document.getElementById(routineActionType);

	console.log(dataContext);

	bindToSourceWithPrefix(actionData, dataContext, "action");

	console.log(actionData);

	form["NewRoutineActionData"].value = JSON.stringify(actionData);

	form.submit();
}