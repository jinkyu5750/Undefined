using System;
using System.Collections;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class TutorialStageManager : MonoBehaviour
{
    public static TutorialStageManager instance;

    [Serializable]
    public class StageSetup
    {
        [Tooltip("스테이지의 오브젝트들을 묶은 부모 오브젝트")]
        [FormerlySerializedAs("objects")]
        public GameObject stageObjects;
    }

    [Header("Stages")]
    [SerializeField] private StageSetup[] stages = new StageSetup[4];

    [Header("Player")]
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private Transform player;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvas;
    [SerializeField] private float fadeToBlackDuration = 1f;
    [SerializeField] private float fadeFromBlackDuration = 0.5f;

    [Header("Events")]
    [SerializeField] private UnityEvent<int> onStageStarted;
    [SerializeField] private UnityEvent onAllStagesCompleted;

    [SerializeField] private int currentStageIndex;
    private bool isTransitioning;

    private PlayerCharacterController playerController;
    private PlayerSystem playerSystem;
    Rigidbody rb;
    public int CurrentStageNumber => currentStageIndex + 1;
    public int TotalStages => stages != null ? stages.Length : 0;
    public bool IsTransitioning => isTransitioning;
    public bool IsLastStage => currentStageIndex >= TotalStages - 1;

    public event Action<int> OnStageChanged;

    private void Awake()
    {
        instance = this;

        if (player != null)
        {
            playerController = player.GetComponent<PlayerCharacterController>();
            playerSystem = player.GetComponent<PlayerSystem>();
            rb = player.GetComponent<Rigidbody>();
        }

        if (fadeCanvas != null)
        {
            fadeCanvas.gameObject.SetActive(true);
            fadeCanvas.alpha = 0f;
            fadeCanvas.blocksRaycasts = false;
        }
    }

    private void Start()
    {
        ApplyStage(0);
    }

    public void CompleteCurrentStage()
    {
        if (isTransitioning)
            return;

        if (IsLastStage)
        {
            onAllStagesCompleted?.Invoke();
            return;
        }

        StartCoroutine(TransitionToNextStage());
    }

    private IEnumerator TransitionToNextStage()
    {
        isTransitioning = true;

        yield return new WaitForSeconds(0.25f);
        SetPlayerControl(false);
        RemovePlayerVelocity();

        yield return Fade(0f, 1f, fadeToBlackDuration);

        ApplyStage(currentStageIndex + 1);
        TeleportPlayerToSpawn();

        yield return Fade(1f, 0f, fadeFromBlackDuration);

        SetPlayerControl(true);
        isTransitioning = false;
    }

    private void ApplyStage(int stageIndex)
    {
        if (stages == null || stages.Length == 0)
            return;

        stageIndex = Mathf.Clamp(stageIndex, 0, stages.Length - 1);

        for (int i = 0; i < stages.Length; i++)
            SetStageActive(i, i == stageIndex);

        currentStageIndex = stageIndex;
        OnStageChanged?.Invoke(CurrentStageNumber);
        onStageStarted?.Invoke(CurrentStageNumber);
    }

    private void SetStageActive(int stageIndex, bool active)
    {
        if (stageIndex < 0 || stageIndex >= stages.Length)
            return;
            
    //    if (stages[stageIndex]?.stageObjects != null)
            stages[stageIndex].stageObjects.SetActive(active);
    }

    private void RemovePlayerVelocity()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    private void TeleportPlayerToSpawn()
    {
        if (player == null || playerSpawnPoint == null)
            return;

        if (rb != null)
        {
            rb.position = playerSpawnPoint.position; // 텔레포트는 리지드바디로.. 트랜스폼으로 텔레포트 시킨들 중력등으로 남아있는 속력으로 동기화가 되어버림
        //    RemovePlayerVelocity();
        }
    }

    private IEnumerator Fade(float from, float to, float duration)
    {
        if (fadeCanvas == null)
            yield break;

        yield return new WaitForSeconds(0.5f);

        fadeCanvas.blocksRaycasts = to > from;

        if (duration <= 0f)
        {
            fadeCanvas.alpha = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fadeCanvas.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }

        fadeCanvas.alpha = to;
        fadeCanvas.blocksRaycasts = to > 0.5f;
    }

    private void SetPlayerControl(bool enabled)
    {
        if (playerController != null)
            playerController.canMove = enabled;

        if (playerSystem != null)
            playerSystem.enabled = enabled;
    }
}
