using System;
using DN.Service;
using QTea.CallbackService;
using UnityEngine;

namespace QTea
{
	[Service]
	public class UnityCallbackService
	{
		public Action OnUpdate;
		public Action OnLateUpdate;

		public UnityCallbackService()
		{
			GameObject callbackObject = new GameObject("[Callback Service] yeah");
			var callbackComp = callbackObject.AddComponent<UnityCallbackObject>();
			UnityEngine.Object.DontDestroyOnLoad(callbackObject);

			callbackComp.OnUpdate += () => OnUpdate?.Invoke();
			callbackComp.OnLateUpdate += () => OnLateUpdate?.Invoke();
		}
	}
}
