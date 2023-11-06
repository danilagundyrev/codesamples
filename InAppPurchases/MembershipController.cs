public class MembershipController : UIWindow
{
    [SerializeField] private InAppPurchasesManager _iapManager;
    
    private UserPurchase _recentUserPurchase;
    public UserPurchase RecentUserPurchase
    {
        get => _recentUserPurchase;
        set => _recentUserPurchase = value;
    }
    
    private const string OCULUS_ID = "oculus_id";
    private const string OCULUS_SKU = "oculus_sku";
    private const string OCULUS_ITEM_ID = "oculus_item_id";

    private Transform _subscriptionsPanel;
    private Transform _subscriptionsPanelContent;
    private GameObject _subscriptionCardprefab;
    private Button _subscriptionLeftButton;
    private Button _subscriptionRightButton;
    private List<SubscriptionCard> _currentSubscriptionCards = new List<SubscriptionCard>();
    private int _cardIndex = 0;
    
    private GameObject _licenseCanvas;
    private Button _activateLicense;
    
    private TMP_Text _utilityMessage;
    private Button _backButton;

    protected override void FindReferences()
    {
        _utilityMessage = WindowContent.Find("UtilityMessage").GetComponent<TMP_Text>();
        _licenseCanvas = WindowContent.Find("ActivateLicenseTab").gameObject;
        _subscriptionCardprefab = Resources.Load<GameObject>("Prefabs/UI/SubscriptionTab");
        _subscriptionsPanel = WindowContent.Find("Subscriptions");
        _subscriptionsPanelContent = _subscriptionsPanel.Find("Content");
        HelpersUI.SetupButton(ref _subscriptionLeftButton,gameObject,"SubscriptionLeft", OnPreviousSubscriptionsButtonClick);
        HelpersUI.SetupButton(ref _subscriptionRightButton,gameObject,"SubscriptionRight", OnNextSubscriptionsButtonClick);
        HelpersUI.SetupButton(ref _activateLicense,_licenseCanvas,"ActivateButton", ActivateLicense);
        HelpersUI.SetupButton(ref _backButton,gameObject,"BackButton", OnBackButton);
    }

    protected override void OnOpen()
    {
        SetUtilityMessage("");
        ClearSubscriptions();
        _iapManager.FetchAvailableProducts();
        _iapManager.FetchPurchasedProducts();
        _cardIndex = 0;
    }

    /// <summary>
    /// Utility method to set a debug message
    /// </summary>
    /// <param name="message"></param>
    public void SetUtilityMessage(string message)
    {
        _utilityMessage.text = message;
    }
    
    /// <summary>
    /// Returns to the Home screen 
    /// </summary>
    private void OnBackButton()
    {
        UIController.Instance.SetActiveScreen(UIControllerBase.MenuType.HomeMenu,MenuControllerBase.MenuScreenType.HomeScreen);
    }

    /// <summary>
    /// Clears current subscription cards list, deletes all children from the Subscriptions Content submenu
    /// </summary>
    private void ClearSubscriptions()
    {
        _currentSubscriptionCards.Clear();
        foreach (Transform child in _subscriptionsPanelContent)
        {
            Destroy(child.gameObject);
        }
        SetScrollingButtonsVisibility(false);
    }

    /// <summary>
    /// Handles UI according to the result of the response received from the AppLab when the system
    /// requests a list of available products
    /// </summary>
    /// <param name="empty"></param>
    public void OnFetchAvailableProductsComplete()
    {
        SetScrollingButtonsVisibility(_currentSubscriptionCards.Count > 3);
        _subscriptionLeftButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Handles UI according to the result of the response received from the AppLab when the system
    /// requests a list of purchased products
    /// </summary>
    /// <param name="empty"></param>
    public void OnFetchPurchasedProductsComplete(bool empty)
    {
        _licenseCanvas.SetActive(!empty);
        _subscriptionsPanel.gameObject.SetActive(empty);
    }

    /// <summary>
    /// Spawns subscription card and sets it up according to the passed product
    /// </summary>
    /// <param name="product">A product that a spawned card should represent</param>
    public void SpawnSubscriptionCard(FetchedProduct product)
    {
        GameObject go = Instantiate(_subscriptionCardprefab, _subscriptionsPanelContent);
        SubscriptionCard subscriptionCard = go.GetComponent<SubscriptionCard>();
        subscriptionCard.InitReferences();
        subscriptionCard.CardSetup(product);
        subscriptionCard.ChooseButton.onClick.AddListener(delegate { PurchaseLicense(product.SKU); });
        _currentSubscriptionCards.Add(subscriptionCard);
    }

    private void SetScrollingButtonsVisibility(bool visible)
    {
        _subscriptionLeftButton.gameObject.SetActive(visible);
        _subscriptionRightButton.gameObject.SetActive(visible);
    }

    /// <summary>
    /// Handles "Next" scrolling button
    /// </summary>
    private void OnNextSubscriptionsButtonClick()
    {
        if (_currentSubscriptionCards.Count <= 3)
        {
            return;
        }
        if (_cardIndex < _currentSubscriptionCards.Count - 3)
        {
            _cardIndex++;
            if (_cardIndex == _currentSubscriptionCards.Count - 3)
            {
                _subscriptionRightButton.gameObject.SetActive(false);
            }
            _subscriptionLeftButton.gameObject.SetActive(true);
            
            _currentSubscriptionCards[_cardIndex].transform.SetSiblingIndex(0);
            _currentSubscriptionCards[_cardIndex+1].transform.SetSiblingIndex(1);
            _currentSubscriptionCards[_cardIndex+2].transform.SetSiblingIndex(2);
        }
    }
    
    /// <summary>
    /// Handles "Previous" scrolling button
    /// </summary>
    private void OnPreviousSubscriptionsButtonClick()
    {
        if (_currentSubscriptionCards.Count <= 3)
        {
            return;
        }
        if (_cardIndex > 0)
        {
            _cardIndex--;
            if (_cardIndex == 0)
            {
                _subscriptionLeftButton.gameObject.SetActive(false);
            }
            _subscriptionRightButton.gameObject.SetActive(true);
            
            _currentSubscriptionCards[_cardIndex].transform.SetSiblingIndex(0);
            _currentSubscriptionCards[_cardIndex+1].transform.SetSiblingIndex(1);
            _currentSubscriptionCards[_cardIndex+2].transform.SetSiblingIndex(2);
        }
    }

    /// <summary>
    /// Sets button.interactable status for each currently spawned subscription card 
    /// </summary>
    /// <param name="status"></param>
    public void SetPurchaseButtonsStatus(bool status)
    {
        foreach (SubscriptionCard card in _currentSubscriptionCards)
        {
            card.ChooseButton.interactable = status;
        }
    }

    /// <summary>
    /// Launches a checkout workflow to purchase a product 
    /// </summary>
    /// <param name="sku">SKU of the product that should be bought</param>
    private void PurchaseLicense(string sku)
    {
        SetPurchaseButtonsStatus(false);
        _iapManager.PurchaseProduct(sku);
    }
    
    /// <summary>
    /// Sends a request to the Cloud to activate a license recently purchased by user
    /// </summary>
    public void ActivateLicense()
    {
        WWWForm post = new WWWForm();
        
        post.AddField(OCULUS_ID, OculusPlatformManager.Instance.LoggedInUserUniqueId);
        post.AddField(OCULUS_SKU, _recentUserPurchase.SKU);
        post.AddField(OCULUS_ITEM_ID, _recentUserPurchase.ID);
        string url = ApiCalls.Redeem();
        SetUtilityMessage("Sending request to activate license...");

        NetworkManager.Instance.Request(NetworkManager.RequestMethod.POST, url, delegate
            {
                SetUtilityMessage("License has been successfully redeemed!");
                LoadLicenseAgain();
            }, delegate(bool networkError, int responseCode, string response)
            {
                SetUtilityMessage(response);
            },
            post);
    }
    
    /// <summary>
    /// Launches a license refresh flow to update current user's status
    /// </summary>
    private void LoadLicenseAgain()
    {
        // Specific internal api call to load updated user license
    }
}
