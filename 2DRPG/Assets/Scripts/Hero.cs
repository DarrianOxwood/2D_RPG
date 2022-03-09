using Scripts.Components;
using UnityEngine;

namespace Scripts
{
    public class Hero : MonoBehaviour
    {

        [SerializeField] private float _speed;
        [SerializeField] private float _jumpSpeed;
        [SerializeField] private float _damageJumpSpeed;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _interactionRadius;
        [SerializeField] private LayerMask _interactionLayer;
        [SerializeField] private int _coins;

        [SerializeField] private LayerCheck _groundCheck;

        [SerializeField] private SpawnComponent _footStepParticles;
        [SerializeField] private ParticleSystem _hitParticles;

        private Collider2D[] _interactionResult = new Collider2D[1];
        private Rigidbody2D _rigidbody;
        private Vector2 _direction;
        private Animator _animator;
        private bool _isGrounded;
        private bool _allowDoubleJump;

        private static readonly int IsGrounKey = Animator.StringToHash("is-ground");
        private static readonly int IsRunningkey = Animator.StringToHash("is-running");
        private static readonly int VetricalVelocityKey = Animator.StringToHash("vertical-velocity");
        private static readonly int HitKey = Animator.StringToHash("hit-trigger");

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
        }

        private void Update()
        {
            _isGrounded = IsGrounded();
        }

        public void SetDirection(Vector2 direction)
        {
            _direction = direction;
        }

        private void FixedUpdate()
        {
            var xvelocity = _direction.x * _speed;
            var yVelocity = CalculateYVelocity();
            _rigidbody.velocity = new Vector2(xvelocity, yVelocity);

            _animator.SetBool(IsRunningkey, _direction.x != 0);
            _animator.SetBool(IsGrounKey, _isGrounded);
            _animator.SetFloat(VetricalVelocityKey, _rigidbody.velocity.y);

            UpdateSpriteDirection();
        }

        private float CalculateYVelocity()
        {
            var yVelocity = _rigidbody.velocity.y;
            var isjumping = _direction.y > 0;

            if (_isGrounded) _allowDoubleJump = true;

            if (isjumping)
            {
                yVelocity = CalculateJumpVelocity(yVelocity);
            }
            else if (_rigidbody.velocity.y > 0)
            {
                yVelocity *= 0.5f;
            }

            return yVelocity;
        }
        private float CalculateJumpVelocity(float yVelocity)
        {
            var isFalling = _rigidbody.velocity.y <= 0.001f;
            if(!isFalling) return yVelocity;

            if(_isGrounded)
            {
                yVelocity += _jumpSpeed;
            }
            else if(_allowDoubleJump)
            {
                yVelocity = _jumpSpeed;
                _allowDoubleJump = false;
            }
            return yVelocity;
        }

        private void UpdateSpriteDirection()
        {
            if (_direction.x > 0)
            {
                transform.localScale = Vector3.one;
            }
            else if (_direction.x < 0)
            {
                transform.localScale = new Vector3(-1, 1, 1);

            }
        }

        private bool IsGrounded()
        {
            return _groundCheck.IsTouchingLayer;
        }

        public void SaySomething()
        {
            Debug.Log("Something!");
        }

        public void TakeDamage()
        {
            _animator.SetTrigger(HitKey);
            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _damageJumpSpeed);

            if(_coins > 0)
            SpawnCoins();
        }

        public void AddCoins(int coins)
        {
            _coins += coins;
            Debug.Log($"{coins} coins added. total coins: {_coins}");
        }
        private void SpawnCoins()
        {
            var numCoinsDispose = Mathf.Min(_coins, 5);
            _coins -= numCoinsDispose;
            Debug.Log($"total coins: {_coins}");

            var burst = _hitParticles.emission.GetBurst(0);
            burst.count = numCoinsDispose;
            _hitParticles.emission.SetBurst(0, burst);

            _hitParticles.gameObject.SetActive(true);
            _hitParticles.Play();
        }

        public void Interact()
        {
            var size = Physics2D.OverlapCircleNonAlloc(
                transform.position, 
                _interactionRadius, 
                _interactionResult,
                _interactionLayer);

            for(int i = 0; i < size; i++)
            {
                var interactable = _interactionResult[i].GetComponent<InteractableComponent>();
                if(interactable != null)
                {
                    interactable.Interact();
                }
            }
        }

        public void SpawnFootDust()
        {
            _footStepParticles.Spawn();
        }
    }

}