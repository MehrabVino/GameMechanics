using UnityEngine;

namespace MechanicGames.Core
{
	[CreateAssetMenu(menuName = "MechanicGames/Quad Tile Factory")]
	public sealed class QuadTileFactory : ScriptableObject, ITileFactory
	{
		[SerializeField]
		private Material material;

		[SerializeField]
		private Color color = Color.white;

		public Renderer CreateTile(Transform parent)
		{
			GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
			quad.transform.SetParent(parent, false);
			Collider col = quad.GetComponent<Collider>();
			if (col != null) DestroyImmediate(col);
			MeshRenderer mr = quad.GetComponent<MeshRenderer>();
			if (mr == null) mr = quad.AddComponent<MeshRenderer>();
			Material mat = material != null ? new Material(material) : new Material(Shader.Find("Unlit/Color"));
			mat.color = color;
			mr.sharedMaterial = mat;
			return mr;
		}
	}
}


