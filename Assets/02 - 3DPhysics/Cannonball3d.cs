using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannonball3d : MonoBehaviour
{
    Transform Renderball;

    Vector3 gravity => new Vector3(0, -10, 0);
    float timeStep => 1 / 60;
    ball myball = new ball();

    //Screen border
    float xMin = -4.5f;
    float xMax = 4.5f;
    float yMin = 0.5f;
    float yMax = 9.5f;
    float zMin = -4.5f;
    float zMax = 4.5f;

    class ball
    {        
        public Vector3 pos;
        public Vector3 vel;

        public ball()
        {            
            pos = new Vector3(0.2f, 0.2f, 0.2f);
            vel = new Vector3(10, 15, 9);
        }
    }

    private void Start()
    {
        Renderball = this.transform;
    }

    void Update()
    {
        Simulate();
        Render();
    }

    void Simulate()
    {
        myball.vel.x += gravity.x * Time.deltaTime;
        myball.vel.y += gravity.y * Time.deltaTime;
        myball.vel.z += gravity.z * Time.deltaTime;
        myball.pos.x += myball.vel.x * Time.deltaTime;
        myball.pos.y += myball.vel.y * Time.deltaTime;
        myball.pos.z += myball.vel.z * Time.deltaTime;

        if (myball.pos.x < xMin)
        {
            myball.pos.x = xMin;
            myball.vel.x = -myball.vel.x;
        }
        if (myball.pos.x > xMax)
        {
            myball.pos.x = xMax;
            myball.vel.x = -myball.vel.x;
        }
        if (myball.pos.y < yMin)
        {
            myball.pos.y = yMin;
            myball.vel.y = -myball.vel.y;
        }
        if (myball.pos.y > yMax)
        {
            myball.pos.y = yMax;
            myball.vel.y = -myball.vel.y;
        }
        if (myball.pos.z < zMin)
        {
            myball.pos.z = zMin;
            myball.vel.z = -myball.vel.z;
        }
        if (myball.pos.z > zMax)
        {
            myball.pos.z = zMax;
            myball.vel.z = -myball.vel.z;
        }

    }

    void Render()
    {
        Renderball.position = myball.pos;
    }
}
