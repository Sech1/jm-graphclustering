using NetMining.ClusteringAlgo;
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
            if (Enum.TryParse<ClusterType>(args[2], ignoreCase: true, result: out var userOut))
            {
                GeneralCluster cluster;
                List<DataOutStruct> dataOut;
                switch (userOut)
                {
                    case ClusterType.G1I:
                        cluster = returnClusterAndPartition(OutType.Int, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        dataOut = G1(GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, Healthyfile, Infectedfile, OutType.Int, outList));
                        break;
                    case ClusterType.G1T:
                        cluster = returnClusterAndPartition(OutType.Ten, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        dataOut = G1(GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, Healthyfile, Infectedfile, OutType.Ten, outList));
                        break;
                    case ClusterType.G1V:
                        cluster = returnClusterAndPartition(OutType.Vat, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        // Healthy Group
                        dataOut = G1(GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, Healthyfile, Infectedfile, OutType.Vat, outList));
                        break;
                    case ClusterType.G2I:
                        cluster = returnClusterAndPartition(OutType.Int, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        dataOut = G2(GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, Healthyfile, Infectedfile, OutType.Int, outList));
                        break;
                    case ClusterType.G2T:
                        cluster = returnClusterAndPartition(OutType.Ten, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        dataOut = G2(GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, Healthyfile, Infectedfile, OutType.Ten, outList));
                        break;
                    case ClusterType.G2V:
                        cluster = returnClusterAndPartition(OutType.Vat, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        dataOut = G2(GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, Healthyfile, Infectedfile, OutType.Vat, outList));
                        break;
                    case ClusterType.G3I:
                        cluster = returnClusterAndPartition(OutType.Int, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        dataOut = G3(GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, Healthyfile, Infectedfile, OutType.Int, outList));
                        break;
                    case ClusterType.G3T:
                        cluster = returnClusterAndPartition(OutType.Ten, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        dataOut = G3(GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, Healthyfile, Infectedfile, OutType.Ten, outList));
                        break;
                    case ClusterType.G3V:
                        cluster = returnClusterAndPartition(OutType.Vat, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        dataOut = G3(GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, Healthyfile, Infectedfile, OutType.Vat, outList));
                        break;
                    case ClusterType.G4I:
                        cluster = returnClusterAndPartition(OutType.Int, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        dataOut = G4(GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, Healthyfile, Infectedfile, OutType.Int, outList));
                        break;
                    case ClusterType.G4T:
                        cluster = returnClusterAndPartition(OutType.Ten, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        dataOut = G4(GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, Healthyfile, Infectedfile, OutType.Ten, outList));
                        break;
                    case ClusterType.G4V:
                        cluster = returnClusterAndPartition(OutType.Vat, healthy, infected, healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        dataOut = G4(GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, Healthyfile, Infectedfile, OutType.Vat, outList));
                        break;
                    case ClusterType.G13:
                        GeneralCluster clusterVat = returnClusterAndPartition(OutType.Vat, healthy, infected,
                            healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        GeneralCluster clusterInt = returnClusterAndPartition(OutType.Int, healthy, infected,
                            healthyClusters,
                            infectedClusters, Healthyfile, Infectedfile);
                        List<DataOutStruct> group1V = G1(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, Healthyfile,
                            Infectedfile, OutType.Vat, outList));
                        List<DataOutStruct> group2I = G2(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int1.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, Healthyfile,
                            Infectedfile, OutType.Int, outList));
                        List<DataOutStruct> group2V = G2(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, Healthyfile,
                            Infectedfile, OutType.Vat, outList));
                        dataOut = group1V.Concat(group2I).Where(x => !group2V.Contains(x)).Distinct().ToList();
                        break;
                }
            }
            else
            {
                Console.WriteLine("Please input a valid output type (VAT, INT, TEN) as the third parameter.");
            }

            return outList;
        }

        private static List<List<DataOutStruct>> GroupInitializer(Partition healthyPart, Partition infectedPart,
            int[] healthyCount, int[] infectedCount, String Healthyfile,
            String Infectedfile, OutType type, List<List<DataOutStruct>> outList)
        {
            String fileEnd = "";
            switch (type)
            {
                case OutType.Vat:
                    fileEnd = "_VAT.csv";
                    break;
                case OutType.Int:
                    fileEnd = "_INT.csv";
                    break;
                case OutType.Ten:
                    fileEnd = "_TEN.csv";
                    break;
            }

            var healthyGroup = rename(healthyPart, healthyCount, Healthyfile, fileEnd, type);
            // Infected Group
            var infectedGroup = rename(infectedPart, infectedCount, Infectedfile, fileEnd, type);
            outList.Add(healthyGroup);
            outList.Add(infectedGroup);
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
                outObj.Bacteria = p.Graph.Nodes[i].sharedName;
                outObj.ClusterType = type.ToString();
                if (cluster[i] != -1)
                {
                    outObj.GroupNum = cluster[i].ToString();
                }
                else
                {
                    outObj.GroupNum = "N/A";
                }

                dataOut.Add(outObj);
            }

            // System.Console.WriteLine( dataOut[i].bacteria );
            using (StreamWriter sw = new StreamWriter(FileName + "_data.csv"))

                for (int i = 0; i < dataOut.Count(); i++)
                {
                    {
                        sw.WriteLine(dataOut[i].Bacteria + "," + dataOut[i].GroupNum + "," + dataOut[i].ClusterType);
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
                List<DataOutStruct> newList = dataSet.Where(x => x.GroupNum.Equals("N/A")).ToList();
                filteredList.Add(newList);
            }

            List<DataOutStruct> outList = filteredList[0].Intersect(filteredList[1]).OrderBy(x => x.Bacteria).Distinct()
                .ToList();
            return outList;
        }

        /// <summary>
        /// G2 finds all unique maching clusters
        /// </summary>
        public static List<DataOutStruct> G2(List<List<DataOutStruct>> dataSet)
        {
            List<DataOutStruct> healthy = removeDuplicate(dataSet[0]);
            List<DataOutStruct> infected = removeDuplicate(dataSet[1]);
            List<DataOutStruct> rename = new List<DataOutStruct>();
            for (int i = 0; i < healthy.Count(); i++)
            {
                for (int j = 0; j < infected.Count(); j++)
                {
                    if (healthy[i].Bacteria.Equals(infected[j].Bacteria))
                    {
                        if (healthy[i].GroupNum.Equals(infected[j].GroupNum))
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
                    sw.WriteLine(rename[i].Bacteria + "," + rename[i].GroupNum);
                }
            }

            return rename;
        }


        /// <summary>
        /// G3 finds all unique singular group numbers that are  
        /// </summary>
        public static List<DataOutStruct> G3(List<List<DataOutStruct>> dataSet)
        {
            List<DataOutStruct> healthy = removeDuplicate(dataSet[0]);
            List<DataOutStruct> infected = removeDuplicate(dataSet[1]);
            List<DataOutStruct> rename = new List<DataOutStruct>();

            addlist(rename, healthy);
            addlist(rename, infected);
            for (int i = 0; i < healthy.Count(); i++)
            {
                for (int j = 0; j < infected.Count(); j++)
                {
                    if (healthy[i].GroupNum.Equals(infected[j].GroupNum))
                    {
                        rename.Remove(healthy[i]);
                    }
                }
            }

            using (StreamWriter sw = new StreamWriter("./Data/G3.csv"))
            {
                for (int i = 0; i < rename.Count(); i++)
                {
                    sw.WriteLine(rename[i].Bacteria + " , " + rename[i].GroupNum);
                }
            }

            return rename;
        }


        /// <summary>
        /// G4 finds all bacteria with group number being "N/A" in one file but not the other 
        /// </summary>
        public static List<DataOutStruct> G4(List<List<DataOutStruct>> dataSet)
        {
            List<DataOutStruct> healthyList = new List<DataOutStruct>();
            List<DataOutStruct> infectedList = new List<DataOutStruct>();

            foreach (DataOutStruct healthy in dataSet[0])
            {
                foreach (DataOutStruct infected in dataSet[1])
                {
                    if ((healthy.Bacteria.Equals(infected.Bacteria)) &&
                        (healthy.GroupNum == "N/A" && infected.GroupNum != "N/A"))
                    {
                        healthyList.Add(healthy);
                    }
                }
            }

            foreach (DataOutStruct infected in dataSet[1])
            {
                foreach (DataOutStruct healthy in dataSet[0])
                {
                    if ((infected.Bacteria.Equals(healthy.Bacteria)) &&
                        (infected.GroupNum == "N/A" && healthy.GroupNum != "N/A"))
                    {
                        infectedList.Add(infected);
                    }
                }
            }

            infectedList = infectedList.Distinct().ToList();
            healthyList = healthyList.Distinct().ToList();
            List<DataOutStruct> outList = healthyList.Union(infectedList).Distinct().OrderBy(x => x.Bacteria).ToList();
            return outList;
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
                //LDOS1 = G4(healthy, infected);
            }

            if (l2.Contains("G2"))
            {
                //LDOS2 = G2(healthy, infected);
            }
            else
            {
                //LDOS2 = G3(healthy, infected);
            }

            if (l3.Contains("G2"))
            {
                //LDOS3 = G2(healthy, infected);
            }
            else
            {
                //LDOS3 = G3(healthy, infected);
            }

            for (int i = 0; i < LDOS2.Count(); i++)
                LDOS1.Add(LDOS2[i]);

            for (int i = 0; i < LDOS3.Count(); i++)
            {
                if (LDOS1[i].Bacteria.Equals(LDOS3[i].Bacteria))
                {
                    LDOS1.Remove(LDOS3[i]);
                }
            }

            using (StreamWriter recycle = new StreamWriter("./Data/G5.csv"))
            {
                for (int i = 0; i < LDOS1.Count(); i++)
                    recycle.WriteLine(LDOS1[i].Bacteria + "," + LDOS1[i].GroupNum);
            }
        }

        public static List<DataOutStruct> reduce(List<DataOutStruct> dos, List<string> bac)
        {
            for (int i = 0; i < dos.Count(); i++)
            {
                if (!bac.Contains(dos[i].Bacteria))
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
                    if (dos[i].Bacteria.Equals(tG1R[j].Bacteria))
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
            List<DataOutStruct> del = a.GroupBy(x => x.GroupNum)
                .Where(x => x.Count() == 1)
                .Select(x => x.FirstOrDefault()).ToList();
            return del;
        }
    } //-end of class
}