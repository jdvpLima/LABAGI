using Assets.Scripts.Model;
using System;
using UnityEngine;

namespace Assets.Scripts.Workshop
{
    public static class AbilityJsonBuilder
    {
        public static string Build(CardDto dto)
        {
            if (dto == null)
                return null;

            var payload = new AbilityJsonPayload
            {
                trigger = dto.trigger,
                effect = dto.effect,
                amount = dto.amount,
                target = dto.target,
                oncePerGame = dto.oncePerGame
            };

            // Neste momento não inventamos conditions/costs/etc.
            // Mais tarde: acrescentas campos ao payload + UI e preenches aqui.

            var json = JsonUtility.ToJson(payload);
            return json;
        }
    }
}
