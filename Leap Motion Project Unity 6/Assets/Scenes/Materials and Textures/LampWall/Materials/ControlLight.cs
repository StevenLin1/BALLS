using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlLight : MonoBehaviour
{

    public Material materialOn;
    public Material materialOff;

    private bool lightStatusOn = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchLight()
    {
        lightStatusOn = !lightStatusOn;
        
        if (lightStatusOn)
        {
            for (int i =0; i < transform.childCount; i++)
                transform.GetChild(i).GetComponent<Renderer>().material = materialOn;
        }
        else
        {
            for (int i =0; i < transform.childCount; i++)
                transform.GetChild(i).GetComponent<Renderer>().material = materialOff;
        }
    }
}
