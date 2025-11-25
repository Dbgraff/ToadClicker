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
    [SerializeField] private int toadLevel = 0;
    [SerializeField] private ToadState currentState = ToadState.Hungry;

    [Header("👁️ UI References")]
    [SerializeField] private TextMeshProUGUI enegryText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button feedButton;
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
    [SerializeField] private float evolveDuration = 3f;

    public enum ToadState {Hungry, Happy, Evolving}

    public int Energy => energy;
    public int ToadLevel => toadLevel;
    public ToadState CurrentState => currentState;

    private Renderer toadRenderer;
    private Vector3 originalScale;
    private Coroutine stateCoroutine;

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
        
    }

    public void OnToadClick()
    {
        if(currentState != ToadState.Evolving)
        {
            AddEnergy(1);
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
        if (energy >= feedCost && currentState != ToadState.Evolving)
        {
            energy -= feedCost;
            SetToadState(ToadState.Happy);
            UpdateUI();
        }
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
                //toadRenderer.material = hungryMaterial;
                //statusText.text = "Голодная 🐸";
                break;
            case ToadState.Happy:
                //toadRenderer.material = happyMaterial;
                //statusText.text = "Счастливая 💫";
                //stateCoroutine = StartCoroutine(ReturnToHungry());
                break;
            case ToadState.Evolving:
                //toadRenderer.material = evolvingMaterial;
                //statusText.text = "Эволюционирует... 🌟";
                //stateCoroutine = StartCoroutine(EvolutionProcess());
                break;
        }
    }

    private void UpdateUI()
    {
        //energyText.text = $"Энергия: {energy}";
        //levelText.text = $"Уровень: {toadLevel}";

        //feedButton.interactable = energy >= feedCost && currentState != ToadState.Evolving;
        //evolveButton.interactable = energy >= evolveCost && currentState != ToadState.Evolving;
    }


    private IEnumerator ReturnToHungry()
    {
        yield return new WaitForSeconds(happyDuration);
        SetToadState(ToadState.Hungry);
    }

    private IEnumerator EvolutionProcess()
    {
        float timer = 0f;
        Vector3 startScale = toadObject.transform.localScale;
        Vector3 targetScale = originalScale * 1.3f;

        while (timer < evolveDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / evolveDuration;

            float pulse = Mathf.Sin(progress * Mathf.PI * 8f) * 0.1f + 1f;
            toadObject.transform.localScale = Vector3.Lerp(startScale, targetScale, progress) * pulse;

            yield return null;
        }

        toadLevel++;
        toadObject.transform.localScale = originalScale * (1f + toadLevel * 0.1f);
        SetToadState(ToadState.Happy);
    }

    private IEnumerator ClickAnimation()
    {
        toadObject.transform.localScale = originalScale * (1f + toadLevel * 0.1f) * 1.2f;
        yield return new WaitForSeconds(0.1f);
        toadObject.transform.localScale = originalScale * (1f + toadLevel * 0.1f);
    }
}
