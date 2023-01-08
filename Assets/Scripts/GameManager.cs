using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private Transform m_Sun;
    [SerializeField] private Transform m_Planet;
    [SerializeField] private TMPro.TextMeshPro m_PopulationText;
    [SerializeField] private TMPro.TextMeshPro m_CountdownText;

    [SerializeField] private GameObject m_GameUiGroup;

    [SerializeField] private UiButton m_CarrotButton;
    [SerializeField] private UiButton m_MiningButton;
    [SerializeField] private UiButton m_ResearchButton;

    [SerializeField] private UiSlider m_CarrotSlider;
    [SerializeField] private UiSlider m_MiningSlider;
    [SerializeField] private UiSlider m_ResearchSlider;

    [SerializeField] private TimeGraph m_PopulationGraph;
    [SerializeField] private TimeGraph m_CarrotGraph;
    [SerializeField] private TimeGraph m_ProductivityGraph;
    [SerializeField] private TimeGraph m_CivilizationGraph;

    [SerializeField] private GameObject m_MenuGroup;
    [SerializeField] private UiButton m_StartButton;
    [SerializeField] private UiButton m_UpgradeButton;
    [SerializeField] private TMPro.TextMeshPro m_UpgradeTitleText;
    [SerializeField] private TMPro.TextMeshPro m_UpgradeDescriptionText;

    [SerializeField] private GameObject m_HarvestGroup;
    [SerializeField] private UiButton m_HarvestButton;

    private float m_Gems = 0;
    const float kGemCost1 = 2;
    const float kGemCost2 = 5;
    const float kGemCost3 = 10;

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
    const float kMaxAge = 1000.0f;
    const float kYearsPerSecond = 200.0f;

    enum GameMode {
        MainMenu,
        PlayingGame,
        GameOver,
    }
    private GameMode m_GameMode = GameMode.MainMenu;

    enum UpgradeLevel {
        NormalBunnies,
        SmarterBunnies,
        BunnyAutonomy,
        SentientBunnies
    }
    private UpgradeLevel m_UpgradeLevel = UpgradeLevel.NormalBunnies;

    // Start is called before the first frame update
    void Start() {
        m_UiLayer = LayerMask.GetMask("UI");

        m_MenuGroup.SetActive(true);

        m_GameUiGroup.SetActive(false);

        m_StartButton.OnTapDown += StartButtonDown;
        m_UpgradeButton.OnTapDown += UpgradeButtonDown;

        m_CarrotButton.OnTapDown += CarrotButtonDown;
        m_MiningButton.OnTapDown += MiningButtonDown;
        m_ResearchButton.OnTapDown += ResearchButtonDown;
        m_CarrotSlider.OnTapDragged += CarrotSliderDragged;
        m_MiningSlider.OnTapDragged += MiningSliderDragged;
        m_ResearchSlider.OnTapDragged += ResearchSliderDragged;

        m_HarvestButton.OnTapDown += HarvestButtonDown;

        m_HarvestGroup.SetActive(false);

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

        if (m_GameMode == GameMode.MainMenu) {

        } else if (m_GameMode == GameMode.PlayingGame) {
            // do simulation
            m_GameAge += Time.deltaTime * kYearsPerSecond;
            m_CarrotButton.value += Time.deltaTime;
            m_MiningButton.value += 0.3f * Time.deltaTime;
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

            if (m_GameAge >= kMaxAge) {
                m_HarvestGroup.SetActive(true);
                print("Gems = " + m_Gems + " + " + Mathf.Log10(m_Population));
                m_Gems += Mathf.Log10(m_Population);
                m_GameMode = GameMode.GameOver;
            }
        }
    }

    void InitGame() {
        UpdateUpgradeButton();

        m_SunRotation = 0;
        m_PlanetRotation = 0;

        m_GameAge = 0;
        m_Population = 10.0f;

        m_Population = 2;
        m_PopulationGraph.Init(m_Population, 1e10f, true);

        m_CarrotButton.m_AutoPress = true;
        m_CarrotSlider.value = 0.5f;
        m_CarrotCount = 100;
        m_CarrotGraph.Init(m_CarrotCount, 1e10f, true);

        m_MiningButton.m_AutoPress = true;
        m_MiningSlider.value = 0.5f;
        m_Productivity = 20;
        m_ProductivityGraph.Init(m_Productivity, 1e10f, true);

        m_ResearchButton.m_AutoPress = true;
        m_ResearchSlider.value = 0.0f;
        m_Civilization = 1;
        m_CivilizationGraph.Init(m_Civilization, 1e10f, true);
    }

    void StartButtonDown() {
        m_MenuGroup.SetActive(false);
        m_GameUiGroup.SetActive(true);
        InitGame();
        m_GameMode = GameMode.PlayingGame;
    }

    void UpgradeButtonDown() {
        if (m_UpgradeLevel == UpgradeLevel.NormalBunnies) {
            if (m_Gems > kGemCost1) {
                m_Gems -= kGemCost1;
                m_UpgradeLevel++;
            }
        } else if (m_UpgradeLevel == UpgradeLevel.SmarterBunnies) {
            if (m_Gems > kGemCost2) {
                m_Gems -= kGemCost2;
                m_UpgradeLevel++;
            }
        } else if (m_UpgradeLevel == UpgradeLevel.BunnyAutonomy) {
            if (m_Gems > kGemCost3) {
                m_Gems -= kGemCost3;
                m_UpgradeLevel++;
            }
        }
        UpdateUpgradeButton();
    }

    void UpdateUpgradeButton() {
        if (m_UpgradeLevel == UpgradeLevel.NormalBunnies) {
            m_UpgradeTitleText.text = "Smarter Bunnies";
            m_UpgradeDescriptionText.text =
                "Bunnies can use tools which\n" +
                "increase productivity(" + (int)m_Gems + "/" + kGemCost1 + " gems)";
            m_UpgradeTitleText.color = m_UpgradeDescriptionText.color =
                m_Gems > kGemCost1 ? Color.white : Color.black;
        } else if (m_UpgradeLevel == UpgradeLevel.SmarterBunnies) {
            m_UpgradeTitleText.text = "Bunny Autonomy";
            m_UpgradeDescriptionText.text =
                "Bunnies will forage and mine\n" +
                "automatically (" + (int)m_Gems + "/" + kGemCost2 + " gems)";
            m_UpgradeTitleText.color = m_UpgradeDescriptionText.color =
                m_Gems > kGemCost2 ? Color.white : Color.black;
        } else if (m_UpgradeLevel == UpgradeLevel.BunnyAutonomy) {
            m_UpgradeTitleText.text = "Sentient Bunnies";
            m_UpgradeDescriptionText.text =
                "Bunnies can do research and\n" +
                "form a civilization (" + (int)m_Gems + "/" + kGemCost3 + " gems)";
            m_UpgradeTitleText.color = m_UpgradeDescriptionText.color =
                m_Gems > kGemCost3 ? Color.white : Color.black;
        } else if (m_UpgradeLevel == UpgradeLevel.SentientBunnies) {
            m_UpgradeTitleText.text = "";
            m_UpgradeDescriptionText.text = "";
        }
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

    void CarrotSliderDragged() {
        if (m_ResearchSlider.gameObject.activeInHierarchy) {
            float researchProportion;
            if (m_ResearchSlider.value == 0 && m_MiningSlider.value == 0) {
                researchProportion = 0.5f;
            } else if (m_MiningSlider.value == 0) {
                researchProportion = 1.0f;
            } else {
                researchProportion = m_ResearchSlider.value / (m_ResearchSlider.value + m_MiningSlider.value);
            }

            m_ResearchSlider.value = (1.0f - m_CarrotSlider.value) * researchProportion;
            m_MiningSlider.value = (1.0f - m_CarrotSlider.value) * (1.0f - researchProportion);
        } else {
            m_MiningSlider.value = 1.0f - m_CarrotSlider.value;
        }
    }

    void MiningSliderDragged() {
        if (m_ResearchSlider.gameObject.activeInHierarchy) {
            float researchProportion;
            if (m_ResearchSlider.value == 0 && m_CarrotSlider.value == 0) {
                researchProportion = 0.5f;
            } else if (m_CarrotSlider.value == 0) {
                researchProportion = 1.0f;
            } else {
                researchProportion = m_ResearchSlider.value / (m_ResearchSlider.value + m_CarrotSlider.value);
            }

            m_ResearchSlider.value = (1.0f - m_MiningSlider.value) * researchProportion;
            m_CarrotSlider.value = (1.0f - m_MiningSlider.value) * (1.0f - researchProportion);
        } else {
            m_CarrotSlider.value = 1.0f - m_MiningSlider.value;
        }
    }

    void ResearchSliderDragged() {
        float carrotProportion;
        if (m_CarrotSlider.value == 0 && m_MiningSlider.value == 0) {
            carrotProportion = 0.5f;
        } else if (m_MiningSlider.value == 0) {
            carrotProportion = 1.0f;
        } else {
            carrotProportion = m_CarrotSlider.value / (m_CarrotSlider.value + m_MiningSlider.value);
        }

        m_CarrotSlider.value = (1.0f - m_ResearchSlider.value) * carrotProportion;
        m_MiningSlider.value = (1.0f - m_ResearchSlider.value) * (1.0f - carrotProportion);
    }

    void HarvestButtonDown() {
        m_HarvestGroup.SetActive(false);
        m_MenuGroup.SetActive(true);
        m_GameUiGroup.SetActive(false);
        UpdateUpgradeButton();
        m_GameMode = GameMode.MainMenu;
    }
}
