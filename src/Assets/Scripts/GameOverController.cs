using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameOverController : MonoBehaviour {

    public string SceneName;

    private Text text;
    private bool released = false;

    // Use this for initialization
	void Start () {
        text = GetComponent<Text>();
        text.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
        if (PlayerController.Gameover) {
            text.enabled = true;
            var restart = false;
            if (Input.GetMouseButtonDown(0)) {
                restart = true;
            }
            if (Input.GetKey(KeyCode.Space)) {
                if (released) {
                    restart = true;
                }
            } else {
                released = true;
            }
            if (restart) {
                UnityEngine.SceneManagement.SceneManager.LoadScene(SceneName);
            }
        } else {
            text.enabled = false;
        }
	}
}
