using Assets.Scripts.Model; // Ensure this namespace matches where CardDto is
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Card
{
    // Properties (Capitalized for C# standard)
    public long CardId { get; private set; }
    public string Name { get; private set; } = "";
    public string Suit { get; private set; } = "";
    public int Points { get; private set; } = 0;
    public string Rarity { get; private set; } = "";
    public string FlavourText { get; private set; } = "";

    public List<string> Actions { get; private set; } = new List<string>();

    // --- CONSTRUCTOR ---
    public Card(long id, string name, string suit, int points = 0, string rarity = "", string flavourText = null)
    {
        CardId = id;
        Name = name;
        Suit = suit;
        Points = points;
        Rarity = rarity;
        if (flavourText != null) FlavourText = flavourText;
    }

    // --- FACTORY METHOD (The Fix is Here) ---
    public static Card FromDto(CardDto dto)
    {
        // 1. Map basic fields using the LOWERCASE variable names from your DTO
        string flavour = dto.flavorText ?? ""; // Uses 'flavorText' (lowercase)
        string cardName = dto.name ?? $"Card {dto.cardId}";
        string cardSuit = dto.suit ?? "";
        string cardRarity = dto.rarity ?? "";

        // 2. Create the Card using the new constructor
        // Notice we use 'dto.cardId' (lowercase c)
        var card = new Card(dto.cardId, cardName, cardSuit, dto.points, cardRarity, flavour);

        // 3. Populate Actions logic
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
            // Fallback for older card formats
            if (!string.IsNullOrEmpty(dto.effect)) card.Actions.Add(dto.effect);
            if (!string.IsNullOrEmpty(dto.ability)) card.Actions.Add(dto.ability);
            if (!string.IsNullOrEmpty(dto.trigger)) card.Actions.Add(dto.trigger);
        }

        return card;
    }
}