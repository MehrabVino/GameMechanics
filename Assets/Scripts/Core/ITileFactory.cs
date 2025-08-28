using UnityEngine;

namespace MechanicGames.Core
{
	public interface ITileFactory
	{
		Renderer CreateTile(Transform parent);
	}
}


