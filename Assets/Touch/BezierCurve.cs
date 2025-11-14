using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BezierCurve
{
    //[Range(0f, 1f)]
    //public float t;
    //List<Vector2> coefficients;
    List<Vector2> points;

    List<float> samplingList;
    int sampleCount = 50; // 샘플링 정밀도
    float length;
    public float Lengh { get => length; }

    //void PrecomputeCoefficients(List<Vector2> controlPoints)
    //{
    //    for (int i = 0; i < controlPoints.Count; i++)
    //    {
    //        Vector2 coeff = Vector2.zero;
    //        for (int j = i; j < controlPoints.Count; j++)
    //        {
    //            float sign = ((j - i) % 2 == 0) ? 1f : -1f;
    //            float bin = Binomial(controlPoints.Count-1, j) * Binomial(controlPoints.Count - 1-j, i-j);
    //            coeff += sign * bin * controlPoints[j];
    //        }
    //        coefficients.Add(coeff);
    //    }
    //}

    void BuildArcLengthTable()
    {
        samplingList = new List<float>();
        samplingList.Add(0f);

        Vector2 prev = Evaluate(0f);
        length = 0f;

        for (int i = 1; i <= sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            Vector2 p = Evaluate(t);
            length += Vector3.Distance(prev, p);
            samplingList.Add(length);
            prev = p;
        }

    }

    // 런타임에서 t만 넣으면 바로 계산
    //public Vector2 Evaluate(float t)
    //{
    //    Vector2 result = Vector2.zero;
    //    float tPower = 1f;


    //    for (int i = 0; i < coefficients.Count; i++)
    //    {
    //        result += coefficients[i] * tPower;
    //        tPower *= t;
    //    }
    //    return result;
    //}
    public Vector2 Evaluate(float t)
    {
        Vector2 result = Vector3.zero;
        int n = points.Count-1;

        for (int i = 0; i <= n; i++)
        {
            float binomial = Binomial(n, i);
            float term = binomial * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i);
            result += term * points[i];
        }


        return result;
    }

    

// 런타임에서 t만 넣으면 바로 계산
public Vector2 EvaluateSameValocity(float ratio)
    {
        float targetLength = length * Mathf.Clamp01(ratio);

        // 이진 탐색으로 arc length에 대응하는 t 찾기
        int low = 0;
        int high = sampleCount;

        while (low < high)
        {
            int mid = (low + high) / 2;
            if (samplingList[mid] < targetLength)
                low = mid + 1;
            else
                high = mid;
        }

        // low가 부드러운 t 범위
        int i = Mathf.Max(low - 1, 0);

        float lengthBefore = samplingList[i];
        float lengthAfter = samplingList[i + 1];

        float segmentT = (targetLength - lengthBefore) /
                         (lengthAfter - lengthBefore);

        float t0 = (float)i / sampleCount;
        float t1 = (float)(i + 1) / sampleCount;

        float t = Mathf.Lerp(t0, t1, segmentT);

        return Evaluate(t);

    }

    // 이항계수 계산
    static float Binomial(int n, int k)
    {
        if (k < 0 || k > n) return 0;
        float result = 1f;
        for (int i = 1; i <= k; i++)
        {
            result *= (n - (k - i));
            result /= i;
        }
        return result;
    }



    public BezierCurve(List<Vector2> controlPoints, int sampleCount = 100)
    {
        this.sampleCount = sampleCount;
        points = controlPoints;
        if (controlPoints.Count < 2)
        {
            Debug.LogError("제어점은 최소 2개 필요합니다.");
            return;
        }
        //PrecomputeCoefficients(controlPoints);
        BuildArcLengthTable();

    }

}