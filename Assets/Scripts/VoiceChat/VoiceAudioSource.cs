using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceAudioSource : MonoBehaviour
{
    private int packetSize;
    private float delayTime;
    private DN.OpusSettingsService opusSettings;

    private bool canPlay = false;
    private bool timerStarted = false;
    private float startWaitTime;

    private List<float> samples;
    private NetworkedMicrophone networkedMicrophone;
    private AudioSource audioSource;
    private AudioClip audioClip;

    private void Start()
    {
        opusSettings = ServiceLocator.Locate<DN.OpusSettingsService>();
        packetSize = opusSettings.FrameSize;
        delayTime = opusSettings.MicPlayDelay;
        samples = new List<float>();
        audioSource = GetComponent<AudioSource>();

        audioClip = AudioClip.Create("new", opusSettings.FrameSize, 1, opusSettings.Frequency, true);
        audioSource.clip = audioClip;
        audioSource.loop = true;
        audioSource.Play();

        Debug.Log(ServiceLocator.Locate<MicrophoneService>().CurrentDeviceName);
    }

    private void Update()
    {
        if(samples.Count > 0 && !timerStarted && !canPlay)
        {
            startWaitTime = Time.realtimeSinceStartup;
            timerStarted = true;
        }

        if(Time.time > startWaitTime + delayTime && timerStarted)
        {
            canPlay = true;
            timerStarted = false;
        }

        if(canPlay)
        {
            
        }
    }

    public void Init(NetworkedMicrophone networkedMicrophone)
    {
        this.networkedMicrophone = networkedMicrophone;
        networkedMicrophone.OnSampleReady += OnSampleReadyEvent;
    }

    private void OnSampleReadyEvent(float[] sample)
    {
        samples.AddRange(sample);
        Debug.Log("got sample");
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        // TODO implement some waiting shit
        if(!canPlay) return;

        if(samples.Count == 0)
        {
            canPlay = false;
            return;
        }

        Debug.Log(data);    

        int size = Mathf.Min(samples.Count, data.Length);

        for (int i = 0; i < size; i += channels)
        {
            data[i] = samples[i];
            for(int j = 1; j < channels; j++)
            {
                data[i + j] = samples[i];
            }
        }

        samples.RemoveRange(0, size);
    }
}
