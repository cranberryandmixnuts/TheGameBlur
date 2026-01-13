using UnityEngine;

[DisallowMultipleComponent]
public class LowFpsAnimator : MonoBehaviour {

	//======================================================================| Fields

	[SerializeField]
	private Transform _providerRoot;
	
	[SerializeField]
	private Transform _mainRoot;

	[SerializeField]
	private int _targetFps;

	private float _accumulated = 0f;

	//======================================================================| Unity Methods

	private void LateUpdate() {
		
		if (_targetFps <= 0) return;

		_accumulated += Time.deltaTime;
		float interval = 1f / _targetFps;

		if (_accumulated < interval) return;
		_accumulated -= interval;

		CopyRecursive(_providerRoot, _mainRoot);

	}

	//======================================================================| Methods

	private void CopyRecursive(Transform provider, Transform main) {

		main.SetLocalPositionAndRotation(provider.localPosition, provider.localRotation);
		main.localScale = provider.localScale;

		int childCount = Mathf.Min(provider.childCount, main.childCount);
		for (int i = 0; i < childCount; i++) {
			CopyRecursive(provider.GetChild(i), main.GetChild(i));
		}

	}

}