using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Service
{
    public class MmrService
    {
        private const string BaseUrl = "https://lagabi-group2-backend.onrender.com/api/Mmr";

        public async Task<long> GetPlayerMMRAsync(long userId)
        {
            using (UnityWebRequest req = UnityWebRequest.Get(BaseUrl + "/" + userId))
            {
                req.SetRequestHeader("X-User", userId.ToString());

                var operation = req.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("GET /MMR/ " + req.error);
                    return -1;
                }

                string json = req.downloadHandler.text;

                MmrDTO data = JsonUtility.FromJson<MmrDTO>(json);

                return data.rating;
            }
        }

    }
}