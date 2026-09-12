using UnityEngine;

public class Tap : MonoBehaviour
{
    [SerializeField] private ClockMove clockMove;
    [SerializeField] private CanvasGroup tapDisplayCanvasGroup; // タップ表示用のUIパネル（◯を含む）

    [Header("検出設定")]
    [Tooltip("検出対象の針。Hour = 時針、Minute = 分針、Second = 秒針")]
    [SerializeField] private HandType targetHand = HandType.Hour;

    [Tooltip("検出範囲（±何度まで検出するか）")]
    [SerializeField] private float detectionRange = 5f;

    [Tooltip("表示のフェードイン・フェードアウト時間")]
    [SerializeField] private float fadeDuration = 0.3f;

    [Header("ランダム設定")]
    [Tooltip("次の◯が出る角度の最小値（度数）")]
    [SerializeField] private float minAngle = 0f;

    [Tooltip("次の◯が出る角度の最大値（度数）")]
    [SerializeField] private float maxAngle = 360f;

    [Tooltip("現在の角度から最低限離す角度（度数）")]
    [SerializeField] private float minDistanceFromCurrent = 30f;

    private float targetAngle = 0f;
    private bool isDisplaying = false;
    private float fadeTimer = 0f;
    private bool wasInRangeLastFrame = false;

    public enum HandType { Hour, Minute, Second }

    private void Start()
    {
        if (clockMove == null)
        {
            clockMove = FindFirstObjectByType<ClockMove>();
        }

        if (tapDisplayCanvasGroup != null)
        {
            tapDisplayCanvasGroup.alpha = 0f;
        }

        // 最初のランダムな角度を生成
        GenerateRandomTargetAngle();
    }

    private void Update()
    {
        if (clockMove == null) return;

        float currentAngle = GetCurrentHandAngle();
        bool shouldDisplay = IsAngleInRange(currentAngle, targetAngle, detectionRange);

        // 検出範囲に入った
        if (shouldDisplay && !wasInRangeLastFrame)
        {
            isDisplaying = true;
            fadeTimer = 0f;
        }
        // 検出範囲から出た
        else if (!shouldDisplay && wasInRangeLastFrame)
        {
            isDisplaying = false;
            fadeTimer = 0f;
            // 新しいターゲット角度を生成
            GenerateRandomTargetAngle();
        }

        wasInRangeLastFrame = shouldDisplay;

        // フェード処理
        UpdateFade();
    }

    /// <summary>
    /// 現在の針の角度を取得
    /// </summary>
    private float GetCurrentHandAngle()
    {
        return targetHand switch
        {
            HandType.Hour => clockMove.GetHourHandAngle(),
            HandType.Minute => clockMove.GetMinuteHandAngle(),
            HandType.Second => clockMove.GetSecondHandAngle(),
            _ => 0f
        };
    }

    /// <summary>
    /// 指定角度が検出範囲内かを判定
    /// </summary>
    private bool IsAngleInRange(float currentAngle, float targetAngle, float range)
    {
        float diff = Mathf.Abs(currentAngle - targetAngle);
        // 360度の折り返しを考慮
        if (diff > 180f) diff = 360f - diff;
        return diff <= range;
    }

    /// <summary>
    /// ランダムな目標角度を生成（現在の角度から十分に離した位置）
    /// </summary>
    private void GenerateRandomTargetAngle()
    {
        float currentAngle = GetCurrentHandAngle();
        float newAngle;
        float diff;
        int maxAttempts = 50;
        int attempts = 0;

        // 現在の角度から十分に離れた角度を生成
        do
        {
            newAngle = Random.Range(minAngle, maxAngle);
            diff = Mathf.Abs(newAngle - currentAngle);
            if (diff > 180f) diff = 360f - diff;

            attempts++;
        } while (diff < minDistanceFromCurrent && attempts < maxAttempts);

        targetAngle = newAngle;
    }

    /// <summary>
    /// フェード処理
    /// </summary>
    private void UpdateFade()
    {
        if (tapDisplayCanvasGroup == null) return;

        fadeTimer += Time.deltaTime;
        float targetAlpha = isDisplaying ? 1f : 0f;

        if (fadeTimer >= fadeDuration)
        {
            tapDisplayCanvasGroup.alpha = targetAlpha;
        }
        else
        {
            tapDisplayCanvasGroup.alpha = Mathf.Lerp(
                isDisplaying ? 0f : 1f,
                targetAlpha,
                fadeTimer / fadeDuration
            );
        }
    }
}
