#pragma strict

public var speed : float = 1.0;
private var chasing : boolean = true;
private var target : GameObject;
private var vehicle : UnityStandardAssets.Vehicles.Car.CarController = null;
private var velocity : Vector3 = Vector3.zero;

function Start () {
	target = GameObject.FindGameObjectWithTag("Player");
	vehicle = GetComponent.<UnityStandardAssets.Vehicles.Car.CarController>();
}

function Update () {
	var mypos2d = Vector2(transform.position.x, transform.position.z);
	var mydir2d = Vector2(transform.forward.x, transform.forward.z);
	var targetpos2d = Vector2(target.transform.position.x, target.transform.position.z);
	var targetdir2d = targetpos2d - mypos2d;
	var roaddir2d = Vector2(0.0, -1.0);
	var steering = 0.0;
	var targetangle = Vector2.Angle(mydir2d, targetdir2d);
	var roadangle = Vector2.Angle(mydir2d, roaddir2d);
	if (targetangle < 5.0) {
		var targetcross = mydir2d.x * targetdir2d.y - mydir2d.y * targetdir2d.x;
		if (targetcross > 0.0) {
			steering += 0.1;
		} else {
			steering -= 0.1;
		}
	} else if (roadangle > 10.0) {
		var roadcross = mydir2d.x * roaddir2d.y - mydir2d.y * roaddir2d.x;
		if (roadcross > 0.0) {
			steering -= 0.1;
		} else {
			steering += 0.1;
		}
	}
	if (Vector2.Dot(mydir2d, targetdir2d) < 0) {
		chasing = false;
		Destroy(gameObject, 2.0);
	}
	if (vehicle) {
		vehicle.Move(steering, speed, 0.0, 0.0);
	}
}
