using UnityEngine;
using System;
using DN.Service;
using System.Collections.Generic;

namespace QTea
{
	/// <summary>
	/// Let's hope this is fast enough... should be, right?
	/// </summary>
	public class MicrophoneBuffer : MonoBehaviour, IPCMBuffer
	{
		public int Count => buffer.Count;
		private List<float> buffer;
		private MicrophoneService microphoneService;

		protected void Awake()
		{
			buffer = new List<float>(5000);

			microphoneService = ServiceLocator.Locate<MicrophoneService>();
			microphoneService.OnSampleReady += PushSamples;
		}

		protected void OnDestroy()
		{
			microphoneService.OnSampleReady -= PushSamples;
		}

		public float[] PopSamples(uint count)
		{
			int min = Mathf.Min((int)count, buffer.Count);
			var b = buffer.GetRange(0, min);
			buffer.RemoveRange(0, min);

			return b.ToArray();
		}

		public void PushSamples(float[] samples)
		{
			buffer.AddRange(samples);
		}
	}
}
