using System;
using UnityEngine;
using UnityEngine.Events;

namespace Scripts.Components
{
    public class EnterCollision : MonoBehaviour
    {
        [SerializeField] private string _tag;
        [SerializeField] private UnityEvent<GameObject> _action;

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag(_tag))
            {
                _action?.Invoke(other.gameObject);
            }
        }
        
        [Serializable]
        private class EnterEvent : UnityEvent<GameObject>
        {

        }

    }

}
