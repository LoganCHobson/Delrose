using UnityEngine;


public class MainMenuManager : MonoBehaviour
{
    public GameObject mainMenuCanv;
    

    public void Toggle(GameObject _obj)
    {
        _obj.SetActive(!_obj.activeInHierarchy);
        mainMenuCanv.SetActive(!mainMenuCanv.activeInHierarchy);
    }
}
