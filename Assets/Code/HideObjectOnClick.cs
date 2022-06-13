using UnityEngine;

public class HideObjectOnClick : MonoBehaviour
{
    public GameObject HideMe;

    public void HideObject()
    {
        HideMe.SetActive(false);
    }
}
