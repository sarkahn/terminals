using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class RenderFromCode : MonoBehaviour
{
    public SimpleTerminal _term;
    SimpleTerminal _aspectTerm;

    [SerializeField]
    int2 _size = new int2(80, 40);

    [SerializeField]
    Material _material;

    [SerializeField]
    Material _aspectMar;

    private void OnEnable()
    {
        _term = new SimpleTerminal(_size.x, _size.y, Allocator.Persistent);
        _term.ImmediateUpdate();

        _aspectTerm = new SimpleTerminal(_size.x, _size.y, Allocator.Persistent);
        _aspectTerm.ImmediateUpdate();

        _term.Print(0, 0, "Hello!");
        _aspectTerm.Print(0, 0, "Hello!");

        _aspectTerm.WithTileSize(new float2(.5f, 1));
    }

    private void OnDisable()
    {
        _term.Dispose();
        _aspectTerm.Dispose();
    }

    private void Update()
    {
        _term.EarlyUpdate();
        _aspectTerm.EarlyUpdate();
    }

    Matrix4x4 FromPos(float3 p)
    {
        return Matrix4x4.TRS(p, Quaternion.identity, Vector3.one);
    }

    private void LateUpdate()
    {
        _term.LateUpdate();
        _aspectTerm.LateUpdate();

        float3 up = new float3(0, _size.y, 0) * .5f;
        float3 p = new float3(0, _size.y, 0);

        p.x = _size.x * .5f;
        Graphics.DrawMesh(_term.Mesh, FromPos(p + up), _material, 0);

        p.x = _size.x * .5f * .5f;
        p.y = -1;
        Graphics.DrawMesh(
            _aspectTerm.Mesh, 
            FromPos(p + new float3(0, 1, 0)), 
            _aspectMar, 0);
    }

    void Scramble(SimpleTerminal term)
    {
        new ScrambleJob
        {
            Tiles = term.Tiles,
            Random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(1, int.MaxValue))
        }.Run();
    }

    private void OnValidate()
    {
        if (!isActiveAndEnabled || !Application.isPlaying)
            return;

        _term.Resize(_size.x, _size.y);
        Scramble(_term);

        _aspectTerm.Resize(_size.x, _size.y);
        Scramble(_aspectTerm);
    }

    [BurstCompile]
    struct ScrambleJob : IJob
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
