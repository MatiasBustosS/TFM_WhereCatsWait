using UnityEngine;

public enum SwitchDirection
{
    Up,
    Down,
    Left,
    Right,
    UpRight,
    UpLeft,
    DownRight,
    DownLeft,
    None
}

[RequireComponent(typeof(Collider))]
public class TrackSwitch : MonoBehaviour
{
    [Header("Riel de Destino")]
    public TrackRail targetRail;

    [Header("Condicion de Entrada")]
    [SerializeField] private SwitchDirection requiredDirection;

    [Header("Ajuste de Progreso")]
    [Range(0f, 1f)]
    public float targetRailT = 0f;   

    private void OnTriggerStay(Collider other)
    {
        RailPlayerMovement player = other.GetComponent<RailPlayerMovement>();
        if (player == null) return;

        Vector2 rawInput = player.MovementInput;
        if (rawInput.sqrMagnitude < 0.25f) return;

        if (requiredDirection == SwitchDirection.None)
        {
            player.SwitchToRail(targetRail, targetRailT);
            return;
        }
        
        if (IsInputValid(rawInput, requiredDirection))
        {
            player.SwitchToRail(targetRail, targetRailT);
        }
    }
    
    private bool IsInputValid(Vector2 input, SwitchDirection expected)
    {
        bool up = input.y > 0.3f;
        bool down = input.y < -0.3f;
        bool right = input.x > 0.3f;
        bool left = input.x < -0.3f;

        return expected switch
        {
            SwitchDirection.Right     => right && !up && !down,
            SwitchDirection.UpRight   => right && up,
            SwitchDirection.Up        => up && !right && !left,
            SwitchDirection.UpLeft    => left && up,
            SwitchDirection.Left      => left && !up && !down,
            SwitchDirection.DownLeft  => left && down,
            SwitchDirection.Down      => down && !right && !left,
            SwitchDirection.DownRight => right && down,
            _ => false
        };
    }
}