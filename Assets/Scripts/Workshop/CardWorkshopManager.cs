using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Workshop
{
    public class CardWorkshopManager : MonoBehaviour
    {
        [SerializeField] private CardWorkshopApiClient apiClient;
        [SerializeField] private CardFormUI formUI;
        [SerializeField] private CardPreviewUI previewUI;

        // opcional: lista de drafts se tiveres um GET /api/Cards/workshop?status=draft

        private void Start()
        {
            formUI.OnFormChanged += dto => previewUI.UpdatePreview(dto);

            formUI.OnSubmitClicked += (dto, status) =>
            {
                StartCoroutine(SendToBackend(dto, status));
            };

            // se quiseres começar com um form limpo:
            formUI.ClearForm();
        }

        private IEnumerator SendToBackend(WorkshopCardDTO dto, string status)
        {
            bool done = false;
            string error = null;
            WorkshopCardDTO saved = null;

            yield return apiClient.PostWorkshopCard(
                dto,
                status,
                onSuccess: c =>
                {
                    saved = c;
                    done = true;
                },
                onError: e =>
                {
                    error = e;
                    done = true;
                });

            if (error != null)
            {
                Debug.LogError("Erro ao enviar carta para workshop: " + error);
                yield break;
            }

            // se o backend devolver a carta com id/status atualizados, recarregas o form
            if (saved != null)
            {
                formUI.LoadFrom(saved);
            }
        }
    }
}
