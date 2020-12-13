using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class IncomingVoiceNetworkMessage : NetworkMessage
{
    public IncomingVoiceNetworkMessage(NetworkIdentity identity, byte[] data, Guid guid)
    {
        this.id = identity;
        this.voiceData = data;
        sendTime = BitConverter.GetBytes(DateTime.UtcNow.Ticks);
        this.guid = guid.ToByteArray();
    }

    public IncomingVoiceNetworkMessage() { }

    public NetworkIdentity id;
    public byte[] guid;
    public byte[] voiceData;
    public byte[] sendTime;

    public DateTime GetDate() => DateTime.FromBinary(BitConverter.ToInt64(sendTime, 0));
}
