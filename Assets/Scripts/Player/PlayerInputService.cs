using DN.Service;
using System;

namespace QTea
{
	[Service]
	public class PlayerInputService
	{
		public PlayerInput PlayerInput { get; private set; }
		public PlayerInput.PlayerActions Player { get; private set; }

		public PlayerInputService()
		{
			PlayerInput = new PlayerInput();
			PlayerInput.Enable();
			Player = PlayerInput.Player;
		}
	}
}
