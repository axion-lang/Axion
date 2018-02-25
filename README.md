# Axion <img align="right" src="https://github.com/F1uctus/Axion/blob/master/Other/Axion_logo.png" width="200" height="200" />

_High-level general-purposed programming language. **(under construction)**_
## Objectives:
- **Multifunctionality**
- **Static typization**
- **With interpreter & compiler**
- **High-performance**
- **Safe work with memory**
- **Simplicity**
- **Readability**

## Things that have been done:
- **Lexer**
- **Parser is still under construction**
- **Next thing will be interpreter**

## Example
(not an exact version of the language yet)
```python
# Calculator, written in Axion.

out.write('Write a first number: ')

firstNum = out.readLine()
while parseType(firstNum) is not number
	out.writeLine('\nPlease write correct number: ')
	firstNum = out.readLine()

out.write('Write a second number: ')

secondNum = out.readLine()
while parseType(secondNum) is not number
	out.writeLine('\nPlease write correct number: ')
	secondNum = out.readLine()

allowedOperations = ['+', '-', '*', '/']
out.write('Write an operation (+, -, *, /): ')

opChar = out.readKey()
while opChar not in allowedOperations
	out.writeLine('\nPlease write only one of: (+, -, *, /): ')
	opChar = out.readKey()

switch opChar
	case '+'
		out.writeLine(firstNum + secondNum)
	case '-'
		out.writeLine(firstNum - secondNum)
	case '*'
		out.writeLine(firstNum * secondNum)
	case '/'
		out.writeLine(firstNum / secondNum)
```
