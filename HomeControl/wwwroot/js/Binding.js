const bindingPrefix = "binding";
const clickBindingPrefix = "clickbinding";

function resolveBinding(source, sourcePropertyPath) {
    let pathElements = sourcePropertyPath.split(".");

    return getPropertyValueRecursive(source, pathElements);
}

function getPropertyValueRecursive(source, pathElements) {
    if (pathElements.length == 0) return source;

    var pathElement = pathElements.shift();

    let indexerIndex = pathElement.indexOf("[");

    if (indexerIndex > 0) {
        let propertyName = pathElement.substring(0, indexerIndex);

        let index = parseInt(pathElement.substring(indexerIndex + 1, pathElement.indexOf("]")));

        let arrayValue = Object.getOwnPropertyDescriptor(source, propertyName).value;

        return getPropertyValueRecursive(arrayValue[index], pathElements);
    }

    let value = Object.getOwnPropertyDescriptor(source, pathElement).value;

    return getPropertyValueRecursive(value, pathElements);
}

function* iterateBoundPropertiesWithPrefix(source, dataset, prefix) {
    for (let [datasetPropertyName, sourcePropertyPath] of dataset) {
        if (datasetPropertyName.startsWith(prefix)) {
            let boundPropertyValue = resolveBinding(source, sourcePropertyPath);

            let targetName = datasetPropertyName.substring(prefix.length);
            targetName = targetName.charAt(0).toLowerCase() + targetName.slice(1);

            yield[targetName, boundPropertyValue];
        }
    }
}

function* iterateBoundProperties(source, dataset) {
    yield* iterateBoundPropertiesWithPrefix(source, dataset, bindingPrefix);
}

function rebind() {
    let elements = document.getElementsByClassName("bound");

    for (element of elements) {
        for ([targetAttributeName, boundPropertyValue] of iterateBoundProperties(model, Object.entries(element.dataset))) {
            if (targetAttributeName == "inner") element.innerHTML = boundPropertyValue;
            else element.setAttribute(targetAttributeName, boundPropertyValue);
        }
    }
}