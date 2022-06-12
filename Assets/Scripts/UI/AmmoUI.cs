using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    public Image fillImage;

    private Material mat;
    
    public void SetAmount(float amount)
    {
        fillImage.fillAmount = amount;
    }

    public void SetColor(Color color)
    {
        mat.color = color;
    }
}
