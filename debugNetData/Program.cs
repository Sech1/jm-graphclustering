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

        public static List<DataOutStruct> rename(Partition p, int[] cluster, String FileName, String FileEnd,
            OutType type)
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

                dataOut.Add(outObj);
            }
            
            return dataOut;
        }
    }
}