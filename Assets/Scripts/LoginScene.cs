using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginScene : BaseScene
{
    public GameObject PhotonManager;

    private void Awake()
    {
        Instantiate(PhotonManager);
    }

    public void LoginButton()
    {
        SceneManager.LoadScene("Lobby");
    }
    
}
