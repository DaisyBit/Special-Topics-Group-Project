using UnityEngine;

public class GuideBotTargetManager : MonoBehaviour
{
    public Transform[] targets;
    private int currentIndex = 0;

    public Transform GetCurrentTarget()
    {
        if (targets == null || targets.Length == 0) return null;
        return targets[currentIndex];
    }

    public void AdvanceTarget()
    {
        if (targets == null || targets.Length == 0) return;
        currentIndex = (currentIndex + 1) % targets.Length;
    }

    public void ResetToFirst()
    {
        currentIndex = 0;
    }

    public void SetRandomTarget()
    {
        if (targets == null || targets.Length == 0) return;
        currentIndex = Random.Range(0, targets.Length);
    }

    public int GetCurrentIndex()
    {
        return currentIndex;
    }
}