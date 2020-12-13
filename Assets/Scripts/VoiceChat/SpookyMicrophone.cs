using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Mirror;
using POpusCodec;
using POpusCodec.Enums;
using UnityEngine;
using Channels = POpusCodec.Enums.Channels;

public class SpookyMicrophone : NetworkBehaviour
{
    public bool IsRecording { get; set; } = true;
    private NetworkedMicrophone networkedMicrophone;
    private OpusService opusService;
    private MicrophoneService microphoneService;

    protected void Awake()
    {
        networkedMicrophone = GetComponent<NetworkedMicrophone>();
        opusService = ServiceLocator.Locate<OpusService>();
        microphoneService = ServiceLocator.Locate<MicrophoneService>();

        microphoneService.StartStream();
        microphoneService.OnFrameReady += OnMicSampleReady;
    }
    
    private void OnMicSampleReady(float[] segment)
    {
        byte[] data = Encode(segment);
        networkedMicrophone.ClientSendData(data);
    }

    private byte[] Encode(float[] segment) => opusService.Encode(segment);
}