using UnityEngine;
using System;

namespace QTea.CallbackService
{
	public class UnityCallbackObject : MonoBehaviour
	{
		public Action OnUpdate;
		public Action OnLateUpdate;

		protected void Update()
		{
			OnUpdate?.Invoke();
		}

		protected void LateUpdate()
		{
			OnLateUpdate?.Invoke();
		}
	}
}
