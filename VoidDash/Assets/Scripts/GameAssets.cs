
using UnityEngine;

[DisallowMultipleComponent]
public class GameAssets : MonoBehaviour
{
	public static GameAssets Instance;

	[Header("Audio")]
	[Header("Player")]
	[SerializeField] private AudioClip[] m_sound_playerDeath;
	public AudioClip[] sound_playerDeath { get { return m_sound_playerDeath; } set { m_sound_playerDeath = value; } }

	[SerializeField] private AudioClip[] m_sound_dash;
	public AudioClip[] sound_dash { get { return m_sound_dash; } set { m_sound_dash = value; } }

	[SerializeField] private AudioClip[] m_sound_turn;
	public AudioClip[] sound_turn { get { return m_sound_turn; } set { m_sound_turn = value; } }

	[SerializeField] private AudioClip[] m_sound_getLance;
	public AudioClip[] sound_getLance { get { return m_sound_getLance; } set { m_sound_getLance = value; } }

	[SerializeField] private AudioClip[] m_sound_playerHit;
	public AudioClip[] sound_playerHit { get { return m_sound_playerHit; } set { m_sound_playerHit = value; } }

	[Header("Pickups")]
	[SerializeField] private AudioClip[] m_sound_pickupScore;
	public AudioClip[] sound_pickupScore { get { return m_sound_pickupScore; } set { m_sound_pickupScore = value; } }

	[SerializeField] private AudioClip[] m_sound_pickupCharge;
	public AudioClip[] sound_pickupCharge { get { return m_sound_pickupCharge; } set { m_sound_pickupCharge = value; } }

	[SerializeField] private AudioClip[] m_sound_pickupPowerup;
	public AudioClip[] sound_pickupPowerup { get { return m_sound_pickupPowerup; } set { m_sound_pickupPowerup = value; } }

	[Header("Effects")]
	[SerializeField] private AudioClip[] m_sound_select;
	public AudioClip[] sound_select { get { return m_sound_select; } set { m_sound_select = value; } }

	[SerializeField] private AudioClip[] m_sound_explosion;
	public AudioClip[] sound_explosion { get { return m_sound_explosion; } set { m_sound_explosion = value; } }

	[Header("Sprites")]
	[Header("Player")]
	[SerializeField] private Sprite[] m_sprite_player;
	public Sprite[] sprite_player { get { return m_sprite_player; } set { m_sprite_player = value; } }

	[SerializeField] private Sprite[] m_sprite_lance;
	public Sprite[] sprite_lance { get { return m_sprite_lance; } set { m_sprite_lance = value; } }

	[SerializeField] private Sprite[] m_sprite_burst;
	public Sprite[] sprite_burst { get { return m_sprite_burst; } set { m_sprite_burst = value; } }

	[Header("Enemies")]
	[SerializeField] private Sprite[] m_sprite_chaser;
	public Sprite[] sprite_chaser { get { return m_sprite_chaser; } set { m_sprite_chaser = value; } }

	[SerializeField] private Sprite[] m_sprite_rotator;
	public Sprite[] sprite_rotator { get { return m_sprite_rotator; } set { m_sprite_rotator = value; } }

	[Header("Effects")]
	[SerializeField] private Sprite[] m_sprite_sparkle;
	public Sprite[] sprite_sparkle { get { return m_sprite_sparkle; } set { m_sprite_sparkle = value; } }

	[SerializeField] private Sprite[] m_sprite_explosion;
	public Sprite[] sprite_explosion { get { return m_sprite_explosion; } set { m_sprite_explosion = value; } }

	private void Awake()
	{
		if (Instance != null)
			Destroy(gameObject);
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}
}
