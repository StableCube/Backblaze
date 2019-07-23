using System.Collections.Generic;

namespace StableCube.Backblaze.DotNetClient
{
    public class CreateNodePoolInput
    {
        /// <summary>
        /// The slug identifier for the type of Droplet to be used as workers in the node pool.
        /// </summary>
        public string Size { get; set; }

        /// <summary>
        /// A human-readable name for the node pool.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The number of Droplet instances in the node pool.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// A flat array of tag names as strings to be applied to the node pool. All node pools will be automatically 
        /// tagged "k8s," "k8s-worker," and "k8s:$K8S_CLUSTER_ID" in addition to any tags provided by the user.
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();
    }
}