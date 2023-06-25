using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DrawCircle : MonoBehaviour
{
    [SerializeField] LineRenderer circleRenderer;
    [SerializeField] int resolution = 50;
    [SerializeField] float lineWidth = 0.5f;

    void Start()
    {
        //InitCircle(Color.red, lineWidth);
    }    

    public void InitCircle(Color color, float width = 0.5f)
    {
        circleRenderer.startColor = color;
        circleRenderer.endColor = color;
        circleRenderer.startWidth = width;
        circleRenderer.endWidth = width;
        circleRenderer.loop = true;
        circleRenderer.positionCount = resolution;
    }

    public void Draw(Vector3 position, float radius = 0.1f)
    {    
        float angle = 0f;

        for (int i = 0; i < resolution; i++)
        {
            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);

            circleRenderer.SetPosition(i, new Vector3(x, y, 0f) + position);

            angle += 2f * Mathf.PI / resolution;
        }
    }

    public void SetLayerOrder(int order)
    {
        this.circleRenderer.sortingOrder = order;
    }

}
