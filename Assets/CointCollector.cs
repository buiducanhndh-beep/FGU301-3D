using UnityEngine;
using TMPro;

public class CoinCollection : MonoBehaviour
{
    private int Coin = 0;

    public TextMeshProUGUI coinText;

    [Header("Effects")]
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectSound;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Coin Tag")
        {
            Coin++;
            coinText.text = "Coin: " + Coin.ToString();
            Debug.Log(Coin);

            // Spawn effect if assigned
            if (collectEffect != null)
                Instantiate(collectEffect, other.transform.position, Quaternion.identity);

            // Play sound if assigned
            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, other.transform.position);

            Destroy(other.gameObject);
        }
    }

    public int GetCoins() => Coin;
}