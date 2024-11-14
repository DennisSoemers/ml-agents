using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundReader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void OnTriggerEnter(Collider other)
    {

        // Check if the object entering the trigger has the "Sound" tag
        if (other.CompareTag("Sound")){

            DiskBehavior collidedObject = other.GetComponent<DiskBehavior>();
            
            if ((collidedObject != null) && (collidedObject.getParentName()!=transform.name)){
                float timeDelay = getTimeDelay(collidedObject);
                var(xSoundCoo,ySoundCoo,zSoundCoo) = getSoundPropagationCoordinates(collidedObject);

                Debug.Log(timeDelay);
                Debug.Log(xSoundCoo);
                Debug.Log(ySoundCoo);
                Debug.Log(zSoundCoo);
            }else{
                // Debug.LogError("The object does not have a DiskBehavior component.");
            }
        }
    }

    public float getTimeDelay(DiskBehavior collidedObject){
        float localTime = Time.time;
        float timeDelay = localTime-collidedObject.getCreationTime();
        return timeDelay;
    }

    public (float, float, float) getSoundPropagationCoordinates(DiskBehavior collidedObject){
        return (collidedObject.getXCoordinate(), collidedObject.getYCoordinate(), collidedObject.getZCoordinate());
    }
}
