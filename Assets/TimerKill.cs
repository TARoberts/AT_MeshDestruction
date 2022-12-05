using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerKill : MonoBehaviour
{
    private float timer;
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > 5f)
        {
            Destroy(this.gameObject);
        }
    }
}
