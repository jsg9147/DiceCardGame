using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    //Screen object variables
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject userDataUI;
    public GameObject scoreboardUI;
    public GameObject lobbyUI;
    public GameObject roomUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }
    public void ClearScreen() //Turn off all screens
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        userDataUI.SetActive(false);
        scoreboardUI.SetActive(false);
        lobbyUI.SetActive(false);
        roomUI.SetActive(false);
    }
    public void LoginScreen()
    {
        ClearScreen();
        loginUI.SetActive(true);
    }

    public void ResisterScreen()
    {
        ClearScreen();
        registerUI.SetActive(true);
    }
    public void UserDataScreen() //Logged in
    {
        ClearScreen();
        userDataUI.SetActive(true);
    }

    public void ScoreboardScreen() //Scoreboard button
    {
        ClearScreen();
        scoreboardUI.SetActive(true);
    }

    public void LobbyScreen() //Scoreboard button
    {
        ClearScreen();
        lobbyUI.SetActive(true);
    }

    public void RoomScreen() //Scoreboard button
    {
        ClearScreen();
        lobbyUI.SetActive(true);
    }
}
