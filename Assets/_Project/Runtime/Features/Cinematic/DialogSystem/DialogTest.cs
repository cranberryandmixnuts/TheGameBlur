using UnityEngine;

public class DialogTest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CinematicManager.Show<CinematicDialog>().BindDialog("TestDialog1");
    }
}
