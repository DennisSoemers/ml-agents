using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiskBehavior : MonoBehaviour
{
    public float growthRate; // Growth amount each period
    public float growthPeriod; // Time (in seconds) between each growth increase
    public float sizeToDieAt; 

    private float growthTimer;
    private float creationTime;
    private float xCoordinate;
    private float yCoordinate;
    private float zCoordinate;

    private string parentName;


    // Start is called before the first frame update
    void Start()
    {   
        growthTimer = 0f;
        creationTime = Time.time;
        xCoordinate = transform.position.x;
        yCoordinate = transform.position.y;
        zCoordinate = transform.position.z;

    }

    // Update is called once per frame
    void Update()
    {
        if(transform.localScale.magnitude > sizeToDieAt){
            Destroy(gameObject);
        }


        growthTimer += Time.deltaTime;
        if (growthTimer >= growthPeriod)
        {
            IncreaseSize();
            growthTimer = 0f; // Reset the timer
        }

    }
    
    public float grouthFunction(){
        return growthRate*transform.localScale.x;
    }

    public void IncreaseSize()
    {
        // Increase the scale by a fixed amount
        // transform.localScale += new Vector3(growthRate, 0, growthRate);
        //alternative with a fucntion
        transform.localScale += new Vector3(grouthFunction(), 0, grouthFunction());
    }

    public void setParentName(string parentName){
        this.parentName = parentName;   
    }

    public string getParentName(){
        return this.parentName;   
    }

    public float getCreationTime(){
        return creationTime;
    }

    public float getXCoordinate(){
        return xCoordinate;
    }

    public float getYCoordinate(){
        return yCoordinate;
    }

    public float getZCoordinate(){
        return zCoordinate;
    }
}
