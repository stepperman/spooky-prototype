using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace QTea.Procedural
{
	/// <summary>
	/// 
	/// </summary>
	public class Walking : MonoBehaviour
	{
		[Serializable]
		public class FootSettings
		{
			[Required]
			public Transform leftFootIK;
			[Required]
			public Transform leftFootCenter;
			[Required]
			public Transform rightFootIK;
			[Required]
			public Transform rightFootCenter;
			public Foot startingFoot;
			public float footLiftHeight = 0.2f;
			public float footStepSize = 0.2f; // Units in front of body or after body.
			public float footBackDistance = 0.05f; // footStepDistance + footBackDistance is for the back foot.
			public float standStillTimeout = 0.5f; // if the player stands still for this amount of time the feet will reset to a neutral position (hopefully naturally).
		}

		[Flags]
		public enum Foot
		{
			LEFT = 1,
			RIGHT = 2,
			ANY = 3
		}

		[FoldoutGroup("Debug")]
		[SerializeField] private float moveSpeed = 1.4f;
		[FoldoutGroup("Debug")]
		[SerializeField] private bool showDebugGizmos = true;

		[FoldoutGroup("Foot Settings"), HideLabel]
		[SerializeField] private FootSettings footSettings; 

		[FoldoutGroup("Body")]
		[SerializeField] private Transform bodyTransform;
		[FoldoutGroup("Body")]
		[SerializeField] private float bodyBendAngle = 10f;
		[FoldoutGroup("Body")]
		[SerializeField] private float bodyStepHeight = 0.1f; // how much the body will raise in the highest walk position.

		private Vector3 nextFootPosition;
		private Vector3 leftFootPosition;
		private Vector3 rightFootPosition;
		private Foot currentMovingFoot;

		private void Start()
		{
			leftFootPosition = footSettings.leftFootIK.position;
			rightFootPosition = footSettings.rightFootIK.position;
			currentMovingFoot = Foot.LEFT;
			CalculateNextFootPosition(currentMovingFoot, transform.forward);
		}

		private void OnDrawGizmos()
		{
			if (!showDebugGizmos)
				return;

			Gizmos.DrawWireCube(nextFootPosition, new Vector3(0.3f, 0.3f));

			Gizmos.color = Color.blue;
			float footStepSize = footSettings.footStepSize;
			Gizmos.DrawWireSphere(footSettings.leftFootCenter.position, footStepSize);
			Gizmos.DrawWireSphere(footSettings.rightFootCenter.position, footStepSize);
		}

		private void Update()
		{
			if (Mathf.Approximately(moveSpeed, 0))
				return;
			Vector3 movingDir = transform.forward;
			transform.position += movingDir * moveSpeed * Time.deltaTime;

			Transform currentlyMovingFoot = currentMovingFoot == Foot.LEFT ? footSettings.leftFootIK : footSettings.rightFootIK;
			Transform groundedFoot = currentMovingFoot == Foot.RIGHT ? footSettings.leftFootIK : footSettings.rightFootIK;

			// the distance of the grounded foot is the distance of the moving foot to the end
			float groundFootDist = GetDistance(currentMovingFoot == Foot.RIGHT ? Foot.LEFT : Foot.RIGHT);
			Debug.Log($"{footSettings.footStepSize * footSettings.footStepSize / groundFootDist} {groundFootDist}");
			currentlyMovingFoot.position = Vector3.Lerp(currentlyMovingFoot.position, nextFootPosition, footSettings.footStepSize * footSettings.footStepSize / groundFootDist);
		}

		private void CalculateNextFootPosition(Foot foot, Vector3 moveDir)
		{
			Transform footIk = GetFootIK(foot);
			nextFootPosition = GetFootCenter(foot).position + moveDir * footSettings.footStepSize;
		}

		private void LateUpdate()
		{
			footSettings.leftFootIK.position = leftFootPosition;
			footSettings.rightFootIK.position = rightFootPosition;
		}

		public float GetDistance(Foot foot)
		{
			float Dist(Vector3 footPos, Vector3 centerPos) =>
				(footPos - centerPos).sqrMagnitude;

			float? dist = foot switch
			{
				Foot.LEFT => Dist(footSettings.leftFootIK.position, footSettings.leftFootCenter.position),
				Foot.RIGHT => Dist(footSettings.rightFootIK.position, footSettings.rightFootCenter.position),
				_ => null
			};

			if(dist == null)
			{
				Debug.LogError($"{foot} is not supported for this function.");
				return 0;
			}

			return dist.Value;
		}

		public Transform GetFootIK(Foot foot)
		{
			return foot switch
			{
				Foot.LEFT => footSettings.leftFootIK,
				Foot.RIGHT => footSettings.rightFootIK,
				_ => null
			};
		}

		public Transform GetFootCenter(Foot foot)
		{
			return foot switch
			{
				Foot.LEFT => footSettings.leftFootCenter,
				Foot.RIGHT => footSettings.rightFootCenter,
				_ => null
			};
		}
	}
}
