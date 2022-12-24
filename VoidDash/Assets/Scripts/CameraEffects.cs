
using System.Collections;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
	private const float PLAYER_FOLLOW_STREGTH = 0.2f;
	private const float DEFAULT_Z = -10f;
	private readonly Vector3 DEFAULT_POS = new Vector3(0f, 0f, DEFAULT_Z);
	private Transform m_cameraTransform;
	private Vector3 m_cameraShakeTranslate = Vector3.zero;
	private Coroutine m_cameraShakeCoroutine;

	private void Start()
	{
		m_cameraTransform = Camera.main.transform;
	}

	private void Update()
	{
		Vector3 totalTranslate = DEFAULT_POS;
		//totalTranslate += PLAYER_FOLLOW_STREGTH * GameObject.FindGameObjectWithTag("Player").transform.position;
		totalTranslate += m_cameraShakeTranslate;
		m_cameraTransform.position = totalTranslate;
	}

	public void StartPlayerHitShake()
	{
		if (m_cameraShakeCoroutine != null)
			StopCoroutine(m_cameraShakeCoroutine);
		StartCoroutine(CameraShake());
	}
	 
	private IEnumerator CameraShake()
	{
		float cameraSpeed = 40f;
		m_cameraShakeTranslate = Vector3.zero;
		Vector3[] points = new Vector3[6];
		for (int i = 0; i < points.Length - 1; ++i)
			points[i] = new Vector3(Random.Range(-1f, 1f), Random.Range(-0.25f, 0.25f), 0f);
		points[points.Length - 1] = Vector3.zero;
		for (int i = 0; i < points.Length; ++i)
		{
			Vector3 direction = (points[i] - m_cameraShakeTranslate).normalized;
			while ((points[i] - m_cameraShakeTranslate).magnitude > 0.01f)
			{
				Vector3 translate = Time.deltaTime * cameraSpeed * direction;
				Vector3 cameraToPoint = points[i] - m_cameraShakeTranslate;
				if (cameraToPoint.magnitude < translate.magnitude)
					m_cameraShakeTranslate = points[i];
				else
					m_cameraShakeTranslate += translate;
				yield return null;
			}
		}
		m_cameraShakeTranslate = Vector3.zero;
	}
}
