using UnityEngine;
using UnityEngine.SceneManagement;

namespace KitchenGame
{
    public class KitchenRuntimeResetHotkey : MonoBehaviour
    {
        [SerializeField]
        private KeyCode resetKey = KeyCode.R;

        [SerializeField]
        private bool requirePlayMode = true;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            var host = new GameObject("KitchenRuntimeResetHotkey");
            DontDestroyOnLoad(host);
            host.AddComponent<KitchenRuntimeResetHotkey>();
        }

        private void Update()
        {
            if (requirePlayMode && !Application.isPlaying)
            {
                return;
            }

            if (!Input.GetKeyDown(resetKey))
            {
                return;
            }

            var activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid())
            {
                return;
            }

            SceneManager.LoadScene(activeScene.buildIndex);
        }
    }
}
