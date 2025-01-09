using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System.Collections.Generic;
using System;



/// <summary>
/// this class will manage the sound observations of the agent.
/// it will keep track of the objects that are making sound and will return the observations to the agent when requested.
/// observation will be the relative position of the objects that are making sound (moving faster than minimum speed).
/// it will pad the observations with (-100,-100,-100) if there are less than 4 objects making sound.
/// </summary>
public class SoundController
{
    // detectable objects are the objects that the agent can hear.
    private static List<string> detectableObjectTags = new List<string> { "ball", "blueAgent", "purpleAgent" };
    // list of objects that are detectable and in the sphere collider of the agent.
    private List<GameObject> detectedGameObjects;
    // minimum speed of the object to be considered as making sound.
    private static float minimalSpeed = 0;
    // index of the ball in the observation list.
    private static int ballLocationInObsList = 0;

    private Queue<Vector3[]> soundMemory;
    // number of frames saved in the sound memory queue
    private int MEM_SIZE = 3;
    // number of observations in each frame
    private int vectorSize = 4;

    public SoundController()
    {
        detectedGameObjects = new List<GameObject>();
        soundMemory = new Queue<Vector3[]>(MEM_SIZE);
        for (int i = 0; i < MEM_SIZE; i++)
        {
            Vector3[] temp = new Vector3[vectorSize];
            for (int j = 0; j < vectorSize; j++)
            {
                temp[j] = new Vector3(-100, -100, -100);
            }
            soundMemory.Enqueue(temp);
        }

    }
    /// <summary>
    /// add gameobjectto the detectedGameObjects only if it's not there and appears in the detectable list.
    /// </summary>
    /// <param name="gameObject"></param>
    public void AddToList(GameObject gameObject)
    {
        if (detectableObjectTags.Contains(gameObject.tag) && !detectedGameObjects.Contains(gameObject))
        {
            detectedGameObjects.Add(gameObject);
        }
    }

    /// <summary>
    /// remove gameobject from the detectedGameObjects list.
    /// </summary>
    /// <param name="gameObject"></param>
    public void RemoveFromList(GameObject gameObject)
    {
        if (detectedGameObjects.Contains(gameObject))
        {
            detectedGameObjects.Remove(gameObject);
        }
    }

    /// <summary>
    /// returns the observations to the agent.
    /// the observations will be the sound memory with the new observations added the the oldest removed.
    /// </summary>
    /// <param name="transform"></param>
    /// <returns>Queue<Vector3[]> of the observations. </returns>
    public Queue<Vector3[]> GetObservations(Transform transform)
    {
        soundMemory.Dequeue();
        Vector3[] currentObservations = createObservations(transform);
        soundMemory.Enqueue(currentObservations);
        return soundMemory;
    }

    /// <summary>
    /// create the observations for the agent based on the current detectedGameObjects.
    /// this function will go over all gameobjects in the detectedGameObjects list add it to the observations array.
    /// if the currentGameObject isn't moveing, it will not be added to the observations.
    /// if the currentGameObject is the ball, it will be added to the first index of the observations array.
    /// if the currentGameObject is not the ball, it will be added to the next avaliable index of the observations array.
    /// </summary>
    /// <param name="transform"> the agent transform variable</param>
    /// <returns>array of vector3</returns>
    private Vector3[] createObservations(Transform transform)
    {
        bool sawBall = false;
        int counter = 1;
        Vector3[] observations = new Vector3[vectorSize];
        foreach (GameObject currentGameObject in detectedGameObjects)
        {
            Rigidbody rigidbody = currentGameObject.GetComponent<Rigidbody>();
            if (!isObjectMoving(rigidbody))
            {
                observations[counter] = new Vector3(-100, -100, -100);
                counter++;
                continue;
            }
            Vector3 relativePosition = transform.position - rigidbody.transform.position;
            if (currentGameObject.tag == "ball")
            {
                sawBall = true;
                observations[ballLocationInObsList] = relativePosition;
                continue;
            }
            if (counter >= vectorSize)
            {
                Debug.LogError("!!!ERROR, TOO MANY OBSERVATIONS!!!" + counter);
                break;
            }
            observations[counter] = relativePosition;
            counter++;
        }
        if (!sawBall)
        {
            observations[ballLocationInObsList] = new Vector3(-100, -100, -100);
        }
        for (int i = counter; i < vectorSize; i++)
        {
            //Debug.Log(observations[i]);
            observations[i] = new Vector3(-100, -100, -100);
        }
        return observations;
    }
    /// <summary>
    /// check if the object is moving based on the minimal speed.
    /// </summary>
    /// <param name="rigidbody"></param>
    /// <returns></returns>
    private bool isObjectMoving(Rigidbody rigidbody)
    {
        // Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        if (rigidbody.velocity.magnitude < minimalSpeed)
        {
            return false;
        }
        return true;
    }
}