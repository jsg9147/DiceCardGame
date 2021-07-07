using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PhotonEventData
{
    public enum Code
    {
        CardSpawn,

        // Photon RaiseEvent 에서 Null 에러 제거용
        EvMove,
        EvFinalMove,        
        //

        OutPost,
        MoveCard,
        Attack
    }
}
