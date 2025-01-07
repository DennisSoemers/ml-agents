using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System.Collections.Generic;


public class SoundController
{
    private static List<string> detectableObjectTags = new List<string> { "ball", "blueAgent", "purpleAgent" };
    private List<GameObject> detectedGameObjects;
    private static float minimalSpeed = 0;
    private static int ballLocationInObsList = 0;
    public SoundController()
    {
        detectedGameObjects = new List<GameObject>();
    }
    public void AddToList(GameObject gameObject)
    {
        //Debug.Log("tag: " + gameObject.tag);
        if (!detectableObjectTags.Contains(gameObject.tag))
        {
            return;
        }
        // foreach (GameObject currentGameObject in detectedGameObjects)
        // {
        //     if (currentGameObject == gameObject)
        //     {
        //         return;
        //     }
        // }
        if (!detectedGameObjects.Contains(gameObject))
            detectedGameObjects.Add(gameObject);
    }

    public void RemoveFromList(GameObject gameObject)
    {
        //    foreach (GameObject currentGameObject in detectedGameObjects)
        //     {
        //         if (currentGameObject == gameObject)
        //         {
        //             detectedGameObjects.Remove(gameObject);
        //             return;
        //         }
        //     }
        if (detectedGameObjects.Contains(gameObject))
        {
            detectedGameObjects.Remove(gameObject);
        }
    }

    public List<GameObject> GetDetectedGameObjectsList()
    {
        return detectedGameObjects;
    }
    public Vector3[] GetObservations(Transform transform)
    {
        int vectorSize = 5;
        bool sawBall = false;
        int counter = 1;
        Vector3[] observations = new Vector3[vectorSize];
        foreach (GameObject currentGameObject in detectedGameObjects)
        {
            Rigidbody rigidbody = currentGameObject.GetComponent<Rigidbody>();
            if (!isObjectMoving(rigidbody))
            {
                continue;
            }
            Vector3 relativePosition = transform.position - rigidbody.transform.position;
            if (currentGameObject.tag == "ball")
            {
                sawBall=true;
                observations[ballLocationInObsList] = relativePosition;
                continue;
            }
            if (counter >= vectorSize)
            {
                break;
            }
            observations[counter] = relativePosition;
            counter++;
        }
        if(!sawBall){
            observations[ballLocationInObsList] = new Vector3(-100, -100, -100);
        }
        for (int i = counter; i < vectorSize; i++)
        {
            // Debug.Log(observations[i]);
            observations[i] = new Vector3(-100, -100, -100);
        }
        return observations;
    }
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