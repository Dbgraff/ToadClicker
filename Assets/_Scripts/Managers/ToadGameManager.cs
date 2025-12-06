using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;
using System;

public class ToadGameManager : MonoBehaviour
{
    public static ToadGameManager Instance;

    //[Header("🐸 Toad Settings")]
    //[Header("🎮 Gameplay")]
    //[Header("👁️ UI")]
    //[Header("🔊 Audio")]
    //[Header("⚙️ System")]

    [Header("🐸 Toad State")]
    [SerializeField] private int energy = 0;
    [SerializeField] private int toadLevel = 1;
    [SerializeField] private ToadState currentState = ToadState.Hungry;
    [SerializeField] private int baseClickEnergy = 1;
    [SerializeField] private int happyClickBonus = 2;


    [Header("🔄 Auto Income")]
    [SerializeField] private float autoEnergyInterval = 5f;
    [SerializeField] private int baseAutoEnergy = 1;
    [SerializeField] private int happyAutoEnergyBonus = 2;

    [Header("👁️ UI References")]
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button feedButton;
    [SerializeField] private TextMeshProUGUI feedButtonText;
    [SerializeField] private Button evolveButton;

    [Header("🎨 Toad Visuals")]
    [SerializeField] private GameObject toadObject;
    [SerializeField] private Material hungryMaterial;
    [SerializeField] private Material happyMaterial;
    [SerializeField] private Material evolvingMaterial;

    [Header("🎮 Gameplay Configuration")]
    [SerializeField] private int feedCost = 10;
    [SerializeField] private int evolveCost = 100;
    [SerializeField] private float happyDuration = 5f;
    [SerializeField] private float baseEvolveDuration = 3f;
    [SerializeField] private float happyEvolveSpeedBonus = 0.5f;
    [SerializeField] private float happyFeedCostMultiplier = 0.8f;

    public enum ToadState {Hungry, Happy, Evolving}

    public int Energy => energy;
    public int ToadLevel => toadLevel;
    public ToadState CurrentState => currentState;

    private Renderer toadRenderer;
    private Vector3 originalScale;
    private Coroutine stateCoroutine;
    private Coroutine autoIncrementCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        toadRenderer = toadObject.GetComponent<Renderer>();
        originalScale = toadObject.transform.localScale;
        SetToadState(ToadState.Hungry);
        UpdateUI();
        autoIncrementCoroutine = StartCoroutine(AutoEnergyGeneration());       
    }

    public void OnToadClick()
    {
        if(currentState != ToadState.Evolving)
        {
            int energyGained = baseClickEnergy;

            if (currentState == ToadState.Happy)
                energyGained += happyClickBonus;

            AddEnergy(energyGained);
            StartCoroutine(ClickAnimation());
        }
    }

    public void AddEnergy(int amount)
    {
        if(amount > 0)
        {
            energy += amount;
            Console.WriteLine(energy);
            UpdateUI();
        }
    }

    public void FeedToad()
    {
        int actualFeedCost = currentState == ToadState.Happy
            ? Mathf.RoundToInt(feedCost * happyFeedCostMultiplier) : feedCost;

        if (energy >= actualFeedCost && currentState != ToadState.Evolving)
        {
            energy -= actualFeedCost;
            SetToadState(ToadState.Happy);
            UpdateUI();

            Debug.Log($"🍕 Накормлено за {actualFeedCost} энергии");
        }
    }

    private int GetCurrentFeedCost()
    {
        if (currentState == ToadState.Happy)
        {
            return Mathf.RoundToInt(feedCost * happyFeedCostMultiplier);
        }
        return feedCost;
    }

    public void EvolveToad()
    {
        if(energy >= evolveCost &&  currentState != ToadState.Evolving)
        {
            energy -= evolveCost;
            SetToadState(ToadState.Evolving);
            UpdateUI();
        }
    }

    public bool TrySpendEnergy(int amount)
    {
        if(energy >= amount)
        {
            energy -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    private void SetToadState(ToadState newState)
    {
        currentState = newState;

        if (stateCoroutine != null)
            StopCoroutine(stateCoroutine);

        switch (currentState)
        {
            case ToadState.Hungry:
                toadRenderer.material = hungryMaterial;
                statusText.text = "Голодная";
                break;
            case ToadState.Happy:
                toadRenderer.material = happyMaterial;
                statusText.text = "Счастливая";
                stateCoroutine = StartCoroutine(ReturnToHungry());
                break;
            case ToadState.Evolving:
                toadRenderer.material = evolvingMaterial;
                statusText.text = "Эволюционирует...";
                stateCoroutine = StartCoroutine(EvolutionProcess());
                break;
        }
    }

    private void UpdateFeedButtonText(int currentFeedCost)
    {
        string discountText = currentState == ToadState.Happy ? " (скидка!)" : "";

        feedButtonText.text = $"Покормить{discountText}\n<size=34>{currentFeedCost} энергии</size=34>";
    }

    private void UpdateUI()
    {
        energyText.text = $"Энергия: {energy}";
        levelText.text = $"Уровень: {toadLevel}";

        int currentFeedCost = GetCurrentFeedCost();
        UpdateFeedButtonText(currentFeedCost);

        feedButton.interactable = energy >= feedCost && currentState != ToadState.Evolving;
        evolveButton.interactable = energy >= evolveCost && currentState != ToadState.Evolving;
    }


    private IEnumerator ReturnToHungry()
    {
        yield return new WaitForSeconds(happyDuration);
        SetToadState(ToadState.Hungry);
    }

    private IEnumerator EvolutionProcess()
    {
        float actualEvolveDuration = baseEvolveDuration;

        if (currentState == ToadState.Happy)
            actualEvolveDuration -= happyEvolveSpeedBonus;

        Debug.Log($"🌟 Эволюция займёт {actualEvolveDuration} сек");

        float timer = 0f;
        Vector3 startScale = toadObject.transform.localScale;
        Vector3 targetScale = originalScale * 1.3f;

        while(timer < actualEvolveDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / actualEvolveDuration;

            float pulse = Mathf.Sin(progress * Mathf.PI * 8f) * 0.1f + 1f;
            toadObject.transform.localScale = Vector3.Lerp(startScale, targetScale, progress) * pulse;
            
            yield return null;
        }

        toadLevel++;
        UpdateUI();
        toadObject.transform.localScale = originalScale * (1f + toadLevel * 0.1f);
        SetToadState(ToadState.Happy);

    }

    private IEnumerator ClickAnimation()
    {
        toadObject.transform.localScale = originalScale * (1f + toadLevel * 0.1f) * 1.2f;
        yield return new WaitForSeconds(0.1f);
        toadObject.transform.localScale = originalScale * (1f + toadLevel * 0.1f);
    }

    private IEnumerator AutoEnergyGeneration()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoEnergyInterval);

            switch (currentState)
            {
                case ToadState.Hungry:
                    break;

                case ToadState.Happy:
                    AddEnergy(baseAutoEnergy + happyAutoEnergyBonus);
                    break;

                case ToadState.Evolving:
                    break;
            }
        }
            
    }
}
