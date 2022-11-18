using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CullTiny : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float size = GetComponent<Collider>().bounds.size.x + GetComponent<Collider>().bounds.size.y + GetComponent<Collider>().bounds.size.z;
        if (size < 1)
        {
            Debug.Log("Culling Object of size " + size);
            Destroy(this.gameObject);
        }
    }

}
