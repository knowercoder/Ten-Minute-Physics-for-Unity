using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

public class BeanOnWire : MonoBehaviour
{
    [SerializeField] GameObject CircleObjectPrefab;

    GameObject WireObject;
    GameObject BeadObject;

    physicsScene myPhysicsScene = new physicsScene();

    struct physicsScene
    {
        public Vector2 gravity;
        public float dt;
        public int numSteps;
        public bool paused;
        public Vector2 wireCenter;
        public float wireRadius;
        public Bead bead;
    }

    class Bead
    {
        public float radius;
        public float mass;
        public Vector2 pos;
        public Vector2 prevPos;
        public Vector2 vel;

        public Bead(float radius, float mass, Vector2 pos)
        {
            this.radius = radius;
            this.mass = mass;
            this.pos = pos;
            this.prevPos = pos;
            this.vel = new Vector2();
        }

        public void startStep(float dt, Vector2 gravity)
        {
            this.vel += gravity * dt;
            this.prevPos =  this.pos;
            this.pos += this.vel * dt;
        }

        public float keepOnWire(Vector2 center, float radius)
        {
            Vector2 dir = new Vector2();
            dir = this.pos - center;
            float len = Mathf.Sqrt(Vector2.SqrMagnitude(dir));
            if (len == 0.0)
                return 0;
            dir = dir * (float)(1.0 / len);
            var lambda = radius - len;
            this.pos += dir * lambda;
            return lambda;
        }
        public void endStep(float dt)
        {
            this.vel = this.pos - prevPos;
            this.vel = vel * (float)(1.0 / dt);
        }
    }

    void Start()
    {        
        //Application.targetFrameRate = 60;
        WireObject = Instantiate(CircleObjectPrefab);
        WireObject.name = "Wire";        
        BeadObject = Instantiate(CircleObjectPrefab);
        BeadObject.name = "Bead";
        DrawWire();
        InitPhysicsScene();
        setupScene();        
    }

    //void Update()
    //{
    //    Simulate();
    //    DrawBead();
    //}

    private void FixedUpdate()
    {
        Simulate();
        DrawBead();
    }

    void InitPhysicsScene()
    {
        myPhysicsScene = new physicsScene
        {
            gravity = new Vector2(0f, -10f),
            dt = 1f/60f,
            numSteps = 100,
            paused = false,
            wireCenter = new Vector2(0f, 0f),
            wireRadius = 4f,
            bead = null
        };
    }

    void setupScene()
    {
        //myPhysicsScene.paused = true;

        myPhysicsScene.wireCenter.x = 0;
        myPhysicsScene.wireCenter.y = 0;
        myPhysicsScene.wireRadius = 4;

        var pos = new Vector2(
            myPhysicsScene.wireCenter.x + myPhysicsScene.wireRadius,
            myPhysicsScene.wireCenter.y);

        myPhysicsScene.bead = new Bead(0.1f, 1.0f, pos);  
    }

    void Simulate()
    {
        if (myPhysicsScene.paused)
            return;

        var sdt = myPhysicsScene.dt / myPhysicsScene.numSteps;
        //var sdt = Time.deltaTime / myPhysicsScene.numSteps;

        for (var step = 0; step < myPhysicsScene.numSteps; step++)
        {

            myPhysicsScene.bead.startStep(sdt, myPhysicsScene.gravity);

            var lambda = myPhysicsScene.bead.keepOnWire(
                    myPhysicsScene.wireCenter, myPhysicsScene.wireRadius);            

            myPhysicsScene.bead.endStep(sdt);            
        }
    }

    void DrawWire()
    {
        WireObject.GetComponent<DrawCircle>().InitCircle(Color.blue, 0.15f);
        BeadObject.GetComponent<DrawCircle>().InitCircle(Color.red);
        BeadObject.GetComponent<DrawCircle>().SetLayerOrder(1);
        WireObject.GetComponent<DrawCircle>().Draw(Vector3.zero, 4f);
    }

    void DrawBead()
    {        
        BeadObject.GetComponent<DrawCircle>().Draw(myPhysicsScene.bead.pos, myPhysicsScene.bead.radius);
    }

    public void run()
    {
        myPhysicsScene.paused = false;
    }

    public void step()
    {
        myPhysicsScene.paused = false;
        Simulate();
        myPhysicsScene.paused = true;
    }
}
