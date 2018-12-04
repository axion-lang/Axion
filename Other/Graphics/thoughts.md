## TODO: TeamCity integration

## Add full-featured REPL
## with saved loaded modules,
## opened files, etc.

## Support for .axProj

#===================

    // unicode name
    // TODO: add unicode database parsing from IPy.
    //case 'N': {
    //    if (c == '{') {
    //        stream.Move();
    //        var nameBuf      = new StringBuilder();
    //        var nameComplete = false;
    //        while (!nameComplete) {
    //            char nameChar = c;
    //            stream.Move();
    //            if (stream.CharIdx == stream.Source.Length) {
    //                break;
    //            }
    //            if (nameChar != '}') {
    //                nameBuf.Append(nameChar);
    //            }
    //            else {
    //                nameComplete = true;
    //            }
    //        }
    //        if (!nameComplete || nameBuf.Length == 0) {
    //            throw new Exception(
    //                $"Can't decode bytes at line {stream.Position.line}, column {stream.Position.column}: malformed \\N character escape"
    //            );
    //        }
    //        try {
    //            var    uVal = "";
    //            string uDef = nameBuf.ToString();
    //            for (var i = 0; i < uDef.Length; i++) {
    //                uVal += char.ConvertFromUtf32(uDef[i]);
    //            }
    //            tokenValue += uVal;
    //        }
    //        catch (KeyNotFoundException) {
    //            throw new Exception(
    //                $"Can't decode bytes at line {stream.Position.line}, column {stream.Position.column}: unknown Unicode character name"
    //            );
    //        }
    //    }
    //    else {
    //        throw new Exception(
    //            $"Can't decode bytes at line {stream.Position.line}, column {stream.Position.column}: malformed \\N character escape"
    //        );
    //    }
    //    continue;
    //}