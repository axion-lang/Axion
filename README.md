<img align="center" src="Other/Graphics/Axion_Mini.png" />

<h2 align="center">Welcome to Axion programming language toolset</h2>
<h3 align="center">:\:\:\:\:\:\:\:\:\: Under construction :/:/:/:/:/:/:/:/:/:</h3>

### Repository consists of 3 parts:

- **Language core (lexer, parser, compiler, etc.) with flexible API to access language tools**
- **A .NET Core wrapper around language core (mainly to provide unit-testing and simple launching)**
- **Import of [ConsoleExtensions](https://github.com/F1uctus/ConsoleExtensions) project (to use the console code editor with syntax highlighting) (still unstable)**

### Objectives:

Designed to: <br/>
**Union languages (provide inter-transpiling to other popular languages).** <br/>
**Implement common design patterns quickly and simply.** <br/>
**Be as close to human, as possible.**

- **General-purposed**
- **High-performance**
- **Invisibly-static typed**
- **Safe**
- **Simple**
- **Readable**

### Paradigms:

- **Object-oriented**
- **Reactive**
- **Functional (extension)**

### Progress:

- **Console interactive interpreter / code editor**
- **Lexer (Tokenizer)**
- **Parser is still under construction.**
	*(Not working by last release time)*
- **Transpiling to C/C++ or Rust is planned**
- **In future - easily transpile Axion to another language**

### Launching:

Now compiler doesn't do anything useful for user
(it just creates lists of language tokens),
but if you want to see basic syntax and etc.
you can launch compiler with `dotnet run Axion.dll`
in bin folder and type `-h` in console to get support
about arguments for compiler CLI interface.
	
### Basic syntax examples:

- ***Numbers***
	```crystal
	n = 0          # Int32
	n = 10         # Int32
	n = 5.3252     # Float 64
	n = 4.3_f32    # Float 32
	n = 321_u8     # UInt 8
	32 + 7j        # Complex numbers
	# etc.
	```

- ***Operations***
	```python
	number-value = 1 + 2 * 3 # = 7
	string-variable += "string"
	float-value = 90.0
	# etc.
	```
- ***Function calls***
	```
	call(arg1, arg2, arg3)
	function-call(nested-call(arg1, arg2), 12345)
	```
- ***Branching***
	```
	if condition
		do-something()
	elif condition
		do-something-else()
	else call()
	```
- ***Loops***
    ```rb
    for x in [0..10]
        Console.print(x)

    for i = 0; i < args.length; i++
        Console.print(i)
    ```

- ***String and character literals***
	```python
	# simple
	string-literal = "value"
	string-literal = 'value'
	# multiline
	string-literal = """
	value
	"""
	string-literal = '''
	value
	'''
	# formatted
	string-literal = f"interpolated {value}"
	string-literal = f'interpolated {value}'
	# formatted
	string-literal = "escaped: \a\b\f\r\n\0"
	string-literal = 'escaped: \a\b\f\r\n\0'
	# raw
	string-literal = r"raw symbols: \a\b\f\r\n\0"
	string-literal = r'raw symbols: \a\b\f\r\n\0'
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

### Licensed code usage:

- **This project uses some code from [IronPython 2 compiler](https://github.com/IronLanguages/ironpython2)
   that is licensed under Apache License 2.0**
