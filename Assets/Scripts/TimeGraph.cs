using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeGraph : MonoBehaviour {
    private LineRenderer m_LineRenderer;

    // Start is called before the first frame update
    void Start() {
        m_LineRenderer = GetComponent<LineRenderer>();
        Debug.Assert(m_LineRenderer != null);
    }

    // Update is called once per frame
    void Update() {

    }

    //public
}
