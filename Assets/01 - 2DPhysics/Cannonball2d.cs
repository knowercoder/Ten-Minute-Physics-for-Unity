using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannonball2d : MonoBehaviour
{
    [SerializeField] DrawCircle drawCircle;

    Vector2 gravity => new Vector2(0, -10);
    float timeStep => 1 / 60;
    ball myball = new ball();

    //Screen border
    float xMin = -10;
    float xMax = 10;
    float yMin = -4.5f;
    float yMax = 6.5f;

    class ball
    {
        public float radius;
        public Vector2 pos;
        public Vector2 vel;

        public ball()
        {
            radius = 0.1f;
            pos = new Vector2(0.2f, 0.2f);
            vel = new Vector2(10, 15);
        }
    }

    private void Start()
    {
        drawCircle.InitCircle(Color.red);
    }


    void Update()
    {
        Simulate();
        DrawImage();
    }

    void Simulate()
    {
        myball.vel.x += gravity.x * Time.deltaTime;
        myball.vel.y += gravity.y * Time.deltaTime;
        myball.pos.x += myball.vel.x * Time.deltaTime;
        myball.pos.y += myball.vel.y * Time.deltaTime;

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

    }

    void DrawImage()
    {
        drawCircle.Draw(new Vector3(myball.pos.x, myball.pos.y, 0), myball.radius);
    }

}
