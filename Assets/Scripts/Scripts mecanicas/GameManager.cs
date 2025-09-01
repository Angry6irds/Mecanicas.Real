using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Setup")]
    public GameObject bombPrefab;
    public Transform[] spawnPoints; // 3
    public Material[] playerMats;   // 3
    public string[] playerNames = { "P1", "P2", "P3" };

    [Header("UI")]
    public TMP_Text roundText;
    public TMP_Text statusText;

    private List<PlayerController3P> players = new();
    private Bomb bomb;
    private int round = 1;

    void Start()
    {
        // deja que PlayerInputManager instancie jugadores
        Invoke(nameof(Setup), 0.1f);
    }

    void Setup()
    {
        players = FindObjectsByType<PlayerController3P>().ToList();

        // ubicar y tintar
        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];
            if (spawnPoints.Length > i) p.transform.position = spawnPoints[i].position;
            if (playerMats.Length > i) p.SetTint(playerMats[i]);
            if (playerNames.Length > i) p.displayName = playerNames[i];
        }

        foreach (var p in players)
        {
            p.RegisterRivals(players);
            p.OnEliminated += HandleElimination;
        }

        SpawnBomb();
        GiveBombToRandomAlive();
        UpdateUI("¡Bomb Rally!", $"Ronda {round}");
    }

    void SpawnBomb()
    {
        var b = Instantiate(bombPrefab, Vector3.up * 3f, Quaternion.identity);
        bomb = b.GetComponent<Bomb>();
        bomb.OnExploded += OnBombExploded;
    }

    void GiveBombToRandomAlive()
    {
        var vivos = players.Where(p => p.IsAlive).ToList();
        if (vivos.Count == 0 || bomb == null) return;
        var starter = vivos[Random.Range(0, vivos.Count)];
        bomb.AttachTo(starter);
        UpdateUI(null, $"{starter.displayName} inicia con la bomba");
    }

    void OnBombExploded(Vector3 pos)
    {
        Invoke(nameof(SpawnNextBombIfNeeded), 0.8f);
    }

    void SpawnNextBombIfNeeded()
    {
        var vivos = players.Where(p => p.IsAlive).ToList();
        if (vivos.Count <= 1) return; // terminará en HandleElimination
        SpawnBomb();
        GiveBombToRandomAlive();
    }

    void HandleElimination(PlayerController3P eliminated)
    {
        UpdateUI($"{eliminated.displayName} fue eliminado 💥", null);

        var vivos = players.Where(p => p.IsAlive).ToList();

        if (vivos.Count == 2)
        {
            round++;
            UpdateUI(null, $"Ronda {round} — {vivos[0].displayName} vs {vivos[1].displayName}");
            if (bomb) bomb.TuneDifficulty(0.9f);
        }
        else if (vivos.Count == 1)
        {
            UpdateUI("¡GANADOR!", $"{vivos[0].displayName} se lleva la corona 🏆");
            Invoke(nameof(ResetMatch), 3f);
        }
    }

    void ResetMatch()
    {
        foreach (var p in players) if (p) Destroy(p.gameObject);
        players.Clear();
        if (bomb) Destroy(bomb.gameObject);
        round = 1;
        Invoke(nameof(Setup), 0.1f);
    }

    void UpdateUI(string big, string small)
    {
        if (roundText && !string.IsNullOrEmpty(small)) roundText.text = small;
        if (statusText && !string.IsNullOrEmpty(big)) statusText.text = big;
    }
}