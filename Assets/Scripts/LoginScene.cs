using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginScene : MonoBehaviour
{
    public void LoginButton()
    {
        SceneManager.LoadScene("Lobby");
    }
    
}
