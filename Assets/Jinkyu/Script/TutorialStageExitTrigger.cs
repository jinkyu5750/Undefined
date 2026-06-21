using DoorScript;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TutorialStageExitTrigger : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private Door door;
    [SerializeField] private bool requireDoorOpen = true;

    [Header("Trigger")]
    [SerializeField] private string playerTag = "Player";

    private bool hasTriggered;

    private void OnEnable()
    {
        if (TutorialStageManager.instance != null)
            TutorialStageManager.instance.OnStageChanged += HandleStageChanged;
    }

    private void OnDisable()
    {
        if (TutorialStageManager.instance != null)
            TutorialStageManager.instance.OnStageChanged -= HandleStageChanged;
    }

    private void HandleStageChanged(int stageNumber)
    {
        hasTriggered = false;
    }

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered)
            return;

        if (!other.CompareTag(playerTag))
            return;

        if (requireDoorOpen && door != null && !door.open)
            return;

        if (TutorialStageManager.instance == null)
        {
            Debug.LogWarning($"{nameof(TutorialStageExitTrigger)}: TutorialStageManager가 씬에 없습니다.");
            return;
        }

        if (TutorialStageManager.instance.IsTransitioning)
            return;

        hasTriggered = true;
        TutorialStageManager.instance.CompleteCurrentStage();
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
