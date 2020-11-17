using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

using static Sark.Terminals.CodePage437;
using Sark.Terminals.TerminalExtensions;

[ExecuteAlways]
public class Scramble : MonoBehaviour
{
    TerminalBehaviour _term;

    [SerializeField]
    bool _disableAfterStart = false;

    private void OnEnable()
    {
        _term = GetComponent<TerminalBehaviour>();
    }

    private void Update()
    {
        Go();

        if (Application.isPlaying && _disableAfterStart)
            enabled = false;
    }

    void Go()
    {
        if (_term == null)
            return;

        var tiles = _term.Tiles;

        tiles.ScrambleJob().Run();

        _term.SetDirty();
    }

    //[BurstCompile]
    //struct ScrambleJob : IJob
    //{
    //    public TileData Tiles;
    //    public Unity.Mathematics.Random Random;

    //    public void Execute()
    //    {
    //        for (int i = 0; i < Tiles.Length; ++i)
    //        {
    //            var t = Tiles[i];
    //            t.glyph = ToCP437((char)Random.NextInt(0, 255));
    //            Tiles[i] = t;
    //        }
    //    }
    //}
}
