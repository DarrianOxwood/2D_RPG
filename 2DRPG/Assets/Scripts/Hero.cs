using Scripts.Components;
using Scripts.Model;
using Scripts.Utils;
using UnityEngine;

namespace Scripts
{
    public class Hero : MonoBehaviour
    {

        [SerializeField] private float _speed;
        [SerializeField] private float _jumpSpeed;
        [SerializeField] private float _damageJumpSpeed;
        [SerializeField] private float _slamDownVelocity;
        [SerializeField] private int _damage;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _interactionRadius;
        [SerializeField] private LayerMask _interactionLayer;

        [SerializeField] private LayerCheck _groundCheck;

        [SerializeField] private RuntimeAnimatorController _armed;
        [SerializeField] private RuntimeAnimatorController _disarmed;


        [SerializeField] private CheckCircleOverlap _attackRange;

        [Space] [Header("Particles")]
        [SerializeField] private SpawnComponent _footStepParticles;
        [SerializeField] private SpawnComponent _jumpParticles;
        [SerializeField] private SpawnComponent _slamDownParticles;
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
        private static readonly int AttackKey = Animator.StringToHash("attack");

        private GameSession _session;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _animator = GetComponent<Animator>();
        }

        public void OnHealthChanged(int currentHealth)
        {
            _session.Data.Hp = currentHealth;
        }
        private void Start()
        {
            _session = FindObjectOfType<GameSession>();
            var health = GetComponent<HealthComponent>();
            health.SetHealth(_session.Data.Hp);
            UpdateHeroWeapon();
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
                _jumpParticles.Spawn();
            }
            else if(_allowDoubleJump)
            {
                yVelocity = _jumpSpeed;
                _jumpParticles.Spawn();
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

            if(_session.Data.Coins > 0)
            SpawnCoins();
        }

        public void AddCoins(int coins)
        {
            _session.Data.Coins += coins;
            Debug.Log($"{coins} coins added. total coins: {_session.Data.Coins}");
        }
        private void SpawnCoins()
        {
            var numCoinsDispose = Mathf.Min(_session.Data.Coins, 5);
            _session.Data.Coins -= numCoinsDispose;
            Debug.Log($"total coins: {_session.Data.Coins}");

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
        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.IsInLayer(_groundLayer))
            {
                var contact = other.contacts[0];
                if(contact.relativeVelocity.y >= _slamDownVelocity)
                {
                    _slamDownParticles.Spawn();
                }
            }
        }
        public void SpawnFootDust()
        {
            _footStepParticles.Spawn();
        }

        public void Attack()
        {
            if (!_session.Data.IsArmed) return;
            _animator.SetTrigger(AttackKey);
        }
        private void OnDoAttak()
        {
            var gos = _attackRange.GetObjectInRange();
            foreach (var go in gos)
            {
                var hp = go.GetComponent<HealthComponent>();
                if (hp != null && go.CompareTag("Enemy"))
                {
                    hp.ModifyHealth(-_damage);
                }
            }
        }

        public void ArmHero()
        {
            _session.Data.IsArmed = true;
            UpdateHeroWeapon();
            _animator.runtimeAnimatorController = _armed;
        }

        private void UpdateHeroWeapon()
        {
            _animator.runtimeAnimatorController = _session.Data.IsArmed ? _armed : _disarmed;
        }
    }

}