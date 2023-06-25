using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoftbodyPhysics : MonoBehaviour
{
    [Header("Physics Variables")]
    [SerializeField] Vector3 gravity = new Vector3(0f, -10f, 0f);
    [SerializeField] float dt = 0.016f;
    [SerializeField] int NumSubSteps = 10;

    [Header("Mesh Input")]
    [SerializeField] TextAsset BunnyMeshJson;
    [SerializeField] Material myMaterial;

    [Header("UI")]
    [SerializeField] Slider slider;
    [SerializeField] Button RunButton;
    [SerializeField] Button SquashButton;
    [SerializeField] Button NewbodyButton;   

    TetMesh tetMesh;
    [HideInInspector] public PhysicsScene pScene; 

    void Start()
    {        
        ReadMeshdatafromJson();
        pScene = new PhysicsScene(gravity, dt, NumSubSteps, true, null);        
        initPhysics();

        slider.onValueChanged.AddListener(ComplianceSlider);
        RunButton.onClick.AddListener(run);
        SquashButton.onClick.AddListener(squash);
        NewbodyButton.onClick.AddListener(newBody);
    }
    
    void Update()
    {        
        simulate();         
       
    }

    void ReadMeshdatafromJson()
    {
        tetMesh = new TetMesh();
        tetMesh = JsonUtility.FromJson<TetMesh>(BunnyMeshJson.ToString());        
    }

    void initPhysics()
    {
        var body = new Softbody(tetMesh, myMaterial);
        pScene.objects.Add(body);        
    }

    void simulate()
    {
        if (pScene.paused)
            return;

        //var sdt = Time.deltaTime / pScene.numSubsteps;
        var sdt = pScene.dt / pScene.numSubsteps;

        for (var step = 0; step < pScene.numSubsteps; step++)
        {

            for (var i = 0; i < pScene.objects.Count; i++)
                pScene.objects[i].preSolve(sdt, pScene.gravity);

            for (var i = 0; i < pScene.objects.Count; i++)
                pScene.objects[i].solve(sdt);

            for (var i = 0; i < pScene.objects.Count; i++)
                pScene.objects[i].postSolve(sdt);

        }       
    }

    void ComplianceSlider(float value)
    {
        for (var i = 0; i < pScene.objects.Count; i++)
            pScene.objects[i].edgeCompliance = value * 50;
    }

    public void run()
    {        
        if (pScene.paused)
            RunButton.GetComponentInChildren<Text>().text = "Stop";
        else
            RunButton.GetComponentInChildren<Text>().text = "Run";
        pScene.paused = !pScene.paused;
    }

    public void squash()
    {
        for (var i = 0; i < pScene.objects.Count; i++)
            pScene.objects[i].squash();
        if (!pScene.paused)
            run();
    }

    public void newBody()
    {
        var body = new Softbody(tetMesh, myMaterial);
        body.translate(-0.5f + UnityEngine.Random.Range(0, 10) * 0.1f, 0, -0.5f + UnityEngine.Random.Range(0, 10) * 0.1f);
        pScene.objects.Add(body);        
    }

    
}

public class PhysicsScene
{
    public Vector3 gravity;
    public float dt;
    public int numSubsteps;
    public bool paused;
    public List<Softbody> objects;

    public PhysicsScene(Vector3 gravity, float dt, int numSubsteps, bool paused, float[] objects)
    {
        this.gravity = gravity;
        this.dt = dt;
        this.numSubsteps = numSubsteps;
        this.paused = paused;
        this.objects = new List<Softbody>();
    }
}
