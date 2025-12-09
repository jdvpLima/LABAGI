namespace Assets.Scripts.Workshop
{
	[System.Serializable]
	public class AbilityJsonPayload
    {
        public string trigger;
        public string effect;
        public int amount;
        public string target;
        public bool oncePerGame;
    }
}