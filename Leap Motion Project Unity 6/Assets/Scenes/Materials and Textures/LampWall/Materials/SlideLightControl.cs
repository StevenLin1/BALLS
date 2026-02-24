using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideLightControl : MonoBehaviour
{

    public float value;
    public Material slideMaterial;

    public void changeLightIntensity(GameObject sliderCursor)
    {

            value = (float)((sliderCursor.transform.localPosition.x + 0.188945)/(0.188945*2));
            Debug.Log(value);
            //-0.188945 +0.188945
            slideMaterial.SetColor("_EmissionColor", new Color(1.0f, 1.0f-value, 1.0f-value, 1.0f));


    }
}
