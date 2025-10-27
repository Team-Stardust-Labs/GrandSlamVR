using UnityEngine;

/**
    Respawns the Ball if all black buttons are pressed simultaneously
*/
public class ButtonComboRespawnBall : ButtonCombo
{

    private BallScoring m_ball_scoring;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_ball_scoring = GetComponent<BallScoring>();
    }

    protected override void TriggerEvent()
    {
        if (m_ball_scoring)
        {
            m_ball_scoring.RespawnButtonCode();
        }
    }
}
