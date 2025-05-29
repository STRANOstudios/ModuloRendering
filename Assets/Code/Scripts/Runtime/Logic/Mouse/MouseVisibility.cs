using UnityEngine;
using Sirenix.OdinInspector;

namespace Project.Runtime.Logic.Mouse
{
    [HideMonoScript]
    public class MouseVisibility : MonoBehaviour
    {
        private void OnEnable()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void OnDisable()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }
}