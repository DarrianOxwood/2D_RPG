using Scripts.Creatures;
using UnityEngine;

namespace Scripts.Components
{
    public class AddCoinsComponent : MonoBehaviour
    {
        [SerializeField] private int _coinAmount;
        [SerializeField] private Hero _hero;

        private void AddCoins(int _coinAmount)
        {
            _hero.AddCoins(_coinAmount);
        }
    }
}
