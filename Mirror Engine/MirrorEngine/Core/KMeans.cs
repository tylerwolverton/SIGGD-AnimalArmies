using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Cluster = System.Tuple<Engine.Vector2, System.Collections.Generic.List<Engine.Actor>, System.Double>;

namespace Engine
{
    public class KMeans
    {
        /*
         * This runs k-means with k = [low_k, high_k) to find the best set of clusters.
         */
        public static List<Tuple<Engine.Vector2, List<Engine.Actor>, Double>> getClusters(List<Actor> actors, int low_k = 2, int high_k = 5)
        {
            double best_sse = Double.PositiveInfinity;
            List<Cluster> best_clusters = null;
            for (int k = low_k; k < high_k; k++)
            {
                List<Cluster> clusters = getClusters(actors, k);

                // Figure out the aggregate sse for tis clustering
                double total_sse = 0;
                foreach (Cluster c in clusters) 
                {
                    total_sse += c.Item3;
                }
                total_sse *= k;

                Console.WriteLine("SSE for k = " + k + ": " + total_sse + " -- best is " + best_sse);
                if (total_sse < best_sse)
                {
                    best_clusters = clusters;
                    best_sse = total_sse;
                }
            }
            return best_clusters;
        }

        /*
         * This runs k-means with k='k'
         * Returns: A list of tuples <center of cluster, list of actors in cluster, the SSE of the actor positions in the cluster>
         */
        public static List<Tuple<Engine.Vector2, List<Engine.Actor>, Double>> getClusters(List<Actor> actors, int k)
        {
            // For rand nums... yo.
            Random rand = new Random();
            // Execute a 'good-enough' number of rounds to make sure we don't get a bad 'starting set' of random points.
            int num_rounds = 5;
            // Keep track of the best clustering assignment
            double best_sse = Double.PositiveInfinity;
            Dictionary<Vector2, List<Vector2>> best_clusters = null;

            // Get the list of points where the actors live
            Dictionary<Vector2, Actor> points = new Dictionary<Vector2, Actor>();
            foreach (Actor a in actors)
            {
                points[new Vector2(a.curTile.xIndex, a.curTile.yIndex)] = a;
            }

            // Loop for 'r' rounds where we pick a new set of starting points
            // so that we don't get our clusters all stuck from the same 'real' cluster.
            for (int r = 0; r < num_rounds; r++)
            {
                // Get 'k' starting means from the list of points.
                Vector2[] means = points.Keys.OrderBy(x => rand.Next()).Take(k).ToArray();

                //Initialize the current list of clusters for this round
                Dictionary<Vector2, List<Vector2>> cur_clusters = new Dictionary<Vector2, List<Vector2>>();
                foreach (Vector2 m in means)
                {
                    cur_clusters[m] = new List<Vector2>();
                }

                // Assign all the points to their closest 'mean', then
                // re-balance the means to be the center of the clusters
                // keep going until the means don't change.
                bool did_change = true;
                while (did_change)
                {
                    did_change = false;

                    // Clear all the cluster assignments
                    foreach (Vector2 m in new List<Vector2>(cur_clusters.Keys))
                    {
                        cur_clusters[m] = new List<Vector2>();
                    }

                    // Assign each point to a cluster around the closest mean
                    foreach (Vector2 p in points.Keys)
                    {
                        double dist = double.PositiveInfinity;
                        Vector2 best_m = null;
                        foreach (Vector2 m in cur_clusters.Keys)
                        {
                            double tmpdist = Math.Sqrt(Math.Pow(m.x - p.x, 2) + Math.Pow(m.y - p.y, 2));
                            if (tmpdist < dist)
                            {
                                dist = tmpdist;
                                best_m = m;
                            }
                        }
                        cur_clusters[best_m].Add(p);
                    }

                    // Calculate 'k' new means as the centers of the newly calculated clusters
                    foreach (Vector2 m in new List<Vector2>(cur_clusters.Keys))
                    {
                        // Find the new center of the cluster
                        Vector2 new_mean = new Vector2(0, 0);
                        foreach (Vector2 p in cur_clusters[m])
                        {
                            new_mean = new_mean + p;
                        }
                        new_mean.x /= cur_clusters[m].Count;
                        new_mean.y /= cur_clusters[m].Count;

                        // If the center has moved, then record it and loop again
                        // Note: Because we're using doubles, the center can wobble around slightly depending on calculation error.
                        // We just make sure the mean is within 1 tile of itself
                        if (euclidean_distance(new_mean, m) > 2.83)
                        {
                            Console.WriteLine(m + "-> " + new_mean);
                            cur_clusters[new_mean] = cur_clusters[m];
                            cur_clusters.Remove(m);
                            // There was an update, so keep going
                            did_change = true;
                        }
                    }
                }

                // Calculate the sum of the square of the errors (euclidean distance between point in a cluster
                // and the cluster's center).
                double sse = 0;
                foreach (Vector2 m in cur_clusters.Keys)
                {
                    foreach (Vector2 p in cur_clusters[m])
                    {
                        sse += Math.Pow(euclidean_distance(m, p), 2);
                    }
                }
                // Keep track of the best set of clusters
                if (sse < best_sse)
                {
                    best_sse = sse;
                    best_clusters = cur_clusters;
                }
            }

            // Return the best clusters
            List<Cluster> rtn = new List<Cluster>();
            foreach (Vector2 m in best_clusters.Keys)
            {
                double sse = 0;
                List<Actor> clusters_actors = new List<Actor>();
                foreach (Vector2 p in best_clusters[m])
                {
                    clusters_actors.Add(points[p]);
                    sse += Math.Pow(euclidean_distance(m, p), 2);
                }
                rtn.Add(new Cluster(m, clusters_actors, sse));
            }

            return rtn;
        }

        /*
         * Calculate the Euclidean distance between two points
         * (I didn't seem to see this functionality anywhere else...)
         */
        private static double euclidean_distance(Vector2 p1, Vector2 p2)
        {
            return Math.Sqrt(Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2));
        }
    }
}
