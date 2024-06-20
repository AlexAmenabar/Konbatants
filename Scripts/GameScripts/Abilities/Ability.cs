using Godot;
using System;

/// <summary>
/// Interface to represent different types of abilities
/// </summary>
public interface Ability
{
    /// <summary>
    /// Set a PlayerController to this ability (only that user can use this ability).
    /// </summary>
    /// <param name="pPlayer"></param>
    public abstract void SetPlayer(PlayerController pPlayer);

    /// <summary>
    /// Use ability.
    /// </summary>
    public abstract void Use();

    /// <summary>
    /// Play ability sound.
    /// </summary>
    public abstract void UseSound();

    /// <summary>
    /// Get ability texture (image).
    /// </summary>
    /// <returns></returns>
    public abstract Texture2D GetTexture();

    /// <summary>
    /// Change ability used attribute.
    /// </summary>
    /// <returns></returns>
    public abstract bool AbilityIsUsed();
}
