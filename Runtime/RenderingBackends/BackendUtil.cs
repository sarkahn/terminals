using Sark.Common.GridUtil;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace Sark.Terminals
{
    public static class BackendUtility
    {
        static public void RebuildTileDataRange(int min, int max,
        TileData tiles,
        NativeArray<VertTileData> vertData)
        {
            float2 uvSize = 1f / 16f;
            float2 uvRight = new float2(uvSize.x, 0);
            float2 uvUp = new float2(0, uvSize.y);

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
                float2 uvOrigin = (float2)glyphIndex * uvSize;

                var fg = tile.fgColor;
                var bg = tile.bgColor;

                vertData[vi + 0] = new VertTileData
                {
                    UV = uvOrigin + uvUp,
                    FGColor = FromColor(fg),
                    BGColor = FromColor(bg)
                };
                vertData[vi + 1] = new VertTileData
                {
                    UV = uvOrigin + uvRight + uvUp,
                    FGColor = FromColor(fg),
                    BGColor = FromColor(bg)
                };
                vertData[vi + 2] = new VertTileData
                {
                    UV = uvOrigin,
                    FGColor = FromColor(fg),
                    BGColor = FromColor(bg)
                };
                vertData[vi + 3] = new VertTileData
                {
                    UV = uvOrigin + uvRight,
                    FGColor = FromColor(fg),
                    BGColor = FromColor(bg)
                };
            }
        }

        static public void RebuildVertsRange(int min, int max, int2 size, float2 tileSize,
            NativeArray<float3> verts,
            NativeArray<ushort> indices)
        {
            Assert.IsFalse(math.any(tileSize == 0));
            Assert.IsFalse(math.any(size == 0));

            float3 worldSize = new float3(size * tileSize, 0);
            float3 start = -worldSize / 2f;

            float3 right = new float3(tileSize.x, 0, 0);
            float3 up = new float3(0, tileSize.y, 0);

            //Debug.Log($"Building verts. Tilesize {tileSize}");

            for (int tileIndex = min; tileIndex < max; ++tileIndex)
            {
                int vi = tileIndex * 4; // Vert Index
                int ii = tileIndex * 6; // Triangle Index

                float2 xy = Grid2D.IndexToPos(tileIndex, size.x);
                xy.x *= tileSize.x;
                xy.y *= tileSize.y;

                float3 p = new float3(xy, 0);
                // 0---1
                // | / | 
                // 2---3
                verts[vi + 0] = start + p + up;
                verts[vi + 1] = start + p + right + up;
                verts[vi + 2] = start + p;
                verts[vi + 3] = start + p + right;

                indices[ii + 0] = (ushort)(vi + 0);
                indices[ii + 1] = (ushort)(vi + 1);
                indices[ii + 2] = (ushort)(vi + 2);
                indices[ii + 3] = (ushort)(vi + 3);
                indices[ii + 4] = (ushort)(vi + 2);
                indices[ii + 5] = (ushort)(vi + 1);
            }

        }

        static float4 FromColor(Color c) =>
            new float4(c.r, c.g, c.b, c.a);

        [BurstCompile]
        public struct TileDataJob : IJobParallelForBatch
        {
            [ReadOnly]
            public TileData Tiles;

            [NativeDisableParallelForRestriction]
            public NativeArray<VertTileData> VertData;

            public void Execute(int startIndex, int count)
            {
                RebuildTileDataRange(startIndex, startIndex + count, Tiles, VertData);
            }
        }

        [BurstCompile]
        public struct VertsJob : IJobParallelForBatch
        {
            public int2 Size;
            public float2 TileSize;

            [NativeDisableParallelForRestriction]
            [WriteOnly]
            public NativeArray<float3> Verts;

            [NativeDisableParallelForRestriction]
            [WriteOnly]
            public NativeArray<ushort> Indices;

            public void Execute(int startIndex, int count)
            {
                RebuildVertsRange(startIndex, 
                    startIndex + count, Size, TileSize, Verts, Indices);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct VertTileData
        {
            public float2 UV;
            public float4 FGColor;
            public float4 BGColor;
        }
    }


}