using NUnit.Framework;
using Sark.Common.GridUtil;
using Sark.Terminals;
using Sark.Terminals.TerminalExtensions;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;


public class JobsBackendTests
{
    [Test]
    public void MeshSizeTest()
    {
        Mesh m = new Mesh();

        var backend = new DataArrayMeshBackend(10, 10);
        var tiles = new TileData(10, 10, Allocator.Temp);

        backend.UpdateDataAndUploadToMesh(tiles, m);

        var verts = m.vertices;
        Assert.AreEqual(10 * 10 * 4, verts.Length);
    }

    [Test]
    public void TileToMeshColorsTest()
    {
        var m = new Mesh();
        var backend = new DataArrayMeshBackend(10, 10);
        var t = new TileData(10, 10, Allocator.Temp);

        var red = new Vector4(1, 0, 0, 1);
        var green = new Vector4(0, 1, 0, 1);
        var blue = new Vector4(0, 0, 1, 1);
        List<Vector4> colorUVs = new List<Vector4>();

        t.Set(0, 0, Color.red, Color.blue, ' ');
        backend.UpdateDataAndUploadToMesh(t, m);

        // FG Colors
        m.GetUVs(1, colorUVs);
        Assert.AreEqual(red, colorUVs[0]);
        Assert.AreEqual(red, colorUVs[1]);
        Assert.AreEqual(red, colorUVs[2]);
        Assert.AreEqual(red, colorUVs[3]);

        colorUVs.Clear();
        // BG Colors
        m.GetUVs(2, colorUVs);
        Assert.AreEqual(blue, colorUVs[0]);
        Assert.AreEqual(blue, colorUVs[1]);
        Assert.AreEqual(blue, colorUVs[2]);
        Assert.AreEqual(blue, colorUVs[3]);

        t.Set(3, 3, Color.green, Color.red, 'a');
        backend.UpdateDataAndUploadToMesh(t, m);

        colorUVs.Clear();
        m.GetUVs(1, colorUVs);

        int i = Grid2D.PosToIndex(3, 3, 10) * 4;
        Assert.AreEqual(green, colorUVs[i + 0]);
        Assert.AreEqual(green, colorUVs[i + 1]);
        Assert.AreEqual(green, colorUVs[i + 2]);
        Assert.AreEqual(green, colorUVs[i + 3]);
    }

    [Test]
    public void JobTest()
    {

        Mesh mesh = new Mesh();
        var backend = new SimpleMeshBackend(10, 10, Allocator.TempJob);
        var tiles = new TileData(10, 10, Allocator.TempJob);

        backend.ScheduleUpdateData(tiles);
        backend.UploadToMesh(mesh);

        Assert.AreEqual(10 * 10 * 4, mesh.vertexCount);

        tiles.Dispose();
        backend.Dispose();
    }
} 
