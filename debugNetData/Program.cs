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

            if (!Directory.Exists(datapath))
            {
                Directory.CreateDirectory(datapath);
                Console.WriteLine("Please move <healthyfile> and/or <infectedfile> to: " + datapath);
                Environment.Exit(0);
            }

            if (args.Length == 0)
            {
                Console.WriteLine(
                    "Usage: Program.cs <Healthyfile> <Infectedfile> <clusterType> ");
                Environment.Exit(0);
            }

            // AUTOMATING IBD 
            // We need both a healthy network and an IBD network
            // COMMAND LINE: clusteringanalysis.exe healthyNet infectedNet VATorINTorTEN  


            //convert from gml to graph
            String healthyfile = $"{workingDir}//Data//{args[0]}";
            String infectedfile = $"{workingDir}//Data//{args[1]}";

            LightWeightGraph healthy = LightWeightGraph.GetGraphFromGML($"{healthyfile}");
            LightWeightGraph infected = LightWeightGraph.GetGraphFromGML($"{infectedfile}");
            healthyfile = healthyfile.Split('.')[0];
            infectedfile = infectedfile.Split('.')[0];
            healthy.SaveGraph(healthyfile + ".graph");
            infected.SaveGraph(infectedfile + ".graph");
            // Makes a list of what the nodes reference
            using (StreamWriter sw = new StreamWriter(healthyfile + ".txt", true))
            {
                for (int i = 0; i < healthy.Nodes.Length; i++)
                {
                    sw.WriteLine(healthy.Nodes[i].sharedName);
                }
            }

            using (StreamWriter sw = new StreamWriter(infectedfile + ".txt", true))
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
                List<DataOutStruct> outData = ConstructList(args, healthy, infected, healthyfile, infectedfile,
                    healthyClusters, infectedClusters);
                using (StreamWriter sw = new StreamWriter(args + ".csv"))
                {
                    for (int i = 0; i < outData.Count(); i++)
                        sw.WriteLine(outData[i].Bacteria + ", " + outData[i].GroupNum);
                }

                Console.WriteLine(args + ".csv successfully created");
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

        private static List<DataOutStruct> ConstructList(string[] args, LightWeightGraph healthy,
            LightWeightGraph infected, String healthyfile, String infectedfile, int healthyClusters,
            int infectedClusters)
        {
            List<List<DataOutStruct>> outList = new List<List<DataOutStruct>>();
            List<DataOutStruct> dataOut = new List<DataOutStruct>();
            if (Enum.TryParse<ClusterType>(args[2], ignoreCase: true, result: out var userOut))
            {
                GeneralCluster cluster;
                GeneralCluster clusterVat = ReturnClusterAndPartition(OutType.Vat, healthy, infected,
                    healthyClusters,
                    infectedClusters, healthyfile, infectedfile);
                GeneralCluster clusterTen = ReturnClusterAndPartition(OutType.Ten, healthy, infected,
                    healthyClusters,
                    infectedClusters, healthyfile, infectedfile);
                GeneralCluster clusterInt = ReturnClusterAndPartition(OutType.Int, healthy, infected,
                    healthyClusters,
                    infectedClusters, healthyfile, infectedfile);
                List<DataOutStruct> d1;
                List<DataOutStruct> d2;
                List<DataOutStruct> d3;
                switch (userOut)
                {
                    case ClusterType.G1I:
                        cluster = ReturnClusterAndPartition(OutType.Int, healthy, infected, healthyClusters,
                            infectedClusters, healthyfile, infectedfile);
                        dataOut = G1(GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, healthyfile, infectedfile, OutType.Int, outList));
                        break;
                    case ClusterType.G1T:
                        cluster = ReturnClusterAndPartition(OutType.Ten, healthy, infected, healthyClusters,
                            infectedClusters, healthyfile, infectedfile);
                        dataOut = G1(GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, healthyfile, infectedfile, OutType.Ten, outList));
                        break;
                    case ClusterType.G1V:
                        cluster = ReturnClusterAndPartition(OutType.Vat, healthy, infected, healthyClusters,
                            infectedClusters, healthyfile, infectedfile);
                        // Healthy Group
                        dataOut = G1(GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, healthyfile, infectedfile, OutType.Vat, outList));
                        break;
                    case ClusterType.G2I:
                        cluster = ReturnClusterAndPartition(OutType.Int, healthy, infected, healthyClusters,
                            infectedClusters, healthyfile, infectedfile);
                        dataOut = G2(GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, healthyfile, infectedfile, OutType.Int, outList));
                        break;
                    case ClusterType.G2T:
                        cluster = ReturnClusterAndPartition(OutType.Ten, healthy, infected, healthyClusters,
                            infectedClusters, healthyfile, infectedfile);
                        dataOut = G2(GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, healthyfile, infectedfile, OutType.Ten, outList));
                        break;
                    case ClusterType.G2V:
                        cluster = ReturnClusterAndPartition(OutType.Vat, healthy, infected, healthyClusters,
                            infectedClusters, healthyfile, infectedfile);
                        dataOut = G2(GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, healthyfile, infectedfile, OutType.Vat, outList));
                        break;
                    case ClusterType.G3I:
                        cluster = ReturnClusterAndPartition(OutType.Int, healthy, infected, healthyClusters,
                            infectedClusters, healthyfile, infectedfile);
                        dataOut = G3(GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, healthyfile, infectedfile, OutType.Int, outList));
                        break;
                    case ClusterType.G3T:
                        cluster = ReturnClusterAndPartition(OutType.Ten, healthy, infected, healthyClusters,
                            infectedClusters, healthyfile, infectedfile);
                        dataOut = G3(GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, healthyfile, infectedfile, OutType.Ten, outList));
                        break;
                    case ClusterType.G3V:
                        cluster = ReturnClusterAndPartition(OutType.Vat, healthy, infected, healthyClusters,
                            infectedClusters, healthyfile, infectedfile);
                        dataOut = G3(GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, healthyfile, infectedfile, OutType.Vat, outList));
                        break;
                    case ClusterType.G4I:
                        cluster = ReturnClusterAndPartition(OutType.Int, healthy, infected, healthyClusters,
                            infectedClusters, healthyfile, infectedfile);
                        dataOut = G4(GroupInitializer(cluster.Int0.Partition, cluster.Int1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, healthyfile, infectedfile, OutType.Int, outList));
                        break;
                    case ClusterType.G4T:
                        cluster = ReturnClusterAndPartition(OutType.Ten, healthy, infected, healthyClusters,
                            infectedClusters, healthyfile, infectedfile);
                        dataOut = G4(GroupInitializer(cluster.Ten0.Partition, cluster.Ten1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, healthyfile, infectedfile, OutType.Ten, outList));
                        break;
                    case ClusterType.G4V:
                        cluster = ReturnClusterAndPartition(OutType.Vat, healthy, infected, healthyClusters,
                            infectedClusters, healthyfile, infectedfile);
                        dataOut = G4(GroupInitializer(cluster.Vat0.Partition, cluster.Vat1.Partition,
                            cluster.HealthyCount,
                            cluster.InfectedCount, healthyfile, infectedfile, OutType.Vat, outList));
                        break;

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //                                            start of G13 - G25                                               //
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    case ClusterType.G13:
                        d1 = G1(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        d2 = G2(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int1.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d3 = G2(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G14:
                        d1 = G1(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        d2 = G2(GroupInitializer(clusterTen.Ten0.Partition,
                            clusterTen.Ten1.Partition, clusterTen.HealthyCount, clusterTen.InfectedCount, healthyfile,
                            infectedfile, OutType.Ten, outList));
                        d3 = G2(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int1.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G15:
                        d1 = G1(GroupInitializer(clusterTen.Ten0.Partition,
                            clusterTen.Ten0.Partition, clusterTen.HealthyCount, clusterTen.InfectedCount, healthyfile,
                            infectedfile, OutType.Ten, outList));
                        d2 = G2(GroupInitializer(clusterTen.Ten0.Partition,
                            clusterTen.Ten1.Partition, clusterTen.HealthyCount, clusterTen.InfectedCount, healthyfile,
                            infectedfile, OutType.Ten, outList));
                        d3 = G2(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int1.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G16:
                        d1 = G1(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int0.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d2 = G2(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int1.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d3 = G2(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G17:
                        d1 = G1(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        d2 = G3(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int1.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d3 = G3(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G18:
                        d1 = G1(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int1.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d2 = G3(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int1.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d3 = G3(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G19:
                        d1 = G1(GroupInitializer(clusterTen.Ten0.Partition,
                            clusterTen.Ten1.Partition, clusterTen.HealthyCount, clusterTen.InfectedCount, healthyfile,
                            infectedfile, OutType.Ten, outList));
                        d2 = G3(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int1.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d3 = G3(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G20:
                        d1 = G1(GroupInitializer(clusterTen.Ten0.Partition,
                            clusterTen.Ten1.Partition, clusterTen.HealthyCount, clusterTen.InfectedCount, healthyfile,
                            infectedfile, OutType.Ten, outList));
                        d2 = G3(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int1.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d3 = G3(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G21:
                        d1 = G4(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        d2 = G3(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int1.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d3 = G3(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G22:
                        d1 = G4(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int1.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d2 = G3(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int1.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d3 = G3(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G23:
                        d1 = G4(GroupInitializer(clusterTen.Ten0.Partition,
                            clusterTen.Ten1.Partition, clusterTen.HealthyCount, clusterTen.InfectedCount, healthyfile,
                            infectedfile, OutType.Ten, outList));
                        d2 = G3(GroupInitializer(clusterInt.Int0.Partition,
                            clusterInt.Int1.Partition, clusterInt.HealthyCount, clusterInt.InfectedCount, healthyfile,
                            infectedfile, OutType.Int, outList));
                        d3 = G3(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G24:
                        d1 = G4(GroupInitializer(clusterTen.Ten0.Partition,
                            clusterTen.Ten1.Partition, clusterTen.HealthyCount, clusterTen.InfectedCount, healthyfile,
                            infectedfile, OutType.Ten, outList));
                        d2 = G3(GroupInitializer(clusterTen.Ten0.Partition,
                            clusterTen.Ten1.Partition, clusterTen.HealthyCount, clusterTen.InfectedCount, healthyfile,
                            infectedfile, OutType.Ten, outList));
                        d3 = G3(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;

                    case ClusterType.G25:
                        d1 = G4(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        d2 = G3(GroupInitializer(clusterTen.Ten0.Partition,
                            clusterTen.Ten1.Partition, clusterTen.HealthyCount, clusterTen.InfectedCount, healthyfile,
                            infectedfile, OutType.Ten, outList));
                        d3 = G3(GroupInitializer(clusterVat.Vat0.Partition,
                            clusterVat.Vat1.Partition, clusterVat.HealthyCount, clusterVat.InfectedCount, healthyfile,
                            infectedfile, OutType.Vat, outList));
                        dataOut = d1.Union(d2).Where(x => !d3.Contains(x)).OrderBy(x => x.Bacteria).Distinct().ToList();
                        break;
                }
            }
            else
            {
                Console.WriteLine("Please input a valid output type (VAT, INT, TEN) as the third parameter.");
            }

            return dataOut;
        }

        private static List<List<DataOutStruct>> GroupInitializer(Partition healthyPart, Partition infectedPart,
            int[] healthyCount, int[] infectedCount, String healthyfile,
            String infectedfile, OutType type, List<List<DataOutStruct>> outList)
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

            var healthyGroup = GroupNumberInitializer(healthyPart, healthyCount, healthyfile, fileEnd, type);
            // Infected Group
            var infectedGroup = GroupNumberInitializer(infectedPart, infectedCount, infectedfile, fileEnd, type);
            outList.Add(healthyGroup);
            outList.Add(infectedGroup);
            return outList;
        }

        private static GeneralCluster ReturnClusterAndPartition(OutType type, LightWeightGraph healthy,
            LightWeightGraph infected, int healthyClusters, int infectedClusters, String healthyfile,
            String infectedfile)
        {
            GeneralCluster cluster = new GeneralCluster();
            switch (type)
            {
                case OutType.Int:
                    cluster.Int0.Cluster =
                        new HIntegrityClust(healthy, healthyClusters + 1, false, 1, 0, false, false);
                    cluster.Int0.Partition = cluster.Int0.Cluster.GetPartition();
                    cluster.Int0.Partition.SavePartition(healthyfile + "_INT.cluster", healthyfile + ".graph");
                    cluster.Int1.Cluster =
                        new HIntegrityClust(infected, infectedClusters + 1, false, 1, 0, false, false);
                    cluster.Int1.Partition = cluster.Int1.Cluster.GetPartition();
                    cluster.Int1.Partition.SavePartition(infectedfile + "_INT.cluster", infectedfile + ".graph");
                    cluster.HealthyCount = new int[cluster.Int0.Partition.DataCount];
                    cluster.InfectedCount = new int[cluster.Int1.Partition.DataCount];
                    break;
                case OutType.Ten:
                    cluster.Ten0.Cluster =
                        new HTenacityClust(healthy, healthyClusters + 1, false, 1, 0, false, false);
                    cluster.Ten0.Partition = cluster.Ten0.Cluster.GetPartition();
                    cluster.Ten0.Partition.SavePartition(healthyfile + "_TEN.cluster", healthyfile + ".graph");
                    cluster.Ten1.Cluster =
                        new HTenacityClust(infected, infectedClusters + 1, false, 1, 0, false, false);
                    cluster.Ten1.Partition = cluster.Ten1.Cluster.GetPartition();
                    cluster.Ten1.Partition.SavePartition(infectedfile + "_TEN.cluster", infectedfile + ".graph");
                    cluster.HealthyCount = new int[cluster.Ten0.Partition.DataCount];
                    cluster.InfectedCount = new int[cluster.Ten1.Partition.DataCount];
                    break;
                case OutType.Vat:
                    cluster.Vat0.Cluster = new HVATClust(healthy, healthyClusters + 1, false, 1, 0, false, false);
                    cluster.Vat0.Partition = cluster.Vat0.Cluster.GetPartition();
                    cluster.Vat0.Partition.SavePartition(healthyfile + "_VAT.cluster", healthyfile + ".graph");
                    cluster.Vat1.Cluster =
                        new HVATClust(infected, infectedClusters + 1, false, 1, 0, false, false);
                    cluster.Vat1.Partition = cluster.Vat1.Cluster.GetPartition();
                    cluster.Vat1.Partition.SavePartition(infectedfile + "_VAT.cluster", infectedfile + ".graph");
                    cluster.HealthyCount = new int[cluster.Vat0.Partition.DataCount];
                    cluster.InfectedCount = new int[cluster.Vat1.Partition.DataCount];
                    break;
            }

            return cluster;
        }

        private static List<DataOutStruct> GroupNumberInitializer(Partition p, int[] cluster, String fileName,
            String fileEnd,
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
            using (StreamWriter sw = new StreamWriter(fileName + fileEnd))

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
            List<DataOutStruct> healthy = RemoveDuplicate(dataSet[0]);
            List<DataOutStruct> infected = RemoveDuplicate(dataSet[1]);
            List<DataOutStruct> dataout = new List<DataOutStruct>();
            for (int i = 0; i < healthy.Count(); i++)
            {
                for (int j = 0; j < infected.Count(); j++)
                {
                    if (healthy[i].Bacteria.Equals(infected[j].Bacteria))
                    {
                        if (healthy[i].GroupNum.Equals(infected[j].GroupNum))
                        {
                            dataout.Add(healthy[i]);
                        }
                    }
                }
            }

            using (StreamWriter sw = new StreamWriter("./Data/G2.csv"))
            {
                for (int i = 0; i < dataout.Count(); i++)
                {
                    sw.WriteLine(dataout[i].Bacteria + "," + dataout[i].GroupNum);
                }
            }

            return dataout;
        }


        /// <summary>
        /// G3 finds all unique singular group numbers that are  
        /// </summary>
        public static List<DataOutStruct> G3(List<List<DataOutStruct>> dataSet)
        {
            List<DataOutStruct> healthy = RemoveDuplicate(dataSet[0]);
            List<DataOutStruct> infected = RemoveDuplicate(dataSet[1]);
            List<DataOutStruct> dataout = new List<DataOutStruct>();

            Addlist(dataout, healthy);
            Addlist(dataout, infected);
            for (int i = 0; i < healthy.Count(); i++)
            {
                for (int j = 0; j < infected.Count(); j++)
                {
                    if (healthy[i].GroupNum.Equals(infected[j].GroupNum))
                    {
                        dataout.Remove(healthy[i]);
                    }
                }
            }

            using (StreamWriter sw = new StreamWriter("./Data/G3.csv"))
            {
                for (int i = 0; i < dataout.Count(); i++)
                {
                    sw.WriteLine(dataout[i].Bacteria + " , " + dataout[i].GroupNum);
                }
            }

            return dataout;
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

        public static List<DataOutStruct> Reduce(List<DataOutStruct> dos, List<string> bac)
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

        public static List<DataOutStruct> Reuse(List<DataOutStruct> dos, List<DataOutStruct> g1R)
        {
            List<DataOutStruct> temp = new List<DataOutStruct>();
            List<DataOutStruct> tG1R = g1R;

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

        public static List<DataOutStruct> Addlist(List<DataOutStruct> addtolist, List<DataOutStruct> existinglist)
        {
            for (int i = 0; i < existinglist.Count(); i++)
            {
                addtolist.Add(existinglist[i]);
            }

            return addtolist;
        }

        public static List<DataOutStruct> RemoveDuplicate(List<DataOutStruct> a)
        {
            List<DataOutStruct> del = a.GroupBy(x => x.GroupNum)
                .Where(x => x.Count() == 1)
                .Select(x => x.FirstOrDefault()).ToList();
            return del;
        }
    } //-end of class
}