public interface ITankComponents
{
    TankController Controller { get; }
    TurretController Turret { get; }
    NavigationStrategy Navigation { get; }
    float FiringRange { get; }
}
