namespace WOS.Networking
{
    /// <summary>
    /// Spawn method for players joining the server
    /// </summary>
    public enum PlayerSpawnMethod
    {
        Random,      // Randomly select from available spawn points
        RoundRobin,  // Cycle through spawn points sequentially
        Specific     // Use a specific spawn point (for ports, etc.)
    }
}
