using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace QTea
{
	/// <summary>
	/// ADD CLASS SUMMARY!
	/// </summary>
	public class TurnControllerTowardsCamera : MonoBehaviour
	{
		[Serializable]
		private class SmoothOptions
        {
			public enum Smoothing
            {
				Lerp,
				SmoothDamp
            }

			public Smoothing smoothType = Smoothing.SmoothDamp;
			[HideIf("@smoothType != Smoothing.Lerp")] public float lerpSpeed = 15.0f;
			[HideIf("@smoothType != Smoothing.SmoothDamp")] public float velocity = 0.2f;
			[HideInInspector] public float lastSmoothing;
			[HideInInspector] public float lastVelocity;
        }

		[SerializeField] new private Transform camera;
		[SerializeField] private Transform model;

		[SerializeField] private bool useSmoothing = false;

		[SerializeField, HideIfGroup("@!useSmoothing"), HideLabel] 
		private SmoothOptions smoothOptions;

		private void Update()
		{
			Vector3 cameraAngles = camera.eulerAngles;
			Vector3 controllerAngles = model.eulerAngles;
			controllerAngles.y = SmoothRotation(cameraAngles.y);
			model.eulerAngles = controllerAngles;
		}

		private float SmoothRotation(float rotation)
        {
			if (useSmoothing)
			{
				float newRotation = Smooth(rotation);
				smoothOptions.lastSmoothing = newRotation;
				return newRotation;
			}

            return rotation;
        }

		private float Smooth(float rotation) => smoothOptions.smoothType switch
        {
            SmoothOptions.Smoothing.Lerp => Mathf.LerpAngle(smoothOptions.lastSmoothing, rotation, Time.deltaTime * smoothOptions.lerpSpeed),
            SmoothOptions.Smoothing.SmoothDamp => Mathf.SmoothDampAngle(smoothOptions.lastSmoothing, rotation, ref smoothOptions.lastVelocity, smoothOptions.velocity)
        };
	}
}
