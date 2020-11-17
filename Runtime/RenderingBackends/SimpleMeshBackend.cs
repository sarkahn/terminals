
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Rendering;

using Mesh = UnityEngine.Mesh;
using Debug = UnityEngine.Debug;

using static Sark.Terminals.BackendUtility;

namespace Sark.Terminals
{
    /// <summary>
    /// A rendering backend using Unity's "Simple Mesh API" 
    /// https://docs.unity3d.com/2019.3/Documentation/ScriptReference/Mesh.html
    /// </summary>
    public class SimpleMeshBackend : ITerminalRenderingBackend
    {
        JobHandle _vertsJob;
        JobHandle _tileDataJob;

        Allocator _allocator;

        NativeArray<ushort> _indices;
        NativeArray<float3> _verts;
        NativeArray<VertTileData> _vertData;

        NativeArray<VertexAttributeDescriptor> _vertexDescriptors;

        bool _sizeChanged;

        int _batchSize = 1024;

        float2 _tileSize = 1;

        public int2 Size { get; private set; }

        public int TotalTiles => Size.x * Size.y;


        public SimpleMeshBackend(int width, int height, Allocator allocator)
        {
            _allocator = allocator;

            int cellCount = width * height;

            _vertData = new NativeArray<VertTileData>(cellCount * 4, allocator);

            _indices = new NativeArray<ushort>(cellCount * 6, allocator);
            _verts = new NativeArray<float3>(cellCount * 4, allocator);

            _sizeChanged = true;

            _vertexDescriptors = new NativeArray<VertexAttributeDescriptor>(4, allocator);

            // Stream 0 - Positions
            _vertexDescriptors[0] = new VertexAttributeDescriptor(
               VertexAttribute.Position,
               VertexAttributeFormat.Float32,
               3, 0);

            // Stream 1 - Vertex Tile Data
            // UV
            _vertexDescriptors[1] = new VertexAttributeDescriptor(
               VertexAttribute.TexCoord0,
               VertexAttributeFormat.Float32,
               2, 1);
            // FG Colors
            _vertexDescriptors[2] = new VertexAttributeDescriptor(
               VertexAttribute.TexCoord1,
               VertexAttributeFormat.Float32,
               4, 1);
            // BG Colors
            _vertexDescriptors[3] = new VertexAttributeDescriptor(
               VertexAttribute.TexCoord2,
               VertexAttributeFormat.Float32,
               4, 1);

            Size = new int2(width, height);

            //Debug.Log($"Initializing backend with size {Size}");
        }

        public void CompleteTileJob()
        {
            _tileDataJob.Complete();
        }

        public SimpleMeshBackend WithTileSize(float2 s)
        {
            _tileSize = s;
            _sizeChanged = true;
            return this;
        }

        public void Resize(int w, int h)
        {
            _tileDataJob.Complete();
            _vertsJob.Complete();

            Size = new int2(w, h);

            int cellCount = w * h;

            DisposeArrays();
            _indices = new NativeArray<ushort>(cellCount * 6, _allocator);
            _verts = new NativeArray<float3>(cellCount * 4, _allocator);
            _vertData = new NativeArray<VertTileData>(cellCount * 4, _allocator);

            _sizeChanged = true;
        }

        public JobHandle ScheduleUpdateData(TileData tiles, JobHandle inputDeps = default)
        {
            if(_allocator == Allocator.Temp)
            {
                if(_sizeChanged)
                    RebuildVertsRange(0, _verts.Length, Size, _tileSize, _verts, _indices);

                RebuildTileDataRange(0, tiles.Length, tiles, _vertData);
            } else
            {
                if( _sizeChanged )
                    _vertsJob = new VertsJob
                    {
                        Verts = _verts,
                        Indices = _indices,
                        Size = Size,
                        TileSize = _tileSize
                    }.ScheduleBatch(tiles.Length, _batchSize, inputDeps);

                _tileDataJob = new TileDataJob
                {
                    Tiles = tiles,
                    VertData = _vertData
                }.ScheduleBatch(tiles.Length, _batchSize, inputDeps);
            }

            inputDeps = JobHandle.CombineDependencies(_vertsJob, _tileDataJob);

            return inputDeps;
        }

        public void UploadToMesh(Mesh mesh)
        {
            // Avoid resizing the mesh until needed. This is a huge savings
            // on regular writes.
            if(_sizeChanged)
            {
                //Debug.Log("Uploading size changes to mesh");
                _vertsJob.Complete();
                mesh.Clear();

                mesh.SetVertexBufferParams(4 * TotalTiles, _vertexDescriptors);
                mesh.SetIndexBufferParams(6 * TotalTiles, IndexFormat.UInt16);

                mesh.SetVertexBufferData(_verts, 0, 0,
                    _verts.Length, 0, 
                    MeshUpdateFlags.DontValidateIndices |
                    MeshUpdateFlags.DontNotifyMeshUsers |
                    MeshUpdateFlags.DontRecalculateBounds |
                    MeshUpdateFlags.DontResetBoneBounds
                    );
                mesh.SetIndexBufferData(_indices, 0, 0,
                    _indices.Length,
                    MeshUpdateFlags.DontValidateIndices |
                    MeshUpdateFlags.DontNotifyMeshUsers |
                    MeshUpdateFlags.DontRecalculateBounds |
                    MeshUpdateFlags.DontResetBoneBounds
                    );

                mesh.SetSubMesh(0, new SubMeshDescriptor(0, _indices.Length),
                    MeshUpdateFlags.DontValidateIndices |
                    MeshUpdateFlags.DontNotifyMeshUsers |
                    MeshUpdateFlags.DontRecalculateBounds |
                    MeshUpdateFlags.DontResetBoneBounds
                    );

                mesh.RecalculateBounds();

                _sizeChanged = false;
            }

            //Debug.Log("Uploading tile data to mesh");
            _tileDataJob.Complete();

            mesh.SetVertexBufferData(_vertData, 0, 0, _vertData.Length, 1,                         MeshUpdateFlags.DontRecalculateBounds | 
                MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontNotifyMeshUsers |
                MeshUpdateFlags.DontResetBoneBounds 
                );
        }

        public void UpdateDataAndUploadToMesh(TileData tiles, Mesh mesh)
        {
            //Debug.Log($"UpdateAndUpload from backend. TileSize {_tileSize}");
            if (_allocator == Allocator.Temp)
            {
                if (_sizeChanged)
                    RebuildVertsRange(0, _verts.Length, Size, _tileSize, _verts, _indices);

                RebuildTileDataRange(0, tiles.Length, tiles, _vertData);
            }
            else
            {
                if (_sizeChanged)
                    new VertsJob
                    {
                        Indices = _indices,
                        Size = Size,
                        Verts = _verts,
                        TileSize = _tileSize
                    }.RunBatch(tiles.Length);

                new TileDataJob
                {
                    Tiles = tiles,
                    VertData = _vertData
                }.RunBatch(tiles.Length);
            }

            UploadToMesh(mesh);

            _sizeChanged = false;
        }

        void DisposeArrays()
        {
            _indices.Dispose();
            _verts.Dispose();
            _vertData.Dispose();
        }

        public void Dispose()
        {
            _tileDataJob.Complete();

            DisposeArrays();

            _vertexDescriptors.Dispose();
        }
    }

}