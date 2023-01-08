using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UiElement : MonoBehaviour {
    public delegate void TapDown();
    public TapDown OnTapDown;
    public virtual void ElementDown(Vector3 position) {
        OnTapDown?.Invoke();
    }

    public delegate void TapDragged();
    public TapDown OnTapDragged;
    public virtual void ElementDragged(Vector3 position) {
        OnTapDragged?.Invoke();
    }

    public delegate void TapUp();
    public TapDown OnTapUp;
    public virtual void ElementUp(Vector3 position) {
        OnTapUp?.Invoke();
    }
}
