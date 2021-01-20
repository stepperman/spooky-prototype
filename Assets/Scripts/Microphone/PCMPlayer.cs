using UnityEngine;
using System;
using DN.Service;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace QTea
{
	/// <summary>
	/// Plays nj bkibljh lkibhjjhbb ih
	/// </summary>
	public class PCMPlayer : MonoBehaviour
	{
		[SerializeField] private bool createSound = true;

		private List<float> buffer;

		private MicrophoneService microphone;
		private FMOD.Sound sound;
		private FMOD.SOUND_PCMREAD_CALLBACK pcmReadCallback;

		private void Awake()
		{
			buffer = new List<float>();

			microphone = ServiceLocator.Locate<MicrophoneService>();

			microphone.StartRecording();
			microphone.OnSampleReady += Microphone_OnSampleReady;
		}

		private void Start()
		{
			if(!createSound)
			{
				Debug.Log("Skipping creating the sound.");
				return;
			}

			FMOD.CREATESOUNDEXINFO exInfo = default;
			exInfo.cbsize = Marshal.SizeOf(typeof(FMOD.CREATESOUNDEXINFO));
			exInfo.pcmreadcallback = (pcmReadCallback = new FMOD.SOUND_PCMREAD_CALLBACK(ReadCallback));
			exInfo.format = FMOD.SOUND_FORMAT.PCMFLOAT;
			exInfo.numchannels = 1;		     // REPLACE WITH OPUS SETTINGS
			exInfo.defaultfrequency = 48000; // REPLACE WITH OPUS SETTINGS
			int delay = 20;					 // REPLACE WITH OPUS SETTINGS
			exInfo.decodebuffersize = (uint)(exInfo.defaultfrequency / 1000 * delay); // REPLACE WITH OPUS SETTINGS
			exInfo.length = (uint)(exInfo.defaultfrequency / 1000 * delay * exInfo.numchannels * sizeof(float));
			var result = FMODUnity.RuntimeManager.CoreSystem.createSound("playback", FMOD.MODE.OPENUSER | FMOD.MODE.LOOP_NORMAL | FMOD.MODE.CREATESTREAM | FMOD
				.MODE.NONBLOCKING, ref exInfo, out sound);
			Debug.Log("Create sound " + result);

			result = FMODUnity.RuntimeManager.CoreSystem.playSound(sound, default, false, out var channel);
			Debug.Log("Play sound " + result);
		}

		private FMOD.RESULT ReadCallback(IntPtr sound, IntPtr data, uint datalen)
		{
			if (buffer == null || buffer.Count == 0)
				return FMOD.RESULT.OK; // no need

			int count = Mathf.Min((int)datalen / 4, buffer.Count);
			float[] bufferData = buffer.GetRange(0, count).ToArray();

			unsafe
			{
				float *pcmData = (float *)data.ToPointer();

				for (int i = 0; i < count; i++)
				{
					*pcmData++ = bufferData[i];
				}
			}

			Debug.Log(bufferData[0]);

			buffer.RemoveRange(0, count);

			return FMOD.RESULT.OK;
		}

		private void Microphone_OnSampleReady(float[] data) => buffer.AddRange(data);
	}
}
