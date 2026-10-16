using UnityEngine;

[RequireComponent(typeof(Collider))]
public sealed class Coin : MonoBehaviour
{
    [SerializeField, Min(1)] private int value = 1;

    private bool collected;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    public void Collect()
    {
        if (collected)
        {
            return;
        }

        collected = true;
        GameManager.Instance.AddCoins(value);
        CoinBubbleEffect.Play(transform.position);
        Destroy(gameObject);
    }
}
