using Fusion;

// Grass interaction is now sampled in PlayerSurfaceController's Burst job.
// Keep this component temporarily so existing prefabs do not get a Missing Script.
// It can be removed from the player prefab and this file can then be deleted.
public sealed class CharacterGrassInteractor : NetworkBehaviour
{
}