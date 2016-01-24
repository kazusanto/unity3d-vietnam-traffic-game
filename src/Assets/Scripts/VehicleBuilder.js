#pragma strict

public var prefav : GameObject;
public var interval : float = 1;
private var time : float = 0;
private var target : GameObject;
private var num : int = 0;

function Start () {
	target = GameObject.FindGameObjectWithTag("Player");
}

function Update () {
	time += Time.deltaTime;
	if (time >= interval) {
		time = 0;
		var vehicle : GameObject = Instantiate(prefav) as GameObject;
		var x = target.transform.position.x;
		var z = target.transform.position.z;
		vehicle.transform.position = Vector3(
			x + Random.Range(0.0, 10.0),
			0.0,
			z + 50.0 + ((num++ % 10) * 20.0));
	}
}