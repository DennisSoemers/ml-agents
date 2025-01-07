using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class SensorControllerForAgents
{
    //private string backCastName = "PurpuleReverseRays";
    private static string BlueReverseRays = "BlueReverseRays";
    private static string PurpleReverseRays = "PurpleReverseRays";
    private static string BlueReverseRays1 = "BlueReverseRays (1)";
    private static string PurpleReverseRays1 = "PurpleReverseRays (1)";
    

public static void DisableBackCast(AgentSoccer agent)
{
        Transform child = null;
    if (agent.team == Team.Blue)
    {
         child = agent.transform.Find(BlueReverseRays);
        if (child != null)
        {
            Debug.Log("Child: " + child.name);
            return;
        }
         child = agent.transform.Find(BlueReverseRays1);
        if (child != null)
        {
            Debug.Log("Child: " + child.name);
            return;
        }
    }
         child = agent.transform.Find(PurpleReverseRays);
        if (child != null)
        {
            Debug.Log("Child: " + child.name);
            return;
        }
         child = agent.transform.Find(PurpleReverseRays1);
        if (child != null)
        {
            Debug.Log("Child: " + child.name);
            return;
        }
    }
}


