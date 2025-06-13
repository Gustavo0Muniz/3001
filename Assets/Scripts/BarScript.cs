using UnityEngine;

public class BarScript : MonoBehaviour
{

    private Transform myCanera;

    void Awake()
    {
        myCanera = Camera.main.transform;
    }
    
    void Update()
    {
        transform.LookAt(transform.position + myCanera.forward);
    }
}
