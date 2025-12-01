using Assets.Scripts.Model;
using Assets.Scripts.Service;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Workshop
{
    public class WorkshopDraftManager : MonoBehaviour
    {

        [SerializeField] private Transform contentRoot;
        [SerializeField] private DraftRowUI rowPrefab;

        private readonly List<WorkshopCardDTO> _currentCards = new();

        public void Init(List<WorkshopCardDTO> cards)
        {
            _currentCards.Clear();
            _currentCards.AddRange(cards ?? new List<WorkshopCardDTO>());

            RebuildList();
        }

        private void RebuildList()
        {
            // destruir filhos antigos e instanciar novos DraftRowUI a partir de _currentCards
        }

        public void AddOrUpdateCard(WorkshopCardDTO dto)
        {
            // atualizar lista local depois de um upsert
        }
        
    }
}
