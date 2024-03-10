using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    public GameObject secondHand;
    public GameObject minuteHand;
    public GameObject hourHand;

    float timer;

    int seconds;
    int minutes;
    int hours;

    public bool currentTime = false;

    [Space]
    [Header("Time Travel")]
    [Range(1, 500)]
    public int clockScalar = 1;

    // Start is called before the first frame update
    void Start()
    {
        clockScalar = 1;

        if(currentTime)
        {
            DateTime dateTime = DateTime.Now;
            timer = (dateTime.Hour * 60 * 60) + (dateTime.Minute * 60) + dateTime.Second;
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime * clockScalar;

        seconds = (int)timer;
        minutes = (int)Mathf.Floor(timer / 60);
        hours = (int)Mathf.Floor(minutes / 60);

        seconds %= 60;
        minutes %= 60;
        hours %= 60;

        int secondPercentage = seconds * 6;
        secondHand.transform.localRotation = Quaternion.Euler(Quaternion.identity.x, secondPercentage, Quaternion.identity.z);

        float minutePercentage = ((float)seconds / 60) * 6 + minutes*6;
        minuteHand.transform.localRotation = Quaternion.Euler(Quaternion.identity.x, minutePercentage, Quaternion.identity.z);

        float hourPercentage = ((float)minutes / 60) * 30f + hours*30; 
        hourHand.transform.localRotation = Quaternion.Euler(Quaternion.identity.x, hourPercentage, Quaternion.identity.z);

        if(hours >= 12)
        {
            timer -= 12 * 60 * 60;
            hours -= 12;
        }
    }
}
