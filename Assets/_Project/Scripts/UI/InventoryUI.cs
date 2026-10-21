using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Inventory playerInventory;
    

    private VisualElement root;
    private VisualElement inventoryContainer;
    private bool isOpen = false;
    private bool isDirty = true;

    private void Awake()
    {
        if(uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        if (playerInventory == null)
        {
            playerInventory = Inventory.Instance;
        }
    }

    private void Update()
    {
        if(Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        inventoryContainer = root.Q<VisualElement>("inventory-container");

        Label inventoryText = new Label();
        inventoryText.AddToClassList("inventory-text");

        inventoryText.text = "Inventory";

        inventoryContainer.Add(inventoryText);

        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += OnInventoryUpdated;
        }
        
        SetInventoryVisibility(false);
    }

    private void OnDisable()
    {
        if(playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= OnInventoryUpdated;
        }
    }

    private void OnInventoryUpdated()
    {
        if(isOpen)
        {
            RefreshUI();
        }

        isDirty = true;
    }

    private void RefreshUI()
    {
        if(root == null || inventoryContainer == null)
        {
            return;
        }

        inventoryContainer.Clear();

        IReadOnlyList<InventorySlot> slots = Inventory.Instance.GetInventoryList();

        foreach(InventorySlot slot in slots)
        {
            if(slot.heldItem == null)
            {
                continue;
            }

            VisualElement slotElement = CreateSlotElement(slot);
            inventoryContainer.Add(slotElement);
        }
    }

    private VisualElement CreateSlotElement(InventorySlot slot)
    {
        Color baseColor = slot.heldItem.itemType.GetColor();

        VisualElement slotBox = new VisualElement();
        slotBox.AddToClassList("inventory-slot");
   

        VisualElement icon = new VisualElement();
        icon.AddToClassList("slot-icon");


        //Color transparentColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.3f);

        slotBox.style.borderLeftColor = baseColor;
        //slotBox.style.borderTopColor = baseColor;
        //slotBox.style.borderRightColor = baseColor;
        //slotBox.style.borderBottomColor = baseColor;

        Sprite itemIcon = slot.heldItem.icon;

        if(itemIcon != null)
        {
            icon.style.backgroundImage= new StyleBackground(itemIcon);
        }

        slotBox.Add(icon);

        VisualElement slotTextContainer = new VisualElement();
        slotTextContainer.AddToClassList("slot-text-container");

        string itemName = slot.heldItem.itemName;
        int itemAmount = slot.itemAmount;

        Label labelItemName = new Label();
        labelItemName.AddToClassList("item-text");

        if(itemName != null)
        {
            labelItemName.text = itemName;
        }

        Label labelItemAmount = new Label();
        labelItemAmount.AddToClassList("item-amount");
        labelItemAmount.text = $"x{itemAmount}";

        slotTextContainer.Add(labelItemName);
        slotTextContainer.Add(labelItemAmount);

        slotBox.Add(slotTextContainer);

        return slotBox;
    }

    private void ToggleInventory()
    {
        isOpen = !isOpen;
        SetInventoryVisibility(isOpen);

        if(isOpen)
        {
            //cursorlock
            if(isDirty)
            {
                RefreshUI();
                isDirty = false;
            }
        }
        //else?
    }

    private void SetInventoryVisibility(bool isVisible)
    {
        root.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
    }


}
