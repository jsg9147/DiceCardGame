using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhotonEventData
{
    public enum Code
    {
        CardSpawn,

        // Photon RaiseEvent ���� Null ���� ���ſ�
        EvMove,
        EvFinalMove,        
        //

        OutPost,
        MoveCard,
        Attack
    }
}
