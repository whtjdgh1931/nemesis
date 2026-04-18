using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

/// <summary>
/// 스킬 툴팁 정보 클래스
/// </summary>
public class SkillTooltipData 
{
		// 키워드
		private string _keyword;
		public string keyword { get { return _keyword; } } 

		// 영문 키워드
		private string _keywordEN;
		public string keywordEN { get { return _keywordEN; } }

		// PC 설명
		private string _PCScript;
		public string PCScript { get { return _PCScript; } }

		// 영문 PC 설명
		private string _PCScriptEN;
		public string PCScriptEN { get {return _PCScriptEN; } }

		// 모바일 설명
		private string _mobileScript;
		public string mobileScript {  get { return _mobileScript; } }

		// 영문 모바일 설명
		private string _mobileScriptEN;
		public string mobileScriptEN { get { return _mobileScriptEN; } }


		public SkillTooltipData(SkillTooltipJsonData data)
		{
				_keyword = data.Keyword;
				_keywordEN = data.KeywordEN;
				_PCScript = data.PCScript;
				_PCScriptEN = data.PCScriptEN;
				_mobileScript = data.MobileScript;
				_mobileScriptEN = data.MobileScriptEn;
		}
}

public class SkillTooltipJsonData
{
    [JsonProperty("키워드")]
    public string Keyword;

		[JsonProperty("영문키워드")]
		public string KeywordEN;

		[JsonProperty("PC설명")]
		public string PCScript;

		[JsonProperty("영문PC설명")]
		public string PCScriptEN;

		[JsonProperty("모바일설명")]
		public string MobileScript;

		[JsonProperty("영문모바일설명")]
		public string MobileScriptEn;
}