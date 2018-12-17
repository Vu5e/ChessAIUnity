using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdateValueScript : MonoBehaviour
{
    public Text depthValue;
    public Slider depthSlider;

    public void Start()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        depthValue.text = depthSlider.value.ToString();
    }
}
