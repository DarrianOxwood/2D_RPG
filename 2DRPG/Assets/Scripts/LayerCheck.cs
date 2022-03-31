using UnityEngine;

namespace Scripts
{
    public class LayerCheck : MonoBehaviour
    {
        [SerializeField] private LayerMask _layer;
        [SerializeField] private bool _isTouchinglayer;
        private Collider2D _collider;

        public bool IsTouchingLayer => _isTouchinglayer;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            _isTouchinglayer = _collider.IsTouchingLayers(_layer);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            _isTouchinglayer = _collider.IsTouchingLayers(_layer);
        }
    }

}
