using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class FootstepNetworkMessage : NetworkMessage
{
    public FootstepNetworkMessage(Vector3 position)
    {
        this.position = position;
        this.clientId = NetworkClient.connection.identity.netId;
    }
    
    public FootstepNetworkMessage() { }
    
    public uint clientId;
    public Vector3 position;
}

public class PlayFridayNightMessage : NetworkMessage
{
    public uint clientId;
}

