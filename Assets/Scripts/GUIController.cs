using UnityEngine;

public class GUIController : MonoBehaviour
{
    [SerializeField] private SparringEnvController envController;
    [SerializeField] private SparringAgent playerAgent;
    [SerializeField] private SparringAgent opponentAgent;  

    private GUIStyle defaultStyle = new GUIStyle();
    private GUIStyle smallDefaultStyle = new GUIStyle();
    private GUIStyle zeroStyle = new GUIStyle();
    private GUIStyle positiveStyle = new GUIStyle();
    private GUIStyle negativeStyle = new GUIStyle();

    void Start()
    {
        //Define GUI styles
        defaultStyle.fontSize = 20;
        defaultStyle.normal.textColor = Color.yellow;

        smallDefaultStyle.fontSize = 16;
        smallDefaultStyle.normal.textColor = Color.white;

        zeroStyle.fontSize = 20;
        zeroStyle.normal.textColor = Color.white;

        positiveStyle.fontSize = 20;
        positiveStyle.normal.textColor = Color.green;

        negativeStyle.fontSize = 20;
        negativeStyle.normal.textColor = Color.red;
    }

    private void OnGUI()
    {
        //Select style based on reward
        GUIStyle rewStylePlayer = envController.PlayerAgentInfo.totalReward > 0 ? positiveStyle :
                                  envController.PlayerAgentInfo.totalReward < 0 ? negativeStyle : zeroStyle;
        GUIStyle rewStyleOpponent = envController.OpponentAgentInfo.totalReward > 0 ? positiveStyle :
                                    envController.OpponentAgentInfo.totalReward < 0 ? negativeStyle : zeroStyle;

        //Display text
        GUI.Label(
            new Rect(Screen.width / 2 - 200, 20, 300, 30),
            $"Episode {envController.episodeCount} - Total Steps: {envController.m_totalSteps}",
            defaultStyle
        );

        GUI.Label(
            new Rect(Screen.width / 2 - 200, 60, 150, 30),
            $"Player Reward: {envController.PlayerAgentInfo.totalReward:F2}",
            rewStylePlayer
        );

        GUI.Label(
            new Rect(Screen.width / 2, 60, 150, 30),
            $" | ",
            zeroStyle
        );

        GUI.Label(
            new Rect(Screen.width / 2 + 20, 60, 150, 30),
            $"Opponent Reward: {envController.OpponentAgentInfo.totalReward:F2}",
            rewStyleOpponent
        );

        GUI.Label(
            new Rect(Screen.width / 2 - 200, 100, 300, 30),
            $"Player Action: {playerAgent.animationController.GetCurrentAnimatorStateName()} | Opponent Action: {opponentAgent.animationController.GetCurrentAnimatorStateName()}",
            smallDefaultStyle
        );
    }

    void Update()
    {

    }
}