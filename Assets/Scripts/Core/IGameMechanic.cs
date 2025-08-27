using UnityEngine;

namespace MechanicGames.Core
{
	/// <summary>
	/// Minimal interface for pluggable mechanics.
	/// </summary>
	public interface IGameMechanic
	{
		/// <summary>
		/// Initialize or reset mechanic state. Called on enable or scene start.
		/// </summary>
		void InitializeMechanic();

		/// <summary>
		/// Enable or disable the mechanic's processing and visuals.
		/// </summary>
		void SetMechanicActive(bool isActive);
	}
}




