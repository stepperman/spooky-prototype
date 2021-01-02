using UnityEngine;
using System;
using CMF;
using UnityEngine.InputSystem;
using DN.Service;

namespace QTea
{
	public class PlayerKeyInput : MonoBehaviour
	{
		public event Action<Vector2> OnMove;
		public event Action OnJump;

		private PlayerInputService playerInput;

		protected void Awake()
		{
			playerInput = ServiceLocator.Locate<PlayerInputService>();
		}

		protected void OnEnable()
		{
			playerInput.Player.Move.performed += OnMovePerformed;
			playerInput.Player.Move.canceled += OnMoveCanceled;
			playerInput.Player.Jump.performed += OnJumpPerformed;
		}


		protected void OnDisable()
		{
			playerInput.Player.Move.performed -= OnMovePerformed;
			playerInput.Player.Move.canceled -= OnMoveCanceled;
			playerInput.Player.Jump.performed -= OnJumpPerformed;
		}

		private void OnMovePerformed(InputAction.CallbackContext context) => OnMove?.Invoke(context.ReadValue<Vector2>());
		private void OnMoveCanceled(InputAction.CallbackContext obj) => OnMove?.Invoke(Vector2.zero);
		private void OnJumpPerformed(InputAction.CallbackContext obj) => OnJump?.Invoke();
	}
}
