using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Input
{
    [CreateAssetMenu(fileName = "InputReader", menuName = "Input/Input Reader")]
    public class InputReader : ScriptableObject, Controls.IPlayerActions
    {
        public event Action<Vector2> MoveEvent = delegate { };
        public event Action<bool> PrimaryFireEvent = delegate { };
        
        private Controls _controls;

        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Player.SetCallbacks(this);
            }
            _controls.Player.Enable();
        }

        public void OnMove(InputAction.CallbackContext context) => MoveEvent.Invoke(context.ReadValue<Vector2>());
        public void OnPrimaryFire(InputAction.CallbackContext context) => PrimaryFireEvent.Invoke(context.performed);
    }
}
