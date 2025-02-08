﻿using System.Collections.Generic;
using UnityEngine;

#if MODULE_IAP
using UnityEngine.Purchasing;
#endif

namespace Watermelon
{
    [HelpURL("https://docs.google.com/document/d/1GlS55aF4z4Ddn4a1QCu5h0152PoOb29Iy4y9RKZ9Y9Y")]
    public static class IAPManager
    {
        private static Dictionary<ProductKeyType, IAPItem> productsTypeToProductLink = new Dictionary<ProductKeyType, IAPItem>();
        private static Dictionary<string, IAPItem> productsKeyToProductLink = new Dictionary<string, IAPItem>();

        private static bool isInitialised = false;
        public static bool IsInitialised => isInitialised;

        private static BaseIAPWrapper wrapper;

        public static event SimpleCallback OnPurchaseModuleInitted;
        public static event ProductCallback OnPurchaseComplete;
        public static event ProductFailCallback OnPurchaseFailded;

        public static void Initialise(GameObject initObject, IAPSettings settings)
        {
            if(isInitialised)
            {
                Debug.Log("[IAP Manager]: Module is already initialized!");
                return;
            }

            IAPItem[] items = settings.StoreItems;
            for (int i = 0; i < items.Length; i++)
            {
                productsTypeToProductLink.Add(items[i].ProductKeyType, items[i]);
                productsKeyToProductLink.Add(items[i].ID, items[i]);
            }

#if MODULE_IAP
            wrapper = new IAPWrapper();
#else
            wrapper = new DummyIAPWrapper();
#endif

            wrapper.Initialise(settings);
        }

        public static IAPItem GetIAPItem(ProductKeyType productKeyType)
        {
            if (productsTypeToProductLink.ContainsKey(productKeyType))
                return productsTypeToProductLink[productKeyType];

            return null;
        }

        public static IAPItem GetIAPItem(string ID)
        {
            if (productsKeyToProductLink.ContainsKey(ID))
                return productsKeyToProductLink[ID];

            return null;
        }

#if MODULE_IAP
        public static Product GetProduct(ProductKeyType productKeyType)
        {
            IAPItem iapItem = GetIAPItem(productKeyType);
            if (iapItem != null)
            {
                return IAPWrapper.Controller.products.WithID(iapItem.ID);
            }

            return null;
        }
#endif

        public static void RestorePurchases()
        {
            wrapper.RestorePurchases();
        }

        public static void SubscribeOnPurchaseModuleInitted(SimpleCallback callback)
        {
            if (isInitialised)
                callback?.Invoke();
            else
                OnPurchaseModuleInitted += callback;
        }

        public static void BuyProduct(ProductKeyType productKeyType)
        {
            wrapper.BuyProduct(productKeyType);
        }

        public static ProductData GetProductData(ProductKeyType productKeyType)
        {
            return wrapper.GetProductData(productKeyType);
        }

        public static bool IsSubscribed(ProductKeyType productKeyType)
        {
            return wrapper.IsSubscribed(productKeyType);
        }

        public static string GetProductLocalPriceString(ProductKeyType productKeyType)
        {
            ProductData product = GetProductData(productKeyType);

            if (product == null)
                return string.Empty;

            return string.Format("{0} {1}", product.ISOCurrencyCode, product.Price);
        }

        public static void OnModuleInitialised()
        {
            isInitialised = true;

            OnPurchaseModuleInitted?.Invoke();

            Debug.Log("[IAPManager]: Module is initialized!");
        }

        public static void OnPurchaseCompled(ProductKeyType productKey)
        {
            OnPurchaseComplete?.Invoke(productKey);
        }

        public static void OnPurchaseFailed(ProductKeyType productKey, Watermelon.PurchaseFailureReason failureReason)
        {
            OnPurchaseFailded?.Invoke(productKey, failureReason);
        }

        public delegate void ProductCallback(ProductKeyType productKeyType);
        public delegate void ProductFailCallback(ProductKeyType productKeyType, Watermelon.PurchaseFailureReason failureReason);
    }

    public class ProductData
    {
        public ProductType ProductType { get; }
        public bool IsPurchased { get; }

        public decimal Price { get; }
        public string ISOCurrencyCode { get; } 

        public bool IsSubscribed { get; }

#if MODULE_IAP
        public Product Product { get; }
#endif

        public ProductData(ProductType productType)
        {
            ProductType = productType;

            Price = 0.00m;
            ISOCurrencyCode = "USD";

            IsPurchased = false;

            IsSubscribed = false;
        }

#if MODULE_IAP
        public ProductData(Product product)
        {
            Product = product;

            ProductType = (ProductType)product.definition.type;

            IsPurchased = product.hasReceipt;

            Price = product.metadata.localizedPrice;
            ISOCurrencyCode = product.metadata.isoCurrencyCode;
        }
#endif
    }

    public enum PurchaseFailureReason
    {
        PurchasingUnavailable = 0,
        ExistingPurchasePending = 1,
        ProductUnavailable = 2,
        SignatureInvalid = 3,
        UserCancelled = 4,
        PaymentDeclined = 5,
        DuplicateTransaction = 6,
        Unknown = 7
    }

    public enum ProductType
    {
        Consumable = 0,
        NonConsumable = 1,
        Subscription = 2
    }
}

// -----------------
// IAP Manager v 1.2.2
// -----------------

// Changelog
// v 1.2.2
// • Fixed serialization bug
// v 1.2.1
// • Added test mode
// v 1.2
// • Support of IAP version 4.11.0
// • Added Editor purchase wrapper
// v 1.1
// • Support of IAP version 4.9.3
// v 1.0.3
// • Support of IAP version 4.7.0
// v 1.0.2
// • Added quick access to the local price of IAP via GetProductLocalPriceString method
// v 1.0.1
// • Added restoration status messages
// v 1.0.0
// • Documentation added
// v 0.4
// • IAPStoreListener inheriting from MonoBehaviour
// v 0.3
// • Editor style update
// v 0.2
// • IAPManager structure changed
// • Enums from UnityEditor.Purchasing has duplicated to prevent serialization problems
// v 0.1
// • Added basic version
