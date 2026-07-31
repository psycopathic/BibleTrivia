using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text coinText;

    private void Start()
    {
        // Debug.Log("CoinUI Started");

        if (CoinManager.Instance != null)
        {
        CoinManager.Instance.SetCoinText(coinText);
        }
    }
}