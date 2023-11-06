public class InAppPurchasesManager : MonoBehaviour
{
    public static InAppPurchasesManager Instance;
    [SerializeField] private SKUResponse _receivedSKUs;
    [SerializeField] private List<FetchedProduct> _fetchedProducts = new List<FetchedProduct>();
    [SerializeField] private List<UserPurchase> _purchasedProducts = new List<UserPurchase>();
    [SerializeField] private MembershipController _membershipController;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Logger.LogWarning("More than one instance of InAppPurchasesManager found!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        _membershipController = GetComponent<MembershipController>();
        
        Core.AsyncInitialize();
        Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);

    }
    
    private void EntitlementCallback(Message message)
    {
        if (message.IsError)
        {
            Logger.LogError("Oculus Platform entitlement error: " + message.GetError());
        }
    }

    #region IAP Methods
    
    /// <summary>
    /// Sends a request to the Cloud to receive a list of available products with their SKUs, on success sends a request with
    /// received SKUs to the AppLab
    /// </summary>
    public void FetchAvailableProducts()
    {
        string url = ApiCalls.FetchOculusSKUs();

        NetworkManager.Instance.Request<SKUResponse>(NetworkManager.RequestMethod.GET, url, delegate(object response)
        {
            _membershipController.SetUtilityMessage("Fetch Oculus SKUs: SUCCESS");
            _receivedSKUs = (SKUResponse) response;
            IAP.GetProductsBySKU(_receivedSKUs.oculus_skus).OnComplete(GetProductsBySKUCallback);
                
        }, delegate(bool networkError, int responseCode, string response)
        {
            _membershipController.SetUtilityMessage("Fetch Oculus SKUs: FAIL");
        },null);
    }

    /// <summary>
    /// Sends a request to the AppLab to receive a list of products purchased by user
    /// </summary>
    public void FetchPurchasedProducts()
    {
        IAP.GetViewerPurchases().OnComplete(GetViewerPurchasesCallback);
    }
    
    /// <summary>
    /// Sends a request to the AppLab to launch a checkout workflow for a product with SKU passed as a parameter
    /// </summary>
    /// <param name="sku">SKU of the product that should be purchased</param>
    public void PurchaseProduct(string sku)
    {
        _membershipController.SetUtilityMessage("");
        
        if(_purchasedProducts.Exists(p => p.SKU == sku))
        {
            Logger.Log("Trying to purchase an already purchased item");
            _membershipController.SetUtilityMessage("Trying to purchase an already purchased item");
            _membershipController.SetPurchaseButtonsStatus(true);
            return;
        }
        IAP.LaunchCheckoutFlow(sku).OnComplete(LaunchCheckoutFlowCallback);
    }
    #endregion
    
    #region IAP Callbacks
    
    /// <summary>
    /// Fills a list of available products received from the AppLab
    /// </summary>
    /// <param name="message"></param>
    private void GetProductsBySKUCallback(Message<ProductList> message)
    {
        _fetchedProducts.Clear();
        if (message.IsError)
        {
            Logger.LogWarning(message.GetError().Message);
            return;
        }

        foreach (Oculus.Platform.Models.Product p in message.GetProductList())
        {
            FetchedProduct product = new FetchedProduct(p.Sku, p.Name, p.FormattedPrice, p.Description);
            _fetchedProducts.Add(product);
            _membershipController.SpawnSubscriptionCard(product);
            _membershipController.SetFetchedPrice(p.Sku, p.FormattedPrice);
        }
        _membershipController.OnFetchAvailableProductsComplete();
    }
    
    /// <summary>
    /// Fills a list of products purchased by user
    /// </summary>
    /// <param name="message"></param>
    private void GetViewerPurchasesCallback(Message<PurchaseList> message)
    {
        _purchasedProducts.Clear();
        if (message.IsError)
        {
            _membershipController.SetUtilityMessage("Failed to get purchased products");
            Logger.Log(message.GetError().Message);
            return;
        }
        if (message.GetPurchaseList().Count == 0)
        {
            _membershipController.SetUtilityMessage("Empty purchased products list");
            _membershipController.OnFetchPurchasedProductsComplete(true);
            Logger.Log("Empty purchase list!");
        }
        else
        {
            foreach (Purchase p in message.GetPurchaseList())
            {
                _purchasedProducts.Add(new UserPurchase(p.Sku, p.ID.ToString(), p.GrantTime, p.ExpirationTime));
            }
            _membershipController.SetUtilityMessage("User has unredeemed license");
            _membershipController.RecentUserPurchase = _purchasedProducts[0];
            _membershipController.OnFetchPurchasedProductsComplete(false);
        }
    }

    /// <summary>
    /// Handles system status after the user has ended a purchase process (a system overlapping window with purchase info)
    /// </summary>
    /// <param name="message"></param>
    private void LaunchCheckoutFlowCallback(Message<Purchase> message)
    {
        if (message.IsError)
        {
            _membershipController.SetUtilityMessage("Failed to purchase an item");
            _membershipController.SetPurchaseButtonsStatus(true);
            Logger.Log(message.GetError().Message);
            return;
        }
        
        Purchase p = message.GetPurchase();
        Logger.Log("User has purchased " + p.Sku);
        _membershipController.SetUtilityMessage("User has purchased " + p.Sku);
        UserPurchase purchase = new UserPurchase(p.Sku, p.ID.ToString(), p.GrantTime, p.ExpirationTime);
        OnUserPurchase(purchase);
    }
    /// <summary>
    /// Handles UI and starts license activation flow after user has purchased a product
    /// </summary>
    /// <param name="purchase"></param>
    private void OnUserPurchase(UserPurchase purchase)
    {
        _membershipController.RecentUserPurchase = purchase;
        _membershipController.SetPurchaseButtonsStatus(false);
        _membershipController.ActivateLicense();
    }
    #endregion
}

/// <summary>
/// Represents a product received from AppLab
/// </summary>
[Serializable]
public class FetchedProduct
{
    public  string SKU;
    public  string Name;
    public  string Price;
    public  string Description;

    public FetchedProduct(string sku, string name, string price, string description)
    {
        SKU = sku;
        Name = name;
        Price = price;
        Description = description;
    }
}

/// <summary>
/// Represent a purchase made by user
/// </summary>
[Serializable]
public class UserPurchase
{
    public  DateTime ExpirationTime;
    public  DateTime GrantTime;
    public  string ID;
    public  string SKU;

    public UserPurchase(string sku, string id, DateTime grantTime, DateTime expiration)
    {
        SKU = sku;
        ID = id;
        GrantTime = grantTime;
        ExpirationTime = expiration;
    }
}

/// <summary>
/// Represents a response with Oculus SKUs received from Cloud
/// </summary>
[Serializable]
public class SKUResponse
{
    public string message;
    public string[] oculus_skus;

    public SKUResponse(string message, string[] oculusSkus)
    {
        this.message = message;
        this.oculus_skus = oculusSkus;
    }
}
