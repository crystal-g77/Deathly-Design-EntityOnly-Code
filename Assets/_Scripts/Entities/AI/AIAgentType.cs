using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIAgentType
{
    GruntFemale,
    GruntMale,
    SoldierFemale,
    SoldierMale,
}

public enum AIAgentGenderType
{
    Female,
    Male,
}

public enum AIAgentUnitType
{
    Grunt,
    Soldier,
}

public static class AIAgentTypeExtensions
{
    // Extension method to check agent types gender
    public static AIAgentGenderType getGender(this AIAgentType type)
    {
        if(type == AIAgentType.GruntFemale || type == AIAgentType.SoldierFemale)
        {
            return AIAgentGenderType.Female;
        }
        else
        {
            return AIAgentGenderType.Male;
        }
    }
    
    // Extension method to check agent types unit type
    public static AIAgentUnitType getUnitType(this AIAgentType type)
    {
        if(type == AIAgentType.GruntFemale || type == AIAgentType.GruntMale)
        {
            return AIAgentUnitType.Grunt;
        }
        else
        {
            return AIAgentUnitType.Soldier;
        }
    }
}
