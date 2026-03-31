#if ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine;

namespace VRPenNamespace
{
    public partial class VRPenLegacyInput : MonoBehaviour, IVrPenInput
    {
        [SerializeField] private string _menuToggle;
        [SerializeField] private string _up;
        [SerializeField] private string _down;
        [SerializeField] private string _enter;
        
        public bool MenuToggle => Input.GetButtonDown(_menuToggle);
        public bool Up         => Input.GetButtonDown(_up);
        public bool Down       => Input.GetButtonDown(_down);
        public bool Enter      => Input.GetButtonDown(_enter);
    }
}
#endif