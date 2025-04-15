using UnityEngine;

public class Roulette : MonoBehaviour
{
    public float spinSpeed = 720f; // Viteza de rotire inițială în grade/sec
    public float spinDuration = 3f; // Durata rotirii
    private float currentSpinTime = 0f;
    private bool isSpinning = false;

    private int[] rouletteNumbers = { 0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10, 5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26 };
    private int currentWinningNumber;

    public Transform rouletteWheel; // Obiectul care reprezintă roata
    public AudioSource tickSound; // Referință la componentul AudioSource pentru sunetul de tick

    private float totalRotation;
    private float startAngle;
    private int lastTickIndex = -1; // Variabilă pentru a reține ultimul segment de tick

    void Update()
    {
        if (isSpinning)
        {
            currentSpinTime += Time.deltaTime;
            float t = currentSpinTime / spinDuration;
            t = Mathf.Clamp01(t);

            // Smooth decelerare (usor easing)
            float smoothT = Mathf.SmoothStep(0, 1, t);
            float currentAngle = startAngle + totalRotation * smoothT;

            // Apelează metoda care redă sunetul de tick
            HandleTickSound(currentAngle);

            // Aplică rotația
            rouletteWheel.eulerAngles = new Vector3(0f, currentAngle, 0f);

            if (t >= 1f)
            {
                StopSpin();
            }
        }
    }

    public void StartSpin()
    {
        if (!isSpinning)
        {
            isSpinning = true;
            currentSpinTime = 0f;
            startAngle = rouletteWheel.eulerAngles.y;

            // Generează o rotație totală aleatoare (între 3 și 6 rotații complete + un offset aleatoriu)
            float randomFullRotations = Random.Range(3, 6) * 360f; // Între 3 și 6 rotații complete
            float anglePerSegment = 360f / rouletteNumbers.Length;
            float randomOffset = Random.Range(0f, 360f); // Adăugăm un offset aleatoriu între segmente

            totalRotation = randomFullRotations + randomOffset;
            lastTickIndex = -1; // Resetează indexul pentru tick
        }
    }

    private void StopSpin()
    {
        isSpinning = false;

        // Când roata se oprește, alegem un număr câștigător pe baza unghiului final
        float finalAngle = rouletteWheel.eulerAngles.y % 360f;
        float anglePerSegment = 360f / rouletteNumbers.Length;

        // Calculăm indexul numărului câștigător
        int index = Mathf.FloorToInt(finalAngle / anglePerSegment);

        // Dacă roata se rotește invers acelor de ceasornic, inversăm indexul
        index = rouletteNumbers.Length - 1 - index;

        currentWinningNumber = rouletteNumbers[index];
        Debug.Log("Numărul câștigător este: " + currentWinningNumber);
    }

    private void HandleTickSound(float currentAngle)
    {
        if (tickSound == null) return; // Verifică dacă AudioSource este setat

        float anglePerSegment = 360f / rouletteNumbers.Length;
        int currentIndex = Mathf.FloorToInt((currentAngle % 360f) / anglePerSegment);

        // Verifică doar dacă segmentul s-a schimbat
        if (currentIndex != lastTickIndex)
        {
            tickSound.PlayOneShot(tickSound.clip); // Redă sunetul pentru fiecare segment
            lastTickIndex = currentIndex;
        }
    }
}
