using UnityEngine;

public class MainMenuController : MonoBehaviour
{


    public void ExitApp()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit(0);
#endif
    }
}
