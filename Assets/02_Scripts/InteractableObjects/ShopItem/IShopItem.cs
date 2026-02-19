public interface IShopItem
{
    int Price { get; }
    /// <summary>
    /// 상점에서 아이템을 구매할 때 호출되는 메서드
    /// </summary>
    /// <returns>구매 성공 여부</returns>
    bool Purchase();
}