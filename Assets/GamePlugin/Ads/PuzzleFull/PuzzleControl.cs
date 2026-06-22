using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using mygame.sdk;
using MyJson;

public class PuzzleControl
{
	private static PuzzleControl _instance = null;

	public int levelShow = 25;
	public int dayShow = 2;
	public int sessionShow = 2;
	public int deltaTimeShow = 90;

	private int idxNextShow = 0;
	public int countImp = 0;
	public int cfNextImp = 2;
	private long tShow = 0;

	List<PromoGameOb> listGames = new List<PromoGameOb>();

	PromoGameOb currGame = null;
	private bool isLoading = false;
	private bool isLoaded = false;
	private Action<int> _cbShow = null;

	public static PuzzleControl getInstance()
	{
		if (_instance == null)
		{
			_instance = new PuzzleControl();
			_instance.loadMemList();
		}
		return _instance;
	}
	void loadMemList()
	{
		SdkUtil.logd($"OtherPuzzle loadMemList");
		string memlistpuzzle = PlayerPrefs.GetString("mem_list_puzzle", "");
		var obmempuzzle = (IDictionary<string, object>)JsonDecoder.DecodeText(memlistpuzzle);
		if (obmempuzzle != null && obmempuzzle.ContainsKey("games"))
		{
			List<object> listmempuzzle = (List<object>)obmempuzzle["games"];
			if (listmempuzzle != null)
			{
				for (int kk = 0; kk < listmempuzzle.Count; kk++)
				{
					var gamepuzzle = (IDictionary<string, object>)listmempuzzle[kk];
					PromoGameOb g = PromoGameOb.newFromData(gamepuzzle);
					listGames.Add(g);
				}
			}
			listmempuzzle.Clear();
		}

		levelShow = PlayerPrefs.GetInt("mem_puzzle_level", 25);
		dayShow = PlayerPrefs.GetInt("mem_puzzle_day", 2);
		sessionShow = PlayerPrefs.GetInt("mem_puzzle_session", 2);
		deltaTimeShow = PlayerPrefs.GetInt("mem_puzzle_deltaTime", 90);
		cfNextImp = PlayerPrefs.GetInt("mem_puzzle_nextImp", 2);

		getIdxShow();
	}

	public void nextShow()
	{
		if (countImp >= cfNextImp)
		{
			countImp = 0;
			isLoaded = false;
			idxNextShow++;
			if (idxNextShow >= listGames.Count)
			{
				idxNextShow = 0;
			}
			SdkUtil.logd($"OtherPuzzle nextShow idxNextShow={idxNextShow}");
			load4Show();
		}
	}

	public void load4Show()
	{
		if (!isLoading && !isLoaded)
		{
			if (listGames.Count > 0)
			{
				if (idxNextShow < 0)
				{
					getIdxShow();
				}
				else if (idxNextShow >= listGames.Count)
				{
					idxNextShow = 0;
				}
				SdkUtil.logd($"OtherPuzzle load4Show idxNextShow={idxNextShow}");
				currGame = listGames[idxNextShow];
				if (currGame != null)
				{
					isLoading = true;
					ImageLoader.loadImageTexture(currGame.getImg(0), 100, 100, (tt) =>
					{
						if (tt != null)
						{
							isLoaded = true;
							isLoading = false;
							SdkUtil.logd($"OtherPuzzle load4Show load icon success");
						}
					});
				}
			}
			else
			{
				SdkUtil.logd($"OtherPuzzle load4Show list game is empty");
			}
		}
		else
		{
			SdkUtil.logd($"OtherPuzzle load4Show isLoading={isLoading} isLoaded={isLoaded}");
		}
	}

	private bool isShowOtherPuzzle()
	{
		if (SDKManager.Instance != null)
		{
			int lvCurr = GameRes.GetLevel(Level_type.Common);
			if (lvCurr < levelShow)
			{
				SdkUtil.logd($"OtherPuzzle isShowOtherPuzzle level:{lvCurr} levelShow:{levelShow}");
				return false;
			}
			if (SDKManager.Instance.counSessionGame < sessionShow)
			{
				SdkUtil.logd($"OtherPuzzle isShowOtherPuzzle session:{SDKManager.Instance.counSessionGame} sessionShow:{sessionShow}");
				return false;
			}
			if (SDKManager.Instance.activeDay < dayShow)
			{
				SdkUtil.logd($"OtherPuzzle isShowOtherPuzzle day:{SDKManager.Instance.activeDay} dayShow:{dayShow}");
				return false;
			}
			long tcurr = SdkUtil.CurrentTimeMilis();
			if (tcurr - tShow < deltaTimeShow * 1000)
			{
				SdkUtil.logd($"OtherPuzzle isShowOtherPuzzle deltaTime:{tcurr - tShow} deltaTimeShow:{deltaTimeShow * 1000}");
				return false;
			}
			return true;
		}
		return false;
	}

	public bool showGame(string placement, Action<int> cbShow)
	{
		if (isShowOtherPuzzle())
		{
			if (!placement.StartsWith("otherpz_"))
			{
				placement = "otherpz_" + placement;
			}
			var cffull = AdsHelper.Instance.getCfAdsPlacement(placement, 1);
			if (cffull != null)
			{
				if (cffull.flagShow <= 0)
				{
					SdkUtil.logd($"OtherPuzzle showGame is disable ads with placement:{placement}");
					return false;
				}
			}
			if (SDKManager.Instance != null)
			{
				if (isGameLoaded())
				{
					SdkUtil.logd($"OtherPuzzle showGame:{currGame.name}");
					tShow = SdkUtil.CurrentTimeMilis();
					_cbShow = cbShow;
					SDKManager.Instance.showOtherPuzzle(currGame);
					return true;
				}
				else
				{
					SdkUtil.logd($"OtherPuzzle showGame not load");
					load4Show();
					return false;
				}
			}
			else
			{
				SdkUtil.logd($"OtherPuzzle showGame SDKManager.Instance null");
				return false;
			}
		}
		else
		{
			SdkUtil.logd($"OtherPuzzle showGame not met condition show");
			return false;
		}
	}

	private bool isGameLoaded()
	{
		if (isLoaded && currGame != null)
		{
			string pathimg = ImageLoader.url2nameData(currGame.getImg(0), 1);
			if (File.Exists(DownLoadUtil.pathCache() + "/" + pathimg))
			{
				return true;
			}
		}
		isLoaded = false;
		return false;
	}

	public void onPuzzleImpression()
	{
		countImp++;
		SdkUtil.logd($"OtherPuzzle onPuzzleImpression countImp={countImp}");
	}

	public void onPuzzleClick()
	{
		SdkUtil.logd($"OtherPuzzle onPuzzleClick");
		countImp = cfNextImp;
	}

	public void onPuzzleClose()
	{
		SdkUtil.logd($"OtherPuzzle onPuzzleClose");
		tShow = SdkUtil.CurrentTimeMilis();
		nextShow();
		if (_cbShow != null)
		{
			_cbShow(1);
			_cbShow = null;
		}
	}

	public bool checkNewGameAndRemoveExist(PromoGameOb newGame)
	{
		for (int kk = 0; kk < listGames.Count; kk++)
		{
			if (listGames[kk].pkg.Equals(newGame.pkg))
			{
				listGames.RemoveAt(kk);
				return false;
			}
		}
		return true;
	}

	public void updateListGames(List<PromoGameOb> listnewgame)
	{
		listGames.InsertRange(0, listnewgame);
	}

	public void saveListGame()
	{
		string memlistpuzzle = "{\"games\":[";
		for (int kk = 0; kk < listGames.Count; kk++)
		{
			string itemgameData = listGames[kk].toJsonData();
			if (kk != 0)
			{
				memlistpuzzle += $",{itemgameData}";
			}
			else
			{
				memlistpuzzle += itemgameData;
			}
		}
		memlistpuzzle += "]}";
		PlayerPrefs.SetString("mem_list_puzzle", memlistpuzzle);
	}

	private void getIdxShow()
	{
		idxNextShow = PlayerPrefs.GetInt("mem_puzzle_will_show", -1);
		if (idxNextShow < 0 && listGames.Count > 0)
		{
			isLoaded = false;
			isLoading = false;
			idxNextShow = new System.Random().Next(listGames.Count);
			PlayerPrefs.SetInt("mem_puzzle_will_show", idxNextShow);
		}
		SdkUtil.logd($"OtherPuzzle getIdxShow idxNextShow={idxNextShow}");
	}

	public void resetShow()
	{
		SdkUtil.logd($"OtherPuzzle resetShow");
		if (listGames.Count > 0)
		{
			idxNextShow = new System.Random().Next(listGames.Count);
		}
		else
		{
			idxNextShow = -1;
		}
		PlayerPrefs.SetInt("mem_puzzle_will_show", idxNextShow);
		countImp = cfNextImp + 1;
		nextShow();
	}

	public void saveCondition()
	{
		PlayerPrefs.SetInt("mem_puzzle_level", levelShow);
		PlayerPrefs.SetInt("mem_puzzle_day", dayShow);
		PlayerPrefs.SetInt("mem_puzzle_session", sessionShow);
		PlayerPrefs.SetInt("mem_puzzle_deltaTime", deltaTimeShow);
		PlayerPrefs.SetInt("mem_puzzle_nextImp", cfNextImp);
	}
}