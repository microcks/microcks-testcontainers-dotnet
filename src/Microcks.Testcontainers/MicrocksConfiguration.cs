namespace Microcks.Testcontainers;

/// <inheritdoc cref="ContainerConfiguration" />
public sealed class MicrocksConfiguration : ContainerConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksConfiguration" /> class.
    /// </summary>
    /// <param name="config">The Microcks config.</param>
    public MicrocksConfiguration(object config = null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public MicrocksConfiguration(IResourceConfiguration<CreateContainerParameters> resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public MicrocksConfiguration(IContainerConfiguration resourceConfiguration)
        : base(resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksConfiguration" /> class.
    /// </summary>
    /// <param name="resourceConfiguration">The Docker resource configuration.</param>
    public MicrocksConfiguration(MicrocksConfiguration resourceConfiguration)
        : this(new MicrocksConfiguration(), resourceConfiguration)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrocksConfiguration" /> class.
    /// </summary>
    /// <param name="oldValue">The old Docker resource configuration.</param>
    /// <param name="newValue">The new Docker resource configuration.</param>
    public MicrocksConfiguration(MicrocksConfiguration oldValue, MicrocksConfiguration newValue)
        : base(oldValue, newValue)
    {
    }
}
