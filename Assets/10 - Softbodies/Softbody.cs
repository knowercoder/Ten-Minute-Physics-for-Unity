using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class Softbody
{
    public int numParticles;
    public int numTets;
    public float[] pos;
    public float[] prevPos;
    public float[] vel;
    public int[] tetIds;
    public int[] edgeIds;
    public float[] restVol;
    public float[] edgeLengths;
    public float[] invMass;

    public float edgeCompliance;
    private float volCompliance;
    private float[] temp;
    private float[] grads;
    private int grabId;
    private float grabInvMass;

    private int[][] volIdOrder;
    private Mesh surfaceMesh;

    //local usage arrays
    private Vector3[] vertices;

    public Softbody(TetMesh tetMesh, Material material, float edgeCompliance = 0, float volCompliance = 0)
    {
        // physics

        this.numParticles = tetMesh.verts.Length / 3;
        this.numTets = tetMesh.tetIds.Length / 4;
        this.pos = tetMesh.verts;
        this.prevPos = tetMesh.verts;
        this.vel = new float[3 * this.numParticles];

        this.tetIds = tetMesh.tetIds;
        this.edgeIds = tetMesh.tetEdgeIds;
        this.restVol = new float[this.numTets];
        this.edgeLengths = new float[this.edgeIds.Length / 2];
        this.invMass = new float[this.numParticles];

        this.edgeCompliance = edgeCompliance;
        this.volCompliance = volCompliance;

        this.temp = new float[4 * 3];
        this.grads = new float[4 * 3];

        this.grabId = -1;
        this.grabInvMass = 0;

        this.initPhysics();        

        var SoftBodyObject = new GameObject();
        SoftBodyObject.name = tetMesh.name;
        SoftBodyObject.AddComponent<MeshFilter>();
        SoftBodyObject.AddComponent<MeshRenderer>().material = material;        
        
        this.surfaceMesh =  new Mesh();
        SoftBodyObject.GetComponent<MeshFilter>().mesh = this.surfaceMesh;
        vertices = new Vector3[this.pos.Length / 3];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(this.pos[i * 3], this.pos[i * 3 + 1], this.pos[i * 3 + 2]);
        }
        
        surfaceMesh.vertices = vertices.ToArray();
        surfaceMesh.triangles = tetMesh.tetSurfaceTriIds.ToArray();
        surfaceMesh.RecalculateNormals();
        //surfaceMesh.RecalculateBounds();

        this.volIdOrder = new int[][] { new int[] { 1, 3, 2 }, new int[] { 0, 2, 3 }, new int[] { 0, 3, 1 }, new int[] { 0, 1, 2 } };

    }

    public void translate(float x, float y, float z)
    {
        for (var i = 0; i < this.numParticles; i++)
        {
            MeshUtils.vecAdd(this.pos, i, new float[] { x, y, z }, 0);
            MeshUtils.vecAdd(this.prevPos, i, new float[] { x, y, z }, 0);
        }
    }

    public void updateMeshes()
    {        
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(this.pos[i * 3], this.pos[i * 3 + 1], this.pos[i * 3 + 2]);
        }

        this.surfaceMesh.vertices = vertices.ToArray();
        //this.surfaceMesh.geometry.attributes.position.needsUpdate = true;
        this.surfaceMesh.RecalculateNormals();
        //this.surfaceMesh.RecalculateBounds();
    }

    public float getTetVolume(int nr)
    {
        int id0 = this.tetIds[4 * nr];
        int id1 = this.tetIds[4 * nr + 1];
        int id2 = this.tetIds[4 * nr + 2];
        int id3 = this.tetIds[4 * nr + 3];
        MeshUtils.vecSetDiff(this.temp, 0, this.pos, id1, this.pos, id0);
        MeshUtils.vecSetDiff(this.temp, 1, this.pos, id2, this.pos, id0);
        MeshUtils.vecSetDiff(this.temp, 2, this.pos, id3, this.pos, id0);
        MeshUtils.vecSetCross(this.temp, 3, this.temp, 0, this.temp, 1);
        return MeshUtils.vecDot(this.temp, 3, this.temp, 2) / 6f;
    }

    public void initPhysics()
    {
        for(int i = 0; i < invMass.Length; i++)
        {
            invMass[i] = 0f;
        }

        for (int i = 0; i < restVol.Length; i++)
        {
            restVol[i] = 0f;
        }        

        for (var i = 0; i < this.numTets; i++)
        {
            float vol = this.getTetVolume(i);
            this.restVol[i] = vol;
            var pInvMass = vol > 0f ? 1f / (vol / 4f) : 0f;
            this.invMass[this.tetIds[4 * i]] += pInvMass;
            this.invMass[this.tetIds[4 * i + 1]] += pInvMass;
            this.invMass[this.tetIds[4 * i + 2]] += pInvMass;
            this.invMass[this.tetIds[4 * i + 3]] += pInvMass;
        }
        for (var i = 0; i < this.edgeLengths.Length; i++)
        {
            var id0 = this.edgeIds[2 * i];
            var id1 = this.edgeIds[2 * i + 1];
            this.edgeLengths[i] = Mathf.Sqrt(MeshUtils.vecDistSquared(this.pos, id0, this.pos, id1));
        }
    }

    public void preSolve(float dt, Vector3 gravity)
    {
        for (var i = 0; i < this.numParticles; i++)
        {
            if (this.invMass[i] == 0f)
                continue;
            MeshUtils.vecAdd(this.vel, i, new float[] {gravity.x, gravity.y, gravity.z }, 0, dt);
            MeshUtils.vecCopy(this.prevPos, i, this.pos, i);
            MeshUtils.vecAdd(this.pos, i, this.vel, i, dt);
            float y = this.pos[3 * i + 1];
            if (y < 0f)
            {
                MeshUtils.vecCopy(this.pos, i, this.prevPos, i);
                this.pos[3 * i + 1] = 0;
            }
        }
    }

    public void solve(float dt)
    {
        this.solveEdges(this.edgeCompliance, dt);
        this.solveVolumes(this.volCompliance, dt);
    }

    public void postSolve(float dt)
    {
        for (var i = 0; i < this.numParticles; i++)
        {
            if (this.invMass[i] == 0f)
                continue;
            MeshUtils.vecSetDiff(this.vel, i, this.pos, i, this.prevPos, i, 1 / dt);
        }
        this.updateMeshes();
    }

    public void solveEdges(float compliance, float dt)
    {
        var alpha = compliance / dt / dt;

        for (var i = 0; i < this.edgeLengths.Length; i++)
        {
            var id0 = this.edgeIds[2 * i];
            var id1 = this.edgeIds[2 * i + 1];
            var w0 = this.invMass[id0];
            var w1 = this.invMass[id1];
            var w = w0 + w1;
            if (w == 0f)
                continue;

            MeshUtils.vecSetDiff(this.grads, 0, this.pos, id0, this.pos, id1);
            var len = Mathf.Sqrt(MeshUtils.vecLengthSquared(this.grads, 0));
            if (len == 0f)
                continue;
            MeshUtils.vecScale(this.grads, 0, 1f / len);
            var restLen = this.edgeLengths[i];
            var C = len - restLen;
            var s = -C / (w + alpha);
            MeshUtils.vecAdd(this.pos, id0, this.grads, 0, s * w0);
            MeshUtils.vecAdd(this.pos, id1, this.grads, 0, -s * w1);
        }
    }

    public void solveVolumes(float compliance, float dt)
    {
        var alpha = compliance / dt / dt;

        for (var i = 0; i < this.numTets; i++)
        {
            var w = 0f;

            for (var j = 0; j < 4; j++)
            {
                var id0 = this.tetIds[4 * i + this.volIdOrder[j][0]];
                var id1 = this.tetIds[4 * i + this.volIdOrder[j][1]];
                var id2 = this.tetIds[4 * i + this.volIdOrder[j][2]];

                MeshUtils.vecSetDiff(this.temp, 0, this.pos, id1, this.pos, id0);
                MeshUtils.vecSetDiff(this.temp, 1, this.pos, id2, this.pos, id0);
                MeshUtils.vecSetCross(this.grads, j, this.temp, 0, this.temp, 1);
                MeshUtils.vecScale(this.grads, j, 1f / 6f);

                w += this.invMass[this.tetIds[4 * i + j]] * MeshUtils.vecLengthSquared(this.grads, j);
            }
            if (w == 0f)
                continue;

            var vol = this.getTetVolume(i);
            var restVol = this.restVol[i];
            var C = vol - restVol;
            var s = -C / (w + alpha);

            for (var j = 0; j < 4; j++)
            {
                var id = this.tetIds[4 * i + j];
                MeshUtils.vecAdd(this.pos, id, this.grads, j, s * this.invMass[id]);
            }
        }
    }

    public void squash()
    {
        for (var i = 0; i < this.numParticles; i++)
        {
            this.pos[3 * i + 1] = 0.5f;
        }
        this.updateMeshes();
    }

    public void startGrab(Vector3 pos)
    {
        var p = new float[] { pos.x, pos.y, pos.z };
        var minD2 = float.MaxValue;// Number.MAX_VALUE;
        this.grabId = -1;
        for (int i = 0; i < this.numParticles; i++)
        {
            var d2 = MeshUtils.vecDistSquared(p, 0, this.pos, i);
            if (d2 < minD2)
            {
                minD2 = d2;
                this.grabId = i;
            }
        }

        if (this.grabId >= 0)
        {
            this.grabInvMass = this.invMass[this.grabId];
            this.invMass[this.grabId] = 0;
            MeshUtils.vecCopy(this.pos, this.grabId, p, 0);
        }
    }

    public void moveGrabbed(Vector3 pos, Vector3 vel)
    {
        if (this.grabId >= 0)
        {
            var p = new float[] { pos.x, pos.y, pos.z };
            MeshUtils.vecCopy(this.pos, this.grabId, p, 0);
        }
    }

    public void endGrab(Vector3 pos, Vector3 vel)
    {
        if (this.grabId >= 0)
        {
            this.invMass[this.grabId] = this.grabInvMass;
            var v = new float[] { vel.x, vel.y, vel.z };
            MeshUtils.vecCopy(this.vel, this.grabId, v, 0);
        }
        this.grabId = -1;
    }
}
