using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameOverController : MonoBehaviour {

    [SerializeField] private string m_SceneName;
    [SerializeField] private AudioClip m_Ending;

    private Text m_text;
    private bool m_released = false;
    private float m_wait_for;

    // Use this for initialization
	void Start () {
        m_text = GetComponent<Text>();
        m_text.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (PlayerController.GameOver) {
            if (!m_text.enabled) {
                m_text.enabled = true;
                m_wait_for = Time.time + 3.0f;
                GetComponent<AudioSource>().PlayOneShot(m_Ending);
            } else if (Time.time > m_wait_for) {
                var restart = false;
                if (Input.GetMouseButtonDown(0)) {
                    restart = true;
                }
                if (Input.GetKey(KeyCode.Space)) {
                    if (m_released) {
                        restart = true;
                    }
                } else {
                    m_released = true;
                }
                if (restart) {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(m_SceneName);
                }
            }
        } else {
            m_text.enabled = false;
        }
	}
}
