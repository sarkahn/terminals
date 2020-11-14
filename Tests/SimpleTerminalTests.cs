using NUnit.Framework;
using Sark.Common.GridUtil;
using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

using static Unity.Collections.Allocator;
using static Sark.Terminals.CodePage437;

[TestFixture]
public class SimpleTerminalTests
{
    [Test]
    public void WriteRead()
    {
        var term = new SimpleTerminal(40, 15, Temp);

        term.Print(0, 0, "Hello");

        var tiles = term.ReadTiles(0, 0, 5, Temp);

        Assert.AreEqual('H', ToChar(tiles[0].glyph));
        Assert.AreEqual('e', ToChar(tiles[1].glyph));
        Assert.AreEqual('l', ToChar(tiles[2].glyph));
        Assert.AreEqual('l', ToChar(tiles[3].glyph));
        Assert.AreEqual('o', ToChar(tiles[4].glyph));
    }

    [Test]
    public void SetGet()
    {
        var term = new SimpleTerminal(10, 10, Temp);

        term.Set(7, 5, Color.blue, Color.green, 'a');

        var t = term.Get(7, 5);

        Assert.AreEqual(Color.blue, t.fgColor);
        Assert.AreEqual(Color.green, t.bgColor);
        Assert.AreEqual('a', t.glyph);
    }

    [Test]
    public void Resize()
    {
        var term = new SimpleTerminal(10, 10, Temp);

        Assert.AreEqual(10, term.Width);
        Assert.AreEqual(10, term.Height);

        term.Resize(30, 5);

        Assert.AreEqual(30, term.Width);
        Assert.AreEqual(5, term.Height);
    }
}
