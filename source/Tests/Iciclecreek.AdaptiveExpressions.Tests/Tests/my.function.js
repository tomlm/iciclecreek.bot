// Export functions
let exports = {};
exports.dogYears = dogYears;
exports.catYears = catYears;
exports.snakeYears = snakeYears;
exports.returnObject = returnObject;
exports.returnArray = returnArray;
exports.tryEval = tryEval;

function dogYears(memory) {
    return memory.age * 7;
}

function catYears(age) {
    return age * 15;
}

function snakeYears(age) {
    return age * 21;
}

function returnObject() {
    return {
        "num": 13,
        "str": "string",
        "complex": {
            "x": 3,
            "y": "y"
        }
    };
}

function returnArray() {
    return [
        "x",
        "y",
        "z"
    ];
}

function tryEval() {
    // evaluate adaptive expression...
    return expression("add(arg1, arg2)", { "arg1": 15, "arg2": 35 });
}
