using Sark.Common;
using Sark.Terminals.TerminalExtensions;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

using Sark.Common.CameraExtensions;
using System.Collections;

// This code is based on the RLTK Terminals made for rust by TheBracket:
// https://github.com/thebracket/bracket-lib/tree/master/bracket-terminal
// https://github.com/thebracket/bracket-lib/blob/master/LICENSE

namespace Sark.Terminals
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class TerminalBehaviour : MonoBehaviour
    {
        const string DefaultMaterialPath = "Terminal8x8";

        SimpleTerminal _term;

        [SerializeField]
        int2 _size = new int2(80,40);

        [SerializeField]
        [HideInInspector]
        MeshRenderer _renderer;

        Material _originalMat;

        public Material Material
        {
            get => _renderer.sharedMaterial;
            set
            {
                _renderer.sharedMaterial = value;
                _term.WithTileSize(GetTileSize());
            }
        }

        public TileData Tiles => _term.Tiles;

        [SerializeField]
        bool _pixelSnap = true;

        public int2 Size => _term.Size;
        public int Width => _term.Width;
        public int Height => _term.Height;
        public int CellCount => _term.CellCount;

        private void OnEnable()
        {
            _term = new SimpleTerminal(_size.x, _size.y, Allocator.Persistent);

            _renderer = GetComponent<MeshRenderer>();
            var filter = GetComponent<MeshFilter>();
            filter.sharedMesh = _term.Mesh;
            filter.hideFlags = HideFlags.HideInInspector;
            _renderer.hideFlags = HideFlags.HideInInspector;

            if(Material == null)
            {
                Material = Resources.Load<Material>(DefaultMaterialPath);
            }

            _originalMat = _renderer.sharedMaterial;

            _term.WithTileSize(GetTileSize());
        }

        private void OnDisable()
        {
            _term.Dispose();

            _renderer.sharedMaterial = _originalMat;
        }

        public TerminalBehaviour WithFont(string fontName)
        {
            var mat = Resources.Load<Material>(fontName);

            return WithFont(mat);
        }

        public TerminalBehaviour WithFont(Material mat)
        {
            _renderer.sharedMaterial = mat;

            float2 tileSize = GetTileSize();
            //Debug.Log($"Setting new tilesize based on {tex.name}: {tileSize}");
            _term.WithTileSize(tileSize);

            return this;
        }

        public TerminalBehaviour WithSize(int w, int h)
        {
            _size = new int2(w, h);
            _term.Resize(w, h);
            return this;
        }

        public void Resize(int w, int h)
        {
            _size = new int2(w, h);
            _term.Resize(w, h);
        }

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

        float2 GetTileSize()
        {
            var tex = _renderer.sharedMaterial.mainTexture;
            float w = tex.width / 16;
            float h = tex.height / 16;
            w = w / h;
            h = 1;
            return new float2(w, h);
        }

        public bool IsInBounds(int2 tileIndex) =>
            _term.IsInBounds(tileIndex);

        public int2 WorldPosToTileIndex(float3 worldPos)
        {
            float3 p = transform.InverseTransformPoint(worldPos);
            return _term.PositionToTileIndex(p);
        }

        float2 GetPixelWorldSize()
        {
            var tex = _renderer.sharedMaterial.mainTexture;
            float w = 1f / (tex.width / 16);
            float h = 1f / (tex.height / 16);
            return new float2(w, h);
        }

        public float3 GetWorldSize()
        {
            var tileSize = GetTileSize();
            var p = new float3(_size, .1f);
            p.xy *= tileSize;
            return p;
        }

        public TerminalBehaviour WithScreenPosition(float x, float y, 
            float alignX, float alignY)
        {
            var size = GetWorldSize();
            var p = Camera.main.GetAlignedViewportPosition(
                new float2(x, y), new float2(alignX, alignY), size.xy);

            transform.position = p;
            return this;
        }

        void Update()
        {
            _term.EarlyUpdate();

            if(_pixelSnap)
            {
                var pixelSize = GetPixelWorldSize();
                float size = math.max(pixelSize.x, pixelSize.y);
                float3 p = transform.position;
                p.xy = MathUtil.roundedincrement(p.xy, size);
                transform.position = p;
            }

#if UNITY_EDITOR
            CheckForEditorChanges();
#endif
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
            if (!isActiveAndEnabled)
                return;

            if (!Application.isPlaying)
                _originalMat = _renderer.sharedMaterial;

            if (_term == null || _renderer == null)
                return;

            _term.Resize(_size.x, _size.y);
            if (_renderer != null 
                && _renderer.sharedMaterial != null 
                && _renderer.sharedMaterial.mainTexture != null)

            WithFont(_renderer.sharedMaterial);
        }

        Material _lastMat = null;

        void CheckForEditorChanges()
        {
            if(Material != _lastMat)
            {
                _lastMat = Material;
                _originalMat = Material;

                var tileSize = GetTileSize();
                _term.WithTileSize(tileSize);
            }
        }
#endif

        public static TerminalBehaviour CreateAtScreenPosition(string name, 
            int sizeX, int sizeY,
            float x, float y, float alignX, float alignY)
        {
            var go = new GameObject(name, typeof(TerminalBehaviour));
            var term = go.GetComponent<TerminalBehaviour>().WithSize(sizeX, sizeY);

            term.WithScreenPosition(x, y, alignX, alignY);

            //term.Tiles.ScrambleJob().Run();
            //term.SetDirty();

            return term;
        }

    }
}