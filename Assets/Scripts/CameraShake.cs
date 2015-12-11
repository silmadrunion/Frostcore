using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour 
{
    Camera mainCam;
    float shakeAmount;

    void Awake()
    {
        mainCam = Camera.main;
    }

    public void Shake(float amt, float length)
    {
        shakeAmount = amt;

        InvokeRepeating("DoShake", 0f, 0.01f);
        Invoke("StopShake", length);
    }

    void DoShake()
    {
        if (shakeAmount > 0)
        {
            Vector3 camPos = mainCam.transform.position;

            float OffsetX = Random.value * shakeAmount * 2 - shakeAmount;
            float OffsetY = Random.value * shakeAmount * 2 - shakeAmount;

            camPos.x += OffsetX;
            camPos.y += OffsetY;

            mainCam.transform.position = camPos;
        }
    }

    void StopShake()
    {
        CancelInvoke("DoShake");
        mainCam.transform.localPosition = Vector3.zero;
    }
}
