using UnityEngine;

namespace MechanicGames.Match3
{
	/// <summary>
	/// Theme data for Match-3 visuals.
	/// You can define explicit Tile Definitions (preferred), or fallback to arrays below.
	/// </summary>
	[CreateAssetMenu(fileName = "Match3Theme", menuName = "MechanicGames/Match3 Theme", order = 0)]
	public sealed class Match3Theme : ScriptableObject
	{
		[System.Serializable]
		public sealed class TileDefinition
		{
			public string id;
			public Sprite sprite;
			public Color color = Color.white;
		}

		[Header("Preferred: Tile Definitions (ordered)")]
		public TileDefinition[] tileDefinitions;

		[Header("Tiles")]
		public Sprite[] tileSprites;
		public Color[] tileColors;

		[Header("Background")]
		public Sprite backgroundSprite;
		public Color backgroundColor = Color.black;

		public int TileTypeCount
		{
			get
			{
				int defs = tileDefinitions != null ? tileDefinitions.Length : 0;
				if (defs > 0)
				{
					return defs;
				}
				int sprites = tileSprites != null ? tileSprites.Length : 0;
				int colors = tileColors != null ? tileColors.Length : 0;
				int max = Mathf.Max(sprites, colors);
				return Mathf.Max(1, max);
			}
		}
	}
}



