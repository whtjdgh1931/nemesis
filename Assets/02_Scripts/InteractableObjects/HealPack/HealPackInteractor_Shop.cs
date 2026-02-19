using UnityEngine;

public class HealPackInteractor_Shop : HealPackInteractor, IShopItem
{
    [SerializeField] int _price = 50;
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
        if(GameManager.Instance.CurrencyManager.TrySpendCredit(_price))
        {
            return true;
        }

        return false;
    }
}