using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using Sark.Terminals.TerminalExtensions;

namespace Sark.Terminals
{
    /// <summary>
    /// A simple terminal for rendering Ascii to a mesh.
    /// </summary>
    public class SimpleTerminal : INativeDisposable
    {
        [SerializeField]
        int2 _size;

        TileData _tiles;
        SimpleMeshBackend _backend;
        
        bool _isDirty;
        bool _dataUpdated;

        float2 _tileSize = 1;

        public TileData Tiles => _tiles;

        public int2 Size => _size;
        public int Width => _size.x;
        public int Height => _size.y;
        public int CellCount => _size.x * _size.y;
        public Mesh Mesh { get; private set; }

        Allocator _allocator;

        public SimpleTerminal(int w, int h, Allocator allocator) : 
            this(w, h, 1, 1, allocator)
        {}

        public SimpleTerminal(int w, int h, 
            float tileWidth, float tileHeight, Allocator allocator)
        {
            Mesh = new Mesh();
            _size = new int2(w, h);
            _tiles = new TileData(w, h, allocator);
            _allocator = allocator;

            _tileSize = new float2(tileWidth, tileHeight);

            _backend = new SimpleMeshBackend(w, h, allocator).WithTileSize(_tileSize);
            _isDirty = true;
            ClearScreen();
            ImmediateUpdate();
        }

        public void Resize(int w, int h)
        {
            if (_size.x == w && _size.y == h )
                return;
            OnResized(w, h);
        }

        void OnResized(int w, int h)
        {
            w = math.max(1, w);
            h = math.max(1, h);
            _size = new int2(w, h);

            _tiles.Dispose();
            _tiles = new TileData(w, h, _allocator);

            _backend.Resize(w, h);

            ClearScreen();

            _isDirty = true;
        }

        public void Set(int x, int y, Tile t)
        {
            _isDirty = true;
            _tiles.Set(x, y, t);
        }

        public void Set(int x, int y, char c)
        {
            _isDirty = true;
            _tiles.Set(x, y, c);
        }

        public void Set(int x, int y, Color fgColor, char c)
        {
            _isDirty = true;
            _tiles.Set(x, y, fgColor, c);
        }

        public void Set(int x, int y, Color fgColor, Color bgColor, char c)
        {
            _isDirty = true;
            _tiles.Set(x, y, fgColor, bgColor, c);
        }

        public void Circle(int x, int y, int radius)
        {
            _isDirty = true;
            _tiles.Circle(x, y, radius);
        }

        public void Print(int x, int y, string str)
        {
            _isDirty = true;
            _tiles.Print(x, y, str);
        }

        public NativeArray<Tile> ReadTiles(int x, int y, int len, Allocator allocator)
        {
            return _tiles.ReadTiles(x, y, len, allocator);
        }

        public Tile Get(int x, int y) => _tiles.Get(x, y);

        public void ClearScreen()
        {
            _isDirty = true;
            _tiles.ClearJob().Run();
        }

        /// <summary>
        /// Force the terminal to refresh on it's next update.
        /// </summary>
        public void SetDirty()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Begin jobs to process tile data into mesh data.
        /// This should be called earlier in the frame after all
        /// terminal modifications are complete.
        /// 
        /// Does nothing if the terminal state hasn't changed.
        /// </summary>
        public void EarlyUpdate()
        {
            if(_isDirty)
            {
                _dataUpdated = true;

                _backend.ScheduleUpdateData(_tiles);
                JobHandle.ScheduleBatchedJobs();
            }
        }
        
        /// <summary>
        /// Complete processing jobs and apply data to the mesh.
        /// 
        /// This should be called later in the frame.
        /// </summary>
        public void LateUpdate()
        {
            if(_isDirty && _dataUpdated)
            {
                _backend.UploadToMesh(Mesh);
                _isDirty = false;
                _dataUpdated = false;
            }
        }

        /// <summary>
        /// Immediately process tile-to-mesh data and upload it to the mesh.
        /// 
        /// Does nothing if the terminal state hasn't changed.
        /// </summary>
        public void ImmediateUpdate()
        {
            _backend.UpdateDataAndUploadToMesh(_tiles, Mesh);
        }

        public JobHandle Dispose(JobHandle inputDeps)
        {
            _tiles.Dispose(inputDeps);
            return inputDeps;
        }

        public void Dispose()
        {
            _tiles.Dispose();
            _backend.Dispose();
        }

        public SimpleTerminal WithTileSize(float2 tileSize)
        {
            if (tileSize.Equals(_tileSize))
                return this;

            _isDirty = true;
            _tileSize = tileSize;
            _backend.WithTileSize(tileSize);
            return this;
        }

        public bool IsInBounds(int2 tileIndex)
        {
            return math.all(tileIndex >= 0) && math.all(tileIndex < _size);
        }

        public int2 PositionToTileIndex(float3 localPos)
        {
            float2 anchor = .5f;
            localPos.xy += _size * anchor;
            int2 p = (int2)math.floor(localPos.xy);
            return p;
        }
    }
}
