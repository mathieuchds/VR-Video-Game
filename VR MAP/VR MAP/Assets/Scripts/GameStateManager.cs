using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    // Énumération des différents états du jeu
    public enum GameState
    {
        Menu,
        Game,
        GameOver
    }

    [Header("Game State")]
    [SerializeField] private GameState currentState = GameState.Menu;

    [Header("Canvas References")]
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private GameObject gameCanvas;
    [SerializeField] private GameObject gameOverWinCanvas;
    [SerializeField] private GameObject gameOverLoseCanvas;

    [Header("Game Result")]
    private bool hasWon = false;

    private void Start()
    {
        // Initialiser le jeu en mode Menu
        SetState(GameState.Menu);
    }

    private void Update()
    {
        // Gérer les états du jeu
        switch (currentState)
        {
            case GameState.Menu:
                HandleMenuState();
                break;

            case GameState.Game:
                HandleGameState();
                break;

            case GameState.GameOver:
                HandleGameOverState();
                break;
        }
    }

    #region State Handlers

    private void HandleMenuState()
    {
        // Logique du menu (vide pour le moment)
    }

    private void HandleGameState()
    {
        // Logique du jeu (à implémenter plus tard)
    }

    private void HandleGameOverState()
    {
        // Logique du game over (vide pour le moment)
    }

    #endregion

    #region State Transitions

    /// <summary>
    /// Change l'état actuel du jeu
    /// </summary>
    private void SetState(GameState newState)
    {
        currentState = newState;
        UpdateCanvasVisibility();
    }

    /// <summary>
    /// Fonction appelée par le bouton Play du menu
    /// Passe en mode Game
    /// </summary>
    public void StartGame()
    {
        SetState(GameState.Game);
        Debug.Log("Game Started!");
    }

    /// <summary>
    /// Passe en mode Game Over
    /// </summary>
    /// <param name="playerWon">True si le joueur a gagné, False s'il a perdu</param>
    public void TriggerGameOver(bool playerWon)
    {
        hasWon = playerWon;
        SetState(GameState.GameOver);
        Debug.Log(playerWon ? "Player Won!" : "Player Lost!");
    }

    /// <summary>
    /// Fonction pour retourner au menu principal
    /// Appelée depuis le Game Over
    /// </summary>
    public void ReturnToMenu()
    {
        hasWon = false;
        SetState(GameState.Menu);
        Debug.Log("Returned to Menu");
    }

    /// <summary>
    /// Fonction pour rejouer directement
    /// Appelée depuis le Game Over
    /// </summary>
    public void RestartGame()
    {
        hasWon = false;
        SetState(GameState.Game);
        Debug.Log("Game Restarted!");
    }

    #endregion

    #region Canvas Management

    /// <summary>
    /// Met à jour la visibilité des canvas en fonction de l'état actuel
    /// </summary>
    private void UpdateCanvasVisibility()
    {
        // Désactiver tous les canvas
        if (menuCanvas != null)
            menuCanvas.SetActive(false);

        if (gameCanvas != null)
            gameCanvas.SetActive(false);

        if (gameOverWinCanvas != null)
            gameOverWinCanvas.SetActive(false);

        if (gameOverLoseCanvas != null)
            gameOverLoseCanvas.SetActive(false);

        // Activer le bon canvas selon l'état
        switch (currentState)
        {
            case GameState.Menu:
                if (menuCanvas != null)
                    menuCanvas.SetActive(true);
                break;

            case GameState.Game:
                if (gameCanvas != null)
                    gameCanvas.SetActive(true);
                break;

            case GameState.GameOver:
                if (hasWon)
                {
                    if (gameOverWinCanvas != null)
                        gameOverWinCanvas.SetActive(true);
                }
                else
                {
                    if (gameOverLoseCanvas != null)
                        gameOverLoseCanvas.SetActive(true);
                }
                break;
        }
    }

    #endregion

    #region Getters

    /// <summary>
    /// Retourne l'état actuel du jeu
    /// </summary>
    public GameState GetCurrentState()
    {
        return currentState;
    }

    /// <summary>
    /// Vérifie si le joueur est en mode jeu
    /// </summary>
    public bool IsPlaying()
    {
        return currentState == GameState.Game;
    }

    #endregion
}