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
    private Sound sound;
    //private static int counter = 0;


    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    public Vector3 initialPos;
    public float rotSign;
    private bool sphereActive = false;

    private Queue<List<Vector3>> soundMemory;
    private int MEM_SIZE = 3;
    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        sphereActive = true;
        soundMemory = new Queue<List<Vector3>>();

        SoccerEnvController envController = GetComponentInParent<SoccerEnvController>();
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
        m_BallTouch = 0.1f;
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
        m_BallTouch = 0.1f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (!sphereActive)
        {

            return;
        }
        List<Vector3> objects = sound.getObjects(agentRb);
        soundMemory.Dequeue();
        soundMemory.Enqueue(objects);
        foreach (List<Vector3> frame in soundMemory)
        {
            foreach (Vector3 t in frame)
            {
                if (Vector3.zero == t)
                {
                    sensor.AddObservation(Vector3.zero);
                    // Debug.Log(transform.name + " empty" );
                    continue;
                }
                Vector3 relativePosition = (transform.position - t).normalized;
                relativePosition.y = (transform.position - t).magnitude;
                sensor.AddObservation(relativePosition);
                // Debug.Log(transform.name + " " + relativePosition);
            }
        }

    }

    public void initThing()
    {
        for (int i = 0; i < MEM_SIZE; i++)
        {
            soundMemory.Enqueue(sound.getObjects(agentRb));

        }
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
    public void setSphereActive(bool active)
    {
        this.sphereActive = active;
    }
    public bool getSphereActive()
    {
        return this.sphereActive;
    }

    public void setSound(Sound sound)
    {
        this.sound = sound;
    }
}