using UnityEngine;
using UnityEngine.UI;

namespace MechanicGames.BlockBlast
{
	public sealed class BlockBlastHUD : MonoBehaviour
	{
		[SerializeField]
		private BlockBlastMechanic mechanic;

		[SerializeField]
		private Text scoreText;

		[SerializeField]
		private string scoreFormat = "BlockBlast Score: {0}";

		private void Awake()
		{
			if (mechanic == null)
			{
				mechanic = FindFirstObjectByType<BlockBlastMechanic>();
			}
		}

		private void Update()
		{
			if (mechanic == null || scoreText == null) return;
			scoreText.text = string.Format(scoreFormat, mechanic.Score);
		}
	}
}


