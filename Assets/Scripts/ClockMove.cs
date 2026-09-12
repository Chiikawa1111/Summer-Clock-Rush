using System;
using UnityEngine;

public class ClockMove : MonoBehaviour
{
    [Header("Hands (割り当て不要のものは空のままにしてください)")]
    [SerializeField] private Transform hourHand;
    [SerializeField] private Transform minuteHand;
    [SerializeField] private Transform secondHand;

    [Header("Clock Center (回転の中心)")]
    [Tooltip("針が回転する中心。未設定ならこの GameObject を中心に扱います。")]
    [SerializeField] private Transform clockCenter;

    [Header("設定")]
    [Tooltip("true = システム時刻に合わせる（滑らかな動き）、false = 指定速度で回し続ける")]
    [SerializeField] private bool useSystemTime = true;

    [Tooltip("定速モード時の回転速度（度/秒）。正の値で時計回りになります。")]
    [SerializeField] private float secondSpeed = 6f;   // 360 / 60
    [SerializeField] private float minuteSpeed = 0.1f; // 360 / 3600
    [SerializeField] private float hourSpeed = 0.008333333f; // 360 / 43200

    [Header("オフセット（必要に応じて調整）")]
    [Tooltip("各針の初期向きが12時基準と異なる場合はここで補正します（度）。")]
    [SerializeField] private float hourOffset = 0f;
    [SerializeField] private float minuteOffset = 0f;
    [SerializeField] private float secondOffset = 0f;

    // 中心から見た各針の初期オフセット（ワールド空間）
    private Vector3 hourInitialOffset;
    private Vector3 minuteInitialOffset;
    private Vector3 secondInitialOffset;

    // 現在の角度（キャッシュ）
    private float currentSecAngle;
    private float currentMinAngle;
    private float currentHourAngle;

    private void Start()
    {
        if (clockCenter == null) clockCenter = transform;

        if (hourHand) hourInitialOffset = hourHand.position - clockCenter.position;
        if (minuteHand) minuteInitialOffset = minuteHand.position - clockCenter.position;
        if (secondHand) secondInitialOffset = secondHand.position - clockCenter.position;
    }

    // Update is called once per frame
    private void Update()
    {
        if (useSystemTime)
        {
            UpdateBySystemTime();
        }
        else
        {
            UpdateByConstantSpeed();
        }
    }

    // システム時刻に合わせて針を設定（滑らかに動く） — 時計の中心を固定点とする
    private void UpdateBySystemTime()
    {
        DateTime now = DateTime.Now;
        float sec = now.Second + now.Millisecond / 1000f;
        float min = now.Minute + sec / 60f;
        float hr = (now.Hour % 12) + min / 60f;

        currentSecAngle = sec * 6f + secondOffset;      // 360 / 60
        currentMinAngle = min * 6f + minuteOffset;      // 360 / 60
        currentHourAngle = hr * 30f + hourOffset;       // 360 / 12

        if (secondHand) SetPositionAndRotationAroundCenter(secondHand, secondInitialOffset, currentSecAngle);
        if (minuteHand) SetPositionAndRotationAroundCenter(minuteHand, minuteInitialOffset, currentMinAngle);
        if (hourHand) SetPositionAndRotationAroundCenter(hourHand, hourInitialOffset, currentHourAngle);
    }

    // 指定速度で回し続ける（インスペクターで速度を調整） — clockCenter を回転中心にする
    private void UpdateByConstantSpeed()
    {
        float dt = Time.deltaTime;
        Vector3 pivot = clockCenter.position;
        if (secondHand) secondHand.RotateAround(pivot, Vector3.forward, -secondSpeed * dt);
        if (minuteHand) minuteHand.RotateAround(pivot, Vector3.forward, -minuteSpeed * dt);
        if (hourHand) hourHand.RotateAround(pivot, Vector3.forward, -hourSpeed * dt);
    }

    // 中心 pivot を固定して、初期オフセットを回転させて位置を計算、針の回転も設定する
    // angleDeg は「正＝時計回り」の度数（内部で負号を使い -angleDeg を適用）
    private void SetPositionAndRotationAroundCenter(Transform hand, Vector3 initialOffset, float angleDeg)
    {
        if (hand == null) return;

        Vector3 pivot = clockCenter.position;
        Quaternion rot = Quaternion.Euler(0f, 0f, -angleDeg);
        Vector3 newPos = pivot + rot * initialOffset;
        hand.SetPositionAndRotation(newPos, rot);
    }

    /// <summary>
    /// 現在の時針の角度を取得（0～360度）
    /// </summary>
    public float GetHourHandAngle() => NormalizeAngle(currentHourAngle);

    /// <summary>
    /// 現在の分針の角度を取得（0～360度）
    /// </summary>
    public float GetMinuteHandAngle() => NormalizeAngle(currentMinAngle);

    /// <summary>
    /// 現在の秒針の角度を取得（0～360度）
    /// </summary>
    public float GetSecondHandAngle() => NormalizeAngle(currentSecAngle);

    /// <summary>
    /// 角度を0～360の範囲に正規化
    /// </summary>
    private float NormalizeAngle(float angle)
    {
        angle = angle % 360f;
        if (angle < 0f) angle += 360f;
        return angle;
    }
}
