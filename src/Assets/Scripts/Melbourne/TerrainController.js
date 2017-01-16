#pragma strict

public var prefav : GameObject;
public var size : float;

private var player : GameObject;
private var terrain1 : GameObject;
private var terrain2 : GameObject;

function Start () {
	var harf = size / 2.0;
	player = GameObject.FindGameObjectWithTag("Player");
	terrain1 = Instantiate(prefav) as GameObject;
	terrain1.transform.position = Vector3(-harf, 0.0, -harf);
	terrain2 = Instantiate(prefav) as GameObject;
	terrain2.transform.position = Vector3(harf, 0.0, -harf);
}

function Update () {
	var harf = size / 2.0;
	var xp = player.transform.position.x;
	var x1 = terrain1.transform.position.x;
	var x2 = terrain2.transform.position.x;
	if (x1 < x2) {
		if (xp > x2 + harf) {
			terrain1.transform.position.x = x2 + size;
		}
	} else {
		if (xp > x1 + harf) {
			terrain2.transform.position.x = x1 + size;
		}
	}
}
