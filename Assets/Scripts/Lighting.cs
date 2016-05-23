using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Lighting : MonoBehaviour
{
    public float Power; // Number of blocks that you can see thru
    public LayerMask discrimination;
    Vector2 dir;
    public float checkAngle = 2f;
    Quaternion angle;
    public float Distance = 20f;
    public float DaylightAddition = 5f;

    public float AirAffection = .1f;

    float originalDistance;
    float originalPower;

    void Start()
    {
        angle = new Quaternion();

        originalDistance = Distance;
        originalPower = Power;
    }

    void Update()
    {
        angle.eulerAngles = Vector3.zero;

        if (transform.position.y >= -(DaylightAddition / 2) && transform.position.y <= DaylightAddition / 2)
        {
            Distance = originalDistance + DaylightAddition / 2 + transform.position.y;
            Power = originalPower + DaylightAddition / 2 + transform.position.y;
        }
        else if(transform.position.y < -(DaylightAddition / 2))
        {
            Distance = originalDistance;
            Power = originalPower;
        }
        else if (transform.position.y > DaylightAddition / 2)
        {
            Distance = originalDistance + DaylightAddition;
            Power = originalPower + DaylightAddition;
        }

        float monotony = 1 / Power;
        for (int i = 0; i < 360 / checkAngle; i++)
        {
            try
            {
                RaycastHit2D[] hit = Physics2D.RaycastAll(transform.position, angle * Vector2.up, Distance + 1, discrimination);
                float modifier = 0;
                bool canStop = false;
                foreach (RaycastHit2D hitmark in hit)
                {
                    var spriteRenderer = hitmark.transform.GetComponent<SpriteRenderer>();
                    if (spriteRenderer == null)
                        continue;

                    if (modifier <= Power)
                    {
                        if (hitmark.transform.tag == "Air")
                        {
                            spriteRenderer.color = new Color(1 - monotony * modifier, 1 - monotony * modifier, 1 - monotony * modifier, monotony * modifier);
                            modifier += AirAffection;
                        }
                        else if (hitmark.transform.tag == "Breakable")
                        {
                            spriteRenderer.color = new Color(1 - monotony * modifier, 1 - monotony * modifier, 1 - monotony * modifier, 1);
                            modifier++;
                        }
                        else
                        {
                            spriteRenderer.color = Color.white;
                        }
                    }
                    else
                    {
                        if (spriteRenderer.color != Color.black)
                            spriteRenderer.color = Color.black;
                        else
                            break;
                    }

                    if (!canStop)
                    {
                        if (Vector3.Distance(hitmark.transform.position, transform.position) > Distance)
                        {
                            spriteRenderer.color = Color.black;
                            canStop = true;
                        }
                    }
                    else
                    {
                        spriteRenderer.color = Color.black;
                    }
                }
            }
            catch { }

            angle.eulerAngles = new Vector3(0, 0, angle.eulerAngles.z + checkAngle);
        }
    }
}
