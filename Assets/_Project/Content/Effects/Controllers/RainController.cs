using UnityEngine;

public class RainController : MonoBehaviour {

	//======================================================================| Fields

	[SerializeField]
	private Transform _followTarget;

	[SerializeField]
	private Vector3 _offset;

	//======================================================================| Unity Methods

	private void Update() {
		transform.position = _followTarget.position + _offset;
	}

}