using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class NotificationPanel : MonoBehaviour
{
    [SerializeField] TMP_Text notificationTMP;

    public void Show(string msg)
    {
        notificationTMP.text = msg;
        //Dotween ease 검색 하면 어떻게 나오는지 여러 종류가 있음
        Sequence sequence = DOTween.Sequence()
            .Append(transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.InOutQuad))
            .AppendInterval(0.9f)
            .Append(transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InOutQuad));
    }

    [ContextMenu("ScaleOne")]
    void ScaleOne() => transform.localScale = Vector3.zero;

    [ContextMenu("ScaleZero")]
    public void ScaleZero() => transform.localScale = Vector3.zero;

    private void Start()
    {
        ScaleZero();
    }

}
