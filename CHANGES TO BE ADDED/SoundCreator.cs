using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundCreator : MonoBehaviour
{

    private float timer = 0f;
    public float timeoutDuration = 5f;
    public float grouthRate = 0.3f;
    public float growthPeriod = 0.05f;
    public float sizeToDieAt = 14f;
    private bool isTimeout = false;


    private float radius = 1f;      
    private int resolution = 60; 
    public Material material; 
    private Vector3 spawnPosition;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float bodySize = transform.localScale.y; // Get the object's scale in the y direction
        //alternative for y transform.position.y - transform.localScale.y / 2f
        // spawnPosition = new Vector3(transform.position.x,transform.position.y,transform.position.z);
        spawnPosition = transform.position;

        //condition to make a sound once a period of time
        timeSoundMaker();

    }

    private void timeSoundMaker(){
         if (!isTimeout)
        {            
            timer += Time.deltaTime;

            if (timer >= timeoutDuration)
            {
                OnTimeout();
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {   
        if (collision.gameObject.CompareTag("SoundOnColide")){
            CreateSound(spawnPosition);
        }
        
    }

    private void colisionSoundMaker(){
         if (!isTimeout)
        {            
            timer += Time.deltaTime;

            if (timer >= timeoutDuration)
            {
                OnTimeout();
            }
        }
    }

    //when timeout expires
    private void OnTimeout()
    {
        CreateSound(spawnPosition);
        isTimeout = true;
        ResetTimer();
    }
    //resets the timeout
    private void ResetTimer()
    {
        timer = 0f;
        isTimeout = false;
    }

    //creates a sound 
    void CreateSound(Vector3 spawnLocation)
    {
        // Create a new Mesh
        Mesh mesh = new Mesh();

        // Define the vertices for the disk (a circle with radius)
        Vector3[] vertices = new Vector3[resolution + 1]; // Center + the points around the circle
        Vector2[] uv = new Vector2[vertices.Length]; // UVs for the material
        int[] triangles = new int[resolution * 6]; // Double the number of triangles for double-sided

        // The center of the disk
        vertices[0] = Vector3.zero;
        uv[0] = new Vector2(0.5f, 0.5f); // UV for the center

        // Create the circle vertices
        for (int i = 0; i < resolution; i++)
        {
            float angle = i * Mathf.PI * 2 / resolution;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            // Set each vertex's position
            vertices[i + 1] = new Vector3(x, 0f, z);
            uv[i + 1] = new Vector2((x / radius + 1f) / 2f, (z / radius + 1f) / 2f); // Normalize UVs
        }

        // Create the triangles (each set of three vertices makes one triangle)
        for (int i = 0; i < resolution; i++)
        {
            int current = i + 1;
            int next = (i + 1) % resolution + 1; // Loop back to the start of the circle

            // Front-facing triangle
            triangles[i * 6] = 0;
            triangles[i * 6 + 1] = current;
            triangles[i * 6 + 2] = next;

            // Back-facing triangle
            triangles[i * 6 + 3] = 0;
            triangles[i * 6 + 4] = next;
            triangles[i * 6 + 5] = current;
        }

        // Set the mesh properties
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        // Create a new GameObject to hold the disk and add a MeshFilter, MeshRenderer, and a Collider
        GameObject diskObject = new GameObject("ZeroHeightDisk");
        diskObject.transform.position = spawnLocation;  // Position the disk at the custom spawn location

        // Add MeshFilter and MeshRenderer
        MeshFilter meshFilter = diskObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        MeshRenderer meshRenderer = diskObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material; // Optional: You can assign a material in the Inspector

        // // Optionally add a MeshCollider (if you want physical interaction)
        // MeshCollider meshCollider = diskObject.AddComponent<MeshCollider>();
        // meshCollider.sharedMesh = mesh;
        // meshCollider.convex = false; // If you want the collider to be used for physics

        // Attach the DiskBehavior script to the diskObject and set any parameters if needed
        DiskBehavior diskBehavior = diskObject.AddComponent<DiskBehavior>();
        diskBehavior.growthRate = grouthRate;  // Set custom growth rate if desired
        diskBehavior.growthPeriod = growthPeriod;   // Set custom growth period if desired
        diskBehavior.sizeToDieAt = sizeToDieAt;   // Set custom size to die at
        diskBehavior.setParentName(transform.name);
        diskBehavior.tag = "Sound";

        //colider
        SphereCollider sphereCollider = diskObject.AddComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
    }
}
