using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Model
{
    [System.Serializable]
    public class CardDto
    {
        public long cardId;
        public string name;
        public string suit;
        public string rarity;
        public int points;
        public string ability;
        public string trigger;
        public string effect;
        public int amount;
        public string target;
        public bool oncePerGame;
        public string abilityJson;
        public int quantity;
        public string expansionCode;
        public string expansionName;
        public string flavorText;
        public string status;
    }
}