#pragma strict

import UnityEngine.UI;

private var player : PlayerControll;
private var text : Text;

function Start () {
	player = GameObject.FindGameObjectWithTag("Player").GetComponent.<PlayerControll>();
	text = GetComponent.<Text>();
}

function Update () {
	text.text = player.score.ToString();
}