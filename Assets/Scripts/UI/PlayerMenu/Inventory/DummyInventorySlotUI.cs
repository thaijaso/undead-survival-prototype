using UndeadSurvivalGame.UI;

/// <summary>
/// A dummy inventory slot UI that does not perform any logic or is rendered on screen. 
/// Is used by ContextMenuController to draw its position.
/// </summary>
public class DummyInventorySlotUI : InventorySlotUI
{
    protected override void SetupInventorySlotUIHandler()
    {
        // Do nothing for dummy slot
    }

    // Override other methods as needed
}
