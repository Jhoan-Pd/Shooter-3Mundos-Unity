using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.FPS.Gameplay
{
    public class ObjectiveKillEnemies : Objective
    {
        [Header("Configuración de transición")]
        [Tooltip("Nombre de la escena que se cargará al completar este objetivo")]
        public string nextSceneName = "Mundo3-Desierto";

        [Tooltip("Chose whether you need to kill every enemies or only a minimum amount")]
        public bool MustKillAllEnemies = true;

        [Tooltip("If MustKillAllEnemies is false, this is the amount of enemy kills required")]
        public int KillsToCompleteObjective = 5;

        [Tooltip("Start sending notification about remaining enemies when this amount of enemies is left")]
        public int NotificationEnemiesRemainingThreshold = 3;

        int m_KillTotal;

        protected override void Start()
        {
            base.Start();

            EventManager.AddListener<EnemyKillEvent>(OnEnemyKilled);

            if (string.IsNullOrEmpty(Title))
                Title = "Eliminate " + (MustKillAllEnemies ? "all the" : KillsToCompleteObjective.ToString()) + " enemies";

            if (string.IsNullOrEmpty(Description))
                Description = GetUpdatedCounterAmount();
        }

        void OnEnemyKilled(EnemyKillEvent evt)
        {
            if (IsCompleted)
                return;

            m_KillTotal++;

            if (MustKillAllEnemies)
                KillsToCompleteObjective = evt.RemainingEnemyCount + m_KillTotal;

            int targetRemaining = MustKillAllEnemies ?
                evt.RemainingEnemyCount :
                KillsToCompleteObjective - m_KillTotal;

            // === cuando ya no hay enemigos ===
            if (targetRemaining == 0)
            {
                CompleteObjective(string.Empty, GetUpdatedCounterAmount(), "Objective complete: " + Title);

                // FORZAR CAMBIO DE ESCENA SIEMPRE
                ForceLoadNextScene();
                return;
            }

            // notificaciones
            string notificationText = "";
            if (NotificationEnemiesRemainingThreshold >= targetRemaining)
            {
                notificationText = (targetRemaining == 1)
                    ? "One enemy left"
                    : targetRemaining + " enemies left";
            }

            UpdateObjective(string.Empty, GetUpdatedCounterAmount(), notificationText);
        }

        string GetUpdatedCounterAmount()
        {
            return m_KillTotal + " / " + KillsToCompleteObjective;
        }

        // CAMBIO DE ESCENA FORZADO (PARA QUE SIEMPRE FUNCIONE)
        void ForceLoadNextScene()
        {
            if (string.IsNullOrEmpty(nextSceneName))
            {
                Debug.LogWarning("No se definió el nombre de la siguiente escena.");
                return;
            }

            Debug.Log("Cargando siguiente escena forzada: " + nextSceneName);
            SceneManager.LoadScene(nextSceneName);
        }

        void OnDestroy()
        {
            EventManager.RemoveListener<EnemyKillEvent>(OnEnemyKilled);
        }
    }
}
