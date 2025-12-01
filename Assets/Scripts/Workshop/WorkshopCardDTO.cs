namespace Assets.Scripts.Service
{
    [System.Serializable]
    public class WorkshopCardDTO
    {
        public long id;          // null enquanto ainda não foi gravada no backend
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
        public string flavorText;
        public string expansionCode;
        public string status;     // "draft" ou "active"
    }
}