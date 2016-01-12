#pragma strict

public static var gameover : boolean = false;
public static var score : int = 0;

private var controller : UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter;
private var hit : boolean = false;

function Start () {
	score = 0;
	gameover = false;
	hit = false;
	controller = GetComponent.<UnityStandardAssets.Characters.ThirdPerson.ThirdPersonCharacter>();
	AudioListener.volume = 10.0;
}

function Update () {
	if (hit) {
		hit = false;
		gameover = true;
		controller.Move(Vector3.zero, false, true);
	} else if (gameover) {
		controller.Move(Vector3(0.05, 0.0, -0.1), false, false);
	}
	if (!gameover) {
		var moving = false;
		if (Input.GetMouseButton(0)) {
			moving = true;
		}
		if (Input.GetKey(KeyCode.Space)) {
			moving = true;
		}
		if (moving) {
			controller.Move(Vector3(1.0, 0.0, 0.0), false, false);
		} else {
			controller.Move(Vector3.zero, false, false);
		}
		score = transform.position.x;
	} else {
	}
}

function OnCollisionEnter(other : Collision) {
	if (other.gameObject.CompareTag("Vehicle")) {
		hit = true;
	}
}
