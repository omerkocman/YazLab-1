using System.Collections.Generic;
using UnityEngine;

public class InteractableMemory : MonoBehaviour
{
    [Header("Interactable Memory Settings")]
    [SerializeField] private List<MemoryEntry> memoryEntries = new List<MemoryEntry>();

    public IReadOnlyCollection<MemoryEntry> MemoryEntries => memoryEntries;

    public void RecordOrUpdate(Interactable interactable, Vector3 lastSeenPos)
    {
        MemoryEntry existingEntry = memoryEntries.Find(e => e.Interactable == interactable);

        if(existingEntry != null)
        {
            existingEntry.LastSeenPosition = lastSeenPos;
            existingEntry.LastSeenTime = Time.time;
        }

        else
        {
            memoryEntries.Add(new MemoryEntry(interactable, lastSeenPos));
        }
    }

    public void MarkAsInteracted(Interactable interactable)
    {
        MemoryEntry entry = memoryEntries.Find(e => e.Interactable == interactable);

        if(entry != null)
        {
            entry.IsInteracted = true;
        }

    }

    public List<MemoryEntry> GetPendingEntries()
    {
        memoryEntries.RemoveAll(e => e.Interactable == null);

        return memoryEntries.FindAll(e => !e.IsInteracted);
    }




}
