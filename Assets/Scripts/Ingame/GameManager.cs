using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 치트, UI, 랭킹, 게임오버
public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }

    [SerializeField] NotificationPanel notificationPanel;

    void InputCheatKey()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
            TurnManager.OnAddCard?.Invoke(true);

        if (Input.GetKeyDown(KeyCode.Keypad2))
            TurnManager.OnAddCard?.Invoke(false);

        if (Input.GetKeyDown(KeyCode.Keypad3))
            TurnManager.Inst.EndTurn();
    }

    public void StartGame()
    {
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }

    public void Notification(string msg)
    {
        notificationPanel.Show(msg);
    }

    private void Awake()
    {
        Inst = this;
    }
    void Start()
    {
        StartGame();
    }

    void Update()
    {
#if UNITY_EDITOR
        InputCheatKey();
#endif
    }
}
