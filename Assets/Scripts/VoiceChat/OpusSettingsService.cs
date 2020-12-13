using POpusCodec.Enums;
using UnityEngine;

namespace DN
{
	[Service]
	public class OpusSettingsService : ScriptableObject
	{
		public int FrameSize { get; set; }

		public int Frequency => frequency;
		[SerializeField] private int frequency = 48000;

		public int BitRate => bitRate;
		[SerializeField] private int bitRate = 28000;

		public Delay Delay => delay;
		[SerializeField] private Delay delay = Delay.Delay20ms;

		public bool voiceFeedback = false;
	}
}