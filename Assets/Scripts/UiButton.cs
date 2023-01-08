using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiButton : UiElement {
    
    private float m_Value;

    public float value {
        get {
            return m_Value;
        }

        set {
            m_Value = Mathf.Clamp01(value);
            transform.parent.localScale = new Vector3(m_Value, 1, 1);
        }
    }
}
