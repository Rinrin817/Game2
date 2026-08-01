using Fusion;
using UnityEngine;

public class NetworkGameState : NetworkBehaviour
{
    [Networked, Capacity(16)]
    public NetworkString<_16> PlayerName { get; set; }

    [Networked] public int thiefSkinNumber { get; set; }
    [Networked] public int polliceSkinNumber { get; set; }
    [Networked] public int skinObjNumber { get; set; }
    [Networked] public float roleExplainTime { get; set; }


    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            thiefSkinNumber = 0;
            polliceSkinNumber = 0;
            skinObjNumber = 0;
            roleExplainTime = 10;
        }
    }
}