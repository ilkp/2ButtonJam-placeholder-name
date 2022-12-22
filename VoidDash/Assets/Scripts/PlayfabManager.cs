using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayfabManager : MonoBehaviour
{
	public static PlayfabManager Instance;

	private void Awake()
	{
		if (Instance != null)
			Destroy(gameObject);
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	private void Start()
	{
		Login();
	}

	private void Login()
	{
		var request = new LoginWithCustomIDRequest
		{
			CustomId = SystemInfo.deviceUniqueIdentifier,
			CreateAccount = true
		};
		PlayFabClientAPI.LoginWithCustomID(request, OnSuccess, OnError);
	}

	private void OnSuccess(LoginResult result)
	{
		Debug.Log("Successful login/account create");
	}

	private void OnError(PlayFabError error)
	{
		Debug.Log("Playfab error");
		Debug.Log(error.GenerateErrorReport());
	}

	public void SendLeaderboard(int score)
	{
		var request = new UpdatePlayerStatisticsRequest
		{
			Statistics = new List<StatisticUpdate>
			{
				new StatisticUpdate
				{
					StatisticName = "Highscore",
					Value = score
				}
			}
		};
		PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnError);
	}

	private void OnLeaderboardUpdate(UpdatePlayerStatisticsResult result)
	{
		Debug.Log("Successful leaderboard sent");
	}

	public void GetLeaderboard()
	{
		var request = new GetLeaderboardRequest()
		{
			StatisticName = "Highscore",
			StartPosition = 0,
			MaxResultsCount = 10
		};
		PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
	}

	private void OnLeaderboardGet(GetLeaderboardResult result)
	{
		foreach (var item in result.Leaderboard)
		{
			Debug.Log(item.Position + " " + item.PlayFabId + " " + item.StatValue);
		}
	}
}
