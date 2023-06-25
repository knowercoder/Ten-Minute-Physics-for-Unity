using System;
using System.Collections.Generic;
using UnityEngine;


public class MouseDragger : MonoBehaviour
{
    private Softbody body;
    public float radius = 1f;
    private int m_mouseParticle = -1;
    private float m_mouseMass;
    private float m_mouseT;
    private Vector3 m_mousePos = default(Vector3);
    private Vector3 vel;

    [SerializeField] SoftbodyPhysics softbodyPhysics;
        
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            List<Softbody> array = softbodyPhysics.pScene.objects;
            for (int i = 0; i < array.Count; i++)
            {
                this.m_mouseParticle = this.PickParticle(ray.origin, ray.direction, array[i], this.radius, ref this.m_mouseT);
                if (this.m_mouseParticle != -1)
                {                    
                    this.body = array[i];
                    this.m_mousePos = ray.origin + ray.direction * this.m_mouseT;
                    this.m_mouseMass = this.body.invMass[this.m_mouseParticle];
                    this.body.invMass[this.m_mouseParticle] = 0f;
                }
            }
        }
        if (Input.GetMouseButtonUp(0) && this.m_mouseParticle != -1)
        {
            this.body.invMass[this.m_mouseParticle] = this.m_mouseMass;

            this.body.vel[this.m_mouseParticle * 3] = this.vel.x / Time.deltaTime;
            this.body.vel[this.m_mouseParticle * 3 + 1] = this.vel.y / Time.deltaTime;
            this.body.vel[this.m_mouseParticle * 3 + 2] = this.vel.z / Time.deltaTime;

            this.m_mouseParticle = -1;
        }
        if (this.m_mouseParticle != -1)
        {
            Ray ray2 = Camera.main.ScreenPointToRay(Input.mousePosition);
            this.m_mousePos = ray2.origin + ray2.direction * this.m_mouseT;
            Vector3 vector = new Vector3(body.pos[this.m_mouseParticle*3], body.pos[this.m_mouseParticle*3 + 1], body.pos[this.m_mouseParticle*3 + 2]);
            Vector3 vector2 = Vector3.Lerp(vector, this.m_mousePos, 0.8f);
            this.vel = vector2 - vector;
            this.body.pos[this.m_mouseParticle*3] = vector2.x;
            this.body.pos[this.m_mouseParticle*3 + 1] = vector2.y;
            this.body.pos[this.m_mouseParticle*3 + 2] = vector2.z; 

            //this.body.vel[this.m_mouseParticle*3] = a.x / Time.deltaTime;
            //this.body.vel[this.m_mouseParticle*3 + 1] = a.y / Time.deltaTime;
            //this.body.vel[this.m_mouseParticle*3 + 2] = a.z / Time.deltaTime;
        }
    }

    
    private int PickParticle(Vector3 origin, Vector3 dir, Softbody body, float radius, ref float t)
    {
        // num - square radius
        // num2 - short distance betwenn the origin and particle
        // num3 - dot product of ray and line-between-origin-and-particle
        float num = radius * radius;
        float num2 = float.MaxValue;
        int result = -1;
        for (int i = 0; i < body.numParticles; i++)
        {
            Vector3 a = new Vector3(body.pos[i*3], body.pos[i*3 + 1], body.pos[i*3 +2]);
            Vector3 vector = a - origin;
            float num3 = Vector3.Dot(vector, dir);
            if (num3 > 0f)
            {
                float sqrMagnitude = (vector - num3 * dir).sqrMagnitude;
                if (sqrMagnitude < num && num3 < num2)
                {
                    num2 = num3;
                    result = i;
                }
            }
        }
        t = num2;
        return result;
    }        
        
}

