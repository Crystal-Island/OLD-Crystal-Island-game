using System;
using System.Collections.Generic;
using System.Linq;

namespace KoboldTools
{
    /// <summary>
    /// Provides a means to perform random selection from a pool,
    /// where individual elements may have different probabilities
    /// of occurrence.
    /// </summary>
    public class Roulette<T> where T: IEquatable<T>
    {
        /// <summary>
        /// The internal set of pockets.
        /// </summary>
        private List<Pocket> _pockets = new List<Pocket>();
        /// <summary>
        /// The current sum of all pocket sizes.
        /// </summary>
        private int _maxSize = 0;
        /// <summary>
        /// The random generator instance.
        /// </summary>
        private Random _rng = new Random();

        /// <summary>
        /// A pocket stores a generic object and the relative probability of
        /// it's occurrence in the urn.
        /// </summary>
        private struct Pocket
        {
            public T pocketObject;
            public int pocketSize;
        }
        /// <summary>
        /// Adds a new pocket to the roulette.
        /// </summary>
        /// <param name="pocketObject">Pocket object instance.</param>
        /// <param name="pocketSize">Pocket size (relative probability of occurrence).</param>
        public void addPocket(T pocketObject, int pocketSize)
        {
            Pocket newPocket = new Pocket();
            newPocket.pocketObject = pocketObject;
            newPocket.pocketSize = pocketSize;
            this._pockets.Add(newPocket);
            this.calcMaxSize();
        }
        /// <summary>
        /// Removes the specified pocket from the roulette.
        /// </summary>
        /// <param name="pocketObject">Pocket object instance.</param>
        public void removePocket(T pocketObject)
        {
            for (int i = this._pockets.Count - 1; i >= 0; i--)
            {
                if (this._pockets[i].pocketObject.Equals(pocketObject))
                {
                    this._pockets.RemoveAt(i);
                }
            }
            this.calcMaxSize();
        }
        /// <summary>
        /// Refresh the internal state of the roulette.
        /// </summary>
        public void refresh()
        {
            this.calcMaxSize();
        }
        /// <summary>
        /// Randomly select an element from the pool.
        /// </summary>
        /// <returns>The selected element.</returns>
        public T spinRoulette()
        {
            int targetSize = this._rng.Next(0, this._maxSize + 1);
            int currentSize = 0;
            foreach (Pocket pck in this._pockets)
            {
                currentSize += pck.pocketSize;
                if (currentSize >= targetSize)
                {
                    return pck.pocketObject;
                }
            }
            return default(T);
        }
        /// <summary>
        /// Calculates the sum of pocket sizes over all pockets.
        /// </summary>
        private void calcMaxSize()
        {
            this._maxSize = this._pockets.Sum(p => p.pocketSize);
        }

    }
}

