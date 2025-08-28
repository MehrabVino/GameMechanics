using System.Collections.Generic;
using UnityEngine;

namespace MechanicGames.Core
{
	/// <summary>
	/// Minimal non-threadsafe pool for GameObjects of a single prototype.
	/// Instantiates on demand up to a soft cap; returns via Deactivate.
	/// </summary>
	public sealed class SimpleGameObjectPool
	{
		private readonly GameObject prototype;
		private readonly Transform parent;
		private readonly Stack<GameObject> inactive = new Stack<GameObject>();
		private readonly List<GameObject> active = new List<GameObject>();

		public SimpleGameObjectPool(GameObject prototype, Transform parent)
		{
			this.prototype = prototype;
			this.parent = parent;
		}

		public GameObject Activate()
		{
			GameObject go = inactive.Count > 0 ? inactive.Pop() : Object.Instantiate(prototype, parent);
			go.transform.SetParent(parent, false);
			go.SetActive(true);
			active.Add(go);
			return go;
		}

		public void Deactivate(GameObject go)
		{
			if (go == null) return;
			go.SetActive(false);
			active.Remove(go);
			inactive.Push(go);
		}

		public void DeactivateAll()
		{
			for (int i = active.Count - 1; i >= 0; i--)
			{
				Deactivate(active[i]);
			}
		}
	}
}


