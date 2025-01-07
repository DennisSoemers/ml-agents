using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System.Collections.Generic;

public class SensorControllerForAgents
{
    //private string backCastName = "PurpuleReverseRays";
    //private static string BlueReverseRays = "BlueReverseRays";
    //private static string PurpleReverseRays = "PurpleReverseRays";
    //private static string BlueReverseRays1 = "BlueReverseRays (1)";
    //private static string PurpleReverseRays1 = "PurpleReverseRays (1)";
    private static List<string> rayNames = new List<string> { "BlueReverseRays" , "PurpleReverseRays" , "BlueReverseRays (1)", "PurpleReverseRays (1)" };
    
/*
 this function will diable the back cast raysof the agent.
*/
public static void ManageBackCast(AgentSoccer agent, bool setActive)
{
        Transform child = null;
        foreach (string currentRayName in rayNames)
        {
            child = agent.transform.Find(currentRayName);
            if (child != null)
            {
                Debug.Log("Child: " + child.name);
                child.gameObject.SetActive(setActive);
                return;
            }
        }
    }

    public static void ManageAgentSensors(AgentSoccer agent)
    {
        if (AgentSoccer.ModelType.OnlyForwardRaycast == agent.modelType)
        {
            ManageBackCast(agent, false);
        }
        else if (AgentSoccer.ModelType.ForwardAndBackwardRaycast == agent.modelType)
        {
            ManageBackCast(agent, true);
        }
        else if (AgentSoccer.ModelType.SoundAndViewRotation == agent.modelType)
        {
            ManageBackCast(agent, false);
        }
    }

}



