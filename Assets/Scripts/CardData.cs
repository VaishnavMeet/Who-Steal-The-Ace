using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Card Game/Card Data")]
public class CardData : ScriptableObject
{
    public enum CardSuit { Hearts, Diamonds, Clubs, Spades }
    public enum CardRank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }


    public CardSuit suit;
    public CardRank rank;
    public Sprite artwork;
}
