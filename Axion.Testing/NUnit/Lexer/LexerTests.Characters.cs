using Axion.Core.Processing.Errors;
using NUnit.Framework;

namespace Axion.Testing.NUnit.Lexer;

public partial class LexerTests {
    [Test]
    public void TestRegularCharacterLiteral() {
        var unit = TestUtils.UnitFromCode("x = `q`");
        Lex(unit);
        Assert.IsEmpty(unit.Blames);
    }

    [Test]
    public void TestEmojiCharacterLiteral() {
        var unit = TestUtils.UnitFromCode("x = `✌`");
        Lex(unit);
        Assert.IsEmpty(unit.Blames);
    }

    [Test]
    public void TestEscapedCharacterLiteral() {
        var unit = TestUtils.UnitFromCode("tab = `\\t`");
        Lex(unit);
        Assert.IsEmpty(unit.Blames);
    }

    [Test]
    public void TestFailEmptyCharacterLiteral() {
        var unit = TestUtils.UnitFromCode("x = ``");
        Lex(unit);
        Assert.AreEqual(1, unit.Blames.Count);
        Assert.AreEqual(
            unit.Blames[0].Message,
            BlameType.EmptyCharacterLiteral.Description
        );
    }

    [Test]
    public void TestFailLongCharacterLiteral() {
        var unit = TestUtils.UnitFromCode("x = `abc`");
        Lex(unit);
        Assert.AreEqual(1, unit.Blames.Count);
        Assert.AreEqual(
            unit.Blames[0].Message,
            BlameType.CharacterLiteralTooLong.Description
        );
    }

    [Test]
    public void TestFailUnclosedCharacterLiteral() {
        var unit = TestUtils.UnitFromCode("x = `abcdef\n");
        Lex(unit);
        Assert.AreEqual(1, unit.Blames.Count);
        Assert.AreEqual(
            unit.Blames[0].Message,
            BlameType.UnclosedCharacterLiteral.Description
        );
    }
}
