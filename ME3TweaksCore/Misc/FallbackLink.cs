using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME3TweaksCore.Misc
{
    /// <summary>
    /// Link pairs for fallback systems (GitHub fallback to ME3Tweaks typically)
    /// </summary>
    public class FallbackLink
    {
        public string MainURL { get; init; }
        public string FallbackURL { get; init; }

        /// <summary>
        /// If URL randomly switches between main and fallback for load balancing
        /// </summary>
        public bool LoadBalancing { get; init; }

        /// <summary>
        /// Fetches in order all populated links.
        /// </summary>
        /// <returns></returns>
        public List<string> GetAllLinks()
        {
            var urls = new List<string>();
            if (MainURL != null) urls.Add(MainURL);
            if (FallbackURL != null) urls.Add(FallbackURL);
            if (LoadBalancing)
            {
                urls = urls.Shuffle().ToList();
            }
            return urls;
        }
    }
}
