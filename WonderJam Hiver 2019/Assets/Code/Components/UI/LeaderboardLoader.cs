﻿using UnityEngine;
using System.Collections.Generic;

public class LeaderboardLoader : MonoBehaviour
{
	[Tooltip("The leaderboard spot's prefab")]
	public GameObject m_spotPrefab;

	[Tooltip("The leaderboard spot's editable prefab")]
	public GameObject m_editableSpotPrefab;

	[Tooltip("Should leaderboards be pulled locally or from the server?")]
	public bool m_local;

	[Tooltip("The IP address to pull the leaderboards from")]
	[ConditionalField("m_local", "false")]
	public string m_serverIPAddress;

	[Tooltip("The port used to pull the leaderboards from the server")]
	[ConditionalField("m_local", "false")]
	public int m_serverPort;

	[Tooltip("The amount of scores to load in the leaderboard (maximum)")]
	public int m_loadedAmount;

	private List<GameObject> m_instantiatedObjects;

	void Start()
	{
		Load();
	}

	public void Load()
	{
		if(m_instantiatedObjects != null && m_instantiatedObjects.Count > 0)
			return;

		m_instantiatedObjects = new List<GameObject>();

		List<LeaderboardScore> scores = m_local ? GetLocalScores() : GetOnlineScores();
		LeaderboardScore pending = GetPendingScore(m_local);

		if(pending != null) scores.Add(pending);

		scores.Sort(new System.Comparison<LeaderboardScore>((LeaderboardScore first, LeaderboardScore second) => { return second.Score - first.Score; }));

		for(int i = 1; i <= m_loadedAmount; i++) 
		{ 
			if(scores.Count < i) break;

			GameObject spotObj;

			if(scores[i - 1].Name.Length == 0) 
				spotObj = Instantiate(m_editableSpotPrefab, transform);
			else spotObj = Instantiate(m_spotPrefab, transform);

			m_instantiatedObjects.Add(spotObj);
			LeaderboardSpot spot = spotObj.GetComponent<LeaderboardSpot>();

			spot.m_local = m_local;
			spot.m_loader = this;
			spot.Set(i, scores[i - 1]);
		}
	}

	public List<LeaderboardScore> GetLocalScores() 
	{
		List<LeaderboardScore> scores = new List<LeaderboardScore>();
		int loaded = 0;
		System.IO.StreamReader Reader = new System.IO.StreamReader(Application.dataPath + "/Data/LocalLeaderboard.JSON");

		for(string json = Reader.ReadLine(); json != null; json = Reader.ReadLine())
		{
			if(loaded >= m_loadedAmount) break;

			scores.Add(JsonUtility.FromJson<LeaderboardScore>(json));
			loaded++;
		}

		Reader.Close();

		return scores;
	}

	public List<LeaderboardScore> GetOnlineScores() 
	{
		List<LeaderboardScore> scores = new List<LeaderboardScore>();

		// request

		return scores;
	}

	public void AddScore(LeaderboardScore p_score, bool p_local) 
	{
		System.IO.StreamWriter Writer = new System.IO.StreamWriter(Application.dataPath + "/Data/" + (p_local ? "Local" : "Online") + "Leaderboard.JSON", true);

		Writer.Write("\n" + JsonUtility.ToJson(p_score));
		Writer.Close();

		// if !p_local, upload online

		DeletePendingScore(p_local);
	}

	public LeaderboardScore GetPendingScore(bool p_local) 
	{
		System.IO.StreamReader Reader = new System.IO.StreamReader(Application.dataPath + "/Data/Pending" + (p_local ? "Local" : "Online") + "Scores.JSON");
		LeaderboardScore score = JsonUtility.FromJson<LeaderboardScore>(Reader.ReadLine());

		Reader.Close();
		return score;
	}

	public void DeletePendingScore(bool p_local) 
	{ 
		System.IO.StreamWriter Writer = new System.IO.StreamWriter(Application.dataPath + "/Data/Pending" + (p_local ? "Local" : "Online") + "Scores.JSON", false);

		Writer.Write("");
		Writer.Close();
	}
}

[System.Serializable]
public class LeaderboardScore
{
	public string Name;
	public int Score;

	public LeaderboardScore(string p_name, int p_score) 
	{ 
		Name = p_name;
		Score = p_score;
	}
}