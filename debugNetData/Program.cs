﻿using NetMining.ClusteringAlgo;
using NetMining.Graphs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace debugNetData
{
    class Program
    {
        static void Main(string[] args)
        {
            String workingDir = Directory.GetCurrentDirectory();
            String datapath = workingDir + "/Data";
            String[] filepaths = Directory.GetFiles(datapath);

            if (!Directory.Exists(datapath))
            {
                Directory.CreateDirectory(datapath);
                Console.WriteLine("Please move <healthyfile> and/or <infectedfile> to: " + datapath);
                Environment.Exit(0);
            }

            if (args.Length == 0)
            {
                System.Console.WriteLine(
                    "Usage: Program.cs <Healthyfile> <Infectedfile> <clusterType> ");
                Environment.Exit(0);
            }

            // AUTOMATING IBD 
            // We need both a healthy network and an IBD network
            // COMMAND LINE: clusteringanalysis.exe healthyNet infectedNet VATorINTorTEN  


            //convert from gml to graph
            String Healthyfile = $"{workingDir}//Data//{args[0]}";
            String Infectedfile = $"{workingDir}//Data//{args[1]}";

            LightWeightGraph healthy = LightWeightGraph.GetGraphFromGML($"{Healthyfile}");
            LightWeightGraph infected = LightWeightGraph.GetGraphFromGML($"{Infectedfile}");
            Healthyfile = Healthyfile.Split('.')[0];
            Infectedfile = Infectedfile.Split('.')[0];
            healthy.SaveGraph(Healthyfile + ".graph");
            infected.SaveGraph(Infectedfile + ".graph");
            // Makes a list of what the nodes reference
            using (StreamWriter sw = new StreamWriter(Healthyfile + ".txt", true))
            {
                for (int i = 0; i < healthy.Nodes.Length; i++)
                {
                    sw.WriteLine(healthy.Nodes[i].sharedName);
                }
            }

            using (StreamWriter sw = new StreamWriter(Infectedfile + ".txt", true))
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
            if (args.Length == 3)
            {
                List<List<DataOutStruct>> outData = constructLists(args, healthy, infected, Healthyfile, Infectedfile,
                    healthyClusters, infectedClusters);
            }
            else
            {
                Console.WriteLine(
                    "Please enter 3 parameters, in this order:\n " +
                    "Healthy data path(.gml)\n " +
                    "Unhealthy data path(.gml)\n " +
                    "Desired Output Group(listed in Readme)\n");
            }
        }

        private static List<List<DataOutStruct>> constructLists(string[] args, LightWeightGraph healthy,
            LightWeightGraph infected, String Healthyfile, String Infectedfile, int healthyClusters,
            int infectedClusters)
        {
            List<List<DataOutStruct>> outList = new List<List<DataOutStruct>>();
            GeneralCluster cluster = new GeneralCluster();
            if (Enum.TryParse<ClusterType>(args[2], ignoreCase: true, result: out var userOut))
            {
                // Healthy Group
                List<DataOutStruct> healthyGroup;
                List<DataOutStruct> infectedGroup;

                switch (userOut)
                {
                    case ClusterType.G1I:
                        cluster = returnClusterAndPartition(OutType.Int, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        // Healthy Group
                        healthyGroup = rename(cluster.Int0.Partition, cluster.HealthyCount, Healthyfile, "_INT.csv",
                            OutType.Int);
                        // Infected Group
                        infectedGroup = rename(cluster.Int1.Partition, cluster.InfectedCount, Infectedfile, "_INT.csv",
                            OutType.Int);

                        outList.Add(healthyGroup);
                        outList.Add(infectedGroup);
                        List<DataOutStruct> g1 = G1(outList);
                        G2(healthyGroup, infectedGroup);
                        G3(healthyGroup, infectedGroup);
                        G4(healthyGroup, infectedGroup);

                        break;
                    case ClusterType.G1T:
                        cluster = returnClusterAndPartition(OutType.Ten, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        // Healthy Group
                        healthyGroup = rename(cluster.Ten0.Partition, cluster.HealthyCount, Healthyfile, "_TEN.csv",
                            OutType.Ten);
                        // Infected Group
                        infectedGroup = rename(cluster.Ten1.Partition, cluster.InfectedCount, Infectedfile, "_TEN.csv",
                            OutType.Ten);
                        outList.Add(healthyGroup);
                        outList.Add(infectedGroup);

                        G1(outList);
                        G2(healthyGroup, infectedGroup);
                        G3(healthyGroup, infectedGroup);
                        G4(healthyGroup, infectedGroup);

                        break;
                    case ClusterType.G1V:
                        cluster = returnClusterAndPartition(OutType.Vat, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        // Healthy Group
                        healthyGroup = rename(cluster.Vat0.Partition, cluster.HealthyCount, Healthyfile, "_VAT.csv",
                            OutType.Vat);
                        // Infected Group
                        infectedGroup = rename(cluster.Vat1.Partition, cluster.InfectedCount, Infectedfile, "_VAT.csv",
                            OutType.Vat);
                        outList.Add(healthyGroup);
                        outList.Add(infectedGroup);

                        G1(outList);
                        G2(healthyGroup, infectedGroup);
                        G3(healthyGroup, infectedGroup);
                        G4(healthyGroup, infectedGroup);

                        break;
                }
            }
            else
            {
                Console.WriteLine("Please input a valid output type (VAT, INT, TEN) as the third parameter.");
            }

            return outList;
        }

        private static GeneralCluster returnClusterAndPartition(OutType type, LightWeightGraph healthy,
            LightWeightGraph infected, int healthyClusters, int infectedClusters, String Healthyfile,
            String Infectedfile)
        {
            GeneralCluster cluster = new GeneralCluster();
            switch (type)
            {
                case OutType.Int:
                    cluster.Int0.Cluster =
                        new HIntegrityClust(healthy, healthyClusters + 1, false, 1, 0, false, false);
                    cluster.Int0.Partition = cluster.Int0.Cluster.GetPartition();
                    cluster.Int0.Partition.SavePartition(Healthyfile + "_INT.cluster", Healthyfile + ".graph");
                    cluster.Int1.Cluster =
                        new HIntegrityClust(infected, infectedClusters + 1, false, 1, 0, false, false);
                    cluster.Int1.Partition = cluster.Int1.Cluster.GetPartition();
                    cluster.Int1.Partition.SavePartition(Infectedfile + "_INT.cluster", Infectedfile + ".graph");
                    cluster.HealthyCount = new int[cluster.Int0.Partition.DataCount];
                    cluster.InfectedCount = new int[cluster.Int1.Partition.DataCount];
                    break;
                case OutType.Ten:
                    cluster.Ten0.Cluster =
                        new HTenacityClust(healthy, healthyClusters + 1, false, 1, 0, false, false);
                    cluster.Ten0.Partition = cluster.Ten0.Cluster.GetPartition();
                    cluster.Ten0.Partition.SavePartition(Healthyfile + "_TEN.cluster", Healthyfile + ".graph");
                    cluster.Ten1.Cluster =
                        new HTenacityClust(infected, infectedClusters + 1, false, 1, 0, false, false);
                    cluster.Ten1.Partition = cluster.Ten1.Cluster.GetPartition();
                    cluster.Ten1.Partition.SavePartition(Infectedfile + "_TEN.cluster", Infectedfile + ".graph");
                    cluster.HealthyCount = new int[cluster.Ten0.Partition.DataCount];
                    cluster.InfectedCount = new int[cluster.Ten1.Partition.DataCount];
                    break;
                case OutType.Vat:
                    cluster.Vat0.Cluster = new HVATClust(healthy, healthyClusters + 1, false, 1, 0, false, false);
                    cluster.Vat0.Partition = cluster.Vat0.Cluster.GetPartition();
                    cluster.Vat0.Partition.SavePartition(Healthyfile + "_VAT.cluster", Healthyfile + ".graph");
                    cluster.Vat1.Cluster =
                        new HVATClust(infected, infectedClusters + 1, false, 1, 0, false, false);
                    cluster.Vat1.Partition = cluster.Vat1.Cluster.GetPartition();
                    cluster.Vat1.Partition.SavePartition(Infectedfile + "_VAT.cluster", Infectedfile + ".graph");
                    cluster.HealthyCount = new int[cluster.Vat0.Partition.DataCount];
                    cluster.InfectedCount = new int[cluster.Vat1.Partition.DataCount];
                    break;
            }

            return cluster;
        }

        private static List<DataOutStruct> rename(Partition p, int[] cluster, String FileName, String FileEnd,
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

            // System.Console.WriteLine( dataOut[i].bacteria );
            using (StreamWriter sw = new StreamWriter(FileName + "_data.csv"))

                for (int i = 0; i < dataOut.Count(); i++)
                {
                    {
                        sw.WriteLine(dataOut[i].bacteria + "," + dataOut[i].groupNum + "," + dataOut[i].clusterType);
                    }
                }

            return dataOut;
        }


        /// <summary>
        /// G1 finds all matching gml clusters with "N/A"
        /// </summary>
        public static List<DataOutStruct> G1(List<List<DataOutStruct>> data)
        {
            List<List<DataOutStruct>> filteredList = new List<List<DataOutStruct>>();
            foreach (List<DataOutStruct> dataSet in data)
            {
                List<DataOutStruct> newList = dataSet.Where(x => x.groupNum.Equals("N/A")).ToList();
                filteredList.Add(newList);
            }

            List<DataOutStruct> outList = filteredList[0].Union(filteredList[1]).ToList();

            return outList;
        }

        /// <summary>
        /// G2 finds all unique maching clusters
        /// </summary>
        public static List<DataOutStruct> G2(List<DataOutStruct> healthy, List<DataOutStruct> infected)
        {
            healthy = removeDuplicate(healthy);
            infected = removeDuplicate(infected);
            List<DataOutStruct> rename = new List<DataOutStruct>();
            for (int i = 0; i < healthy.Count(); i++)
            {
                for (int j = 0; j < infected.Count(); j++)
                {
                    if (healthy[i].bacteria.Equals(infected[j].bacteria))
                    {
                        if (healthy[i].groupNum.Equals(infected[j].groupNum))
                        {
                            rename.Add(healthy[i]);
                        }
                    }
                }
            }

            using (StreamWriter sw = new StreamWriter("./Data/G2.csv"))
            {
                for (int i = 0; i < rename.Count(); i++)
                {
                    sw.WriteLine(rename[i].bacteria + "," + rename[i].groupNum);
                }
            }

            return rename;
        }


        /// <summary>
        /// G3 finds all unique singular group numbers that are  
        /// </summary>
        public static List<DataOutStruct> G3(List<DataOutStruct> healthy, List<DataOutStruct> infected)
        {
            healthy = removeDuplicate(healthy);
            infected = removeDuplicate(infected);
            List<DataOutStruct> rename = new List<DataOutStruct>();

            addlist(rename, healthy);
            addlist(rename, infected);
            for (int i = 0; i < healthy.Count(); i++)
            {
                for (int j = 0; j < infected.Count(); j++)
                {
                    if (healthy[i].groupNum.Equals(infected[j].groupNum))
                    {
                        rename.Remove(healthy[i]);
                    }
                }
            }

            using (StreamWriter sw = new StreamWriter("./Data/G3.csv"))
            {
                for (int i = 0; i < rename.Count(); i++)
                {
                    sw.WriteLine(rename[i].bacteria + " , " + rename[i].groupNum);
                }
            }

            return rename;
        }


        /// <summary>
        /// G4 finds all bacteria with group number being "N/A" in one file but not the other 
        /// </summary>
        public static List<DataOutStruct> G4(List<DataOutStruct> healthy, List<DataOutStruct> infected)
        {
            List<string> IBAC = new List<string>();
            List<DataOutStruct> G1Ret = new List<DataOutStruct>();
            for (int j = 0; j < infected.Count(); j++)
            {
                IBAC.Add(infected[j].bacteria);
            }

            healthy = reduce(healthy, IBAC);
            infected = reduce(infected, IBAC);
            for (int i = 0; i < healthy.Count(); i++)
            {
                for (int j = 0; j < IBAC.Count(); j++)
                {
                    if (healthy[i].bacteria.Equals(IBAC[j]))
                    {
                        G1Ret.Add(healthy[i]);
                    }
                }
            }

            List<DataOutStruct> health = reuse(healthy, G1Ret);
            List<DataOutStruct> infect = reuse(infected, G1Ret);
            G1Ret.Clear();
            for (int i = 0; i < health.Count(); i++)
            {
                for (int j = 0; j < infect.Count(); j++)
                {
                    if (health[i].bacteria.Equals(infect[j].bacteria))
                    {
                        if ((health[i].groupNum.Equals(("N/A")) && infect[i].groupNum.Equals("N/A")))
                        {
                            G1Ret.Add(health[i]);
                        }
                    }
                }
            }

            using (StreamWriter recycle = new StreamWriter("./Data/G4.csv"))
            {
                for (int i = 0; i < G1Ret.Count(); i++)
                    recycle.WriteLine(G1Ret[i].bacteria + "," + G1Ret[i].groupNum);
            }

            return G1Ret;
        }

        public static void G5(List<DataOutStruct> healthy, List<DataOutStruct> infected, String l1, String l2,
            String l3)
        {
            List<DataOutStruct> LDOS1 = new List<DataOutStruct>();
            List<DataOutStruct> LDOS2 = new List<DataOutStruct>();
            List<DataOutStruct> LDOS3 = new List<DataOutStruct>();

            if (l1.Contains("G1"))
            {
                //LDOS1 = G1(healthy, infected);
            }
            else
            {
                LDOS1 = G4(healthy, infected);
            }

            if (l2.Contains("G2"))
            {
                LDOS2 = G2(healthy, infected);
            }
            else
            {
                LDOS2 = G3(healthy, infected);
            }

            if (l3.Contains("G2"))
            {
                LDOS3 = G2(healthy, infected);
            }
            else
            {
                LDOS3 = G3(healthy, infected);
            }

            for (int i = 0; i < LDOS2.Count(); i++)
                LDOS1.Add(LDOS2[i]);

            for (int i = 0; i < LDOS3.Count(); i++)
            {
                if (LDOS1[i].bacteria.Equals(LDOS3[i].bacteria))
                {
                    LDOS1.Remove(LDOS3[i]);
                }
            }

            using (StreamWriter recycle = new StreamWriter("./Data/G5.csv"))
            {
                for (int i = 0; i < LDOS1.Count(); i++)
                    recycle.WriteLine(LDOS1[i].bacteria + "," + LDOS1[i].groupNum);
            }
        }

        public static List<DataOutStruct> reduce(List<DataOutStruct> dos, List<string> bac)
        {
            for (int i = 0; i < dos.Count(); i++)
            {
                if (!bac.Contains(dos[i].bacteria))
                {
                    dos.Remove(dos[i]);
                }
            }

            return dos;
        }

        public static List<DataOutStruct> reuse(List<DataOutStruct> dos, List<DataOutStruct> G1R)
        {
            List<DataOutStruct> temp = new List<DataOutStruct>();
            List<DataOutStruct> tG1R = G1R;

            for (int i = 0; i < dos.Count(); i++)
            {
                for (int j = 0; j < tG1R.Count(); j++)
                {
                    if (dos[i].bacteria.Equals(tG1R[j].bacteria))
                    {
                        temp.Add(dos[i]);
                    }
                }
            }

            return temp;
        }

        public static List<DataOutStruct> addlist(List<DataOutStruct> addtolist, List<DataOutStruct> existinglist)
        {
            for (int i = 0; i < existinglist.Count(); i++)
            {
                addtolist.Add(existinglist[i]);
            }

            return addtolist;
        }

        public static List<DataOutStruct> removeDuplicate(List<DataOutStruct> a)
        {
            List<DataOutStruct> del = a.GroupBy(x => x.groupNum)
                .Where(x => x.Count() == 1)
                .Select(x => x.FirstOrDefault()).ToList();
            return del;
        }
    } //-end of class
}