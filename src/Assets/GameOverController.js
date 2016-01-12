#pragma strict

import UnityEngine.UI;

public var sceneName : String;

private var player : PlayerControll;
private var text : Text;
private var released : boolean = false;

function Start () {
	player = GameObject.FindGameObjectWithTag("Player").GetComponent.<PlayerControll>();
	text = GetComponent.<Text>();
	text.enabled = false;
}

function Update () {
	if (player.gameover) {
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
			UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
		}
	} else {
		text.enabled = false;
	}
}