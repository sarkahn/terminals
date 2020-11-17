using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Sark.Terminals
{
    public struct TileData : INativeDisposable
    {
        public NativeArray<Tile> Tiles;
        public readonly int2 Size;
        public readonly Allocator Allocator;

        public int Width => Size.x;
        public int Height => Size.y;
        public int Length => Tiles.Length;
        public bool IsCreated => Tiles.IsCreated;

        public TileData(int w, int h, float2 anchor, Allocator allocator)
        {
            Allocator = allocator;
            Size = new int2(w, h);
            Tiles = new NativeArray<Tile>(w * h, allocator);
        }

        public TileData(int w, int h, Allocator allocator) :
            this(w, h, .5f, allocator)
        { }

        public Tile this[int i]
        {
            get => Tiles[i];
            set => Tiles[i] = value;
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            Tiles.Dispose(inputDeps);
            return inputDeps;
        }

        public void Dispose()
        {
            Tiles.Dispose();
        }
    } 
}
