using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Data;
using Mono.Data.Sqlite;
using System.IO;

public class Roulette : MonoBehaviour
{
    public float spinSpeed = 720f;
    public float spinDuration = 3f;
    private float currentSpinTime = 0f;
    private bool isSpinning = false;

    private int[] rouletteNumbers = { 0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10, 5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26 };
    private int currentWinningNumber;

    public Transform rouletteWheel;
    public AudioSource tickSound;

    private float totalRotation;
    private float startAngle;
    private int lastTickIndex = -1;

    public Button butonRosu;
    public Button butonNegru;
    public TMP_InputField inputSuma;
    public TextMeshProUGUI mesajText;
    public TextMeshProUGUI soldText;
    private string dbPath;

    private enum CuloarePariu { Niciuna, Rosu, Negru }
    private CuloarePariu culoareSelectata = CuloarePariu.Niciuna;

    private float sold = 1000f; // Sold inițial

    void Start()
    {
        dbPath = "URI=file:" + Application.persistentDataPath + "/rouletteDB.db";
        CreeazaBazaDacaNuExista();

        sold = CitesteSoldDinBaza();
        ActualizeazaSold();

        butonRosu.onClick.AddListener(() => SelecteazaCuloare(CuloarePariu.Rosu));
        butonNegru.onClick.AddListener(() => SelecteazaCuloare(CuloarePariu.Negru));
    }

    void Update()
    {
        if (isSpinning)
        {
            currentSpinTime += Time.deltaTime;
            float t = currentSpinTime / spinDuration;
            t = Mathf.Clamp01(t);

            float smoothT = Mathf.SmoothStep(0, 1, t);
            float currentAngle = startAngle + totalRotation * smoothT;

            HandleTickSound(currentAngle);

            rouletteWheel.eulerAngles = new Vector3(0f, currentAngle, 0f);

            if (t >= 1f)
            {
                StopSpin();
            }
        }
    }

    private void CreeazaBazaDacaNuExista()
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS Jucator (id INTEGER PRIMARY KEY, sold REAL);";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "SELECT COUNT(*) FROM Jucator;";
                int count = int.Parse(cmd.ExecuteScalar().ToString());
                if (count == 0)
                {
                    cmd.CommandText = "INSERT INTO Jucator (sold) VALUES (1000);";
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }

    private float CitesteSoldDinBaza()
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT sold FROM Jucator WHERE id = 1;";
                return float.Parse(cmd.ExecuteScalar().ToString());
            }
        }
    }

    private void ActualizeazaSoldInBaza(float sumaNoua)
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "UPDATE Jucator SET sold = @sold WHERE id = 1;";
                cmd.Parameters.AddWithValue("@sold", sumaNoua);
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void StartSpin()
    {
        if (isSpinning) return;

        if (!float.TryParse(inputSuma.text, out float sumaPariata) || sumaPariata <= 0f)
        {
            AfiseazaMesaj("Te rog introdu o sumă validă.", Color.yellow);
            return;
        }

        if (culoareSelectata == CuloarePariu.Niciuna)
        {
            AfiseazaMesaj("Te rog selectează o culoare.", Color.yellow);
            return;
        }

        if (sumaPariata > sold)
        {
            AfiseazaMesaj("Nu ai suficienți bani pentru acest pariu.", Color.red);
            return;
        }

        sold -= sumaPariata; // Scade pariu din sold imediat
        ActualizeazaSoldInBaza(sold);
        inputSuma.interactable = false;

        currentSpinTime = 0f;
        isSpinning = true;
        startAngle = rouletteWheel.eulerAngles.y;

        float randomFullRotations = Random.Range(3, 6) * 360f;
        float randomOffset = Random.Range(0f, 360f);
        totalRotation = randomFullRotations + randomOffset;

        lastTickIndex = -1;
    }

    private void StopSpin()
    {
        isSpinning = false;
        inputSuma.interactable = true;

        float finalAngle = rouletteWheel.eulerAngles.y % 360f;
        float anglePerSegment = 360f / rouletteNumbers.Length;

        int index = Mathf.FloorToInt(finalAngle / anglePerSegment);
        index = rouletteNumbers.Length - 1 - index;

        currentWinningNumber = rouletteNumbers[index];
        CuloarePariu culoareCastigatoare = VerificaCuloare(currentWinningNumber);

        if (culoareCastigatoare == culoareSelectata)
        {
            float sumaPariata = float.Parse(inputSuma.text);
            float castig = sumaPariata * 2f;
            sold += castig;
            AfiseazaMesaj($"🎉 Ai câștigat! Numărul: {currentWinningNumber}, culoarea: {culoareCastigatoare}. +{castig} lei!", Color.green);
        }
        else
        {
            AfiseazaMesaj($"❌ Ai pierdut. Numărul: {currentWinningNumber}, culoarea: {culoareCastigatoare}.", Color.red);
        }

        ActualizeazaSold();
        ActualizeazaSoldInBaza(sold);
    }

    private void HandleTickSound(float currentAngle)
    {
        if (tickSound == null) return;

        float anglePerSegment = 360f / rouletteNumbers.Length;
        int currentIndex = Mathf.FloorToInt((currentAngle % 360f) / anglePerSegment);

        if (currentIndex != lastTickIndex)
        {
            tickSound.PlayOneShot(tickSound.clip);
            lastTickIndex = currentIndex;
        }
    }

    private void SelecteazaCuloare(CuloarePariu culoare)
    {
        culoareSelectata = culoare;
        AfiseazaMesaj("Ai selectat culoarea: " + culoare, Color.white);
    }

    private CuloarePariu VerificaCuloare(int numar)
    {
        if (numar == 0) return CuloarePariu.Niciuna;

        int[] rosii = { 32, 19, 21, 25, 34, 27, 36, 30, 23, 5, 16, 1, 14, 9, 18, 7, 12, 3 };
        if (System.Array.Exists(rosii, element => element == numar))
        {
            return CuloarePariu.Rosu;
        }

        return CuloarePariu.Negru;
    }

    private void AfiseazaMesaj(string mesaj, Color culoare)
    {
        if (mesajText != null)
        {
            mesajText.text = mesaj;
            mesajText.color = culoare;
        }

        Debug.Log(mesaj);
    }

    private void ActualizeazaSold()
    {
        if (soldText != null)
        {
            soldText.text = $"Sold: {sold:0.00} lei";
        }
    }
    public void InchideSiSchimbaScena(string numeScena)
    {
         SceneManager.LoadScene(numeScena);
    }
    public void InchideSiSchimbaScena()
    {
        SceneManager.LoadScene("casino"); // înlocuiește "NumeScena" cu numele scenei tale
    }
}
