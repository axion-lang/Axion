h = 10
x = 2
y = 2
decorations = "@*+0O"
rnd = new Random ( )
Console.WriteLine("".PadRight(y, '\n'))
treeSpaces = h
Console.ForegroundColor = ConsoleColor.Red
Console.WriteLine("".PadRight(x + treeSpaces, ' ') + "*")
levelWidth = 1
treeSpaces += 1
lastWasDecor = false
while levelWidth < h:
    treeLevelHeight = 1 if levelWidth == 1 or levelWidth == h - 1 else (2)
    k = 0
    while k < treeLevelHeight:
        Console.Write("".PadRight(x + treeSpaces, ' '))
        r = 0
        while r < levelWidth:
            if rnd.Next(5) == 0 and not (lastWasDecor):
                Console.ForegroundColor = ConsoleColor.Cyan
                Console.Write(decorations[rnd.Next(decorations.Length)])
                lastWasDecor = true
            else:
                Console.ForegroundColor = ConsoleColor.DarkGreen
                Console.Write("/")
                lastWasDecor = false
            
            r += 1
        
        Console.ForegroundColor = ConsoleColor.DarkGreen
        Console.Write("|")
        r = 0
        while r < levelWidth:
            if rnd.Next(5) == 0 and not (lastWasDecor):
                Console.ForegroundColor = ConsoleColor.Cyan
                Console.Write(decorations[rnd.Next(decorations.Length)])
                lastWasDecor = true
            else:
                Console.ForegroundColor = ConsoleColor.DarkGreen
                Console.Write("\\")
                lastWasDecor = false
            
            r += 1
        
        Console.WriteLine()
        k += 1
    
    levelWidth += 1
    treeSpaces += 1

Console.ForegroundColor = ConsoleColor.DarkYellow
stem-width: int = h / 3
if stem_width % 2 == 0:
    stem_width += 1

stem-height: int = 1 + h / 8
stem-spaces: int = levelWidth - stem-width / 2
l = 0
while l < stem_height:
    Console.Write("".PadRight(x + stem_spaces, ' '))
    Console.Write("".PadRight(stem_width, '|'))
    Console.WriteLine()
    l += 1
