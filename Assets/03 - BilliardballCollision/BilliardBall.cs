using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BilliardBall : MonoBehaviour
{
    [SerializeField] GameObject CircleObjectPrefab;

    GameObject[] circleObjects;
    physicsScene myPhysicsScene;
    int numBalls = 20;

    class ball
    {
        public float radius;
        public float mass;
        public Vector2 pos;
        public Vector2 vel;

        public ball(float radius, float mass, Vector2 pos, Vector2 vel)
        {
            this.radius = radius;
            this.mass = mass;
            this.pos = pos;
            this.vel = vel;
        }

        public void simulate(float dt, Vector2 gravity)
        {
            this.vel.x += gravity.x * Time.deltaTime;
            this.vel.y += gravity.y * Time.deltaTime;
            this.pos.x += this.vel.x * Time.deltaTime;
            this.pos.y += this.vel.y * Time.deltaTime;            
        }
    }

    struct physicsScene
    {
        public Vector2 gravity;
        public float dt;
        public float World_xMin;
        public float World_xMax;
        public float World_yMin;
        public float World_yMax;
        public bool paused;
        public ball[] balls;
        public float restitution;
	}

    void Start()
    {
        circleObjects = new GameObject[numBalls];
        for(int i = 0; i < numBalls; i++)
        {
            circleObjects[i] = Instantiate(CircleObjectPrefab);
            circleObjects[i].GetComponent<DrawCircle>().InitCircle(Color.red);
        }
        InitPhysicsScene();
        setupScene();
    }

    private void Update()
    {
        Simulate();
        Drawball();
    }

    void InitPhysicsScene()
    {
        myPhysicsScene = new physicsScene
        {
            gravity = new Vector2(0, 0),
            dt = 1f / 60f,
            World_xMin = -10,
            World_xMax = 10,
            World_yMin = -4.5f,
            World_yMax = 6.5f,
            paused = false,
            balls = new ball[numBalls],
            restitution = 1.0f
        };
    }

    void setupScene()
    {      
        for (int i = 0; i < numBalls; i++)
        {
            float radius = 0.05f + UnityEngine.Random.Range(0,10) * 0.03f;
            float mass = Mathf.PI * radius * radius;
            Vector2 pos = new Vector2(UnityEngine.Random.Range(0, 10) * (myPhysicsScene.World_xMax - myPhysicsScene.World_xMin),
                UnityEngine.Random.Range(0, 10) * (myPhysicsScene.World_yMax - myPhysicsScene.World_yMin));
            Vector2 vel = new Vector2(-1.0f + 2.0f * UnityEngine.Random.Range(0, 5), -1.0f + 2.0f * UnityEngine.Random.Range(0, 5));
            
            myPhysicsScene.balls[i] = new ball(radius, mass, pos, vel);
        }
    }

    void handleBallCollision(ball ball1, ball ball2, float restitution)
    {
        Vector2 dir = ball2.pos - ball1.pos;

        float d = Mathf.Sqrt(Vector2.SqrMagnitude(dir));
        if (d == 0.0 || d > ball1.radius + ball2.radius)
            return;

        dir *= 1/ d;

        float corr = (ball1.radius + ball2.radius - d) / 2;
        ball1.pos += dir * -corr;
        ball2.pos += dir * corr;

        var v1 = Vector2.Dot(ball1.vel, dir);
        var v2 = Vector2.Dot(ball2.vel, dir);        

        var m1 = ball1.mass;
        var m2 = ball2.mass;

        var newV1 = (m1 * v1 + m2 * v2 - m2 * (v1 - v2) * restitution) / (m1 + m2);
        var newV2 = (m1 * v1 + m2 * v2 - m1 * (v2 - v1) * restitution) / (m1 + m2);

        ball1.vel += dir * (newV1 - v1);
        ball2.vel += dir * (newV2 - v2);        
    }

    void handleWallCollision(ball myball, float xMin, float xMax, float yMin, float yMax)
    {    
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

    void Drawball()
    {
        for (int i = 0; i < myPhysicsScene.balls.Length; i++)
        {
            var ball = myPhysicsScene.balls[i];
            circleObjects[i].GetComponent<DrawCircle>().Draw(ball.pos, ball.radius);
        }
    }

    void Simulate()
    {
        for (int i = 0; i < myPhysicsScene.balls.Length; i++)
        {
            var ball1 = myPhysicsScene.balls[i];
            ball1.simulate(myPhysicsScene.dt, myPhysicsScene.gravity);

            for (int j = i + 1; j < myPhysicsScene.balls.Length; j++)
            {
                var ball2 = myPhysicsScene.balls[j];
                handleBallCollision(ball1, ball2, myPhysicsScene.restitution);
            }

            handleWallCollision(ball1, myPhysicsScene.World_xMin, myPhysicsScene.World_xMax, myPhysicsScene.World_yMin, myPhysicsScene.World_yMax);
        }
    }
    
}
