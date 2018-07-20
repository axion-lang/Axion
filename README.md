# Axion programming language toolset <img align="right" src="https://github.com/F1uctus/Axion/blob/master/Other/Axion_logo.png" width="152" height="152" />

## **Under construction**

## Objectives:
- **General-purposed**
- **Static typed**
- **High-performance**
- **Safe**
- **Simple**
- **Readable**

## Paradigms (alpha):
- Object-oriented (built-in)
- Reactive
- Functional (extension)

## Progress:
- **Lexer (Tokenizer) works mostly correct**
- **Parser is still under construction.**
	_(Not working by last release time)_
	
	Now parser supports:
	- Binary operations:
	```python
	1 + 2, variable += "string", otherVar = 90.0, etc.
	```
	- Branching (if, elif, else):
	```python 
	if condition: 
		doSomething()
	elif condition: 
		doSomethingElse()
	else: call()
	```
	- Collections indexers:
	```csharp
	// invoke same way as object's field
	item = collection.index
	item = collection.0
	value = someMap."key"
	```
	- Collections initializing (partly):
	```csharp
	// inaccurate version yet
	//
	// ARRAY
	collection = int[5]
	collection = { 1, 2, 3, 4, 5 }
	collection = int { 1, 2, 3, 4, 5 }
	//
	// MATRIX
	collection = int[3, 2]
	collection = { {1, 2}, {3, 4}, {5, 6} }
	collection = int { {1, 2}, {3, 4}, {5, 6} }
	//
	// LIST
	collection = int[*]
	collection = int* { 1, 2, 3, 4, 5 }
	//
	// MAP
	collection = { int, str }
	collection = { 1: "Text1", 2: "Hello, world!", 55: "Other string" }
	```
	- Function calls (partly):
	```csharp
	call(arg1, arg2, arg3)
	functionCall(nestedCall(arg1, arg2), 12345)
	```
- **Next thing will be interpreter**
- **Then translator to C or C++**
