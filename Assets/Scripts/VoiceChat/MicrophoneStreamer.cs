using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophoneStreamer : MonoBehaviour
{
	public event Action<float[]> OnSampleReady;
	public event Action OnUpdate;

	private AudioClip audioClip;
	private int frameSize;
	private MicrophoneService micService;
	private DN.OpusSettingsService opusSettings;
	private Coroutine audioCoroutine;

	public void SetInfo(AudioClip audioClip, MicrophoneService micService, int frameSize)
	{
		opusSettings = ServiceLocator.Locate<DN.OpusSettingsService>();

		this.audioClip = audioClip;
		this.frameSize = frameSize;
		this.micService = micService;
		var audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = audioClip;
		audioSource.loop = true;
		audioSource.mute = true;
		audioSource.Play();

		audioCoroutine = StartCoroutine(AudioRead());
	}

	protected void Update()
	{
		OnUpdate?.Invoke();
	}

	private IEnumerator AudioRead()
	{
		int sampleSize = (int)(opusSettings.Frequency / 1000 * (decimal)opusSettings.Delay / 2);
		float[] Sample = new float[sampleSize];
		int loops = 0;
		int readAbsPos = 0;
		int prevPos = 0;

		while(!(micService.Position > 0)){ yield return null; }

		while(micService.Recording && audioClip)
		{
			bool newDataAvailable = true;

			while(newDataAvailable)
			{
				int currentPos = micService.Position;
				if(currentPos < prevPos) loops ++;
				prevPos = currentPos;

				int curAbsPos = loops * audioClip.samples + currentPos;
				int nextAbsPos = readAbsPos + sampleSize;

				if(nextAbsPos < curAbsPos)
				{
					audioClip.GetData(Sample, readAbsPos % audioClip.samples);
					OnSampleReady?.Invoke(Sample);

					readAbsPos = nextAbsPos;
				} else newDataAvailable = false;
			}

			yield return null;
		}
	}

	/*protected void OnAudioFilterRead(float[] data, int channels)
	{
		OnSampleReady?.Invoke(data);

		for (int i = 0; i < data.Length; i++)
		{
			data[i] = 0;
		}
	}*/
}
