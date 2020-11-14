using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using MeshData = UnityEngine.Mesh.MeshData;
using MeshDataArray = UnityEngine.Mesh.MeshDataArray;

using static Sark.Common.GridUtil.Grid2D;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace Sark.Terminals
{

    /* A Note:
    Profiling has shown this to be much slower than the "simple" backend for 
    standard writes, while this backend is much faster for resizing. This is 
    because the simple backend can perform writes without needing to resize 
    the mesh, whereas the MeshDataArray API forces you write *all* the data and 
    then discard it all when it gets uploaded to the mesh, every single time
    with "ApplyAndDisposeWritableMeshData"
    */

    /// <summary>
    /// A rendering backend that uses the "MeshDataArray" api to update a mesh.
    /// </summary>
    public class DataArrayMeshBackend : ITerminalRenderingBackend
    {
        JobHandle _tileDataJob;
        JobHandle _vertsJob;

        MeshDataArray _dataArray;
        
        float _aspect;

        bool _sizeChanged;

        [SerializeField]
        int _batchSize = 2048;

        public int2 Size { get; private set; }

        int TotalTiles => Size.x * Size.y;

        public DataArrayMeshBackend(int w, int h, Allocator allocator) : this(w,h)
        { }

        public DataArrayMeshBackend(int w, int h, int batchSize = 1024)
        {
            //Debug.Log($"Initializing backend with size of {w}, {h}");
            _batchSize = batchSize;
            _aspect = 1;
            Resize(w, h);
        }

        public DataArrayMeshBackend(int w, int h, float aspect, Mesh mesh, int batchSize = 1024)
        {
            _batchSize = batchSize;
            _aspect = aspect;
            Resize(w, h);
        }

        public void Dispose()
        {
            _tileDataJob.Complete();
        }

        public void Resize(int width, int height)
        {
            Size = new int2(width, height);
            _sizeChanged = true;
        }

        MeshDataArray GetDataArray()
        {
            var dataArray = Mesh.AllocateWritableMeshData(1);

            dataArray[0].SetVertexBufferParams(TotalTiles * 4,
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal,
                VertexAttributeFormat.Float32, 3, 1),
            // UVs
            new VertexAttributeDescriptor(
                VertexAttribute.TexCoord0,
                VertexAttributeFormat.Float32,
                2, 2),
            // FGColor
            new VertexAttributeDescriptor(
                VertexAttribute.TexCoord1,
                VertexAttributeFormat.Float32,
                4, 2),
            // BGColor
            new VertexAttributeDescriptor(
                VertexAttribute.TexCoord2,
                VertexAttributeFormat.Float32,
                4, 2));

            dataArray[0].SetIndexBufferParams(TotalTiles * 6, IndexFormat.UInt16);

            return dataArray;
        }

        /// <summary>
        /// Begin jobs to process tile data into mesh data.
        /// Should be called earlier in the frame.
        /// </summary>
        public JobHandle ScheduleUpdateData(TileData tiles, JobHandle inputDeps = default)
        {
            _vertsJob.Complete();
            _tileDataJob.Complete();

            _dataArray = GetDataArray();

            _vertsJob = new VertsJobDataArray
            {
                MeshData = _dataArray[0],
                Size = Size,
                Aspect = _aspect,
            }.ScheduleBatch(tiles.Length, _batchSize * 2, inputDeps);

            _vertsJob = new SetSubMeshJob
            {
                MeshData = _dataArray[0],
            }.Schedule(_vertsJob);

            // Can't pass temp allocated containers to jobs 
            // - not even main thread jobs called with "Run"
            if ( tiles.Allocator == Allocator.Temp )
            {
                WriteTilesRange(0, tiles.Length, _dataArray[0], tiles);
            }
            else
            {
                _tileDataJob = new TileDataJob
                {
                    MeshData = _dataArray[0],
                    Tiles = tiles
                }.ScheduleBatch(tiles.Length, _batchSize, inputDeps);
            }

            return JobHandle.CombineDependencies(_vertsJob, _tileDataJob);
        }

        /// <summary>
        /// Complete process jobs and upload data to the mesh.
        /// This does nothing if no process jobs are running.
        /// Should be called later in the frame.
        /// </summary>
        public void UploadToMesh(Mesh mesh)
        {
            _vertsJob.Complete();
            _tileDataJob.Complete();

            Mesh.ApplyAndDisposeWritableMeshData(_dataArray, mesh,
                //MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices );

            if(_sizeChanged)
            {
                mesh.RecalculateBounds();
                //_mesh.RecalculateNormals();
                //_mesh.RecalculateTangents();
                _sizeChanged = false;
            }
        }

        public void UpdateDataAndUploadToMesh(TileData tiles, Mesh mesh)
        {
            _dataArray = GetDataArray();

            var data = _dataArray[0];
    
            if( tiles.Allocator == Allocator.Temp )
            {
                WriteVertsRange(0, tiles.Length, data, Size, _aspect);
                SetSubMesh(data);
                WriteTilesRange(0, tiles.Length, data, tiles);
            }
            else
            {
                new VertsJobDataArray
                {
                    MeshData = data,
                    Aspect = _aspect,
                    Size = Size
                }.RunBatch(tiles.Length);

                new SetSubMeshJob
                {
                    MeshData = data,
                }.Run();

                new TileDataJob
                {
                    MeshData = data,
                    Tiles = tiles
                }.RunBatch(tiles.Length);
            }

            UploadToMesh(mesh);
        }

        static readonly float2 UvSize = 1f / 16f;
        static readonly float2 UvRight = new float2(UvSize.x, 0);
        static readonly float2 UvUp = new float2(0, UvSize.y);

        static float4 FromColor(Color c) =>
            new float4(c.r, c.g, c.b, c.a);

        [StructLayout(LayoutKind.Sequential)]
        struct VertTileData
        {
            public float2 UV;
            public float4 FGColor;
            public float4 BGColor;
        }

        [BurstCompile]
        public struct VertsJobDataArray : IJobParallelForBatch
        {
            public MeshData MeshData;
            public int2 Size;
            public float Aspect;

            public void Execute(int startIndex, int count)
            {
                WriteVertsRange(startIndex, startIndex + count, MeshData, Size, Aspect);
            }
        }

        [BurstCompile]
        public struct TileDataJob : IJobParallelForBatch
        {
            public MeshData MeshData;

            [ReadOnly]
            public TileData Tiles;

            public void Execute(int startIndex, int count)
            {
                WriteTilesRange(startIndex, startIndex + count, MeshData, Tiles);
            }
        }

        [BurstCompile]
        struct SetSubMeshJob : IJob
        {
            public MeshData MeshData;
            public void Execute()
            {
                SetSubMesh(MeshData);
            }
        }

        static void SetSubMesh(MeshData meshData)
        {

            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0,
                meshData.GetIndexData<ushort>().Length));
        }

        static void WriteVertsRange(int min, int max, MeshData meshData, int2 size, float aspect)
        {

            var pos = meshData.GetVertexData<float3>(0);
            var idx = meshData.GetIndexData<ushort>();

            float3 start = -new float3(size.x, size.y, 0) * .5f;

            float3 vertRight = new float3(aspect, 0, 0);
            float3 vertUp = new float3(0, 1, 0);

            //Debug.Log($"VertsJob Settings verts from {min} to {max}. Total size: {size.x * size.y}");

            for (int tileIndex = min; tileIndex < max; ++tileIndex)
            {
                int vi = tileIndex * 4; // Vert Index
                int ti = tileIndex * 6; // Triangle index

                // Positions
                int2 xy = IndexToPos(tileIndex, size.x);
                float3 vOrigin = new float3(xy, 0);
                vOrigin.x *= vertRight.x;

                pos[vi + 0] = start + vOrigin + vertUp;
                pos[vi + 1] = start + vOrigin + vertRight + vertUp;
                pos[vi + 2] = start + vOrigin;
                pos[vi + 3] = start + vOrigin + vertRight;

                // Indices
                idx[ti + 0] = (ushort)(vi + 0);
                idx[ti + 1] = (ushort)(vi + 1);
                idx[ti + 2] = (ushort)(vi + 2);
                idx[ti + 3] = (ushort)(vi + 3);
                idx[ti + 4] = (ushort)(vi + 2);
                idx[ti + 5] = (ushort)(vi + 1);
            }
        }

        static void WriteTilesRange(int min, int max, MeshData meshData, TileData tiles)
        {
            var tileData = meshData.GetVertexData<VertTileData>(2);

            //Debug.Log($"TileData job Setting tile data from {startIndex} to {startIndex + count}. Tiles length {Tiles.Length}");

            //0-1
            //|/|
            //2-3
            for (int tileIndex = min; tileIndex < max; ++tileIndex)
            {
                var tile = tiles[tileIndex];

                int vi = tileIndex * 4; // Vert Index

                int glyph = tile.glyph;

                // UVs
                int2 glyphIndex = new int2(
                    glyph % 16,
                    // Y is flipped on the spritesheet
                    16 - 1 - (glyph / 16));
                float2 uvOrigin = (float2)glyphIndex * UvSize;

                var fg = tile.fgColor;
                var bg = tile.bgColor;

                tileData[vi + 0] = new VertTileData
                {
                    UV = uvOrigin + UvUp,
                    FGColor = FromColor(fg),
                    BGColor = FromColor(bg)
                };
                tileData[vi + 1] = new VertTileData
                {
                    UV = uvOrigin + UvRight + UvUp,
                    FGColor = FromColor(fg),
                    BGColor = FromColor(bg)
                };
                tileData[vi + 2] = new VertTileData
                {
                    UV = uvOrigin,
                    FGColor = FromColor(fg),
                    BGColor = FromColor(bg)
                };
                tileData[vi + 3] = new VertTileData
                {
                    UV = uvOrigin + UvRight,
                    FGColor = FromColor(fg),
                    BGColor = FromColor(bg)
                };
            }
        }
    }
}