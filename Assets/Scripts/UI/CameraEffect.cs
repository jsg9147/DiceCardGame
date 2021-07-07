using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffect : MonoBehaviour
{
    [SerializeField] Material effectMat;


    // ��� ���������� �̹��� �������� �Ϸ��� ����, ���� �߿� �۵�
    private void OnRenderImage(RenderTexture _src, RenderTexture _dest)
    {
        if (effectMat == null)
            return;

        Graphics.Blit(_src, _dest, effectMat);
    }

    private void OnDestroy()
    {
        SetGrayScale(false);
    }

    public void SetGrayScale(bool isGrayscale)
    {
        effectMat.SetFloat("_GrayscaleAmount", isGrayscale ? 1 : 0);
        effectMat.SetFloat("_DarkAmount", isGrayscale ? 0.12f : 0);
    }
}
