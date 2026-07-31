using UnityEngine;
using TMPro;
public class CoinManager : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;
    public static CoinManager Instance;

    private const string CoinKey = "Coins";
    private int coins;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load saved coins
            coins = PlayerPrefs.GetInt(CoinKey, 100);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
   {
     ;
   }
   public void SetCoinText(TMP_Text text)
{
    coinText = text;
    UpdateUI();
}

private void UpdateUI()
{
    if (coinText != null)
        coinText.text = coins.ToString();
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
            return false;

        coins -= amount;
        SaveCoins();
        return true;
    }

    private void SaveCoins()
{
    PlayerPrefs.SetInt(CoinKey, coins);
    PlayerPrefs.Save();

    UpdateUI();
}
}