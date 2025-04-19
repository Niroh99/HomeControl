const bindingPrefix = "binding";
const clickBindingPrefix = "clickbinding";
const propertySeparator = ".";

function correctCasing(value) {
    return value.charAt(0).toLowerCase() + value.slice(1);
}

function getPropertyValueRecursive(source, pathElements) {
    if (pathElements.length == 0) return source;

    var pathElement = pathElements.shift();

    let indexerIndex = pathElement.indexOf("[");

    let propertyName;

    if (indexerIndex > 0) {
        propertyName = pathElement.substring(0, indexerIndex);

        let index = pathElement.substring(indexerIndex + 1, pathElement.indexOf("]"));

        pathElements.splice(0, 0, index);
    }
    else propertyName = pathElement;

    return getPropertyValueRecursive(source[correctCasing(propertyName)], pathElements);
}

function resolveBinding(source, sourcePropertyPath) {
    let pathElements = sourcePropertyPath.split(propertySeparator);

    return getPropertyValueRecursive(source, pathElements);
}

function* iterateBoundPropertiesWithPrefix(source, dataset, prefix) {
    for (let [datasetPropertyName, sourcePropertyPath] of dataset) {
        if (datasetPropertyName.startsWith(prefix)) {
            let boundPropertyValue = resolveBinding(source, sourcePropertyPath);

            let targetAttributeName = datasetPropertyName.substring(prefix.length);
            targetAttributeName = correctCasing(targetAttributeName);

            yield[targetAttributeName, boundPropertyValue, sourcePropertyPath];
        }
    }
}

function* iterateBoundProperties(source, dataset) {
    yield* iterateBoundPropertiesWithPrefix(source, dataset, bindingPrefix);
}

function bindFromSource(source, context) {
    let elements = context.getElementsByClassName("bound");

    for (element of elements) {
        for ([targetAttributeName, boundPropertyValue] of iterateBoundProperties(source, Object.entries(element.dataset))) {
            console.log([targetAttributeName, boundPropertyValue]);
            if (targetAttributeName == "inner") element.innerHTML = boundPropertyValue;
            else element[targetAttributeName] = boundPropertyValue;
        }
    }
}

function setPropertyValueRecursive(target, pathElements, newValue) {
    var pathElement = pathElements.shift();

    let indexerIndex = pathElement.indexOf("[");

    let propertyName;

    if (indexerIndex >= 0) {
        propertyName = pathElement.substring(0, indexerIndex);

        let index = pathElement.substring(indexerIndex + 1, pathElement.indexOf("]"));

        pathElements.splice(0, 0, index);
    }
    else {
        propertyName = pathElement;

        if (pathElements.length == 0) {
            target[correctCasing(propertyName)] = newValue;
            return;
        }
    }

    setPropertyValueRecursive(target[correctCasing(propertyName)], pathElements, newValue);
}

function bindToSource(source, context) {
    bindToSourceWithPrefix(source, context, bindingPrefix);
}

function bindToSourceWithPrefix(source, context, prefix) {
    let elements = context.getElementsByClassName("bound");

    for (element of elements) {
        for ([targetAttributeName, boundPropertyValue, sourcePropertyPath] of iterateBoundPropertiesWithPrefix(source, Object.entries(element.dataset), prefix)) {
            let pathElements = sourcePropertyPath.split(propertySeparator);

            if (targetAttributeName == "inner") setPropertyValueRecursive(source, pathElements, element.innerHTML);
            else setPropertyValueRecursive(source, pathElements, element[targetAttributeName]);
        }
    }
}