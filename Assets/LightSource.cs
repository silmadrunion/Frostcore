using UnityEngine;
using System.Collections;

public class LightSource : MonoBehaviour
{
    public float Power = 1;

    public int MapPosX;

    public int MapPosY;

    public bool Stationary = true;
    public bool Added = false;
    public float[][] mobileLight;
    public float[][] fadeOutLight;

    public void UpdateMapPos()
    {
        MapPosX = (int)(transform.position.x * 2);
        if ((int)(transform.position.y * 2) >= 0)
            MapPosY = GameMaster.gm.mapGen.height - (int)(transform.position.y * 2);
        else
            MapPosY = ((int)(transform.position.y * 2) * -1) + GameMaster.gm.mapGen.height;
    }

    void Start()
    {
        if (!Stationary)
        {
            if ((int)(Power % 0.05f) % 2 == 0)
                mobileLight = new float[(int)(Power * 40) + 5][];
            else
                mobileLight = new float[(int)(Power * 40) + 4][];

            for (int i = 0; i < mobileLight.GetLength(0); i++)
                mobileLight[i] = new float[mobileLight.GetLength(0)];

            fadeOutLight = new float[mobileLight.GetLength(0)][];

            for (int i = 0; i < fadeOutLight.GetLength(0); i++)
                fadeOutLight[i] = new float[fadeOutLight.GetLength(0)];
        }

        GameMaster.gm.lightSources.Add(this);
        GameMaster.gm.PreRenderLight(Power);
    }
}
