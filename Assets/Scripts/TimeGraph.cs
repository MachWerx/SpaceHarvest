using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeGraph : MonoBehaviour {
    private LineRenderer m_LineRenderer;
    private const int kGranularity = 100;
    private int m_PrevTimeStep;
    private float m_MaxValue;
    private bool m_LogGraph;

    public void Init(float initialValue, float maxValue, bool logGraph = false) {
        m_MaxValue = maxValue;
        m_LogGraph = logGraph;

        m_LineRenderer = GetComponent<LineRenderer>();
        Debug.Assert(m_LineRenderer != null);

        m_LineRenderer.positionCount = 2;
        initialValue = normalize(initialValue);
        m_LineRenderer.SetPosition(1, new Vector3(0, initialValue, 0));
        m_LineRenderer.SetPosition(0, new Vector3(0, initialValue, 0));
        m_PrevTimeStep = 0;
    }

    public void AddData(float time, float value) {
        value = normalize(value);

        int timeStep = (int)(time * kGranularity);
        if (timeStep > m_PrevTimeStep) {
            m_LineRenderer.positionCount++;
            m_PrevTimeStep = timeStep;
        }
        m_LineRenderer.SetPosition(m_LineRenderer.positionCount - 1,
            new Vector3(time, value, 0));
    }

    private float normalize(float value) {
        if (m_LogGraph) {
            value = Mathf.Clamp(value, 1.0f, m_MaxValue);
            return Mathf.Log10(value) / Mathf.Log10(m_MaxValue);
        } else {
            value = Mathf.Clamp(value, 0.0f, m_MaxValue);
            return value / m_MaxValue;
        }
    }
}
