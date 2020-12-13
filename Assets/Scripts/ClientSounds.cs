using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ClientSounds : NetworkBehaviour
{
    [SerializeField] private AudioSource a1;
    [SerializeField] private AudioClip agf;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!isLocalPlayer) return;
        
        NetworkClient.RegisterHandler<FootstepNetworkMessage>(message =>
        {
            if (!NetworkIdentity.spawned.TryGetValue(message.clientId, out var identity)) return;
            FirstPersonAIO a = identity.gameObject.GetComponent<FirstPersonAIO>();
            if (!a) return;

            a.ServerPlayFootstep(message.position);
        });
        
        NetworkClient.RegisterHandler<PlayFridayNightMessage>(message =>
        {
            if (!NetworkIdentity.spawned.TryGetValue(message.clientId, out var identity)) return;
            ClientSounds a = identity.gameObject.GetComponent<ClientSounds>();
            if (!a) return;
            a.NetworkPlayFN();
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer) return;

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            uint id = NetworkClient.connection.identity.netId;
            NetworkClient.Send(new PlayFridayNightMessage()
            {
                clientId = id
            });

            NetworkPlayFN();
        }
    }

    public void NetworkPlayFN()
    {
        a1.clip = agf;
        a1.time = 0;
        a1.Play();
    }
}
