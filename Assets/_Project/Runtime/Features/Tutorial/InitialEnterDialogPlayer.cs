using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class InitialEnterDialogPlayer : MonoBehaviour
{
    [SerializeField] private DialogData dialog;

    private bool isEntered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isEntered)
            return;

        if(other.gameObject.TryGetComponent<Player>(out var _))
        {
            isEntered = true;
            CinematicManager.Show<CinematicDialog>().BindDialog(dialog);
        }
    }
}
