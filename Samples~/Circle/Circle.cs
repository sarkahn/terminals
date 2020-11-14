using Sark.Terminals;
using Sark.Terminals.TerminalExtensions;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Circle : MonoBehaviour
{
    SimpleTerminal _term;

    [SerializeField]
    Material _mat;

    [SerializeField]
    int _radius = 5;

    private void OnEnable()
    {
        _term = new SimpleTerminal(80, 60, Allocator.Persistent);

        //DrawCircle();
    }

    private void OnDisable()
    {
        _term.Dispose();
    }

    private void Update()
    {
        new CircleJob { Tiles = _term.Tiles, Elapsed = Time.time * 5 }.Run();
        _term.SetDirty();
        _term.EarlyUpdate();
    }

    private void LateUpdate()
    {
        _term.LateUpdate();

        Graphics.DrawMesh(_term.Mesh, Matrix4x4.identity, _mat, 0);
    }

    void DrawCircle()
    {
        _term.ClearScreen();
        _term.Circle(_term.Width / 2, _term.Height / 2, _radius);
    }

    private void OnValidate()
    {
        if (!isActiveAndEnabled || !Application.isPlaying)
            return;

        //DrawCircle();
    }

    [BurstCompile]
    struct CircleJob : IJob
    {
        public TileData Tiles;
        public float Elapsed; 
        public void Execute()
        {
            int x = (int)(40 + math.sin(Elapsed * .35f) * 29.9f);
            int y = (int)(30 + math.cos(Elapsed) * 19.9f); 
            int r = (int)(5 + math.sin(Elapsed * 0.5f) * 3.5f);

            Tiles.Clear();
            Tiles.Circle(x, y, r);
        }
    }
}
