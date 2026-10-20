using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class InventoryUI : MonoBehaviour
{
    [Header("Inventory Settings")]
    //[SerializeField] private int slotCapacity = 32;
    //public List<InventorySlot> slots = new List<InventorySlot>();

    private bool isOpen = false;

    private UIDocument uiDocument;
    private VisualElement root;
    private VisualElement inventoryContainer;
    private List<VisualElement> uiSlotElements = new List<VisualElement>();

    private void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        inventoryContainer = root.Q<VisualElement>("inventory-container");

        if (inventoryContainer == null)
        {
            Debug.Log("inventory-container not found! Check UXML name value");
            return;
        }

        uiSlotElements = inventoryContainer
     .Query<VisualElement>(name: "inventory-slot")
     .ToList();

        if (uiSlotElements.Count == 0)
        {
            Debug.Log("inventory-slot not found! Check UXML name value");
            return;
        }

        SetInventoryVisible(false);
        UpdateUI();
    }

    private void SetInventoryVisible(bool visible)
    {
        isOpen = visible;

        root.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void Update()
    {
        if(Keyboard.current.tabKey.wasPressedThisFrame)
        {
            SetInventoryVisible(!isOpen);
            if(isOpen)
            {
                UpdateUI();
            }
        }
    }

    private void UpdateUI()
    {
        if(Inventory.Instance == null)
        {
            return;
        }

        IReadOnlyList<InventorySlot> inventory = Inventory.Instance.Slots;

        for(int i = 0; i < uiSlotElements.Count; i++)
        {
            VisualElement slotElement = uiSlotElements[i];

            Image icon = slotElement.Q<Image>("slot-icon");
            Label amountText = slotElement.Q<Label>("slot-text");

            if (icon == null || amountText == null)
            {
                Debug.LogWarning($"Slot: {i} slot-icon or slot-text not found!");
                continue;
            }

            if(i >= inventory.Count)
            {
                icon.image = null;
                icon.style.display = DisplayStyle.None;
                amountText.text = "";
                continue;
            }

            InventorySlot slot = inventory[i];

            if(slot.HasAnyItem())
            {
                ItemSO item = slot.GetItem();

                icon.image = item.icon != null ? item.icon.texture : null;

                icon.style.display = item.icon != null ? DisplayStyle.Flex : DisplayStyle.None;
                amountText.text = slot.GetAmount() > 1
                    ? $"x{slot.GetAmount()}"
                    : "";
            }

            else
            {
                icon.image = null;
                icon.style.display = DisplayStyle.None;
                amountText.text = "";
            }
        }

    }
}