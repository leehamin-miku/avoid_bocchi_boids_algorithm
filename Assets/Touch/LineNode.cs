using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitLine : MonoBehaviour, ITouchableObject
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] List<UnitLineNode> listNode;
    BezierCurve curve;
    LineRenderer lineRenderer;

    [SerializeField] GameObject circle;

    int resolution = 30; //선 해상도

    [SerializeField] float duration;

    void Start()
    {
        List<Vector2> vectorList = new List<Vector2>();
        vectorList.Add(transform.position); //나로부터 모든 unit들
        for (int i=0; i< listNode.Count; i++)
        {
            vectorList.Add(listNode[i].transform.position);
        }


        curve = new BezierCurve(vectorList);
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = resolution;
        lineRenderer.startWidth = 0.2f;

        for (int i = 0; i < resolution; i++)
        {
            float t = (float)i / (resolution-1);
            lineRenderer.SetPosition(i, curve.Evaluate(t));
        }
    }





    public bool IsTouchable = true;
    Vector2 nowVec;
    float errorSum;
    Coroutine coroutine;

    public void TouchThisObject(Vector2 vec)
    {
        
        nowVec = Camera.main.ScreenToWorldPoint(vec);
        coroutine = StartCoroutine(DragCoroutine());
    }
    public void Drag(Vector2 vec)
    {
        nowVec = Camera.main.ScreenToWorldPoint(vec);
    }


    public IEnumerator DragCoroutine()
    {
        float t = 0;
        errorSum = 0;
        Vector2 curveVector;
        while (t < duration)
        {
            curveVector = curve.EvaluateSameValocity(t / duration);
            errorSum += Vector2.Distance(curveVector, nowVec)*Time.deltaTime;
            circle.transform.position = new Vector3(curveVector.x, curveVector.y);
            t += Time.deltaTime;
            yield return null;
        }

        float errorRate = errorSum/(duration*curve.Lengh);
        coroutine = null;
        Debug.Log("오차율 : "+errorRate+ "\n평균 오차 : "+ (errorSum /duration));
    }

    public void TouchEnd(Vector2 vec)
    {
        if(coroutine!= null)
        {
            Debug.Log("너무 빨리 땜!");
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }

    //무조건 screen좌표를 받으므로 월드의 경우 변환이 필요함!
}

