using System;
using System.Globalization;
using TMPro;
using UnityEngine;

public class RecordPrefab : MonoBehaviour
{
		[SerializeField] TextMeshProUGUI _rankingText;
		[SerializeField] TextMeshProUGUI _userNameText;
		[SerializeField] TextMeshProUGUI _TimeRecordText;
		[SerializeField] TextMeshProUGUI _TimeText;

		public void SetData(int rank, string userName, float timeRecord, float timeUnixMilliseconds)
		{
				_rankingText.text = rank.ToString();
				_userNameText.text = userName;
				_TimeRecordText.text = FormatTime(timeRecord);
				_TimeText.text = FormatUnixTimeLocalized((long)timeUnixMilliseconds);
		}

		private string FormatTime(float timeInSeconds)
		{
				int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
				int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
				

				return $"{minutes:D2}:{seconds:D2}";
		}

		private string FormatUnixTimeLocalized(long unixMilliseconds)
		{
				DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
				DateTime dateTime = epoch.AddMilliseconds(unixMilliseconds).ToLocalTime();

				string language = Application.systemLanguage.ToString();

				if (language == Constants.STRING_Korean)
				{
						return dateTime.ToString("yyyy년 MM월 dd일 HH시 mm분 ss초", new CultureInfo("ko-KR"));
				}
				else
				{
						return dateTime.ToString("yyyy-MM-dd HH:mm:ss", new CultureInfo("en-US"));
				}
		}
}
