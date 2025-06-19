using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIGuardSpawner : MonoBehaviour
{    
    [SerializeField]
    private DefaultBuilding building;
    [SerializeField]
    private float guardRadius;
    [HideInInspector][SerializeField][EnumArray(typeof(AIAgentType))]
    private int[] numToGuard = new int[Enum.GetValues(typeof(AIAgentType)).Length]; // number of each type of agent to spawn in a wave

    // Start is called before the first frame update
    void Start()
    {
        AIManager.Instance.addToGuardSpawns(building, guardRadius, numToGuard);
    }
}
