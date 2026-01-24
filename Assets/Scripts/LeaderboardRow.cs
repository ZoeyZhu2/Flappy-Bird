using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardRow : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text usernameText;
    [SerializeField] private TMP_Text scoreText;

    public void SetRow(int rank, string username, int score)
    {
        rankText.text = rank.ToString();
        usernameText.text = username;
        scoreText.text = score.ToString();
    }
}
