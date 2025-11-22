using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.CreateDeck
{
    [System.Serializable]
    public class DecksDto 
    {
        public int id;
        public string name;
        public List<DeckCards> cards;
    }
}