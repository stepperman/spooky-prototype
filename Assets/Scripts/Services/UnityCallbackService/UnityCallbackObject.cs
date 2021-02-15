using UnityEngine;
using System;

namespace QTea.CallbackService
{
	public class UnityCallbackObject : MonoBehaviour
	{
		public Action OnUpdate;
		public Action OnLateUpdate;
		public Action OnDestroyEvent;

		protected void Update()
		{
			OnUpdate?.Invoke();
		}

		protected void LateUpdate()
		{
			OnLateUpdate?.Invoke();
		}

		protected void OnDestroy()
		{
			OnDestroyEvent?.Invoke();
		}
	}
}
