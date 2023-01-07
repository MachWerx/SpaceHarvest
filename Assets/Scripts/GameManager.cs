using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    [SerializeField] private Transform m_Sun;
    [SerializeField] private Transform m_Planet;
    [SerializeField] private TMPro.TextMeshPro m_PopulationText;
    [SerializeField] private TMPro.TextMeshPro m_CountdownText;

    private float m_Population;
    private float m_Countdown;

    private float m_SunRotation;
    private float m_PlanetRotation;

    const float kSunRotationSpeed = 36.0f;
    const float kPlanetRotationSpeed = 72.0f;

    // Start is called before the first frame update
    void Start() {
        InitGame();
    }

    // Update is called once per frame
    void Update() {
        m_SunRotation += Time.deltaTime * kSunRotationSpeed;
        m_Sun.localRotation = Quaternion.Euler(0, m_SunRotation, 0);

        m_PlanetRotation += Time.deltaTime * kPlanetRotationSpeed;
        m_Planet.localRotation = Quaternion.Euler(0, m_PlanetRotation, 0);
    }

    void InitGame() {
        m_SunRotation = 0;
        m_PlanetRotation = 0;

        m_Population = 10.0f;
        m_Countdown = 10000.0f;
    }
}
