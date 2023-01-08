using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiSlider : UiElement {
    private float m_Value;
    private Vector3 m_LastPosition;

    public float value {
        get {
            return m_Value;
        }

        set {
            m_Value = Mathf.Clamp01(value);
            transform.localPosition = m_Value * Vector3.right;
        }
    }


    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public override void ElementDown(Vector3 position) {
        m_LastPosition = transform.parent.InverseTransformPoint(position);
        base.ElementDown(position);
    }

    public override void ElementDragged(Vector3 position) {
        Vector3 localPosition = transform.parent.InverseTransformPoint(position);
        value += localPosition.x - m_LastPosition.x;
        m_LastPosition = localPosition;
        base.ElementDragged(position);
    }
}
