using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiButton : UiElement {
    public bool m_AutoPress;
    private float m_Value;

    public float value {
        get {
            return m_Value;
        }

        set {
            m_Value = Mathf.Clamp01(value);
            transform.parent.localScale = new Vector3(m_Value, 1, 1);
            if (gameObject.activeInHierarchy && m_AutoPress && m_Value == 1) {
                OnTapDown?.Invoke();
            }
        }
    }
}
