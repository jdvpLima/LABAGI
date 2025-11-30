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

        [Header("Dependencies")]
        [SerializeField] private CardWorkshopApiClient apiClient;
        [SerializeField] private CardFormUI formUI;
        [SerializeField] private CardPreviewUI previewUI;

        [Header("Draft List UI")]
        [SerializeField] private Transform draftListRoot;
        [SerializeField] private DraftRowUI draftRowPrefab;

        private readonly List<WorkshopCardDTO> _drafts = new List<WorkshopCardDTO>();
        private WorkshopCardDTO _selectedDraft;

        private void Start()
        {
            // Carregar drafts assim que o painel abrir
            StartCoroutine(RefreshDrafts());
        }

        /// <summary>
        /// Pode ser chamado externamente (por ex. pelo CardWorkshopManager)
        /// depois de um POST que mudou o status da carta.
        /// </summary>
        public void ForceRefresh()
        {
            StartCoroutine(RefreshDrafts());
        }

        private IEnumerator RefreshDrafts()
        {
            List<WorkshopCardDTO> collection = null;
            string error = null;

            yield return apiClient.GetUserCollection(
                cards => collection = cards,
                err => error = err
            );

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError("[WorkshopDraftManager] Erro ao carregar coleção: " + error);
                yield break;
            }

            if (collection == null)
            {
                Debug.LogWarning("[WorkshopDraftManager] Coleção vazia ou null.");
                collection = new List<WorkshopCardDTO>();
            }

            _drafts.Clear();

            // Filtrar apenas cartas com status "draft"
            foreach (var card in collection)
            {
                if (string.Equals(card.status, "draft", StringComparison.OrdinalIgnoreCase))
                {
                    _drafts.Add(card);
                }
            }

            BuildDraftList();
        }

        private void BuildDraftList()
        {
            foreach (Transform child in draftListRoot)
            {
                Destroy(child.gameObject);
            }

            foreach (var draft in _drafts.OrderBy(d => d.name))
            {
                var row = Instantiate(draftRowPrefab, draftListRoot);
                row.Setup(draft, () => OnDraftSelected(draft));
            }
        }

        private void OnDraftSelected(WorkshopCardDTO draftHeader)
        {
            _selectedDraft = draftHeader;
            StartCoroutine(LoadWorkshopCard(draftHeader));
        }

        private IEnumerator LoadWorkshopCard(WorkshopCardDTO header)
        {
            if (header == null)
            {
                yield break;
            }

            // cardId vem do DTO da collection (/api/Cards/collection)
            long? cardId = header.cardId;

            WorkshopCardDTO dto = null;
            string error = null;

            yield return apiClient.GetWorkshopCard(
                cardId,
                card => dto = card,
                err => error = err
            );

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError("[WorkshopDraftManager] Erro ao carregar draft do workshop: " + error);
                yield break;
            }

            if (dto == null)
            {
                Debug.LogWarning("[WorkshopDraftManager] WorkshopCardDTO devolvido a null.");
                yield break;
            }

            _selectedDraft = dto;

            // Preencher formulário e preview com a versão completa
            formUI.LoadFrom(dto);
            previewUI.UpdatePreview(dto);
        }
    }
}
