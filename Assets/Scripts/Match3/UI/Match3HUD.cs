using UnityEngine;
using UnityEngine.UI;

namespace MechanicGames.Match3
{
	/// <summary>
	/// Simple Unity UI HUD to display the current score for the active Match3Mechanic.
	/// Create a Canvas in scene, add this component to a child with a Text, and assign references.
	/// </summary>
	public sealed class Match3HUD : MonoBehaviour
	{
		[SerializeField]
		private Match3Mechanic mechanic;

		[SerializeField]
		private Text scoreText;

		[SerializeField]
		private string scoreFormat = "Score: {0}";

		private void Awake()
		{
			if (mechanic == null)
			{
				mechanic = FindObjectOfType<Match3Mechanic>();
			}
		}

		private void Update()
		{
			if (mechanic == null || scoreText == null)
			{
				return;
			}
			scoreText.text = string.Format(scoreFormat, mechanic.Score);
		}
	}
}



