using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardUI : MonoBehaviour, IPointerClickHandler
{
    Image image;
    public CardData cardData;
    private CardDistributor distributor;
    public void SetImage(CardData cardData)
    {
        this.cardData = cardData;
        image = GetComponent<Image>();
        image.sprite = cardData.artwork;
    }
    void Start()
    {
        distributor = FindObjectOfType<CardDistributor>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Card clicked via UI");
        if (distributor != null && distributor.CanPlayerPick(this.gameObject))
        {
            distributor.SelectCard(this.gameObject);
        }
    }

}
