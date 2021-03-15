using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CMF
{
	//This character movement input class is an example of how to get input from a keyboard to control the character;
    public class CharacterKeyboardInput : CharacterInput
    {
		public string horizontalInputAxis = "Horizontal";
		public string verticalInputAxis = "Vertical";
		public KeyCode jumpKey = KeyCode.Space;

		//If this is enabled, Unity's internal input smoothing is bypassed;
		public bool useRawInput = true;

		[SerializeField, HideIfGroup("useRawInput")] private bool useSmoothing = true;
		[SerializeField, HideIfGroup("useRawInput")] private float smoothing = 2.5f;

		private float previousHorizontalInput;
		private float previousVerticalInput;

        public override float GetHorizontalMovementInput()
		{
			if(useRawInput)
				return Input.GetAxisRaw(horizontalInputAxis);
			else
				return SmoothInput(Input.GetAxis(horizontalInputAxis), ref previousHorizontalInput);
		}

		private float SmoothInput(float value, ref float curInput)
        {
			if(useSmoothing)
            {
				float newV = Mathf.Lerp(curInput, value, Time.deltaTime * smoothing);
				curInput = newV;
				return newV;
            }

			return value;
        }

		public override float GetVerticalMovementInput()
		{
			if(useRawInput)
				return Input.GetAxisRaw(verticalInputAxis);
			else
				return SmoothInput(Input.GetAxis(verticalInputAxis), ref previousVerticalInput);
		}

		public override bool IsJumpKeyPressed()
		{
			return Input.GetKey(jumpKey);
		}
    }
}
