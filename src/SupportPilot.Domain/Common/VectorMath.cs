namespace SupportPilot.Domain.Common;

/// <summary>
/// Pure vector math used for semantic comparison. Lives in Domain because it
/// has no dependencies on any AI provider or vector database — it is plain
/// mathematics that any layer can use.
/// </summary>
public static class VectorMath
{
    /// <summary>
    /// Cosine similarity between two vectors. Measures the angle (meaning
    /// overlap), independent of text length. Range: -1 (opposite) to 1 (identical).
    /// </summary>
    public static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Vectors must have the same length.");

        float dot = 0f, magnitudeA = 0f, magnitudeB = 0f;

        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        return dot / (MathF.Sqrt(magnitudeA) * MathF.Sqrt(magnitudeB));
    }
}