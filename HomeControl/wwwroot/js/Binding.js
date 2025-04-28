const bindingPrefix = "binding";
const propertySeparator = ".";
const converterFunctionSeparator = ";";

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
            let converterFunctionIndex = sourcePropertyPath.indexOf(converterFunctionSeparator)

            let converterFunctionName = null;

            if (converterFunctionIndex >= 0) {
                converterFunctionName = sourcePropertyPath.substring(converterFunctionIndex + 1);
                sourcePropertyPath = sourcePropertyPath.substring(0, converterFunctionIndex);
                console.log(sourcePropertyPath);
            }

            let boundPropertyValue = resolveBinding(source, sourcePropertyPath);

            let targetAttributeName = datasetPropertyName.substring(prefix.length);
            targetAttributeName = correctCasing(targetAttributeName);

            yield[targetAttributeName, boundPropertyValue, sourcePropertyPath, converterFunctionName];
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
        for ([targetAttributeName, boundPropertyValue, sourcePropertyPath, converterFunctionName] of iterateBoundPropertiesWithPrefix(source, Object.entries(element.dataset), prefix)) {
            let pathElements = sourcePropertyPath.split(propertySeparator);

            let value;

            if (targetAttributeName == "inner") value = element.innerHTML;
            else value = element[targetAttributeName];

            if (converterFunctionName != null) {
                value = window[converterFunctionName](value);
            }

            setPropertyValueRecursive(source, pathElements, value);
        }
    }
}

function stringToJson(string) {
    return JSON.parse(string);
}