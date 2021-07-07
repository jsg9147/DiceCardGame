using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 치트, UI, 랭킹, 게임오버
public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }

    [Multiline(10)]
    [SerializeField] string cheatInfo;
    [SerializeField] NotificationPanel notificationPanel;
    [SerializeField] ResultPanel resultPanel;
    [SerializeField] CameraEffect cameraEffect;
    [SerializeField] GameObject endTurnBtn;

    WaitForSeconds delay2 = new WaitForSeconds(2);

    private void Awake()
    {
        Inst = this;
    }

    public void UISetup()
    {
        notificationPanel.ScaleZero();
        resultPanel.ScaleZero();
        cameraEffect.SetGrayScale(false);
    }

    void Update()
    {

    }

    public void StartGame()
    {
        StartCoroutine(TurnManager.Inst.StartGameCo());
    }

    public void Notification(string msg)
    {
        notificationPanel.Show(msg);
    }

    public IEnumerator GameOver(bool isMyWin)
    {
        TurnManager.Inst.isLoading = true;
        endTurnBtn.SetActive(false);
        yield return delay2;

        TurnManager.Inst.isLoading = true;
        resultPanel.Show(isMyWin ? "승리" : "패배");
        cameraEffect.SetGrayScale(true);
    }
}
