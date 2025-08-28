using UnityEngine;

namespace MechanicGames.Core
{
	/// <summary>
	/// Builds a single combined mesh for a cell background grid to minimize draw calls.
	/// </summary>
	public static class GridBackgroundMesh
	{
		public static Mesh BuildGridQuads(int width, int height, float cellSize, Vector3 origin, float z)
		{
			int quadCount = width * height;
			int vertCount = quadCount * 4;
			int indexCount = quadCount * 6;
			Vector3[] vertices = new Vector3[vertCount];
			int[] indices = new int[indexCount];
			Vector2[] uvs = new Vector2[vertCount];

			int v = 0;
			int t = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					float cx = origin.x + (x + 0.5f) * cellSize;
					float cy = origin.y + (y + 0.5f) * cellSize;
					float hs = cellSize * 0.5f;
					vertices[v + 0] = new Vector3(cx - hs, cy - hs, z);
					vertices[v + 1] = new Vector3(cx - hs, cy + hs, z);
					vertices[v + 2] = new Vector3(cx + hs, cy + hs, z);
					vertices[v + 3] = new Vector3(cx + hs, cy - hs, z);
					uvs[v + 0] = new Vector2(0f, 0f);
					uvs[v + 1] = new Vector2(0f, 1f);
					uvs[v + 2] = new Vector2(1f, 1f);
					uvs[v + 3] = new Vector2(1f, 0f);
					indices[t + 0] = v + 0;
					indices[t + 1] = v + 1;
					indices[t + 2] = v + 2;
					indices[t + 3] = v + 0;
					indices[t + 4] = v + 2;
					indices[t + 5] = v + 3;
					v += 4;
					t += 6;
				}
			}

			Mesh mesh = new Mesh();
			mesh.indexFormat = (vertCount > 65000) ? UnityEngine.Rendering.IndexFormat.UInt32 : UnityEngine.Rendering.IndexFormat.UInt16;
			mesh.vertices = vertices;
			mesh.triangles = indices;
			mesh.uv = uvs;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			return mesh;
		}
	}
}


