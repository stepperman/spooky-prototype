using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using POpusCodec;
using POpusCodec.Enums;
using UnityEngine;
using Channels = POpusCodec.Enums.Channels;

public class NetworkedMicrophone : NetworkBehaviour
{
    public static event Action<float[]> OnNewSampleWow;

    [SerializeField] private AudioSource voiceSource;
    private AudioClip voiceClip;
    
    private bool isServer = false;
    private SpookyMicrophone microphone;
    private NetworkIdentity netID;
    private OpusService opusService;
    private DN.OpusSettingsService opusSettings;
    private bool voiceFeedback => opusSettings.voiceFeedback;

    private List<float> sampleList = new List<float>();

    protected void Awake()
    {
        netID = GetComponent<NetworkIdentity>();
        opusSettings = ServiceLocator.Locate<DN.OpusSettingsService>();
        opusService = ServiceLocator.Locate<OpusService>();
        var micService = ServiceLocator.Locate<MicrophoneService>();
        voiceClip = AudioClip.Create("voice", opusSettings.FrameSize, micService.Channels, AudioSettings.outputSampleRate, true, OnAudioRead);
        voiceSource.clip = voiceClip;
        voiceSource.loop = true;
        voiceSource.Play();
    }

    private void Start()
    {
        StartCoroutine(ClearAudio());
    }

    private IEnumerator ClearAudio()
    {
        yield return new WaitForSeconds(5.0f);
        sampleList.Clear();
        Debug.Log("Nigger");
    }

    /// <summary>
    /// client sends data to host
    /// if the client is the host, instead send data to all other clients
    /// </summary>
    public void ClientSendData(byte[] data)
    {
        var gu = Guid.NewGuid();
        var message = new IncomingVoiceNetworkMessage(netID, data, gu);
        if (isServer) ServerSendState(message); // if server, send to other clients
        else NetworkClient.Send(message, 1); // if client, send to host
    }

    public override void OnStartClient()
    {
        Debug.Log("OnStartClient()");

    }

    public override void OnStartLocalPlayer()
    {
        microphone = gameObject.AddComponent<SpookyMicrophone>();
        // only register handler if not host.
        if (NetworkServer.active)
            return;
        NetworkClient.ReplaceHandler<IncomingVoiceNetworkMessage>(OnIncomingVoice);
    }

    public override void OnStartServer()
    {
        NetworkServer.ReplaceHandler<IncomingVoiceNetworkMessage>(OnIncomingVoice);
        isServer = true;
    }

    public override void OnStopClient()
    {
        NetworkClient.UnregisterHandler<IncomingVoiceNetworkMessage>();
        //decoder.Dispose();
    }
    public override void OnStopServer() => OnStopClient(); 
    
    private void OnIncomingVoice(NetworkConnection con, IncomingVoiceNetworkMessage message)
    {
        // if server, send to others as well before playing
        if (isServer && !netID.Equals(con.identity)) // don't send back to yourself either
        {
            ServerSendState(message);
        }
        else
        {
            Debug.Log("Am Client and received message");
        }
        
        // get the game object where the voice is handled
        message.id.GetComponent<NetworkedMicrophone>().PlayData(message.voiceData);
    }

    public void PlayData(byte[] data)
    {
        float[] packet = opusService.Decode(data);
        sampleList.AddRange(packet);
    }

    [Server]
    private void ServerSendState(IncomingVoiceNetworkMessage message)
    {
        // send to all other states except the owner and the host (since the host already has it)
        var spawned = netID.observers;

        foreach (var player in spawned)
        {
            NetworkIdentity identity = player.Value.identity;

            if(voiceFeedback)
            {
                PlayData(message.voiceData);
                // check time
            }

            if (identity != message.id && identity != netID)
            {
                player.Value.Send(message, 1);
            }
        }
    }

    private void OnAudioRead(float[] data)
    {
        int size = Mathf.Min(data.Length, sampleList.Count);
        Debug.Log(sampleList.Count);
        float[] buffer = sampleList.GetRange(0, size).ToArray();
        buffer.CopyTo(data, 0);
        sampleList.RemoveRange(0, size);
    }
}
