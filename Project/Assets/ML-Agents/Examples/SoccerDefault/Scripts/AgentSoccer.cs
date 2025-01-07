using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using System;
using System.Collections.Generic;

public enum Team
{
    Blue = 0,
    Purple = 1
}

public class AgentSoccer : Agent
{
    // Note that that the detectable tags are different for the blue and purple teams. The order is
    // * ball
    // * own goal
    // * opposing goal
    // * wall
    // * own teammate
    // * opposing player

    public enum Position
    {
        Striker,
        Goalie,
        Generic
    }

    //public enum ModelType
    //{
    //    ForwardAndBackwardRaycast,
    //    SoundAndViewRotation,
    //    OnlyForwardRaycast
    //}

    [HideInInspector]
    public Team team;
    float m_KickPower;
    // The coefficient for the reward for colliding with a ball. Set using curriculum.
    float m_BallTouch;
    public Position position;
    private SoccerSettings.ModelType modelType;
    const float k_Power = 2000f;
    float m_Existential;
    float m_LateralSpeed;
    float m_ForwardSpeed;


    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;
    private SoundController soundController;

    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        SoccerEnvController envController = GetComponentInParent<SoccerEnvController>();
        soundController = new SoundController();
        if (envController != null)
        {
            m_Existential = 1f / envController.MaxEnvironmentSteps;
        }
        else
        {
            m_Existential = 1f / MaxStep;
        }

        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        //m_BehaviorParameters.ActionSpec = Actions.MakeContinuous(3);
        //var currentActionSpec = m_BehaviorParameters.BrainParameters.VectorActionSize;
        //Debug.Log($"Current Discrete Action Space: {string.Join(", ", currentBranchSizes)}");
        if (m_BehaviorParameters.TeamId == (int)Team.Blue)
        {
            team = Team.Blue;
            initialPos = new Vector3(transform.position.x - 5f, .5f, transform.position.z);
            rotSign = 1f;
        }
        else
        {
            team = Team.Purple;
            initialPos = new Vector3(transform.position.x + 5f, .5f, transform.position.z);
            rotSign = -1f;
        }
        if (position == Position.Goalie)
        {
            m_LateralSpeed = 1.0f;
            m_ForwardSpeed = 1.0f;
        }
        else if (position == Position.Striker)
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.3f;
        }
        else
        {
            m_LateralSpeed = 0.3f;
            m_ForwardSpeed = 1.0f;
        }
        m_SoccerSettings = FindFirstObjectByType<SoccerSettings>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;
        //SensorControllerForAgents.ManageAgentSensors(this);
        m_ResetParams = Academy.Instance.EnvironmentParameters;

    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        m_KickPower = 0f;

        var forwardAxis = act[0];
        var rightAxis = act[1];
        var rotateAxis = act[2];

        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward * m_ForwardSpeed;
                m_KickPower = 1f;
                break;
            case 2:
                dirToGo = transform.forward * -m_ForwardSpeed;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo = transform.right * m_LateralSpeed;
                break;
            case 2:
                dirToGo = transform.right * -m_LateralSpeed;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed,
            ForceMode.VelocityChange);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)

    {

        if (position == Position.Goalie)
        {
            // Existential bonus for Goalies.
            AddReward(m_Existential);
        }
        else if (position == Position.Striker)
        {
            // Existential penalty for Strikers
            AddReward(-m_Existential);
        }
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        //forward
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[0] = 2;
        }
        //rotate
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[2] = 2;
        }
        //right
        if (Input.GetKey(KeyCode.E))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            discreteActionsOut[1] = 2;
        }
    }
    /// <summary>
    /// Used to provide a "kick" to the ball.
    /// </summary>
    void OnCollisionEnter(Collision c)
    {
        var force = k_Power * m_KickPower;
        if (position == Position.Goalie)
        {
            force = k_Power;
        }
        if (c.gameObject.CompareTag("ball"))
        {
            AddReward(.2f * m_BallTouch);
            var dir = c.contacts[0].point - transform.position;
            dir = dir.normalized;
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
        }
    }

    public override void OnEpisodeBegin()
    {
        m_BallTouch = m_ResetParams.GetWithDefault("ball_touch", 0);
    }
    public void setModelType(SoccerSettings.ModelType modelType)
    {
        this.modelType = modelType;
        SensorControllerForAgents.ManageAgentSensors(this);
    }
    public SoccerSettings.ModelType GetModelType()
    {
        return this.modelType;
    }
    void OnTriggerEnter(Collider other)
    {
        // if (other.gameObject.tag == "ball")
        // {
        //     if (other.gameObject.GetComponent<Rigidbody>().velocity.magnitude > 0)
        //     {
        //         addGameObject(other.gameObject);
        //     }
        // }
        // if (other.CompareTag("objectWithSound"))
        // {
        //     //Debug.Log("Object with sound 'YourTag' is within the sphere collider.");
        //     addGameObject(other.gameObject);
        // }
        // soundController
        soundController.AddToList(other.gameObject);
    }




    void OnTriggerStay(Collider other)
    {
        // if (other.CompareTag("objectWithSound"))
        // {
        //     if (other.gameObject == null)
        //     {
        //         //Debug.LogError("Rigidbody component is not found on the agent!");
        //     }
        //     addGameObject(other.gameObject);
        // }
        soundController.AddToList(other.gameObject);
    }

    // private void addGameObject(GameObject gameObject)
    // {
    //     foreach (GameObject currentGameObject in detectedObjects)
    //     {
    //         if (currentGameObject == gameObject)
    //         {
    //             return;
    //         }
    //     }
    //     detectedObjects.Add(gameObject);
    // }

    void OnTriggerExit(Collider other)
    {
        // removeRigidbody(other.gameObject);
        soundController.RemoveFromList(other.gameObject);
    }

    // private void removeRigidbody(GameObject gameObject)
    // {
    //     foreach (GameObject currentGameObject in detectedObjects)
    //     {
    //         if (currentGameObject == gameObject)
    //         {
    //             detectedObjects.Remove(gameObject);
    //             return;
    //         }
    //     }
    // }

    public override void CollectObservations(VectorSensor sensor)
    {
        //  List<GameObject>detectedObjects = soundController.GetDetectedGameObjectsList();
        // int vectorSize = 5;
        // if (sensor == null)
        // {
        //     Debug.LogError("sensor is null");
        // }
        // if (detectedObjects.Count > vectorSize)
        // {
        //     Debug.Log("count: " + detectedObjects.Count);
        // }
        // //Debug.Log("count: " + detectedObjects.Count);

        // // Add the number of detected objects as an observation
        // //sensor.AddObservation(detectedObjects.Count);
        // //List<Vector3> observations = new List<Vector3>();
        // // Optionally, add more detailed observations for each detected object (e.g., position, distance)
        // //Debug.Log("Number of detected objects: " + detectedObjects.Count);
        // Vector3[] observations = new Vector3[vectorSize];
        // int counter = 0;
        // foreach (GameObject gameObject in detectedObjects)
        // {
        //     if (counter < vectorSize)
        //     {
        //         Rigidbody r = gameObject.GetComponent<Rigidbody>();
        //         //Vector3 currentVelocity = r.velocity;
        //         Vector3 relativePosition = transform.position - r.transform.position;
        //         //Debug.Log("Observation - Relative Position of Object: " + relativePosition);
        //         //sensor.AddObservation(relativePosition);
        //         observations[counter] = relativePosition;
        //         counter++;
        //     }
        // }
        // if (counter < vectorSize)
        // {
        //     for (int i = counter; i < vectorSize; i++)
        //     {
        //         //Debug.Log(observations[i]);
        //         observations[i] = Vector3.zero;
        //     }
        // }
        // int count = 0;
        // foreach (Vector3 vector in observations)
        // {
        //     if (count < vectorSize)
        //     {
        //         //Debug.Log(vector);
        //         sensor.AddObservation(vector);
        //         count++;
        //     }
        // }
        // detectedObjects.Clear();
        Vector3[] observations = soundController.GetObservations(transform);
        if(this.gameObject.name !="BlueStriker"){
            return;
        }
        // Debug.Log("-------------------------------------------------");
        foreach (Vector3 vector in observations)
        {
            //Debug.Log(vector);
            sensor.AddObservation(vector);
        }
    }



}
