using UnityEngine;

public class SoccerSettings : MonoBehaviour
{
    public Material purpleMaterial;
    public Material blueMaterial;
    public bool randomizePlayersTeamForTraining = true;
    public float agentRunSpeed;

    public enum ModelType
    {
        ForwardAndBackwardRaycast,
        SoundAndViewRotation,
        OnlyForwardRaycast
    }
    public ModelType modelTypeBlueTeam;
    public ModelType modelTypePurpleTeam;

    public enum ModelCompare{
        Yes,
        No
    }
    public ModelCompare modelCompare;
    public int limit=0;
}