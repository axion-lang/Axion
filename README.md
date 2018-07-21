<img align="center" src="Other/Axion_Mini.png" />

<h2 align="center">Welcome to Axion programming language toolset</h2>
<h3 align="center">:\:\:\:\:\:\:\:\:\: Under construction :/:/:/:/:/:/:/:/:/:</h3>

<h3 align="center">Objectives</h3>

- **General-purposed**
- **High-performance**
- **Static typed**
- **Safe**
- **Simple**
- **Readable**

<h3 align="center">Paradigms</h3>

- **Object-oriented**
- **Reactive**
- **Functional (extension)**

<h3 align="center">Progress</h3>

- **Console interactive interpreter**
- **Lexer (Tokenizer)**
- **Parser is still under construction.**
	*(Not working by last release time)*
- **Transpiling to C or C++ is planned**
	
<h3 align="center">Syntax examples</h3>

- ***Basic operations***
	```python
	1 + 2, variable += "string", otherVar = 90.0, etc.
	```
- ***Branching***
	```python
	if condition: 
		doSomething()
	elif condition: 
		doSomethingElse()
	else: call()
	```
- ***Collections indexers***
	```python
	item = collection[index]
	item = collection[0]
	value = someMap["key"]
	```
- ***Collections initializing***
	```python
	# inaccurate version yet
	#
	# ARRAY
	collection = int[5]
	collection = { 1, 2, 3, 4, 5 }
	collection = int { 1, 2, 3, 4, 5 }
	#
	# MATRIX
	collection = int[3, 2]
	collection = { {1, 2}, {3, 4}, {5, 6} }
	collection = int { {1, 2}, {3, 4}, {5, 6} }
	#
	# LIST
	collection = int[*]
	collection = int* { 1, 2, 3, 4, 5 }
	#
	# MAP
	collection = { int, str }
	collection = { 1: "Text1", 2: "Hello, world!", 55: "Other string" }
	```
- ***Function calls***
	```js
	call(arg1, arg2, arg3)
	functionCall(nestedCall(arg1, arg2), 12345)
	```

<h3 align="center">Licensed code usage</h3>

- **This project uses some code from [IronPython 2 compiler](https://github.com/IronLanguages/ironpython2)
   that is licensed under Apache License 2.0**
