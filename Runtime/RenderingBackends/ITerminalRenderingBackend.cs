using Sark.Terminals;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Sark.Terminals
{
	public interface ITerminalRenderingBackend : System.IDisposable
	{
		/// <summary>
		/// Schedule jobs to update internal mesh data.
		/// </summary>
		JobHandle ScheduleUpdateData(TileData tiles, JobHandle inputDeps);

		/// <summary>
		/// Complete any running jobs and upload data to the mesh.
		/// </summary>
		void UploadToMesh(Mesh mesh);

		/// <summary>
		/// Immediately process tile data and upload it to  the mesh.
		/// </summary>
		void UpdateDataAndUploadToMesh(TileData tiles, Mesh mesh);

		void Resize(int w, int h);
	}
}