
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Profiling;

namespace Sark.Terminals
{
    public class BackendProfiling : MonoBehaviour
    {
        SimpleMeshBackend _backend;
        //MeshDataJobBackend _backend;
        TileData _tiles;
        Mesh _mesh;

        [SerializeField]
        int _size = 10;

        // Start is called before the first frame update
        void OnEnable()
        {
            _mesh = new Mesh();
            OnResize();
        }

        void OnDisable()
        {
            _backend.Dispose();
            _tiles.Dispose();
        }

        void Update()
        {
            Profiler.BeginSample("Backend update");
            _backend.UpdateDataAndUploadToMesh(_tiles, _mesh);
            Profiler.EndSample();
        }

        void OnResize()
        {
            _backend?.Dispose();
            if (_tiles.IsCreated)
                _tiles.Dispose();


            _backend = new SimpleMeshBackend(_size, _size, Allocator.Persistent);
            //_backend = new MeshDataJobBackend(_size, _size, Allocator.Persistent);
            _tiles = new TileData(_size, _size, Allocator.Persistent);
        }

        private void OnValidate()
        {
            if (!isActiveAndEnabled || !Application.isPlaying)
                return;

            if(_size != _tiles.Width )
            {
                OnResize();
            }
        }
    }
}