using DN;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Service]
public class MicrophoneService 
{
	public event Action<float[]> OnFrameReady;

	public IReadOnlyList<string> Devices { get; }
	public int CurrentDeviceIndex { get; private set; }
	public string CurrentDeviceName => Devices[CurrentDeviceIndex];
	public bool Recording => Microphone.IsRecording(CurrentDeviceName);

	/// <summary>
	/// Get the position in samples of the recording
	/// </summary>
	public int Position => Microphone.GetPosition(CurrentDeviceName);
	public int Channels { get; private set; }

	private OpusSettingsService opusSettings;
	private MicrophoneStreamer streamer;

	private List<float> buffer = new List<float>();

	public MicrophoneService()
	{
		opusSettings = ServiceLocator.Locate<OpusSettingsService>();

		Devices = Microphone.devices.ToList();
		
		// start a recording to populate the information.
		PopulateMicInfo();
	}

	public void SetDevice(int index)
	{
		EndStream(); // end the current device to be sure.
		CurrentDeviceIndex = index;
		PopulateMicInfo();
	}

	public AudioClip Start() => Microphone.Start(CurrentDeviceName, true, 1, opusSettings.Frequency);
	public void End() => Microphone.End(CurrentDeviceName);

	public void StartStream()
	{
		if(streamer)
		{
			Debug.LogError("Can not start a stream while a stream is already busy.");
			return;
		}

		AudioClip audioClip = Start();
		GameObject microphoneStreamer = new GameObject("[Service] Microphone Streamer");
		streamer = microphoneStreamer.AddComponent<MicrophoneStreamer>();
		streamer.SetInfo(audioClip, this, opusSettings.FrameSize);
		streamer.OnSampleReady += OnSampleReadyEvent;
		streamer.OnUpdate += OnUpdateEvent;
		GameObject.DontDestroyOnLoad(microphoneStreamer);
	}

	private void OnUpdateEvent()
	{
		while(buffer.Count > opusSettings.FrameSize)
		{
			float[] data = buffer.GetRange(0, opusSettings.FrameSize).ToArray();
			OnFrameReady?.Invoke(data);
			buffer.RemoveRange(0, opusSettings.FrameSize);
		}
	}

	private void OnSampleReadyEvent(float[] data)
	{
		buffer.AddRange(data);
	}

	public void EndStream()
	{
		if(streamer) GameObject.Destroy(streamer.gameObject);
		End();
	}

	public void SetDevice(string name)
	{
		for (int i = 0; i < Devices.Count; i++)
		{
			if (name == Devices[i])
			{
				SetDevice(i);
				return;
			}
		}
	}

	private void PopulateMicInfo()
	{
		var audioClip = Start();
		Channels = audioClip.channels;
		Microphone.End(CurrentDeviceName);
	}
}
