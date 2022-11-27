using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clicker : MonoBehaviour
{
    Vector3 pos1, pos2;
    private Plane planee;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            pos1 = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            pos2 = Input.mousePosition;

            float fuck = Vector3.Distance(pos1, pos2);
            planee = new Plane(pos1, fuck);
            Debug.Log(planee);
        }

    }
}
