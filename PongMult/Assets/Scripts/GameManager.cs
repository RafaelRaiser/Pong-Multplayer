using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-1)]
public class GameManager : MonoBehaviour
{
    [SerializeField] private Ball ball;
    [SerializeField] private Paddle player1Paddle;
    [SerializeField] private Paddle player2Paddle;
    [SerializeField] private Text player1ScoreText;
    [SerializeField] private Text player2ScoreText;

    private int player1Score;
    private int player2Score;

    private void Start()
    {
        NewGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            NewGame();
        }
    }

    public void NewGame()
    {
        SetPlayer1Score(0);
        SetPlayer2Score(0);
        NewRound();
    }

    public void NewRound()
    {
        player1Paddle.ResetPosition();
        player2Paddle.ResetPosition();
        ball.ResetPosition();

        CancelInvoke();
        Invoke(nameof(StartRound), 1f);
    }

    private void StartRound()
    {
        ball.AddStartingForce();
    }

    public void OnPlayer1Scored()
    {
        SetPlayer1Score(player1Score + 1);
        NewRound();
    }

    public void OnPlayer2Scored()
    {
        SetPlayer2Score(player2Score + 1);
        NewRound();
    }

    private void SetPlayer1Score(int score)
    {
        player1Score = score;
        player1ScoreText.text = score.ToString();
    }

    private void SetPlayer2Score(int score)
    {
        player2Score = score;
        player2ScoreText.text = score.ToString();
    }
}
