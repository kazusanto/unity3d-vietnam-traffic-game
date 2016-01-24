#pragma strict

public var prefab : GameObject;
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
		var vehicle : GameObject = Instantiate(prefab) as GameObject;
		var x = target.transform.position.x;
		var z = target.transform.position.z;
		var rx = Random.Range(0.0, 30.0);
		var vx = x + rx;
		var vz = z + 50.0 - ((num++ % 10) * rx / 10.0);
		vehicle.transform.position = Vector3(vx, 0.0, vz);
	}
}