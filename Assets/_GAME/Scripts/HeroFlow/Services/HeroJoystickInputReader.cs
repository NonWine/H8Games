using UnityEngine;

public class HeroJoystickInputReader : IHeroInputReader
{
    private readonly PlayerView heroView;

    public HeroJoystickInputReader(PlayerView heroView)
    {
        this.heroView = heroView;
    }

    public Vector3 ReadMovement()
    {
        Vector2 input = heroView.MovementJoystick != null
            ? heroView.MovementJoystick.Direction
            : Vector2.zero;

        Vector3 movement = new Vector3(input.x, 0f, input.y);
        return movement.sqrMagnitude > 1f ? movement.normalized : movement;
    }
}
