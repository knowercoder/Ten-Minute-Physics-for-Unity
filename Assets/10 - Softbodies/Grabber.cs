using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    [SerializeField] SoftbodyPhysics softbodyPhysics;
    private Ray raycaster;
    private Softbody physicsObject;
    private float distance;
    private Vector3 prevPos;
    private Vector3 vel;
    private float time;

    public Grabber()
    {         
        
        this.physicsObject = null;
        this.distance = 3f;
        this.prevPos = new Vector3();
        this.vel = new Vector3();
        this.time = 0f;
    }
    public void increaseTime(float dt)
    {
        this.time += dt;
    }
    void updateRaycaster()
    {
        this.raycaster = Camera.main.ScreenPointToRay(Input.mousePosition);
    }
    public void start()
    {
        this.physicsObject = null;
        this.updateRaycaster();
        
        this.physicsObject = softbodyPhysics.pScene.objects[0];
        //this.distance = intersects[0].distance;
        var pos = this.raycaster.origin;
        pos = pos + this.raycaster.direction * this.distance;
        this.physicsObject.startGrab(pos);
        this.prevPos = pos;
        this.vel.Set(0, 0, 0);
        this.time = 0f;                
           
    }
    public void move()
    {
        //Debug.Log("move called");
        if (this.physicsObject != null)
        {
            Debug.Log("move called2");
            this.updateRaycaster();
            var pos = this.raycaster.origin;
            pos = pos + this.raycaster.direction * this.distance;

            this.vel = pos;
            this.vel = vel - prevPos;
            if (this.time > 0.0)
                this.vel = vel / this.time;
            else
                this.vel.Set(0, 0, 0);
            this.prevPos = pos;
            this.time = 0;

            this.physicsObject.moveGrabbed(pos, this.vel);
        }
    }
    public void end()
    {
        if (this.physicsObject != null)
        {
            this.physicsObject.endGrab(this.prevPos, this.vel);
            this.physicsObject = null;
        }
    }

}

