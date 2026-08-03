using TMPro;
using UnityEngine;

public class CoinManager : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;

    public static CoinManager Instance;

    private const string CoinKey = "Coins";
    private int coins;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        EnsureInstance();
    }

    public static CoinManager EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        Instance = FindAnyObjectByType<CoinManager>();
        if (Instance != null)
        {
            return Instance;
        }

        GameObject coinManagerObject = new GameObject("CoinManager");
        Instance = coinManagerObject.AddComponent<CoinManager>();
        Debug.Log("[CoinManager] Auto-created runtime instance.", Instance);
        return Instance;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[CoinManager] Duplicate instance found. Destroying the new instance.", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        coins = PlayerPrefs.GetInt(CoinKey, 100);
        UpdateUI();
    }

    public void SetCoinText(TMP_Text text)
    {
        coinText = text;
        UpdateUI();
    }

    public int GetCoins()
    {
        return coins;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        SaveCoins();
    }

    public bool SpendCoins(int amount)
    {
        if (coins < amount)
        {
            return false;
        }

        coins -= amount;
        SaveCoins();
        return true;
    }

    private void UpdateUI()
    {
        if (coinText != null)
        {
            coinText.text = coins.ToString();
        }
    }

    private void SaveCoins()
    {
        PlayerPrefs.SetInt(CoinKey, coins);
        PlayerPrefs.Save();
        UpdateUI();
    }
}
