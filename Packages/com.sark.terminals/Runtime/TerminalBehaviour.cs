using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using static Sark.Terminals.TerminalExtensions.TerminalExtension;

namespace Sark.Terminals
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TerminalBehaviour : MonoBehaviour
    {
        SimpleTerminal _term;

        [SerializeField]
        int2 _size = new int2(80,40);

        [SerializeField]
        float2 _tileSize = 1;

        private void OnEnable()
        {
            _term = new SimpleTerminal(_size.x, _size.y, Allocator.Persistent);

            GetComponent<MeshFilter>().sharedMesh = _term.Mesh;
            var renderer = GetComponent<MeshRenderer>();
            if( renderer.sharedMaterial == null )
            {
                renderer.sharedMaterial = Resources.Load<Material>("TerminalMaterial");
            }

            _term.Tiles.ScrambleJob().Run();
            _term.SetDirty();
        }

        private void OnDisable()
        {
            _term.Dispose();
        }

        public void Resize(int w, int h) => _term.Resize(w, h);

        public void Set(int x, int y, Tile t) => _term.Set(x, y, t);

        public void Set(int x, int y, char c) => _term.Set(x, y, c);

        public void Set(int x, int y, Color fgColor, char c) => 
            _term.Set(x, y, fgColor, c);

        public void Set(int x, int y, Color fgColor, Color bgColor, char c) =>
            _term.Set(x, y, fgColor, bgColor, c);

        /// <summary>
        /// Draw a circle! Note: As fun as this is, it's pretty slow.
        /// </summary>
        public void Circle(int x, int y, int radius) => _term.Circle(x, y, radius);

        public void Print(int x, int y, string str) => _term.Print(x, y, str);

        public NativeArray<Tile> ReadTiles(int x, int y, int len, 
            Allocator allocator)
        {
            return _term.ReadTiles(x, y, len, allocator);
        }

        public Tile Get(int x, int y) => _term.Get(x, y);

        public void ClearScreen() => _term.ClearScreen();

        /// <summary>
        /// Force the terminal to refresh on it's next update.
        /// </summary>
        public void SetDirty() => _term.SetDirty();

        void Update()
        {
            _term.EarlyUpdate();
        }

        public void LateUpdate()
        {
            _term.LateUpdate();
        }

        /// <summary>
        /// Immediately process tile-to-mesh data and upload it to the mesh.
        /// 
        /// Does nothing if the terminal state hasn't changed.
        /// </summary>
        public void ImmediateUpdate()
        {
            _term.ImmediateUpdate();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!isActiveAndEnabled || !Application.isPlaying)
                return;

            _term.Resize(_size.x, _size.y);
            _term.WithTileSize(_tileSize);

            _term.Tiles.ScrambleJob().Run();
            _term.SetDirty();
        }
#endif
    }
}