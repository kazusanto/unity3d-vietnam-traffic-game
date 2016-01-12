#pragma strict

private var target : GameObject;

function Start () {
	target = GameObject.FindGameObjectWithTag("Player");
}

function Update () {
	var x = target.transform.position.x + 1.0;
	var z = target.transform.position.z - 4.0;
	transform.position.x = x;
	transform.position.z = z;
}