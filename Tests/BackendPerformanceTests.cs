using NUnit.Framework;
using Sark.Terminals;
using Sark.Terminals.TerminalExtensions;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.PerformanceTesting;
using UnityEngine;

[TestFixture]
public class BackendPerformanceTests
{
    [Test, Performance]
    [TestCase(10)]
    [TestCase(50)]
    [TestCase(100)]
    [TestCase(150)]
    [TestCase(200)]
    [TestCase(250)]
    [TestCase(300)]
    public void SimpleResizeTest(int size)
    {
        ResizeTest(size, GetSimpleBackend);
    }

    [Test, Performance]
    [TestCase(10)]
    [TestCase(50)]
    [TestCase(100)]
    [TestCase(150)]
    [TestCase(200)]
    [TestCase(250)]
    [TestCase(300)]
    public void MeshDataResizeTest(int size)
    {
        ResizeTest(size, GetMeshDataBackend);
    }

    [Test, Performance]
    [TestCase(10)]
    [TestCase(50)]
    [TestCase(100)]
    [TestCase(150)]
    [TestCase(200)]
    [TestCase(250)]
    [TestCase(300)]
    public void SimpleWriteTest(int size)
    {
        WriteTest(size, GetSimpleBackend);
    }

    [Test, Performance]
    [TestCase(10)]
    [TestCase(50)]
    [TestCase(100)]
    [TestCase(150)]
    [TestCase(200)]
    [TestCase(250)]
    [TestCase(300)]
    public void MeshDataWriteTest(int size)
    {
        WriteTest(size, GetMeshDataBackend);
    }

    SimpleMeshBackend GetSimpleBackend(int size, Allocator allocator)
    {
        return new SimpleMeshBackend(size, size, allocator);
    }

    DataArrayMeshBackend GetMeshDataBackend(int size, Allocator allocator)
    {
        return new DataArrayMeshBackend(size, size);
    }

    public void ResizeTest<T>(int size, System.Func<int, Allocator, T> GetBackendFunc)
        where T : ITerminalRenderingBackend
    {
        Mesh mesh = default;
        TileData tiles = default;
        T backend = default;

        Measure.Method(() =>
        {
            backend.Resize(size, size);
            backend.UpdateDataAndUploadToMesh(tiles, mesh);
        })
            .WarmupCount(3)
            .MeasurementCount(200)
            .SetUp(() =>
            {
                mesh = new Mesh();
                tiles = new TileData(size, size, Allocator.TempJob);
                backend = GetBackendFunc(size - 10, Allocator.TempJob);
            })
            .CleanUp(() =>
            {
                tiles.Dispose();
                backend.Dispose();
            })
            .Run();
    }

    public void WriteTest<T>(int size, System.Func<int, Allocator, T> GetBackendFunc)
    where T : ITerminalRenderingBackend
    {
        Mesh mesh = default;
        TileData tiles = default;
        T backend = default;

        Measure.Method(() =>
        {
            backend.UpdateDataAndUploadToMesh(tiles, mesh);
        })
            .WarmupCount(3)
            .MeasurementCount(200)
            .SetUp(() =>
            {
                mesh = new Mesh();
                tiles = new TileData(size, size, Allocator.TempJob);
                backend = GetBackendFunc(size, Allocator.TempJob);
                backend.UpdateDataAndUploadToMesh(tiles, mesh);
            })
            .CleanUp(() =>
            {
                tiles.Dispose();
                backend.Dispose();
            })
            .Run();
    }


}
