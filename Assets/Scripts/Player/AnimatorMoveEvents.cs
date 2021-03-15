using UnityEngine;
using System;

namespace QTea
{
	/// <summary>
	/// ADD CLASS SUMMARY!
	/// </summary>
	public class AnimatorMoveEvents : MonoBehaviour
	{
  
        public void OnAnimatorMove()
        {
			var velocity = GetComponent<Animator>().velocity;
			Debug.Log(velocity);
        }
    }
}
