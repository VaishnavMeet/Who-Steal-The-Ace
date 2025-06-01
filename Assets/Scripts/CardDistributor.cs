using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CardDistributor : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform[] playerPanels;
   public List<CardData> deck;
    public Sprite unfoldCardImage;
    public Transform centerTarget;

    private int currentPlayer = 0;
    private GameObject selectedCard = null;
    private bool waitingForHumanInput = false;
    private Transform validTargetPanel = null;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadDeck();
        ShuffleDeck();
        DistibuteCards();
        StartCoroutine(RemovePairsAfterDelay());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool CanPlayerPick(GameObject card)
    {
        return waitingForHumanInput && card.transform.parent == validTargetPanel;
    }

    IEnumerator RemovePairsAfterDelay()
    {
        yield return new WaitForSeconds(2f); // Wait after dealing cards

        for (int i = 0; i < playerPanels.Length; i++)
        {
            yield return StartCoroutine(RemovePairsFromPanel(playerPanels[i]));
        }

        yield return new WaitForSeconds(1f);
        StartCoroutine(HandleTurns());
    }

    public void SelectCard(GameObject card)
    {
        if (waitingForHumanInput)
        {
            selectedCard = card;
            waitingForHumanInput = false;
        }
    }

    int ActivePlayersCount()
    {
        return playerPanels.Count(p => p.childCount > 0);
    }

    IEnumerator HandleTurns()
    {
        while (ActivePlayersCount() > 1)
        {
            int nextPlayer = GetNextActivePlayer(currentPlayer);
            if (nextPlayer == currentPlayer) break; // Only 1 player left

            Transform currentPanel = playerPanels[currentPlayer];
            Transform nextPanel = playerPanels[nextPlayer];

            if (currentPlayer == 0)
            {
               
                validTargetPanel = playerPanels[nextPlayer];
                waitingForHumanInput = true;

                yield return new WaitUntil(() => selectedCard != null);

                var cardUI = selectedCard.GetComponent<CardUI>();
                selectedCard.GetComponent<Image>().sprite = cardUI.cardData.artwork;

                waitingForHumanInput = false;
                validTargetPanel = null;
            }
            else
            {
                // AI Turn
                yield return new WaitForSeconds(1f);
                selectedCard = GetRandomCardFromPanel(nextPanel);

                if (selectedCard != null)
                {
                    var cardUI = selectedCard.GetComponent<CardUI>();
                    selectedCard.GetComponent<Image>().sprite = unfoldCardImage;
                }
            }

            if (selectedCard != null)
            {
                AddCardToPlayer(currentPanel, selectedCard);
                selectedCard = null;

                yield return StartCoroutine(RemovePairsFromPanel(currentPanel));
                yield return new WaitForSeconds(1f);
            }

            currentPlayer = GetNextActivePlayer(currentPlayer);
        }

        Debug.Log("Game Over");
    }

    void AddCardToPlayer(Transform panel, GameObject card)
    {
        card.transform.SetParent(panel);
    }

    GameObject GetRandomCardFromPanel(Transform panel)
    {
        if (panel.childCount == 0) return null;
        int randomIndex = Random.Range(0, panel.childCount);
        return panel.GetChild(randomIndex).gameObject;
    }

    int GetNextActivePlayer(int current)
    {
        for (int i = 1; i < playerPanels.Length; i++)
        {
            int index = (current + i) % playerPanels.Length;
            if (playerPanels[index].childCount > 0)
                return index;
        }
        return current;
    }





    IEnumerator RemovePairsFromPanel(Transform panel)
    {
        var cardUIs = panel.GetComponentsInChildren<CardUI>().ToList();
        var rankGroup=cardUIs.GroupBy(c=>c.cardData.rank).Where(g=>g.Count() >= 2).ToList();

        foreach (var group in rankGroup)
        {
            var pair = group.Take(2).ToList(); // Only one pair per rank

            foreach (var card in pair)
            {
                StartCoroutine(MoveToCenter(card.gameObject));
            }

            yield return new WaitForSeconds(1f);

            foreach (var card in pair)
            {
                Destroy(card.gameObject);
            }
        }
    }

    IEnumerator MoveToCenter(GameObject card)
    {
        Vector3 startPos = card.transform.position;
        Vector3 targetPos = centerTarget.position;
        float duration = 0.5f;
        float elapsed = 0f;
        card.GetComponent<CardUI>().SetImage(card.GetComponent<CardUI>().cardData);
        while (elapsed < duration)
        {
            card.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        card.transform.position = targetPos;
    }


    void LoadDeck()
    {
        var aces=deck.FindAll(c=>c.rank==CardData.CardRank.Ace);
        if(aces.Count>0 )
        {
            deck.Remove(aces[Random.Range(0,3)]);
        }
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
           var temp=deck[i];
            int randNumber=Random.Range(0,deck.Count);
            deck[i]=deck[randNumber];
            deck[randNumber]=temp;
        }
    }

   

  
    void DistibuteCards()
    {
        int playerNumber = 0;
        foreach (var card in deck) {
            GameObject cardGO = Instantiate(cardPrefab, playerPanels[playerNumber].transform);

            if (playerNumber == 0)
            {
            cardGO.GetComponent<CardUI>().SetImage(card);
            }
            else
            {
                cardGO.GetComponent<CardUI>().SetImage(card);
                cardGO.GetComponent<Image>().sprite=unfoldCardImage;
            }
            playerNumber = (playerNumber + 1) % playerPanels.Length;
        }
    }
}
