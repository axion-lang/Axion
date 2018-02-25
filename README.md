# Axion <img align="right" src="https://github.com/F1uctus/Axion/blob/master/Other/Axion_logo.png" width="200" height="200" />


High-level general-purposed programming language.
## Objectives
- **_simplicity_**
- **_readability_**
- **_multifunctionality_**

### Example
#### (not an exact version of the language yet)
```python
# Calculator, written in Axion.

out.write('Write a first number: ')

firstNum = out.readLine()
while int.parse(firstNum) is not number
	out.writeLine('\nPlease write correct number: ')
	firstNum = out.readLine()

out.write('Write a second number: ')

secondNum = out.readLine()
while int.parse(secondNum) is not Number
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
