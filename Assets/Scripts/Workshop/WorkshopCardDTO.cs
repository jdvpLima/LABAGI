using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Workshop
{
    public class WorkshopCardDTO
    {
        public long? cardId;
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
        public string status; // usado para filtrar "draft"
        public string flavorText;
    }
}
