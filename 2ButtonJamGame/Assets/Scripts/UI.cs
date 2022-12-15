
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
	[SerializeField] private PlayerStateMachine m_player;
    [SerializeField] private Image[] m_powerupUI;
    [SerializeField] private Sprite m_powerupEmpty;
    [SerializeField] private Sprite m_powerupFilled;

    private void OnEnable()
    {
		m_player.PowerupChargesChanged += OnPowerupChargesChanged;
	}

    private void OnDisable()
	{
		m_player.PowerupChargesChanged -= OnPowerupChargesChanged;
	}

    private void OnPowerupChargesChanged()
	{
		int powerups = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateMachine>().PowerupCharges;
		int i = 0;
		for (; i < powerups; ++i)
			m_powerupUI[i].sprite = m_powerupFilled;
		for (; i < PlayerStateMachine.MAX_POWERUPS; ++i)
			m_powerupUI[i].sprite = m_powerupEmpty;
	}
}
