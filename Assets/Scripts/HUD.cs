using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    //rainbow gauge variables
    public Slider fillRainbowMeter;
    public Slider fillDamage;
    public float reductionAmount;           //controls how fast damage fill depletes

    //static variable
    public static HUD instance;

    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //adjust damage gauge
        StartCoroutine(ReduceDamageBar());
        //if (fillDamage.value > fillRainbowMeter.value)
            //fillDamage.value -= reductionAmount * Time.deltaTime;
    }

    IEnumerator ReduceDamageBar()
    {
        while (fillDamage.value > fillRainbowMeter.value)
        {
            fillDamage.value -= reductionAmount * Time.deltaTime;
            yield return null;
        }
    }

    public void AdjustRainbowGauge(float amount)
    {
        fillRainbowMeter.value += amount;

        if (fillRainbowMeter.value < fillDamage.value)  //if we took damage, do nothing more because couroutine will handle damage bar value
            return;

        fillDamage.value = fillRainbowMeter.value;
    }

    public void SetRainbowGaugeMaxValue(float amount)
    {
        fillRainbowMeter.maxValue = amount;
        fillRainbowMeter.value = 0;
        fillDamage.maxValue = fillRainbowMeter.maxValue;
        fillDamage.value = fillRainbowMeter.value;
    }
}
