using UnityEngine;

namespace MechanicGames.Match3
{
	/// <summary>
	/// Narrow context to decouple view from mechanic implementation.
	/// </summary>
	public interface IMatch3Context
	{
		IMatch3BoardReadOnly Board { get; }
		Match3Theme Theme { get; }
		float CellSize { get; }
		Vector3 BoardOriginWorld { get; }
	}
}



