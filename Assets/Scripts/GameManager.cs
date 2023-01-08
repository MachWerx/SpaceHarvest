using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private Transform m_Sun;
    [SerializeField] private Transform m_Planet;
    [SerializeField] private TMPro.TextMeshPro m_PopulationText;
    [SerializeField] private TMPro.TextMeshPro m_CountdownText;

    [SerializeField] private GameObject m_GameUiGroup;

    [SerializeField] private TimeGraph m_PopulationGraph;
    [SerializeField] private TimeGraph m_CarrotGraph;
    [SerializeField] private TimeGraph m_ProductivityGraph;
    [SerializeField] private TimeGraph m_CivilizationGraph;

    [SerializeField] private TMPro.TextMeshPro m_PopulationLegend;
    [SerializeField] private TMPro.TextMeshPro m_CarrotLegend;
    [SerializeField] private TMPro.TextMeshPro m_ProductivityLegend;
    [SerializeField] private TMPro.TextMeshPro m_CivilizationLegend;

    [SerializeField] private TMPro.TextMeshPro m_CarrotButtonLabel;
    [SerializeField] private TMPro.TextMeshPro m_MiningButtonLabel;

    [SerializeField] private UiButton m_CarrotButton;
    [SerializeField] private UiButton m_MiningButton;
    [SerializeField] private UiButton m_ResearchButton;

    [SerializeField] private UiSlider m_CarrotSlider;
    [SerializeField] private UiSlider m_MiningSlider;
    [SerializeField] private UiSlider m_ResearchSlider;

    [SerializeField] private GameObject m_MenuGroup;
    [SerializeField] private UiButton m_StartButton;
    [SerializeField] private UiButton m_UpgradeButton;
    [SerializeField] private UiButton m_ResetButton;
    [SerializeField] private TMPro.TextMeshPro m_UpgradeTitleText;
    [SerializeField] private TMPro.TextMeshPro m_UpgradeDescriptionText;

    [SerializeField] private GameObject m_HarvestGroup;
    [SerializeField] private TMPro.TextMeshPro m_HarvestDescriptionText;
    [SerializeField] private UiButton m_HarvestButton;

    [SerializeField] private GameObject m_BunnyWinGroup;
    [SerializeField] private UiButton m_BunnyWinButton;

    [SerializeField] private AudioSource m_GemsAudio;
    [SerializeField] private AudioSource m_ChompAudio;

    private float m_Gems = 0;
    const float kGemCost1 = 200;
    const float kGemCost2 = 400;
    const float kGemCost3 = 600;

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
        m_Gems = 0;
        //m_Gems = 20;
        m_UiLayer = LayerMask.GetMask("UI");

        m_MenuGroup.SetActive(true);
        m_ResetButton.transform.parent.parent.parent.gameObject.SetActive(false);

        m_GameUiGroup.SetActive(false);

        m_StartButton.OnTapDown += StartButtonDown;
        m_UpgradeButton.OnTapDown += UpgradeButtonDown;
        m_ResetButton.OnTapDown += ResetButtonDown;

        m_CarrotButton.OnTapDown += CarrotButtonDown;
        m_MiningButton.OnTapDown += MiningButtonDown;
        m_ResearchButton.OnTapDown += ResearchButtonDown;
        m_CarrotSlider.OnTapDragged += CarrotSliderDragged;
        m_MiningSlider.OnTapDragged += MiningSliderDragged;
        m_ResearchSlider.OnTapDragged += ResearchSliderDragged;

        m_HarvestButton.OnTapDown += HarvestButtonDown;

        m_HarvestGroup.SetActive(false);
        m_BunnyWinGroup.SetActive(false);
        m_BunnyWinButton.OnTapDown += BunnyWinButtonDown;

        InitGame();
    }

    // Update is called once per frame
    void Update() {
        float gameDeltaTime = 1.5f * Time.deltaTime;

        // rotate sun and planet
        m_SunRotation += gameDeltaTime * kSunRotationSpeed;
        m_Sun.localRotation = Quaternion.Euler(0, m_SunRotation, 0);
        m_PlanetRotation += gameDeltaTime * kPlanetRotationSpeed;
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
            // increment age
            m_GameAge += gameDeltaTime * kYearsPerSecond;
            if (m_GameAge > kMaxAge) {
                m_GameAge = kMaxAge;
            }

            // advance buttons
            m_CarrotButton.value += m_CarrotSlider.value * gameDeltaTime;
            if (m_CarrotButton.value >= 1) {
                m_CarrotButtonLabel.text = "Harvest!";
            } else {
                m_CarrotButtonLabel.text = "Carrots";
            }
            float miningFactor = m_Population / m_Productivity / 2.0f;
            m_MiningButton.value += m_MiningSlider.value * miningFactor * gameDeltaTime;
            if (m_MiningButton.value >= 1) {
                m_MiningButtonLabel.text = "Harvest!";
            } else {
                m_MiningButtonLabel.text = "Mining";
            }
            float researchFactor = m_Population / m_Civilization / 0.5f;
            m_ResearchButton.value += m_ResearchSlider.value * researchFactor * gameDeltaTime;

            // bunnies eat carrots
            m_CarrotCount -= gameDeltaTime * m_Population;

            // figure out reproduction factor
            float reproductionFactor = Mathf.Clamp01(m_CarrotCount / m_Population / 10.0f);
            reproductionFactor = 0.5f + 1.5f * reproductionFactor;
            m_Population *= Mathf.Pow(reproductionFactor, gameDeltaTime);
            m_Population = Mathf.Clamp(m_Population, 2.0f, 1e10f);

            // update graphs
            m_PopulationGraph.AddData(m_GameAge / kMaxAge, m_Population);
            m_CarrotGraph.AddData(m_GameAge / kMaxAge, m_CarrotCount);
            m_ProductivityGraph.AddData(m_GameAge / kMaxAge, m_Productivity);
            m_CivilizationGraph.AddData(m_GameAge / kMaxAge, m_Civilization);

            // update status messages
            m_PopulationText.text = "Bunny Population: " + m_Population.ToString("n0");
            m_CountdownText.text = "Space Cats will arrive in: " + (kMaxAge - m_GameAge).ToString("n0") + " years";

            if (m_Civilization >= 5e9) {
                m_BunnyWinGroup.SetActive(true);
                m_GameMode = GameMode.GameOver;
            } else if (m_GameAge >= kMaxAge) {
                float reward = 100.0f * Mathf.Log10(m_Population);
                m_HarvestGroup.SetActive(true);
                m_HarvestDescriptionText.text =
                    "The space cats have arrived\n" +
                    "and look hungrily upon the fruits\n" +
                    "of your labor.They hand you\n" +
                    (int)reward + " gems as a reward.";
                //print("Gems = " + m_Gems + " + " + reward);
                m_Gems += reward;
                m_GemsAudio.Play();
                m_GameMode = GameMode.GameOver;
            }
        }
    }

    void InitGame() {
        UpdateUpgradeButton();

        m_ProductivityGraph.gameObject.SetActive(false);
        m_CivilizationGraph.gameObject.SetActive(false);

        m_ProductivityLegend.gameObject.SetActive(false);
        m_CivilizationLegend.gameObject.SetActive(false);

        m_CarrotSlider.value = 1.0f;
        m_MiningSlider.value = 0.0f;
        m_ResearchSlider.value = 0.0f;

        m_CarrotButton.m_AutoPress = false;
        //m_CarrotButton.m_AutoPress = true;
        m_CarrotSlider.transform.parent.gameObject.SetActive(false);
        m_MiningButton.transform.parent.parent.parent.gameObject.SetActive(false);
        m_MiningButton.m_AutoPress = false;
        //m_MiningButton.m_AutoPress = true;
        m_MiningSlider.transform.parent.gameObject.SetActive(false);
        m_ResearchButton.transform.parent.parent.parent.gameObject.SetActive(false);
        m_ResearchSlider.transform.parent.gameObject.SetActive(false);

        if (m_UpgradeLevel == UpgradeLevel.NormalBunnies) {
            // default UI
        } else if (m_UpgradeLevel == UpgradeLevel.SmarterBunnies) {
            // add mining button
            m_CarrotSlider.value = 0.5f;
            m_MiningSlider.value = 0.5f;

            m_ProductivityGraph.gameObject.SetActive(true);
            m_ProductivityLegend.gameObject.SetActive(true);
            m_MiningButton.transform.parent.parent.parent.gameObject.SetActive(true);
        } else if (m_UpgradeLevel == UpgradeLevel.BunnyAutonomy) {
            // add sliders
            m_CarrotSlider.value = 0.5f;
            m_MiningSlider.value = 0.5f;

            m_ProductivityGraph.gameObject.SetActive(true);
            m_ProductivityLegend.gameObject.SetActive(true);
            m_CarrotButton.m_AutoPress = true;
            m_CarrotSlider.transform.parent.gameObject.SetActive(true);
            m_MiningButton.transform.parent.parent.parent.gameObject.SetActive(true);
            m_MiningButton.m_AutoPress = true;
            m_MiningSlider.transform.parent.gameObject.SetActive(true);
        } else {
            // add research
            m_CarrotSlider.value = 0.5f;
            m_MiningSlider.value = 0.5f;

            m_ProductivityGraph.gameObject.SetActive(true);
            m_ProductivityLegend.gameObject.SetActive(true);
            m_CarrotButton.m_AutoPress = true;
            m_CivilizationGraph.gameObject.SetActive(true);
            m_CivilizationLegend.gameObject.SetActive(true);
            m_CarrotSlider.transform.parent.gameObject.SetActive(true);
            m_MiningButton.transform.parent.parent.parent.gameObject.SetActive(true);
            m_MiningButton.m_AutoPress = true;
            m_MiningSlider.transform.parent.gameObject.SetActive(true);
            m_ResearchButton.transform.parent.parent.parent.gameObject.SetActive(true);
            m_ResearchButton.m_AutoPress = true;
            m_ResearchSlider.transform.parent.gameObject.SetActive(true);
        }


        m_SunRotation = 0;
        m_PlanetRotation = 0;

        m_GameAge = 0;
        m_Population = 10.0f;

        m_Population = 2;
        m_PopulationGraph.Init(m_Population, 1e10f, true);

        m_CarrotButton.value = 0;
        m_CarrotCount = 5;
        m_CarrotGraph.Init(m_CarrotCount, 1e10f, true);

        m_MiningButton.value = 0;
        m_Productivity = 20;
        m_ProductivityGraph.Init(m_Productivity, 1e10f, true);

        m_ResearchButton.value = 0;
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
        m_UpgradeButton.transform.parent.parent.parent.gameObject.SetActive(true);
        if (m_UpgradeLevel == UpgradeLevel.NormalBunnies) {
            m_UpgradeTitleText.text = "Upgrade Bunnies";
            m_UpgradeDescriptionText.text =
                "Bunnies can use tools which\n" +
                "increase productivity(" + (int)m_Gems + "/" + kGemCost1 + " gems)";
            m_UpgradeTitleText.color = m_UpgradeDescriptionText.color =
                m_Gems > kGemCost1 ? Color.white : 0.4f * Color.white;
        } else if (m_UpgradeLevel == UpgradeLevel.SmarterBunnies) {
            m_UpgradeTitleText.text = "Bunny Autonomy";
            m_UpgradeDescriptionText.text =
                "Bunnies will forage and mine\n" +
                "automatically (" + (int)m_Gems + "/" + kGemCost2 + " gems)";
            m_UpgradeTitleText.color = m_UpgradeDescriptionText.color =
                m_Gems > kGemCost2 ? Color.white : 0.4f * Color.white;
        } else if (m_UpgradeLevel == UpgradeLevel.BunnyAutonomy) {
            m_UpgradeTitleText.text = "Sentient Bunnies";
            m_UpgradeDescriptionText.text =
                "Bunnies can do research and\n" +
                "form a civilization (" + (int)m_Gems + "/" + kGemCost3 + " gems)";
            m_UpgradeTitleText.color = m_UpgradeDescriptionText.color =
                m_Gems > kGemCost3 ? Color.white : 0.4f * Color.white;
        } else if (m_UpgradeLevel == UpgradeLevel.SentientBunnies) {
            m_UpgradeButton.transform.parent.parent.parent.gameObject.SetActive(false);
        }
    }

    void ResetButtonDown() {
        m_Gems = 0;
        m_UpgradeLevel = UpgradeLevel.NormalBunnies;
        m_ResetButton.transform.parent.parent.parent.gameObject.SetActive(false);
        UpdateUpgradeButton();
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
            m_Productivity *= 5;
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
        m_ChompAudio.Play();
    }

    void BunnyWinButtonDown() {
        m_BunnyWinGroup.SetActive(false);
        m_MenuGroup.SetActive(true);
        m_ResetButton.transform.parent.parent.parent.gameObject.SetActive(true);
        m_GameUiGroup.SetActive(false);
        UpdateUpgradeButton();
        m_GameMode = GameMode.MainMenu;
    }
}
