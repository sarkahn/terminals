using Sark.Common.GridUtil;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Sark.Terminals.CodePage437;
using Color = UnityEngine.Color;

namespace Sark.Terminals.TerminalExtensions
{
    public static class TerminalExtension
    {
        public static Tile Get(this TileData tiles, int x, int y)
        {
            int i = Grid2D.PosToIndex(x, y, tiles.Width);
            return tiles[i];
        }

        public static void Set(this TileData tiles, int x, int y, Tile t)
        {
            int i = Grid2D.PosToIndex(x, y, tiles.Width);
            tiles[i] = t;
        }

        public static void Set(this TileData tiles, int x, int y, char c)
        {
            int i = Grid2D.PosToIndex(x, y, tiles.Width);

            var t = tiles[i];
            t.glyph = ToCP437(c);
            tiles[i] = t;
        }
        public static void Set(this TileData tiles, int x, int y, Color fgColor, char c)
        {
            int i = Grid2D.PosToIndex(x, y, tiles.Width);

            var t = tiles[i];
            t.glyph = ToCP437(c);
            t.fgColor = fgColor;
            tiles[i] = t;
        }
        public static void Set(this TileData tiles, 
            int x, int y, Color fgColor, Color bgColor, char c)
        {
            int i = Grid2D.PosToIndex(x, y, tiles.Width);

            var t = tiles[i];
            t.glyph = ToCP437(c);
            t.fgColor = fgColor;
            t.bgColor = bgColor;
            tiles[i] = t;
        }

        public static void Clear(this TileData tiles)
        {
            for (int i = 0; i < tiles.Length; ++i)
                tiles[i] = Tile.EmptyTile;
        }

        public static ClearScreenJob ClearJob(this TileData tiles)
        {
            return new ClearScreenJob { Tiles = tiles };
        }

        public static ScrambleTilesJob ScrambleJob(this TileData tiles)
        {
            return new ScrambleTilesJob
            {
                Tiles = tiles,
                Random = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue))
            };
        }


        public static NativeArray<Tile> ReadTiles(this TileData tiles, 
            int x, int y, int len, Allocator allocator)
        {
            var buff = new NativeArray<Tile>(len, allocator);
            len = math.min(len, tiles.Length - x);
            int i = Grid2D.PosToIndex(x, y, tiles.Width);
            NativeArray<Tile>.Copy(tiles.Tiles, i, buff, 0, len);

            return buff;
        }

        static void SubsequencePoints(this TileData tiles,
            int xc, int yc, 
            int x, int y)
        {
            tiles.Set(xc + x, yc + y, '█');
            tiles.Set(xc - x, yc + y, '█');
            tiles.Set(xc + x, yc - y, '█');
            tiles.Set(xc - x, yc - y, '█');
            tiles.Set(xc + y, yc + x, '█');
            tiles.Set(xc - y, yc + x, '█');
            tiles.Set(xc + y, yc - x, '█');
            tiles.Set(xc - y, yc - x, '█');
        }

        public static void Circle(this TileData tiles, int xc, int yc, int radius)
        {
            int x = 0, y = radius;
            int d = 3 - 2 * radius;
            tiles.SubsequencePoints(xc, yc, x, y);
            while (y >= x)
            {
                x++;

                if (d > 0)
                {
                    y--;
                    d = d + 4 * (x - y) + 10;
                }
                else
                    d = d + 4 * x + 6;

                tiles.SubsequencePoints(xc, yc, x, y);
            }
        }

        public static void Print(this TileData tiles, int x, int y, string str)
        {
            var bytes = StringToCP437(str, Allocator.Temp);
            for (int i = 0; i < bytes.Length; ++i)
            {
                int index = Grid2D.PosToIndex(x, y, tiles.Width);
                if (index >= 0 && index < tiles.Length)
                {
                    var t = tiles[index];
                    t.glyph = bytes[i];
                    tiles[index] = t;
                }
                else
                    return;

                ++x;

                if (x >= tiles.Width)
                {
                    x = 0;
                    y--;
                }
            }
        }

        [BurstCompile]
        public struct ClearScreenJob : IJob
        {
            public TileData Tiles;

            public void Execute()
            {
                Tiles.Clear();
            }
        }


        [BurstCompile]
        public struct ScrambleTilesJob : IJob
        {
            public TileData Tiles;
            public Unity.Mathematics.Random Random;
            public void Execute()
            {
                var t = Tiles.Tiles;
                for (int i = 0; i < t.Length; ++i)
                {
                    var tile = t[i];
                    tile.glyph = (byte)Random.NextInt(0, 255);
                    t[i] = tile;
                }
            }
        }
    }
}
