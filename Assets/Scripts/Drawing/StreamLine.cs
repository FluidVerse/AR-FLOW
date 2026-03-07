using NUnit.Framework;
using System.Collections.Generic;
using System.Drawing;
//using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class StreamLine : MonoBehaviour
{
    SplineContainer spline;
    LineRenderer line;
    SpriteRenderer arrow;
    Transform arrowRef;
    List<Transform> arrowList = new();
    public int resolution = 10;
    public float speed = 0.000f;
    public float position = 0.0f;
    public int ArrowCount = 2;
    private float originalStartWidth;
    private float originalEndWidth;


    private void Awake()
    {
        spline = GetComponent<SplineContainer>();
        line = GetComponent<LineRenderer>(); 
        arrow = GetComponentInChildren<SpriteRenderer>();
        line.positionCount = resolution;
        line.useWorldSpace = true;
        line.alignment = LineAlignment.View;

        // Store original widths to counteract scaling
        originalStartWidth = line.startWidth;
        originalEndWidth = line.endWidth;

        arrowRef = transform.Find("Arrow");
        arrowList.Add(arrowRef);

        for (int i = 1;  i < ArrowCount; i++)
        {
            Transform clone = Instantiate(arrowRef, transform);
            clone.name = $"Arrow_{i}";
            arrowList.Add(clone);
        }
    }
    void Start()
    {
        // Set line positions in world space
        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / (resolution - 1);
            Vector3 pos = spline.EvaluatePosition(t);
            line.SetPosition(i, pos);
        }
        
        // Adjust line width to scale with the object (world space width)
        line.startWidth = originalStartWidth * transform.lossyScale.x;
        line.endWidth = originalEndWidth * transform.lossyScale.x;
        
        Debug.Log(name + ": " + line.GetPosition(0));
    }

    // Update is called once per frame
    void Update()
    {
        // Update line positions in world space to follow the moving/scaling object
        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / (resolution - 1);
            Vector3 pos = spline.EvaluatePosition(t);
            line.SetPosition(i, pos);
        }

        position += speed * Time.deltaTime;
        if (position < 0.0f) position += 1.0f;
        if (position > 1.0f) position -= 1.0f;
        for (int i = 0; i < ArrowCount; i++)
        {
            float positionI = position + (float)i / (float)ArrowCount;
            if (positionI < 0.0f) positionI += 1.0f;
            if (positionI > 1.0f) positionI -= 1.0f;
            Vector3 arrowPos = spline.EvaluatePosition(positionI);
            Vector3 arrowTan = spline.EvaluateTangent(positionI);
            arrowTan = arrowTan.normalized;

            arrowList[i].position = arrowPos;
            Quaternion q = Quaternion.LookRotation(arrowTan);
            arrowList[i].transform.rotation = q * Quaternion.Euler(0, -90f, 0);
        }
    }

}
