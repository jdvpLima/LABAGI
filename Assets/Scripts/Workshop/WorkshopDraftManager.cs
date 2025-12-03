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

        public event Action<WorkshopCardDTO> OnDraftSelected;

        public void Init(List<WorkshopCardDTO> cards)
        {
            _currentCards.Clear();
            // Se quiseres só drafts:
            // _currentCards.AddRange((cards ?? new List<WorkshopCardDTO>())
            //     .Where(c => c.status == "draft"));

            _currentCards.AddRange(cards ?? new List<WorkshopCardDTO>());

            RebuildList();
        }

        private void RebuildList()
        {
            if (contentRoot == null || rowPrefab == null)
            {
                Debug.LogWarning("[WorkshopDraftManager] contentRoot ou rowPrefab não estão atribuídos.");
                return;
            }

            // limpar rows antigos
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(contentRoot.GetChild(i).gameObject);
            }

            // opcional: ordenar por status/name
            _currentCards.Sort((a, b) =>
            {
                var s1 = a.status ?? "";
                var s2 = b.status ?? "";
                int cmp = string.Compare(s1, s2, StringComparison.Ordinal);
                if (cmp != 0) return cmp;
                return string.Compare(a.name, b.name, StringComparison.Ordinal);
            });

            // instanciar rows
            foreach (var card in _currentCards)
            {
                var row = Instantiate(rowPrefab, contentRoot);
                row.Setup(card, () => OnDraftSelected?.Invoke(card));
            }
        }

        public void AddOrUpdateCard(WorkshopCardDTO dto)
        {
            if (dto == null)
                return;

            // se o backend ainda não devolve id, isto nunca vai "matchar" – mas em princípio devolve
            var index = _currentCards.FindIndex(c => c.id == dto.id);

            if (index >= 0)
            {
                _currentCards[index] = dto;
            }
            else
            {
                _currentCards.Add(dto);
            }

            RebuildList();
        }

    }
}
