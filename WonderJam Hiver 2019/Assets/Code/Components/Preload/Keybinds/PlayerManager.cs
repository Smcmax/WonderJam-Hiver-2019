﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour 
{
	[Tooltip("Prefabs matching the player's id to spawn")]
	public List<GameObject> m_playerPrefabs;

	public GameEvent m_gameStartTimerEvent;

	[HideInInspector] public List<Player> m_players;

	void Awake()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnSceneLoaded(Scene p_scene, LoadSceneMode p_mode)
	{
		if(p_scene.name != "DevTest") return;

		m_players = new List<Player>();

		foreach(InputUser user in Game.m_keybinds.GetAllActiveInputUsers())
			AddPlayer(user);
	}

	public bool AddPlayer(InputUser p_user) 
	{ 
		if(m_players.Count == 4) return false;

		int id = 0;

		for(int i = 1; i <= 4; i++)
			if(!m_players.Exists(p => p.m_playerId == i))
			{
				id = i;
				break;
			}

		GameObject playerObj = Instantiate(m_playerPrefabs[id - 1]);
		Player player = playerObj.GetComponent<Player>();

		player.m_input = p_user;
		player.m_playerId = id;

		if(!player.Spawn()) 
		{
			player.Despawn();
			return false;
		} 
		else m_players.Add(player);

		if(m_players.Count == 1)
			m_gameStartTimerEvent.Raise();

		return true;
	}

	public bool RemovePlayer(InputUser p_user) 
	{
		if(m_players.Count == 1) return false;

		Player player = m_players.Find(p => p.m_input.m_profile == p_user.m_profile && p.m_input.m_controllerId == p_user.m_controllerId);

		if(player != null) 
		{
			m_players.Remove(player);
			player.Despawn();

			return true;
		}

		return false;
	}
}
