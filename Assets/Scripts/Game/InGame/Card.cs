using Assets.Scripts.Model; 
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Card
{
    public long CardId { get; private set; }
    public string Name { get; private set; } = "";
    public string Suit { get; private set; } = "";
    public int Points { get; private set; } = 0;
    public string Rarity { get; private set; } = "";
    public string FlavourText { get; private set; } = "";

    // Initialize here to be safe
    public List<string> Actions { get; private set; } = new List<string>();

    public Card(long id, string name, string suit, int points = 0, string rarity = "", string flavourText = null)
    {
        CardId = id;
        Name = name;
        Suit = suit;
        Points = points;
        Rarity = rarity;
        if (flavourText != null) FlavourText = flavourText;
        
        // Ensure list is never null
        if (Actions == null) Actions = new List<string>();
    }

    public static Card FromDto(CardDto dto)
    {
        string flavour = dto.flavorText ?? "";
        string cardName = dto.name ?? $"Card {dto.cardId}";
        string cardSuit = dto.suit ?? "";
        string cardRarity = dto.rarity ?? "";

        var card = new Card(dto.cardId, cardName, cardSuit, dto.points, cardRarity, flavour);

        if (!string.IsNullOrEmpty(dto.abilityJson))
        {
            try
            {
                var actions = JsonConvert.DeserializeObject<List<string>>(dto.abilityJson);
                if (actions != null) card.Actions.AddRange(actions);
                else card.Actions.Add(dto.abilityJson);
            }
            catch
            {
                card.Actions.Add(dto.abilityJson);
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(dto.effect)) card.Actions.Add(dto.effect);
            if (!string.IsNullOrEmpty(dto.ability)) card.Actions.Add(dto.ability);
            if (!string.IsNullOrEmpty(dto.trigger)) card.Actions.Add(dto.trigger);
        }

        return card;
    }
}