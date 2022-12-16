
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
	[SerializeField] private PlayerStateMachine m_player;
    [SerializeField] private Image[] m_powerupUI;
	[SerializeField] private Image[] m_hpUI;
	[SerializeField] private Sprite m_powerupEmpty;
    [SerializeField] private Sprite m_powerupFilled;
	[SerializeField] private Sprite m_hpEmpty;
	[SerializeField] private Sprite m_hpFilled;

	private void OnEnable()
    {
		m_player.UIChanged += OnPowerupChargesChanged;
	}

    private void OnDisable()
	{
		m_player.UIChanged -= OnPowerupChargesChanged;
	}

    private void OnPowerupChargesChanged()
	{
		int powerups = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateMachine>().PowerupCharges;
		int i = 0;
		for (; i < powerups; ++i)
			m_powerupUI[i].sprite = m_powerupFilled;
		for (; i < PlayerStateMachine.MAX_POWERUPS; ++i)
			m_powerupUI[i].sprite = m_powerupEmpty;

		int hp = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStateMachine>().Hp;
		i = 0;
		for (; i < hp; ++i)
			m_hpUI[i].sprite = m_hpFilled;
		for (; i < PlayerStateMachine.MAX_HP; ++i)
			m_hpUI[i].sprite = m_hpEmpty;
	}
}
