using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private Transform m_Sun;
    [SerializeField] private Transform m_Planet;
    [SerializeField] private TMPro.TextMeshPro m_PopulationText;
    [SerializeField] private TMPro.TextMeshPro m_CountdownText;

    [SerializeField] private UiButton m_CarrotButton;
    [SerializeField] private UiButton m_MiningButton;
    [SerializeField] private UiButton m_ResearchButton;

    [SerializeField] private TimeGraph m_PopulationGraph;
    [SerializeField] private TimeGraph m_CarrotGraph;
    [SerializeField] private TimeGraph m_ProductivityGraph;
    [SerializeField] private TimeGraph m_CivilizationGraph;

    private float m_Population;
    private float m_CarrotCount;
    private float m_Productivity;
    private float m_Civilization;
    private float m_GameAge;

    private float m_SunRotation;
    private float m_PlanetRotation;
    private int m_UiLayer;

    private UiElement m_ActiveUiElement;

    const float kSunRotationSpeed = 36.0f;
    const float kPlanetRotationSpeed = 72.0f;
    const float kMaxAge = 10000.0f;
    const float kYearsPerSecond = 200.0f;

    // Start is called before the first frame update
    void Start() {
        m_UiLayer = LayerMask.GetMask("UI");
        m_CarrotButton.OnTapDown += CarrotButtonDown;
        m_MiningButton.OnTapDown += MiningButtonDown;
        m_ResearchButton.OnTapDown += ResearchButtonDown;

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
        m_GameAge += Time.deltaTime * kYearsPerSecond;
        m_CarrotButton.value += Time.deltaTime;
        //if (m_CarrotButton.value == 1) {
        //    CarrotButtonDown();
        //}
        m_MiningButton.value += 0.3f * Time.deltaTime;
        //if (m_MiningButton.value == 1) {
        //    MiningButtonDown();
        //}
        m_ResearchButton.value += 0.5f * Time.deltaTime;

        // bunnies eat carrots
        m_CarrotCount -= Time.deltaTime * m_Population;

        // figure out reproduction factor
        float reproductionFactor = Mathf.Clamp01(m_CarrotCount / m_Population / 10.0f);
        reproductionFactor = 0.5f + 1.5f * reproductionFactor;
        m_Population *= Mathf.Pow(reproductionFactor, Time.deltaTime);
        m_Population = Mathf.Clamp(m_Population, 2.0f, 1e10f);

        // update graphs
        m_PopulationGraph.AddData(m_GameAge / kMaxAge, m_Population);
        m_CarrotGraph.AddData(m_GameAge / kMaxAge, m_CarrotCount);
        m_ProductivityGraph.AddData(m_GameAge / kMaxAge, m_Productivity);
        m_CivilizationGraph.AddData(m_GameAge / kMaxAge, m_Civilization);

        // update status messages
        m_PopulationText.text = "Bunny Population: " + m_Population.ToString("n0");
        m_CountdownText.text = "Space Cats will arrive in: " + (kMaxAge - m_GameAge).ToString("n0") + " years";
    }

    void InitGame() {
        m_SunRotation = 0;
        m_PlanetRotation = 0;

        m_GameAge = 0;
        m_Population = 10.0f;

        m_Population = 10;
        m_PopulationGraph.Init(m_Population, 1e10f, true);

        m_CarrotCount = 100;
        m_CarrotGraph.Init(m_CarrotCount, 1e10f, true);

        m_Productivity = 20;
        m_ProductivityGraph.Init(m_Productivity, 1e10f, true);

        m_Civilization = 1;
        m_CivilizationGraph.Init(m_Civilization, 1e10f, true);

        m_CarrotButton.m_AutoPress = true;
        m_MiningButton.m_AutoPress = true;
        m_ResearchButton.m_AutoPress = true;
    }

    void CarrotButtonDown() {
        if (m_CarrotButton.value == 1) {
            m_CarrotButton.value = 0;
            m_CarrotCount += m_Productivity;
            m_CarrotCount = Mathf.Clamp(m_CarrotCount, 1, 3e10f);
        }
    }

    void MiningButtonDown() {
        if (m_MiningButton.value == 1) {
            m_MiningButton.value = 0;
            m_Productivity += 100 * Mathf.Pow(10.0f, Mathf.Log10(m_Civilization));
            m_Productivity = Mathf.Clamp(m_Productivity, 100, 1e10f);
        }
    }

    void ResearchButtonDown() {
        if (m_ResearchButton.value == 1) {
            m_ResearchButton.value = 0;
            m_Civilization *= 10;
            m_Civilization = Mathf.Clamp(m_Civilization, 1, 1e10f);
        }
    }
}
