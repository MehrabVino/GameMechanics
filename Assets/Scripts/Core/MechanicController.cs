using System.Collections.Generic;
using UnityEngine;

namespace MechanicGames.Core
{
	/// <summary>
	/// Simple controller that manages enabling/disabling pluggable mechanics.
	/// </summary>
	public sealed class MechanicController : MonoBehaviour
	{
		[SerializeField]
		private List<MonoBehaviour> mechanicBehaviours = new List<MonoBehaviour>();

		private readonly List<IGameMechanic> mechanics = new List<IGameMechanic>();

		private void Awake()
		{
			mechanics.Clear();
			for (int i = 0; i < mechanicBehaviours.Count; i++)
			{
				MonoBehaviour behaviour = mechanicBehaviours[i];
				if (behaviour == null)
				{
					continue;
				}

				if (behaviour is IGameMechanic gameMechanic)
				{
					mechanics.Add(gameMechanic);
				}
				else
				{
					Debug.LogWarning($"Referenced behaviour '{behaviour.name}' does not implement IGameMechanic.", behaviour);
				}
			}
		}

		private void Start()
		{
			for (int i = 0; i < mechanics.Count; i++)
			{
				mechanics[i].InitializeMechanic();
				mechanics[i].SetMechanicActive(true);
			}
		}

		public void SetAllMechanicsActive(bool isActive)
		{
			for (int i = 0; i < mechanics.Count; i++)
			{
				mechanics[i].SetMechanicActive(isActive);
			}
		}
	}
}




