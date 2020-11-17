using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

using static Sark.Terminals.CodePage437;

public class Scramble : MonoBehaviour
{
    private void Start()
    {
        var term = GetComponent<TerminalBehaviour>();
        var tiles = term.Tiles;

        new ScrambleJob
        {
            Tiles = tiles,
            Random = new Unity.Mathematics.Random(
                (uint)Random.Range(1, int.MaxValue)
        )}.Run();
    }

    [BurstCompile]
    struct ScrambleJob : IJob
    {
        public TileData Tiles;
        public Unity.Mathematics.Random Random;

        public void Execute()
        {
            for (int i = 0; i < Tiles.Length; ++i)
            {
                var t = Tiles[i];
                t.glyph = ToCP437((char)Random.NextInt(0, 255));
                Tiles[i] = t;
            }
        }
    }
}
