def dogYears(memory):
    return memory.age * 7

def catYears(age):
    return age * 15

def snakeYears(age):
    return age * 21

def returnObject():
    return { "num": 13, "str": "string", "complex": { "x": 3, "y": "y" } }

def returnArray():
    return ["x", "y", "z"]

def tryEval():
    return expression("add(arg1, arg2)", { "arg1": 15, "arg2": 35 })
