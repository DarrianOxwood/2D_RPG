using UnityEngine;
using UnityEngine.InputSystem;

namespace Scripts
{
    public class HeroInputReader : MonoBehaviour
    {
        [SerializeField] private Hero _hero;

        //private HeroInputActions _inputActions;

        //private void Awake()
        //{
        //    _inputActions = new HeroInputActions();

        //    _inputActions.Hero.Movement.performed += OnMovement;
        //    _inputActions.Hero.Movement.canceled += OnMovement;
        //    _inputActions.Hero.Interact.performed +=  OnInteract;
        //    _inputActions.Hero.Interact.canceled += OnInteract;
        //    _inputActions.Hero.SaySomething.performed += OnSaySomething;
        //}
        //private void OnEnable()
        //{
        //    _inputActions.Enable();
        //}

        public void OnMovement(InputAction.CallbackContext context)
        {
            var direction = context.ReadValue<Vector2>();
            _hero.SetDirection(direction);
        }

        public void OnSaySomething(InputAction.CallbackContext context)
        {
            if(context.canceled)
            {
                _hero.SaySomething();
            }
        }
        public void OnInteract(InputAction.CallbackContext context)
        {
            if(context.canceled)
            {
                _hero.Interact();
            }
        }
        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.canceled)
            {
                _hero.Attack();
            }
        }

    }

}


