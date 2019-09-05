using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMining.ClusteringAlgo;
using NetMining.Data;
using NetMining.Graphs;
using NetMining.Graphs.Generator;
using NetMining.ADT;
using NetMining.Files;
using NetMining.Evaluation;
using NetMining.ExtensionMethods;
using System.IO;
using System.Diagnostics;

namespace debugNetData
{
    class Program
    {
        public enum OutType
        {
            Ten,
            Vat,
            Int
        }

        static void Main(string[] args)
        {
            String workingDir = Directory.GetCurrentDirectory();
            if (args.Length == 0)
            {
                System.Console.WriteLine(
                    "Usage: Program.cs <HealthyPath> <InfectedPath> <HealthyFile> <InfectedFile> ");
                Environment.Exit(0);
            }

            // AUTOMATING IBD 
            // We need both a healthy network and an IBD network
            // COMMAND LINE: clusteringanalysis.exe healthyNet infectedNet VATorINTorTEN  


            //convert from gml to graph
            //string path = "C:\\Users\\John\\Dropbox\\Tayo\\Yasser\\graphs_John_Tayo\\CoNet\\A-TEST\\";
            String healthyPath = $"{workingDir}//Data//{args[0]}";
            //"C:\\Users\\jmatta\\Dropbox\\Tayo\\Yasser\\graphs_John_Tayo\\CoNet\\A-TEST\\healthy_CoNet_25";
            String infectedPath = $"{workingDir}//Data//{args[1]}";
            //"C:\\Users\\jmatta\\Dropbox\\Tayo\\Yasser\\graphs_John_Tayo\\CoNet\\A-TEST\\IBD_CoNet_25";

            LightWeightGraph healthy = LightWeightGraph.GetGraphFromGML($"{healthyPath}"); //healthyFile + ".gml");
            healthy.SaveGraph(healthyPath + ".graph");
            LightWeightGraph infected = LightWeightGraph.GetGraphFromGML($"{infectedPath}"); //infectedFile + ".gml");
            infected.SaveGraph(infectedPath + ".graph");
            // Makes a list of what the nodes reference
            using (StreamWriter sw = new StreamWriter(healthyPath + ".txt", true))
            {
                for (int i = 0; i < healthy.Nodes.Length; i++)
                {
                    sw.WriteLine(healthy.Nodes[i].sharedName);
                }
            }

            using (StreamWriter sw = new StreamWriter(infectedPath + ".txt", true))
            {
                for (int i = 0; i < infected.Nodes.Length; i++)
                {
                    sw.WriteLine(infected.Nodes[i].sharedName);
                }
            }

            //we don't actually know the number of clusters in each graph - we want to cluster for 1 more than we start with
            //so cluster for 1 just to get the file.
            //HVATClust clust1 = new HVATClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);
            HVATClust healthyClust1 = new HVATClust(healthy, 1, false, 1, 0, false, false);
            Partition t1 = healthyClust1.GetPartition();
            int healthyClusters = t1.Clusters.Count;
            HVATClust infectedClust1 = new HVATClust(infected, 1, false, 1, 0, false, false);
            Partition t2 = infectedClust1.GetPartition();
            int infectedClusters = t2.Clusters.Count;

            // Now we know the intital number of clusters, do the actual clustering
            //HVATClust clust1 = new HVATClust(lwg2, K, useweights, 1, 0, reassign, hillclimb);

            // This sees if the input cluster type can be parsed as the Enum, and if so 
            // Uses a switch statement to decide which clustering to run.
            if (args.Length >= 3)
            {
                if (Enum.TryParse<OutType>(args[2], ignoreCase: true, out var userOut))
                {
                    switch (userOut)
                    {
                        case OutType.Int:
                            HIntegrityClust hclust2 =
                                new HIntegrityClust(healthy, healthyClusters + 1, false, 1, 0, false, false);
                            Partition p2 = hclust2.GetPartition();
                            p2.SavePartition(healthyPath + "_INT.cluster", healthyPath + ".graph");

                            HIntegrityClust iclust2 =
                                new HIntegrityClust(infected, infectedClusters + 1, false, 1, 0, false, false);
                            Partition p5 = iclust2.GetPartition();
                            p5.SavePartition(infectedPath + "_INT.cluster", infectedPath + ".graph");

                            int[] clusts2 = new int[p2.DataCount];
                            int[] clusts5 = new int[p5.DataCount];
                            // Healthy Group
                            rename(p2, clusts2, healthyPath, "_INT.csv", userOut);
                            // Infected Group
                            rename(p5, clusts5, infectedPath, "_INT.csv", userOut);
                            break;
                        case OutType.Ten:
                            HTenacityClust hclust3 =
                                new HTenacityClust(healthy, healthyClusters + 1, false, 1, 0, false, false);
                            Partition p3 = hclust3.GetPartition();
                            p3.SavePartition(healthyPath + "_TEN.cluster", healthyPath + ".graph");

                            HTenacityClust iclust3 =
                                new HTenacityClust(infected, infectedClusters + 1, false, 1, 0, false, false);
                            Partition p6 = iclust3.GetPartition();
                            p6.SavePartition(infectedPath + "_TEN.cluster", infectedPath + ".graph");

                            int[] clusts3 = new int[p3.DataCount];
                            int[] clusts6 = new int[p6.DataCount];
                            // Healthy Group
                            rename(p3, clusts3, healthyPath, "_TEN.csv", userOut);
                            // Infected Group
                            rename(p6, clusts6, infectedPath, "_TEN.csv", userOut);
                            break;
                        case OutType.Vat:
                            HVATClust hclust1 = new HVATClust(healthy, healthyClusters + 1, false, 1, 0, false, false);
                            Partition p1 = hclust1.GetPartition();
                            p1.SavePartition(healthyPath + "_VAT.cluster", healthyPath + ".graph");

                            HVATClust iclust1 =
                                new HVATClust(infected, infectedClusters + 1, false, 1, 0, false, false);
                            Partition p4 = iclust1.GetPartition();
                            p4.SavePartition(infectedPath + "_VAT.cluster", infectedPath + ".graph");

                            int[] clusts1 = new int[p1.DataCount];
                            int[] clusts4 = new int[p4.DataCount];
                            // Healthy Group
                            rename(p1, clusts1, healthyPath, "_VAT.csv", userOut);
                            // Infected Group
                            rename(p4, clusts4, infectedPath, "_VAT.csv", userOut);
                            break;
                    }
                }
                else
                {
                    Console.WriteLine("Please input a valid output type (VAT, INT, TEN) as the third parameter.");
                }
            }
            else
            {
                Console.WriteLine("Please enter a valid cluster type (INT, VAT, TEN).");
            }
        } // brace closes main()

        public static void rename(Partition p, int[] cluster, String FileName, String FileEnd, OutType type)
        {
            List<DataOutStruct> dataOut = new List<DataOutStruct>();
            
            for (int i = 0; i < p.DataCount; i++)
            {
                cluster[i] = -1;
            }

            for (int i = 0; i < p.Clusters.Count(); i++)
            {
                for (int j = 0; j < p.Clusters[i].Points.Count(); j++)
                {
                    cluster[p.Clusters[i].Points[j].Id] = p.Clusters[i].Points[j].ClusterId;
                }
            }

            using (StreamWriter sw = new StreamWriter(FileName + FileEnd, true))
            {
                for (int i = 0; i < p.DataCount; i++)
                {
                    DataOutStruct outObj = new DataOutStruct();
                    outObj.bacteria = p.Graph.Nodes[i].sharedName;
                    outObj.clusterType = type.ToString();
                    if (cluster[i] != -1)
                    {
                        outObj.groupNum = cluster[i].ToString();
                    }
                    else
                    {
                        outObj.groupNum = "N/A";
                    }
                }
            }
        }

        public static Partition combineClusters(Partition partition, int minK)
        {
            // we want to do (partition.Clusters.count - minK) merges
            int startPartitions = partition.Clusters.Count;
            for (int numMerges = 0; numMerges < startPartitions - minK; numMerges++)
            {
                Console.WriteLine("combining iteration = " + numMerges + " out of " + (startPartitions - minK));
                int[,] connections = new int[partition.Clusters.Count, partition.Clusters.Count];
                LightWeightGraph g = (LightWeightGraph) partition.Data;

                // for quick reference let's make a list of which nodes are in which clusters
                int[] clustAssignments = new int[g.Nodes.Count()];
                for (int i = 0; i < clustAssignments.Length; i++)
                {
                    clustAssignments[i] = -1;
                }

                for (int i = 0; i < partition.Clusters.Count; i++)
                {
                    for (int j = 0; j < partition.Clusters[i].Points.Count; j++)
                    {
                        clustAssignments[partition.Clusters[i].Points[j].Id] =
                            partition.Clusters[i].Points[j].ClusterId;
                    }
                }
                // right here we should go through and assign the attack nodes to a cluster

                for (int i = 0; i < partition.removedNodes.Count; i++)
                {
                    int[] attackNodeStatus = new int[partition.Clusters.Count];
                    int numAdjacencies = partition.Graph.Nodes[partition.removedNodes[i]].Edge.Length;
                    for (int j = 0; j < numAdjacencies; j++)
                    {
                        int clusterImIn = clustAssignments[partition.Graph.Nodes[partition.removedNodes[i]].Edge[j]];
                        if (clusterImIn != -1)
                        {
                            attackNodeStatus[clusterImIn]++;
                        }
                    }

                    // now go through attackNodeStatus list and find the largest
                    int maxSize = 0;
                    int maxLoc = 0;
                    for (int j = 0; j < attackNodeStatus.Length; j++)
                    {
                        if (attackNodeStatus[j] > maxSize)
                        {
                            maxSize = attackNodeStatus[j];
                            clustAssignments[partition.removedNodes[i]] = j;
                        }
                    }
                }

                // now removed nodes have been added to a cluster.  There might be some -1 in clustAssignments, so replace those with 0, because we don't know what to do with them
                for (int i = 0; i < clustAssignments.Length; i++)
                {
                    if (clustAssignments[i] == -1) clustAssignments[i] = 0;
                }


                // now go through each node and count its edges out to each cluster
                // add these edges to the connections[] matrix
                for (int i = 0; i < g.Nodes.Count(); i++)
                {
                    int currentCluster = clustAssignments[i];
                    for (int e = 0; e < g.Nodes[i].Edge.Count(); e++)
                    {
                        int adjacentNode = g.Nodes[i].Edge[e];
                        int adjacentCluster = clustAssignments[adjacentNode];
                        connections[currentCluster, adjacentCluster]++;
                    }
                }

                // keep a list of which partitions will be merged
                // List<int> merges = new List<int>();

                // find the largest connections[i,j] and merge clusters i and j

                int largestI = 0;
                int largestJ = 0;
                double largestValue = 0;
                for (int i = 0; i < partition.Clusters.Count; i++)
                {
                    for (int j = 0; j < partition.Clusters.Count; j++)
                    {
                        if (j <= i) continue;
                        int sizeI = partition.Clusters[i].Points.Count;
                        int sizeJ = partition.Clusters[j].Points.Count;
                        double score = ((double) connections[i, j]) / (sizeI * sizeJ);
                        //double score = connections[i, j];
                        //if (sizeI > 40 || sizeJ > 40) score = 0;
                        if (score > largestValue)
                        {
                            largestValue = score;
                            largestI = i;
                            largestJ = j;
                        }

                        // we want to merge smaller into larger clusters
                        if (sizeI > sizeJ)
                        {
                            int temp = largestI;
                            largestI = largestJ;
                            largestJ = temp;
                        }
                    }
                }

                // if everything's zero, there is no hope ;-)
                if (largestValue == 0)
                {
                    continue;
                }

                List<int> replacedNodes = new List<int>();
                // Now we need to find the overlap nodes between largestI and largestJ and add them to one of the clusters
                for (int i = 0; i < partition.removedNodes.Count; i++)
                {
                    int[] attackNodeStatus = new int[partition.Clusters.Count];
                    int numAdjacencies = partition.Graph.Nodes[partition.removedNodes[i]].Edge.Length;
                    for (int j = 0; j < numAdjacencies; j++)
                    {
                        int clusterImIn = clustAssignments[partition.Graph.Nodes[partition.removedNodes[i]].Edge[j]];
                        if (clusterImIn != -1)
                        {
                            attackNodeStatus[clusterImIn]++;
                        }
                    }

                    // THIS CONDITION COULD BE CHANGED!!
                    double std = 0;

                    if (attackNodeStatus[largestI] + attackNodeStatus[largestJ] > 0)
                    {
                        double firstNormalized = (double) attackNodeStatus[largestI] /
                                                 (attackNodeStatus[largestI] + attackNodeStatus[largestJ]);
                        double secondNormalized = (double) attackNodeStatus[largestJ] /
                                                  (attackNodeStatus[largestI] + attackNodeStatus[largestJ]);
                        double mean = (firstNormalized + secondNormalized) / 2;
                        std = Math.Sqrt(Math.Pow(firstNormalized - mean, 2) + Math.Pow(secondNormalized - mean, 2));
                    }
                    else std = 1;

                    if (std < 0.4) //if (attackNodeStatus[largestI]> 0 && attackNodeStatus[largestJ]>0)
                    {
                        //now add to list of replaced nodes
                        replacedNodes.Add(partition.removedNodes[i]);
                        // it is a member of both partitions so add to one of the clusters
                        ClusteredItem newpoint = new ClusteredItem(partition.removedNodes[i]);
                        newpoint.ClusterId = largestI;
                        partition.Clusters[largestI].AddPoint(newpoint);
                        //now remove from attack set
                        //partition.removedNodes.RemoveAt(i);
                    }
                }


                // now we want to merge cluster largestJ into cluster largestI, 
                // remove cluster largestJ, and renumber all clusters after the first
                // adds the points of the second cluster to the first cluster
                for (int i = 0; i < partition.Clusters[largestJ].Points.Count; i++)
                {
                    partition.Clusters[largestI].Points.Add(partition.Clusters[largestJ].Points[i]);
                }

                int set = 1;
                int KNN = 100;
                int D = 2;
                int K = 8;
                int KNNinit = KNN;
                int end = KNN + 1;
                string prefix = "NOverlapEqDens\\set" + set + "aa\\";
                string filename = "synthD" + D + "K" + K + "_Euclidean_KNN_" + KNN + ".graph";
                //string pointSetName = "synthD" + D + "K" + K + "." + set + ".txt";

                // remove largestJ cluster
                partition.Clusters.RemoveAt(largestJ);


                // renumber the clusters
                for (int i = 0; i < partition.Clusters.Count; i++)
                {
                    partition.Clusters[i].Points.Sort();
                    for (int j = 0; j < partition.Clusters[i].Points.Count; j++)
                    {
                        partition.Clusters[i].Points[j].ClusterId = i;
                    }
                }

                // add a line to meta
                partition.MetaData += "Iteration " + numMerges + ": \n";
                for (int i = 0; i < replacedNodes.Count; i++)
                {
                    partition.MetaData += replacedNodes[i] + " ";
                }

                partition.MetaData += "\n";
                // now remove those nodes from the attack set
                for (int i = 0; i < replacedNodes.Count; i++)
                {
                    for (int j = 0; j < partition.removedNodes.Count; j++)
                    {
                        if (partition.removedNodes[j] == replacedNodes[i])
                        {
                            partition.removedNodes.RemoveAt(j);
                        }
                    }
                }
            }

            partition.MetaData += "Removed Count:" + partition.removedNodes.Count + "\n";

            if (partition.removedNodes.Count > 0)
            {
                partition.MetaData += partition.removedNodes[0];
            }

            for (int i = 1; i < partition.removedNodes.Count; i++)
            {
                partition.MetaData += "," + partition.removedNodes[i];
            }

            return partition;
        }
    }
}