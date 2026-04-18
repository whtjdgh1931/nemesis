using System;
using System.Collections.Generic;

/// <summary>
/// 각 방의 런타임 정보를 담는 클래스
/// </summary>
[Serializable]
public class RoomInfo
{       
    public RoomType RoomType;                          // 방 타입
    public NormalRoomType? NormalType;             // 일반 방 타입(일반 방이 아닐 경우 null)
    public TechSelectPackType? TechType;     // 일반 방의 보상이 기술팩 타입일 경우 해당 기술팩 타입(기술팩이 아닐 경우 null)

    // 로컬라이즈된 문자열 대신 "키"를 보관/생성
    Dictionary<RoomType, string> _roomTitleKey = new Dictionary<RoomType, string>()
    {
        { RoomType.Start, "_roomTitle_Start" },
        { RoomType.Normal, "_roomTitle_Normal" },
        { RoomType.Shop, "_roomTitle_Shop" },
        { RoomType.Colosseum, "_roomTitle_Colosseum" },
        { RoomType.Lab, "_roomTitle_Lab" },
        { RoomType.Boss, "_roomTitle_Boss" },
    };

    Dictionary<RoomType, string> _roomDescriptionKey = new Dictionary<RoomType, string>()
    {
        { RoomType.Start, "_roomDescription_Start" },
        { RoomType.Normal, "_roomDescription_Normal" },
        { RoomType.Shop, "_roomDescription_Shop" },
        { RoomType.Colosseum, "_roomDescription_Colosseum" },
        { RoomType.Lab, "_roomDescription_Lab" },
        { RoomType.Boss, "_roomDescription_Boss" },
    };

    public RoomInfo(RoomType roomType, NormalRoomType? normalRoomType = null, TechSelectPackType? techSelectPackType = null)
    {
        RoomType = roomType;
        NormalType = normalRoomType;
        TechType = techSelectPackType;
    }

    public bool TryGetTechSelect(out TechSelectPackType value)
    {
        if (RoomType == RoomType.Normal &&
            NormalType.HasValue &&
            NormalType.Value == NormalRoomType.TechSelect &&
            TechType.HasValue)
        {
            value = TechType.Value;
            return true;
        }
        value = default;
        return false;
    }

    public bool TryGetNormal(out NormalRoomType value)
    {
        if (RoomType == RoomType.Normal && NormalType.HasValue)
        {
            value = NormalType.Value;
            return true;
        }
        value = default;
        return false;
    }

    public string GetTitleKey()
    {
        if (_roomTitleKey.TryGetValue(RoomType, out var key)) return key;
        return "_roomTitle_Unknown";
    }

    public string GetDescriptionKey()
    {
        if (_roomDescriptionKey.TryGetValue(RoomType, out var key)) return key;
        return "_roomDescription_Unknown";
    }
}