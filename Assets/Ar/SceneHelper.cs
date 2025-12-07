using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic; // Necessário para usar Listas

public static class SceneHelper
{
    /// <summary>
    /// Retorna uma lista com TODOS os GameObjects de todas as cenas carregadas.
    /// Inclui objetos desativados e filhos de filhos.
    /// </summary>
    /// <returns>Lista de GameObjects</returns>
    public static GameObject[] GetAllObjectsInAllScenes()
    {
        // Cria a lista vazia que vamos encher e devolver
        List<GameObject> allObjects = new List<GameObject>();

        // 1. Percorre todas as cenas abertas (Aditivas incluídas)
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
           
            Scene scene = SceneManager.GetSceneAt(i);

            Debug.Log(scene.name);

            if (!scene.isLoaded) continue;

            // 2. Pega nos objetos de raiz dessa cena
            GameObject[] rootObjects = scene.GetRootGameObjects();

            foreach (GameObject root in rootObjects)
            {
                // 3. Pega em TODOS os descendentes (true = inclui inativos)
                Transform[] allTransforms = root.GetComponentsInChildren<Transform>(true);

                foreach (Transform t in allTransforms)
                {
                    allObjects.Add(t.gameObject);
                }
            }
        }

        // Devolve a lista completa
        return allObjects.ToArray();
    }
}