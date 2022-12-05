using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public static bool flip = false;
    public GameObject rotation1, rotation2;
    private float timer = 1f;
    private bool flipped;

    void Update()
    {
        Vector3 pos = Input.mousePosition;

        pos.z += 7.5f;
        transform.position = Camera.main.ScreenToWorldPoint(pos);

        if (Input.GetMouseButtonDown(1) && timer <= 0)
        {
            timer = 1f;
            flip = !flip;
        }

        if (flip)
        {
            if (!flipped)
            {
                flipped = !flipped;
                Vector3 rotationToAdd = new Vector3(0, 90, 0);
                transform.Rotate(rotationToAdd);
            }
        }
        else
        {
            if (flipped)
            {
                flipped = !flipped;
                Vector3 rotationToAdd = new Vector3(0, -90, 0);
                transform.Rotate(rotationToAdd);
            }
        }

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }
}
