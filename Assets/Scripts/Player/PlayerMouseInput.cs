using CMF;
using DN.Service;
using System;
using UnityEngine;

namespace QTea
{
	public class PlayerMouseInput : MonoBehaviour
	{
		public Action<Vector2> OnLook;

		private PlayerInputService playerInput;

		protected void Awake()
		{
			playerInput = ServiceLocator.Locate<PlayerInputService>();
		}

		protected void OnEnable()
		{
			//playerInput.Player.Look.performed += OnLookPerformed;
		}
		
		protected void OnDisable()
		{
			playerInput.Player.Look.performed -= OnLookPerformed;
		}

		protected void LateUpdate()
		{
			OnLook?.Invoke(playerInput.Player.Look.ReadValue<Vector2>());
		}

		private void OnLookPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
			=> OnLook?.Invoke(context.ReadValue<Vector2>());
	}
}
