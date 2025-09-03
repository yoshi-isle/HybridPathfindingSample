using UnityEngine;

public static class CollisionHelpers
{
    public static bool CheckPlayerOverlap(Collider collider1, Collider collider2)
    {
        if (collider1 != null && collider2 != null)
        {
            // Use 99% of the bounds to avoid edge-case issues with exact tile boundaries
            Bounds bounds1 = collider1.bounds;
            Bounds bounds2 = collider2.bounds;

            // Shrink bounds by 1% (0.5% on each side) to create a small buffer
            Vector3 collider1Size = bounds1.size * 0.99f;
            Vector3 collider2Size = bounds2.size * 0.99f;

            Bounds shrunkBounds1 = new Bounds(bounds1.center, collider1Size);
            Bounds shrunkBounds2 = new Bounds(bounds2.center, collider2Size);

            return shrunkBounds1.Intersects(shrunkBounds2);
        }

        return false;
    }
}