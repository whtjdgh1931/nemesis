using UnityEngine;
public class TechSelectPackInteractor_Shop : TechSelectPackInteractor, IShopItem
{
    [SerializeField] int _price = 150;
    public int Price => _price;
    public override bool TryInteract(Transform subject)
    {
        if(Purchase())
        {
            return base.TryInteract(subject);
        }
        return false;
    }
    public bool Purchase()
    {
        if (GameManager.Instance.CurrencyManager.TrySpendCredit(_price))
        {
            return true;
        }
        return false;
    }
}