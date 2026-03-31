#if ENABLE_INPUT_SYSTEM

using UnityEngine;
using UnityEngine.InputSystem;

namespace VRPenNamespace
{
    public partial class VRPenInputActions : MonoBehaviour, IVrPenInput
    {
        [SerializeField] private InputActionReference _menuToggle;
        [SerializeField] private InputActionReference _menuUpDown;
        [SerializeField] private InputActionReference _menuEnter;


        public bool MenuToggle
        {
            get
            {
                if (_menuToggle == null) return false;
                return _menuToggle.action.WasPressedThisFrame();
            }
        }

        public bool Up => Snap(ref _oldUp)==1;
        public bool Down => Snap(ref _oldDown)==-1;

        private int _oldUp;
        private int _oldDown;

        private int Snap(ref int oldValue)
        {
            var value = _menuUpDown.action.ReadValue<Vector2>().y;

            int newValue                          = 0;
            if (Mathf.Abs(value) < 0.2f) newValue = 0;
            if (value > 0.8f) newValue            = 1;
            if (value < -0.8f) newValue           = -1;

            if (oldValue != newValue)
            {
                oldValue = newValue;
                return newValue;
            }
            
            return 0;
        }
       
        public bool Enter
        {
            get
            {
                if (_menuEnter == null) return false;
                return _menuEnter.action.WasPressedThisFrame();
            }
        }
    }
}

#endif