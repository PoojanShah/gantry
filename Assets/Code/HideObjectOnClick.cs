using UnityEngine;

//To be removed
public class HideObjectOnClick : MonoBehaviour
{
	[SerializeField] private GameObject _objectToHide;

	public void HideObject() => _objectToHide.SetActive(false);
}
