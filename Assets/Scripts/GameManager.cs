using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private Transform m_Sun;
    [SerializeField] private Transform m_Planet;
    [SerializeField] private TMPro.TextMeshPro m_PopulationText;
    [SerializeField] private TMPro.TextMeshPro m_CountdownText;

    [SerializeField] private UiButton m_CarrotButton;

    [SerializeField] private TimeGraph m_CarrotGraph;

    private float m_GameAge;
    private float m_Population;
    private float m_Countdown;

    private float m_CarrotCount;

    private float m_SunRotation;
    private float m_PlanetRotation;
    private int m_UiLayer;

    private UiElement m_ActiveUiElement;

    const float kSunRotationSpeed = 36.0f;
    const float kPlanetRotationSpeed = 72.0f;

    // Start is called before the first frame update
    void Start() {
        m_UiLayer = LayerMask.GetMask("UI");
        m_CarrotButton.OnTapDown += CarrotButtonDown;

        InitGame();
    }

    // Update is called once per frame
    void Update() {
        // rotate sun and planet
        m_SunRotation += Time.deltaTime * kSunRotationSpeed;
        m_Sun.localRotation = Quaternion.Euler(0, m_SunRotation, 0);
        m_PlanetRotation += Time.deltaTime * kPlanetRotationSpeed;
        m_Planet.localRotation = Quaternion.Euler(0, m_PlanetRotation, 0);

        // check for elements being interacted with
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 screenPoint = ray.origin + 10.0f * ray.direction;
        if (m_ActiveUiElement != null && Input.GetMouseButton(0)) {
            m_ActiveUiElement.ElementDragged(screenPoint);
        }
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100.0f, m_UiLayer)) {
            if (Input.GetMouseButtonDown(0)) {
                Debug.Assert(m_ActiveUiElement == null);
                UiElement uiElement = hit.collider.GetComponent<UiElement>();
                //print("down on " + hit.collider.name);
                Debug.Assert(uiElement != null);
                m_ActiveUiElement = uiElement;
                m_ActiveUiElement.ElementDown(screenPoint);
            }
        }
        if (m_ActiveUiElement != null && !Input.GetMouseButton(0)) {
            m_ActiveUiElement.ElementUp(screenPoint);
            m_ActiveUiElement = null;
        }

        // do simulation
        m_GameAge += Time.deltaTime;
        m_CarrotButton.value += Time.deltaTime;

        // update graphs
        m_CarrotGraph.AddData(m_GameAge / 100.0f, m_CarrotCount);
    }

    void InitGame() {
        m_SunRotation = 0;
        m_PlanetRotation = 0;

        m_GameAge = 0;
        m_Population = 10.0f;
        m_Countdown = 10000.0f;

        m_CarrotCount = 20;
        m_CarrotGraph.Init(m_CarrotCount, 1e6f, true);
    }

    void CarrotButtonDown() {
        m_CarrotButton.value = 0;
        m_CarrotCount += 10f;
    }
}
