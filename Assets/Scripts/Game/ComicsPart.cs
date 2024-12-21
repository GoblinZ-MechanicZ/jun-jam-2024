namespace GoblinzMechanics.Game
{
    using UnityEngine;
    using System.Collections.Generic;
    using System;

    [Serializable]
    public class ComicsPage
    {
        [SerializeField] public List<ComicsFrame> comicsParts = new();
    }
}