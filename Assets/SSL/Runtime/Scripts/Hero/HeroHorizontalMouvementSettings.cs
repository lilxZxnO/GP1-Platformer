using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]

public class HeroHorizontalMouvementSettings 
{
    public float acceleration = 20f;
    public float deceleration = 15f;
    public float turnBackFrictions = 25f;
    public float speedMax = 5f;
}

[Serializable]
public class DashSettings
{
    public float dashSpeed = 40f;
    public float dashDuration = 0.1f;
}

